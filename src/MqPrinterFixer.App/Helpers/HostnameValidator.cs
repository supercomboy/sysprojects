using MqPrinterFixer.App.Models;

namespace MqPrinterFixer.App.Helpers;

/// <summary>
/// Validate hostname theo Windows NetBIOS rules (Master Prompt Section 24).
/// Pure function – testable, không side effect.
/// </summary>
public static class HostnameValidator
{
    public const int MinLength = 1;
    public const int MaxLength = 15;

    public static HostnameValidationResult Validate(string? name, string currentName)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return HostnameValidationResult.Invalid("Tên máy không được để trống.");
        }

        var trimmed = name.Trim();

        if (trimmed.Length < MinLength)
        {
            return HostnameValidationResult.Invalid($"Tên máy phải có ít nhất {MinLength} ký tự.");
        }

        if (trimmed.Length > MaxLength)
        {
            return HostnameValidationResult.Invalid($"Tên máy không được dài quá {MaxLength} ký tự.");
        }

        foreach (var ch in trimmed)
        {
            if (!IsAllowedCharacter(ch))
            {
                return HostnameValidationResult.Invalid(
                    $"Tên máy chứa ký tự không hợp lệ '{ch}'. Chỉ cho phép chữ cái, chữ số và dấu gạch ngang.");
            }
        }

        if (trimmed.StartsWith('-'))
        {
            return HostnameValidationResult.Invalid("Tên máy không được bắt đầu bằng dấu gạch ngang.");
        }

        if (trimmed.EndsWith('-'))
        {
            return HostnameValidationResult.Invalid("Tên máy không được kết thúc bằng dấu gạch ngang.");
        }

        if (trimmed.All(char.IsDigit))
        {
            return HostnameValidationResult.Invalid("Tên máy không được toàn số.");
        }

        if (string.Equals(trimmed, currentName, StringComparison.OrdinalIgnoreCase))
        {
            return HostnameValidationResult.Invalid("Tên mới giống tên hiện tại.");
        }

        return HostnameValidationResult.Valid(trimmed);
    }

    public static bool IsAllowedCharacter(char ch) =>
        (ch >= 'A' && ch <= 'Z') ||
        (ch >= 'a' && ch <= 'z') ||
        (ch >= '0' && ch <= '9') ||
        ch == '-';
}