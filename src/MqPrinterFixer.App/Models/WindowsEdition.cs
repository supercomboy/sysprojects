namespace MqPrinterFixer.App.Models;

/// <summary>
/// Edition của Windows (Home/Pro/Enterprise/Education...).
/// </summary>
public enum WindowsEdition
{
    Unknown = 0,
    Home,
    Pro,
    ProEducation,
    ProWorkstation,
    Enterprise,
    Education,
    ServerStandard,
    ServerDatacenter
}