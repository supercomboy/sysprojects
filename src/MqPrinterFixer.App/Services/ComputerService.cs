using MqPrinterFixer.App.Helpers;
using MqPrinterFixer.App.Interfaces;
using MqPrinterFixer.App.Models;

namespace MqPrinterFixer.App.Services;

public sealed class ComputerService : IComputerService
{
    private readonly IPowerShellService _powerShell;
    private readonly ISystemInfoService _systemInfo;

    public ComputerService(IPowerShellService powerShell, ISystemInfoService systemInfo)
    {
        _powerShell = powerShell ?? throw new ArgumentNullException(nameof(powerShell));
        _systemInfo = systemInfo ?? throw new ArgumentNullException(nameof(systemInfo));
    }

    public async Task<RenameComputerResult> RenameComputerAsync(
        string newName,
        CancellationToken cancellationToken = default)
    {
        var current = await _systemInfo.GetComputerInfoAsync(cancellationToken);
        var validation = HostnameValidator.Validate(newName, current.ComputerName);

        if (!validation.IsValid)
        {
            return new RenameComputerResult(
                Success: false,
                OldName: current.ComputerName,
                NewName: newName,
                RestartRequired: false,
                ErrorMessage: validation.ErrorMessage);
        }

        if (!current.IsAdministrator)
        {
            return new RenameComputerResult(
                Success: false,
                OldName: current.ComputerName,
                NewName: validation.NormalizedName,
                RestartRequired: false,
                ErrorMessage:
                    "Cần quyền Administrator. Vui lòng đóng app và chạy lại bằng 'Run as administrator'.");
        }

        // Không dùng -Restart để app không tự khởi động lại máy.
        // Rename-Computer yêu cầu quyền admin – đã kiểm tra ở trên.
        var escaped = validation.NormalizedName.Replace("'", "''");
        var script = $@"
$ErrorActionPreference = 'Stop'
try {{
    Rename-Computer -NewName '{escaped}' -Force -ErrorAction Stop | Out-Null
    Write-Output 'RENAMED'
}} catch {{
    [Console]::Error.WriteLine($_.Exception.Message)
    exit 1
}}";

        var result = await _powerShell.RunAsync(script, cancellationToken: cancellationToken);

        if (!result.Success)
        {
            var msg = string.IsNullOrWhiteSpace(result.StandardError)
                ? "Rename-Computer thất bại (không rõ nguyên nhân)."
                : result.StandardError;
            return new RenameComputerResult(
                Success: false,
                OldName: current.ComputerName,
                NewName: validation.NormalizedName,
                RestartRequired: false,
                ErrorMessage: msg);
        }

        return new RenameComputerResult(
            Success: true,
            OldName: current.ComputerName,
            NewName: validation.NormalizedName,
            RestartRequired: true,
            ErrorMessage: string.Empty);
    }

    public async Task<PowerShellResult> RestartComputerNowAsync(
        CancellationToken cancellationToken = default)
    {
        // shutdown.exe là công cụ Windows built-in, không cần PowerShell.
        // /r = restart, /t 5 = delay 5s để app kịp đóng, /f = force.
        var result = await _powerShell.RunAsync(
            "shutdown.exe /r /t 5 /f /c \"Restart by MQ Printer Fixer\"",
            cancellationToken: cancellationToken);

        return result;
    }
}