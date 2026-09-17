namespace MqPrinterFixer.App.Models;

/// <summary>
/// 4 action độc lập — KHÔNG gộp thành Fix All (Master Prompt Section 30, 73).
/// </summary>
public enum NetworkChangeAction
{
    ChangeNetworkToPrivate = 0,
    EnableNetworkDiscovery,
    EnableFileAndPrinterSharing,
    TurnOffPasswordProtectedSharing
}