using System.ServiceProcess;
using MqPrinterFixer.App.Helpers;
using MqPrinterFixer.App.Interfaces;
using MqPrinterFixer.App.Models;

namespace MqPrinterFixer.App.Services;

public sealed class SpoolerService : ISpoolerService
{
    private const string SpoolerServiceName = "Spooler";
    private const string SpoolerDisplayName = "Print Spooler";

    private readonly ISystemInfoService _systemInfo;

    public SpoolerService(ISystemInfoService systemInfo)
    {
        _systemInfo = systemInfo ?? throw new ArgumentNullException(nameof(systemInfo));
    }

    public Task<SpoolerInfo> GetStatusAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            using var sc = new ServiceController(SpoolerServiceName);
            var status = SpoolerStatusMapper.FromServiceControllerStatus(sc.Status.ToString());

            return Task.FromResult(new SpoolerInfo(
                Status: status,
                CanStart: SpoolerStatusMapper.CanStart(status),
                CanRestart: SpoolerStatusMapper.CanRestart(status),
                DisplayName: SpoolerDisplayName));
        }
        catch
        {
            // ServiceController có thể throw nếu service không tồn tại
            // (rất hiếm) hoặc bị policy chặn.
            return Task.FromResult(SpoolerInfo.Unknown);
        }
    }

    public async Task<NetworkChangeResult> StartAsync(
        CancellationToken cancellationToken = default)
    {
        const NetworkChangeAction action = NetworkChangeAction.EnableFileAndPrinterSharing;

        var info = await _systemInfo.GetComputerInfoAsync(cancellationToken);
        if (!info.IsAdministrator)
        {
            return NetworkChangeResult.Blocked(action,
                "Cần quyền Administrator để start Print Spooler.");
        }

        var before = await GetStatusAsync(cancellationToken);
        if (!before.CanStart)
        {
            return NetworkChangeResult.Blocked(action,
                $"Không thể start từ trạng thái '{before.DisplayName}'.");
        }

        var startResult = await Task.Run(() => TryStartService(), cancellationToken);
        if (!startResult.Success)
        {
            return new NetworkChangeResult(
                Success: false,
                Action: action,
                BeforeState: before.DisplayName,
                AfterState: before.DisplayName,
                Verified: false,
                ErrorMessage: startResult.Error);
        }

        // Đợi service vào trạng thái Running (tối đa 5s)
        var after = await WaitForStatusAsync(SpoolerStatus.Running, TimeSpan.FromSeconds(5), cancellationToken);
        var verified = after.Status == SpoolerStatus.Running;

        return new NetworkChangeResult(
            Success: true,
            Action: action,
            BeforeState: before.DisplayName,
            AfterState: after.DisplayName,
            Verified: verified,
            ErrorMessage: verified ? string.Empty : "Service start xong nhưng verify không xác nhận Running.");
    }

    public async Task<NetworkChangeResult> RestartAsync(
        CancellationToken cancellationToken = default)
    {
        const NetworkChangeAction action = NetworkChangeAction.EnableFileAndPrinterSharing;

        var info = await _systemInfo.GetComputerInfoAsync(cancellationToken);
        if (!info.IsAdministrator)
        {
            return NetworkChangeResult.Blocked(action,
                "Cần quyền Administrator để restart Print Spooler.");
        }

        var before = await GetStatusAsync(cancellationToken);
        if (!before.CanRestart)
        {
            return NetworkChangeResult.Blocked(action,
                $"Không thể restart từ trạng thái '{before.DisplayName}'.");
        }

        var restartResult = await Task.Run(() => TryRestartService(), cancellationToken);
        if (!restartResult.Success)
        {
            return new NetworkChangeResult(
                Success: false,
                Action: action,
                BeforeState: before.DisplayName,
                AfterState: before.DisplayName,
                Verified: false,
                ErrorMessage: restartResult.Error);
        }

        var after = await WaitForStatusAsync(SpoolerStatus.Running, TimeSpan.FromSeconds(10), cancellationToken);
        var verified = after.Status == SpoolerStatus.Running;

        return new NetworkChangeResult(
            Success: true,
            Action: action,
            BeforeState: before.DisplayName,
            AfterState: after.DisplayName,
            Verified: verified,
            ErrorMessage: verified ? string.Empty : "Service restart xong nhưng verify không xác nhận Running.");
    }

    // -----------------------------------------------------------------
    //   Internals
    // -----------------------------------------------------------------

    private static (bool Success, string Error) TryStartService()
    {
        try
        {
            using var sc = new ServiceController(SpoolerServiceName);
            sc.Start();
            return (true, string.Empty);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    private static (bool Success, string Error) TryRestartService()
    {
        try
        {
            using var sc = new ServiceController(SpoolerServiceName);

            // Stop nếu đang chạy
            if (sc.Status != ServiceControllerStatus.Stopped)
            {
                sc.Stop();
                sc.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(10));
            }

            sc.Start();
            return (true, string.Empty);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    private async Task<SpoolerInfo> WaitForStatusAsync(
        SpoolerStatus target,
        TimeSpan timeout,
        CancellationToken cancellationToken)
    {
        var deadline = DateTime.UtcNow + timeout;

        while (DateTime.UtcNow < deadline)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var info = await GetStatusAsync(cancellationToken);
            if (info.Status == target)
            {
                return info;
            }

            try
            {
                await Task.Delay(200, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        return await GetStatusAsync(cancellationToken);
    }
}