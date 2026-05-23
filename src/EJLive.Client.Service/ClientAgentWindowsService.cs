using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using EJLive.Client.Service.Compatibility;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EJLive.Client.Service;

/// <summary>
/// Windows Service host for the EJLive client agent.
/// Uses BackgroundService and IAgentController so the production client remains
/// headless, service-safe, and free from direct WinForms dependencies.
/// </summary>
public sealed class ClientAgentWindowsService : BackgroundService
{
    private readonly ILogger<ClientAgentWindowsService> _logger;
    private readonly object _lifecycleLock = new();

    private IAgentController? _agent;
    private AgentHealthReporter? _health;
    private DateTime _lastStatusLogUtc;
    private DateTime _lastCompanionRegistrationCheckUtc;
    private DateTime _lastRestartAttemptUtc;

    public ClientAgentWindowsService(ILogger<ClientAgentWindowsService> logger)
    {
        _logger = logger;
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("EJLive Client Agent Service starting.");
        return base.StartAsync(cancellationToken);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        stoppingToken.Register(StopAgentSafely);
        return RunSupervisionLoopAsync(stoppingToken);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("EJLive Client Agent Service stopping.");
        StopAgentSafely();
        return base.StopAsync(cancellationToken);
    }

    private async Task RunSupervisionLoopAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                EnsureAgentRunning();
                RestartAgentIfFailed();
                LogAgentStatusPeriodically();
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Service supervision loop encountered an error. Retrying.");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken).ConfigureAwait(false);
            }
        }
    }

    private void EnsureAgentRunning()
    {
        lock (_lifecycleLock)
        {
            if (_agent != null)
                return;

            _agent = CreateAgentController();
            _agent.OnLog += message => _logger.LogInformation("{Message}", message);
            _agent.OnStatusUpdate += status =>
                _logger.LogInformation(
                    "Agent status: state={State}, connected={Connected}, handshake={Handshake}, pending={Pending}",
                    status.State,
                    status.Connected,
                    status.HandshakeComplete,
                    status.PendingOutboxItems);

            _agent.StartAll();

            var healthFile = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "EJLive", "Agent", "health.json");

            _health = new AgentHealthReporter(_agent, TimeSpan.FromSeconds(30), healthFile);
            _health.Publish();

            _lastStatusLogUtc = DateTime.UtcNow;
            _lastCompanionRegistrationCheckUtc = DateTime.MinValue;

            _logger.LogInformation("Agent supervision initialized. Controller={ControllerType}", _agent.GetType().Name);
        }
    }

    private void RestartAgentIfFailed()
    {
        var agent = _agent;
        if (agent == null)
            return;

        var status = SafeGetStatus(agent);
        if (status.State != AgentControllerState.Failed)
            return;

        var now = DateTime.UtcNow;
        if (now - _lastRestartAttemptUtc < TimeSpan.FromMinutes(1))
            return;

        _lastRestartAttemptUtc = now;
        _logger.LogWarning("Agent is in Failed state. Restarting headless controller.");

        StopAgentSafely();
        EnsureAgentRunning();
    }

    private AgentStatus SafeGetStatus(IAgentController agent)
    {
        try
        {
            return agent.GetStatus();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Unable to read agent status.");
            return new AgentStatus(
                AgentControllerState.Failed,
                false,
                false,
                0,
                0,
                0,
                null,
                null,
                null,
                ex.Message);
        }
    }

    private void StopAgentSafely()
    {
        lock (_lifecycleLock)
        {
            try
            {
                _health?.Dispose();
                _agent?.StopAll();
                _agent?.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error while stopping EJLive agent.");
            }
            finally
            {
                _health = null;
                _agent = null;
            }
        }
    }

    private IAgentController CreateAgentController()
    {
        try
        {
            var legacyType =
                ReflectionSafe.FindType(
                    "EJLive.Client.WinForms.Agent.AgentBootstrapper",
                    "EJLive.Client.WinForms.Agent.AgentBootstrapper, EJLive.Client.WinForms");

            if (legacyType != null && typeof(IAgentController).IsAssignableFrom(legacyType))
            {
                var instance = (IAgentController?)Activator.CreateInstance(legacyType);
                if (instance != null)
                    return instance;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Legacy bootstrapper discovery failed.");
        }

        return new AgentHeadlessController();
    }

    private void LogAgentStatusPeriodically()
    {
        var agent = _agent;
        if (agent == null)
            return;

        var now = DateTime.UtcNow;
        if (now - _lastStatusLogUtc < TimeSpan.FromMinutes(2))
            return;

        _lastStatusLogUtc = now;

        var status = SafeGetStatus(agent);
        _logger.LogInformation(
            "Health check: state={State}, connected={Connected}, handshake={Handshake}, pending={Pending}, session={SessionId}",
            status.State,
            status.Connected,
            status.HandshakeComplete,
            status.PendingOutboxItems,
            status.SessionId);

        EnsureSessionCompanionStartupRegistration();
    }

    private void EnsureSessionCompanionStartupRegistration()
    {
        var now = DateTime.UtcNow;
        if (now - _lastCompanionRegistrationCheckUtc < TimeSpan.FromMinutes(10))
            return;

        _lastCompanionRegistrationCheckUtc = now;

        try
        {
            var startupServiceType =
                ReflectionSafe.FindType(
                    "EJLive.Client.WinForms.Services.WindowsStartupService",
                    "EJLive.Client.WinForms.Services.WindowsStartupService, EJLive.Client.WinForms");

            if (startupServiceType == null)
            {
                _logger.LogDebug("WindowsStartupService not available; companion registration skipped.");
                return;
            }

            var staticEnsureMethod = startupServiceType.GetMethod(
                "EnsureCompanionStartup",
                BindingFlags.Public | BindingFlags.Static);

            if (staticEnsureMethod != null)
            {
                staticEnsureMethod.Invoke(null, null);
                _logger.LogInformation("Session companion startup ensured by legacy static method.");
                return;
            }

            var isRegistered = (bool?)startupServiceType
                .GetMethod("IsUserSessionCompanionRegistered")?
                .Invoke(null, null);

            if (isRegistered == true)
                return;

            var companionExe = ResolveClientCompanionExecutable();
            if (string.IsNullOrWhiteSpace(companionExe))
            {
                _logger.LogWarning("Session companion executable was not found; registration skipped.");
                return;
            }

            var result = startupServiceType
                .GetMethod("RegisterUserSessionCompanion")?
                .Invoke(null, new object[] { companionExe });

            if (result == null)
            {
                _logger.LogWarning("Companion registration returned no result.");
                return;
            }

            var success = (bool?)result.GetType().GetProperty("Success")?.GetValue(result) ?? false;
            var message = (string?)result.GetType().GetProperty("Message")?.GetValue(result) ?? string.Empty;

            if (success)
                _logger.LogInformation("Session companion startup registration ensured.");
            else
                _logger.LogWarning("Session companion startup registration warning: {Message}", message);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Session companion startup registration check failed.");
        }
    }

    private static string ResolveClientCompanionExecutable()
    {
        var candidates = new[]
        {
            Path.Combine(AppContext.BaseDirectory, "EJLive.Client.exe"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "EJLive", "ClientService", "EJLive.Client.exe"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "EJLive", "Client", "EJLive.Client.exe")
        };

        foreach (var candidate in candidates)
        {
            try
            {
                var full = Path.GetFullPath(candidate);
                if (File.Exists(full))
                    return full;
            }
            catch
            {
                // Ignore invalid paths.
            }
        }

        return string.Empty;
    }
}
