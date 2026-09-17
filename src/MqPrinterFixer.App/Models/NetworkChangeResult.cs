namespace MqPrinterFixer.App.Models;

public sealed record NetworkChangeResult(
    bool Success,
    NetworkChangeAction Action,
    string BeforeState,
    string AfterState,
    bool Verified,
    string ErrorMessage)
{
    public static NetworkChangeResult Blocked(
        NetworkChangeAction action, string reason)
        => new(false, action, string.Empty, string.Empty, false, reason);
}