namespace MqPrinterFixer.App.Models;

/// <summary>
/// Snapshot trạng thái spooler.
/// </summary>
public sealed record SpoolerInfo(
    SpoolerStatus Status,
    bool CanStart,
    bool CanRestart,
    string DisplayName)
{
    public static readonly SpoolerInfo Unknown = new(
        Status: SpoolerStatus.Unknown,
        CanStart: false,
        CanRestart: false,
        DisplayName: "Unknown");
}