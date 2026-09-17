using MqPrinterFixer.App.Models;

namespace MqPrinterFixer.App.Helpers;

/// <summary>
/// Map trạng thái service (từ System.ServiceProcess.ServiceController.Status
/// hoặc PowerShell Get-Service) sang SpoolerStatus chuẩn hóa.
/// Pure logic, testable 100%.
/// </summary>
public static class SpoolerStatusMapper
{
    /// <summary>
    /// Map từ System.ServiceProcess.ServiceControllerStatus (enum chuẩn .NET).
    /// Tên dạng: "Running", "Stopped", "StartPending", "StopPending", "Paused", ...
    /// </summary>
    public static SpoolerStatus FromServiceControllerStatus(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return SpoolerStatus.Unknown;
        }

        return status.Trim() switch
        {
            "Running"        => SpoolerStatus.Running,
            "Stopped"        => SpoolerStatus.Stopped,
            "StartPending"   => SpoolerStatus.Starting,
            "StopPending"    => SpoolerStatus.Stopping,
            "Paused"         => SpoolerStatus.Paused,
            "PausePending"   => SpoolerStatus.Paused,
            "ContinuePending"=> SpoolerStatus.Starting,
            _                => SpoolerStatus.Unknown
        };
    }

    public static string ToDisplayName(SpoolerStatus status) => status switch
    {
        SpoolerStatus.Running  => "Running",
        SpoolerStatus.Stopped  => "Stopped",
        SpoolerStatus.Starting => "Starting",
        SpoolerStatus.Stopping => "Stopping",
        SpoolerStatus.Paused   => "Paused",
        _                      => "Unknown"
    };

    /// <summary>
    /// Có thể Start từ trạng thái hiện tại không?
    /// </summary>
    public static bool CanStart(SpoolerStatus status) =>
        status == SpoolerStatus.Stopped;

    /// <summary>
    /// Có thể Restart từ trạng thái hiện tại không?
    /// </summary>
    public static bool CanRestart(SpoolerStatus status) =>
        status == SpoolerStatus.Running || status == SpoolerStatus.Paused;
}