namespace MqPrinterFixer.App.Models;

/// <summary>
/// Thông tin tổng hợp về máy tính hiện tại (read-only).
/// </summary>
public sealed record ComputerInfo(
    string ComputerName,
    string UserName,
    WindowsEdition Edition,
    string EditionDisplayName,
    string DisplayVersion,
    string BuildNumber,
    bool IsAdministrator);