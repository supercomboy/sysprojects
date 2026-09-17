namespace MqPrinterFixer.App.Models;

/// <summary>
/// Snapshot trạng thái sharing hiện tại (read-only).
/// </summary>
public sealed record SharingState(
    NetworkCategory NetworkCategory,
    string InterfaceAlias,
    bool NetworkDiscoveryEnabled,
    bool FileAndPrinterSharingEnabled,
    bool PasswordProtectedSharingEnabled)
{
    public static readonly SharingState Unknown = new(
        NetworkCategory: NetworkCategory.Unknown,
        InterfaceAlias: "—",
        NetworkDiscoveryEnabled: false,
        FileAndPrinterSharingEnabled: false,
        PasswordProtectedSharingEnabled: true);
}