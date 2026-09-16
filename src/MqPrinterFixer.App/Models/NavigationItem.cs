namespace MqPrinterFixer.App.Models;

/// <summary>
/// Metadata cho 1 mục trong sidebar navigation.
/// </summary>
public sealed record NavigationItem(
    NavigationItemKey Key,
    string DisplayName,
    string IconGlyph);