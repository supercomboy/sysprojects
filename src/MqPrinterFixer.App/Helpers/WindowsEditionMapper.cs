using MqPrinterFixer.App.Models;

namespace MqPrinterFixer.App.Helpers;

/// <summary>
/// Pure helpers để map EditionID / ProductName từ Registry sang WindowsEdition
/// và build tên hiển thị "Windows 11 Pro 23H2".
/// </summary>
public static class WindowsEditionMapper
{
    /// <summary>
    /// Windows 11 bắt đầu từ build 22000. Registry ProductName vẫn ghi "Windows 10" – cần sửa.
    /// </summary>
    private const int Windows11MinBuild = 22000;

    /// <summary>
    /// Map EditionID (chính xác) hoặc ProductName (fallback) sang enum.
    /// </summary>
    public static WindowsEdition Map(string? editionId, string? productName)
    {
        if (!string.IsNullOrWhiteSpace(editionId))
        {
            var id = editionId.Trim().ToLowerInvariant();
            switch (id)
            {
                case "core":
                case "coresinglelanguage":
                case "corecountryspecific":
                case "coren":
                    return WindowsEdition.Home;

                case "professional":
                case "professionaln":
                    return WindowsEdition.Pro;

                case "professionaleducation":
                case "professionaleducationn":
                    return WindowsEdition.ProEducation;

                case "professionalworkstation":
                case "professionalworkstationn":
                    return WindowsEdition.ProWorkstation;

                case "education":
                case "educationn":
                    return WindowsEdition.Education;

                case "enterprise":
                case "enterprisen":
                case "enterprises":
                    return WindowsEdition.Enterprise;

                case "serverstandard":
                case "serverstandardcore":
                    return WindowsEdition.ServerStandard;

                case "serverdatacenter":
                case "serverdatacentercore":
                    return WindowsEdition.ServerDatacenter;
            }
        }

        // Fallback: parse ProductName. THỨ TỰ QUAN TRỌNG — kiểm tra cụm dài trước,
        // cụm ngắn sau để tránh "Pro Education" bị nhận nhầm là "Education".
        if (!string.IsNullOrWhiteSpace(productName))
        {
            var pn = productName.ToLowerInvariant();

            // 1. Professional Workstation / Pro Workstation
            if (pn.Contains("professional workstation")) return WindowsEdition.ProWorkstation;
            if (pn.Contains("pro workstation"))          return WindowsEdition.ProWorkstation;

            // 2. Professional Education / Pro Education
            if (pn.Contains("professional education"))   return WindowsEdition.ProEducation;
            if (pn.Contains("pro education"))            return WindowsEdition.ProEducation;

            // 3. Professional / Pro (generic)
            if (pn.Contains("professional"))             return WindowsEdition.Pro;
            if (pn.Contains(" pro"))                     return WindowsEdition.Pro;
            if (pn.EndsWith("pro"))                      return WindowsEdition.Pro;

            // 4. Home
            if (pn.Contains("home"))                     return WindowsEdition.Home;

            // 5. Education
            if (pn.Contains("education"))                return WindowsEdition.Education;

            // 6. Enterprise
            if (pn.Contains("enterprise"))               return WindowsEdition.Enterprise;

            // 7. Server
            if (pn.Contains("server standard"))          return WindowsEdition.ServerStandard;
            if (pn.Contains("server datacenter"))        return WindowsEdition.ServerDatacenter;
        }

        return WindowsEdition.Unknown;
    }

    /// <summary>
    /// Build tên hiển thị "Windows 11 Pro 23H2" (hoặc "Windows 10 Home 22H2").
    /// </summary>
    public static string BuildDisplayName(
        string? productName,
        WindowsEdition edition,
        string? displayVersion,
        int buildNumber)
    {
        var baseName = !string.IsNullOrWhiteSpace(productName)
            ? productName
            : $"Windows ({edition})";

        // Windows 11 dùng ProductName = "Windows 10 Pro" trong registry → sửa.
        if (buildNumber >= Windows11MinBuild &&
            baseName.StartsWith("Windows 10", StringComparison.OrdinalIgnoreCase))
        {
            baseName = "Windows 11" + baseName.Substring("Windows 10".Length);
        }

        return string.IsNullOrWhiteSpace(displayVersion)
            ? baseName
            : $"{baseName} {displayVersion}";
    }
}