using System.Security.Principal;
using Microsoft.Win32;
using MqPrinterFixer.App.Helpers;
using MqPrinterFixer.App.Interfaces;
using MqPrinterFixer.App.Models;

namespace MqPrinterFixer.App.Services;

public sealed class SystemInfoService : ISystemInfoService
{
    private const string WindowsCurrentVersionKey =
        @"SOFTWARE\Microsoft\Windows NT\CurrentVersion";

    public Task<ComputerInfo> GetComputerInfoAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var computerName = SafeGet(() => Environment.MachineName, "Unknown");
        var userName = SafeGet(() => Environment.UserName, "Unknown");

        var (edition, editionDisplay, displayVersion, buildNumber) = ReadWindowsEdition();
        var isAdmin = IsCurrentProcessAdministrator();

        var info = new ComputerInfo(
            computerName,
            userName,
            edition,
            editionDisplay,
            displayVersion,
            buildNumber,
            isAdmin);

        return Task.FromResult(info);
    }

    private static (WindowsEdition Edition, string DisplayName, string DisplayVersion, string BuildNumber)
        ReadWindowsEdition()
    {
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(WindowsCurrentVersionKey);
            if (key is null)
            {
                return (WindowsEdition.Unknown, "Windows (Unknown)", string.Empty, string.Empty);
            }

            var editionId      = key.GetValue("EditionID") as string;
            var productName    = key.GetValue("ProductName") as string;
            var displayVersion = key.GetValue("DisplayVersion") as string ?? string.Empty;
            var buildString    = key.GetValue("CurrentBuild") as string
                                 ?? key.GetValue("CurrentBuildNumber") as string
                                 ?? string.Empty;

            var edition = WindowsEditionMapper.Map(editionId, productName);

            int buildNumber = int.TryParse(buildString, out var b) ? b : 0;

            var display = WindowsEditionMapper.BuildDisplayName(
                productName, edition, displayVersion, buildNumber);

            return (edition, display, displayVersion, buildString);
        }
        catch
        {
            return (WindowsEdition.Unknown, "Windows (Unknown)", string.Empty, string.Empty);
        }
    }

    /// <summary>
    /// Trả về true nếu process hiện tại đang chạy với quyền Administrator (UAC elevated).
    /// Khi UAC bật và process non-elevated → trả false dù user thuộc nhóm Administrators.
    /// </summary>
    private static bool IsCurrentProcessAdministrator()
    {
        try
        {
            using var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
        catch
        {
            return false;
        }
    }

    private static string SafeGet(Func<string> getter, string fallback)
    {
        try
        {
            var value = getter();
            return string.IsNullOrWhiteSpace(value) ? fallback : value;
        }
        catch
        {
            return fallback;
        }
    }
}