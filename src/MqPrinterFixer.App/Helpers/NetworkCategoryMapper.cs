using MqPrinterFixer.App.Models;

namespace MqPrinterFixer.App.Helpers;

public static class NetworkCategoryMapper
{
    /// <summary>
    /// Map chuỗi từ PowerShell (Get-NetConnectionProfile.NetworkCategory) sang enum.
    /// </summary>
    public static NetworkCategory Parse(string? category)
    {
        if (string.IsNullOrWhiteSpace(category))
        {
            return NetworkCategory.Unknown;
        }

        var c = category.Trim();

        if (c.Equals("Public", StringComparison.OrdinalIgnoreCase))
        {
            return NetworkCategory.Public;
        }
        if (c.Equals("Private", StringComparison.OrdinalIgnoreCase))
        {
            return NetworkCategory.Private;
        }
        if (c.Equals("DomainAuthenticated", StringComparison.OrdinalIgnoreCase) ||
            c.Equals("Domain", StringComparison.OrdinalIgnoreCase))
        {
            return NetworkCategory.DomainAuthenticated;
        }

        return NetworkCategory.Unknown;
    }

    public static string ToDisplayName(NetworkCategory category) => category switch
    {
        NetworkCategory.Public              => "Public",
        NetworkCategory.Private             => "Private",
        NetworkCategory.DomainAuthenticated => "DomainAuthenticated",
        _                                   => "Unknown"
    };

    /// <summary>
    /// Public là category không an toàn cho printer sharing (Master Prompt Section 31).
    /// </summary>
    public static bool IsPublic(NetworkCategory category) =>
        category == NetworkCategory.Public;

    /// <summary>
    /// DomainAuthenticated phải được giữ nguyên — không ép thành Private (Section 31).
    /// </summary>
    public static bool IsProtectedFromChange(NetworkCategory category) =>
        category == NetworkCategory.DomainAuthenticated;
}