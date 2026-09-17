namespace MqPrinterFixer.App.Models;

/// <summary>
/// Category của network profile (Master Prompt Section 31).
/// </summary>
public enum NetworkCategory
{
    Unknown = 0,
    Public,
    Private,
    DomainAuthenticated
}