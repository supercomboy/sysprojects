namespace MqPrinterFixer.App.Models;

/// <summary>
/// Kết quả chạy một PowerShell command – có cấu trúc, không phải chuỗi thô.
/// </summary>
public sealed record PowerShellResult(
    bool Success,
    int ExitCode,
    string StandardOutput,
    string StandardError)
{
    public static PowerShellResult Ok(string output = "") =>
        new(true, 0, output, string.Empty);

    public static PowerShellResult Fail(int exitCode, string error, string output = "") =>
        new(false, exitCode, output, error);
}