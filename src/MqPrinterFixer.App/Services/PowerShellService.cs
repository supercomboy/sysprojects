using System.Diagnostics;
using System.IO;
using System.Text;
using MqPrinterFixer.App.Interfaces;
using MqPrinterFixer.App.Models;

namespace MqPrinterFixer.App.Services;

public sealed class PowerShellService : IPowerShellService
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(60);
    private static readonly string DebugLogPath =
        Path.Combine(Path.GetTempPath(), "mq-ps-debug.log");

    public async Task<PowerShellResult> RunAsync(
        string script,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(script);

        // -EncodedCommand: Base64(UTF-16LE(script)). Cách chuẩn của Microsoft
        // để truyền script nhiều dòng không có vấn đề escaping/stdin.
        var encoded = Convert.ToBase64String(Encoding.Unicode.GetBytes(script));

        var psi = new ProcessStartInfo
        {
            FileName = "powershell.exe",
            Arguments = $"-NoProfile -NonInteractive -ExecutionPolicy Bypass -EncodedCommand {encoded}",
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

                var msg = $"PowerShell script vượt quá timeout {(timeout ?? DefaultTimeout).TotalSeconds:0}s.";
                LogDebug(script, -1, stdout.ToString(), msg);
                return PowerShellResult.Fail(-1, msg, stdout.ToString());
            }

            // Gọi WaitForExit() đồng bộ sau WaitForExitAsync để flush
            // tất cả async output events còn đang chờ trong queue.
            process.WaitForExit();

            var exitCode = process.ExitCode;
            var outStr = stdout.ToString().Trim();
            var errStr = stderr.ToString().Trim();

            // Strip BOM nếu có (PowerShell 5.1 đôi khi thêm \uFEFF vào đầu stdout).
            outStr = outStr.TrimStart('\uFEFF', ' ', '\r', '\n', '\t');

            LogDebug(script, exitCode, outStr, errStr);

            return exitCode == 0
                ? PowerShellResult.Ok(outStr)
                : PowerShellResult.Fail(exitCode, errStr, outStr);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            LogDebug(script, -1, stdout.ToString(), ex.ToString());
            return PowerShellResult.Fail(-1, ex.Message, stdout.ToString());
        }
    }

    private static void LogDebug(string script, int exitCode, string stdout, string stderr)
    {
        try
        {
            var sb = new StringBuilder();
            sb.AppendLine("========================================");
            sb.AppendLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}]");
            sb.AppendLine("--- SCRIPT ---");
            sb.AppendLine(script);
            sb.AppendLine($"--- EXIT CODE: {exitCode} ---");
            sb.AppendLine($"--- STDOUT ({stdout.Length} chars) ---");
            sb.AppendLine(stdout);
            sb.AppendLine($"--- STDERR ({stderr.Length} chars) ---");
            sb.AppendLine(stderr);
            sb.AppendLine();

            File.AppendAllText(DebugLogPath, sb.ToString(), Encoding.UTF8);
        }
        catch
        {
            // Không bao giờ để debug log làm hỏng app.
        }
    }
}