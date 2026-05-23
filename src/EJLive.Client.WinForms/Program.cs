using System.Diagnostics;
using System.Security.Principal;
using EJLive.Client.WinForms.Services;

namespace EJLive.Client.WinForms;

internal static class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        var plan = ClientStartupPlanner.Create(args, IsAdministrator());
        if (plan.RequiresElevation)
        {
            TryRelaunchElevated(plan.ElevationArguments);
            return;
        }

        ApplicationConfiguration.Initialize();
        AppBootstrapper.Init();

        if (plan.IsBackground)
        {
            using var mutex = new Mutex(true, plan.MutexName, out var firstInstance);
            if (!firstInstance)
                return;

            RegisterStartupIfPossible();
            Application.Run(new ClientBackgroundApplicationContext());
            return;
        }

        Application.Run(new ClientMainForm());
    }

    private static bool IsAdministrator()
    {
        try
        {
            using var identity = WindowsIdentity.GetCurrent();
            return new WindowsPrincipal(identity).IsInRole(WindowsBuiltInRole.Administrator);
        }
        catch
        {
            return false;
        }
    }

    private static void TryRelaunchElevated(string? arguments)
    {
        var processPath = Environment.ProcessPath;
        if (string.IsNullOrWhiteSpace(processPath))
            return;

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = processPath,
                Arguments = arguments ?? ClientStartupPlanner.AutoStartArgument,
                UseShellExecute = true,
                Verb = "runas"
            });
        }
        catch
        {
        }
    }

    private static void RegisterStartupIfPossible()
    {
        var processPath = Environment.ProcessPath;
        if (!string.IsNullOrWhiteSpace(processPath) && File.Exists(processPath))
            _ = WindowsStartupService.RegisterClientAutostart(processPath);
    }
}
