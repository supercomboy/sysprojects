using System.Diagnostics;
using System.Text;
using MqPrinterFixer.App.Interfaces;
using MqPrinterFixer.App.Models;

namespace MqPrinterFixer.App.Services;

public sealed class PowerShellService : IPowerShellService
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(60);

    public async Task<PowerShellResult> RunAsync(
        string script,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(script);

        var psi = new ProcessStartInfo
        {
            FileName = "powershell.exe",
            Arguments = "-NoProfile -NonInteractive -ExecutionPolicy Bypass -Command -",
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };

        using var process = new Process { StartInfo = psi };

        var stdout = new StringBuilder();
        var stderr = new StringBuilder();

        process.OutputDataReceived += (_, e) => { if (e.Data is not null) stdout.AppendLine(e.Data); };
        process.ErrorDataReceived  += (_, e) => { if (e.Data is not null) stderr.AppendLine(e.Data); };

        try
        {
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            await process.StandardInput.WriteAsync(script.AsMemory(), cancellationToken);
            process.StandardInput.Close();

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(timeout ?? DefaultTimeout);

            try
            {
                await process.WaitForExitAsync(cts.Token);
            }
            catch (OperationCanceledException)
            {
                try { process.Kill(entireProcessTree: true); } catch { /* best effort */ }

                if (cancellationToken.IsCancellationRequested)
                {
                    throw;
                }

                return PowerShellResult.Fail(
                    -1,
                    $"PowerShell script vượt quá timeout {(timeout ?? DefaultTimeout).TotalSeconds:0}s.",
                    stdout.ToString());
            }

            var exitCode = process.ExitCode;
            var outStr = stdout.ToString().Trim();
            var errStr = stderr.ToString().Trim();

            return exitCode == 0
                ? PowerShellResult.Ok(outStr)
                : PowerShellResult.Fail(exitCode, errStr, outStr);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return PowerShellResult.Fail(-1, ex.Message, stdout.ToString());
        }
    }
}