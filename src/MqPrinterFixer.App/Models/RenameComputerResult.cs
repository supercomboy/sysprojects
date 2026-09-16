namespace MqPrinterFixer.App.Models;

/// <summary>
/// Kết quả của một lần đổi tên máy.
/// </summary>
public sealed record RenameComputerResult(
    bool Success,
    string OldName,
    string NewName,
    bool RestartRequired,
    string ErrorMessage);