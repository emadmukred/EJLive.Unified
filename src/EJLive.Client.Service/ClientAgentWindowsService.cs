using System.IO;
using EJLive.Client.WinForms.Agent;
using EJLive.Client.WinForms.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EJLive.Client.Service;

public sealed class ClientAgentWindowsService(ILogger<ClientAgentWindowsService> logger) : BackgroundService
{
    private readonly ILogger<ClientAgentWindowsService> _logger = logger;
    private AgentBootstrapper? _agent;
    private DateTime _lastStatusLogUtc;
    private DateTime _lastCompanionRegistrationCheckUtc;

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("EJLive Windows service starting.");
        return base.StartAsync(cancellationToken);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        stoppingToken.Register(StopAgentSafely);
        return RunSupervisionLoopAsync(stoppingToken);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("EJLive Windows service stopping.");
        StopAgentSafely();
        return base.StopAsync(cancellationToken);
    }

    private void StopAgentSafely()
    {
        try
        {
            _agent?.Dispose();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error while stopping EJLive agent.");
        }
        finally
        {
            _agent = null;
        }
    }

    private async Task RunSupervisionLoopAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                EnsureAgentRunning();
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
        if (_agent != null)
            return;

        _agent = new AgentBootstrapper();
        _agent.OnLog += message => _logger.LogInformation("{Message}", message);
        _agent.OnStatusUpdate += status =>
            _logger.LogInformation(
                "Agent status: connected={Connected}, handshake={Handshake}, pending={Pending}",
                status.connected,
                status.handshake,
                status.pending);
        _agent.StartAll();
        _lastStatusLogUtc = DateTime.UtcNow;
        _lastCompanionRegistrationCheckUtc = DateTime.MinValue;
        _logger.LogInformation("Agent supervision initialized.");
    }

    private void LogAgentStatusPeriodically()
    {
        if (_agent == null)
            return;

        var now = DateTime.UtcNow;
        if (now - _lastStatusLogUtc < TimeSpan.FromMinutes(2))
            return;

        _lastStatusLogUtc = now;
        var status = _agent.GetStatus();
        _logger.LogInformation(
            "Health check: connected={Connected}, handshake={Handshake}, pending={Pending}",
            status.connected,
            status.handshake,
            status.pending);

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
            if (WindowsStartupService.IsUserSessionCompanionRegistered())
                return;

            var companionExe = ResolveClientCompanionExecutable();
            if (string.IsNullOrWhiteSpace(companionExe))
            {
                _logger.LogWarning("Session companion executable was not found; registration skipped.");
                return;
            }

            var result = WindowsStartupService.RegisterUserSessionCompanion(companionExe);
            if (result.Success)
                _logger.LogInformation("Session companion startup registration ensured.");
            else
                _logger.LogWarning("Session companion startup registration warning: {Message}", result.Message);
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
            }
        }

        return string.Empty;
    }
}
