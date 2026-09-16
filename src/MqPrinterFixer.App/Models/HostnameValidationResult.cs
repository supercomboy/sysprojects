namespace MqPrinterFixer.App.Models;

public sealed record HostnameValidationResult(
    bool IsValid,
    string NormalizedName,
    string ErrorMessage)
{
    public static HostnameValidationResult Valid(string normalizedName) =>
        new(true, normalizedName, string.Empty);

    public static HostnameValidationResult Invalid(string error) =>
        new(false, string.Empty, error);
}