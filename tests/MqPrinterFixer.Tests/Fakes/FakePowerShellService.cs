using MqPrinterFixer.App.Interfaces;
using MqPrinterFixer.App.Models;

namespace MqPrinterFixer.Tests.Fakes;

internal sealed class FakePowerShellService : IPowerShellService
{
    public PowerShellResult NextResult { get; set; } = PowerShellResult.Ok(string.Empty);

    public int CallCount { get; private set; }
    public string? LastScript { get; private set; }

    public Task<PowerShellResult> RunAsync(
        string script,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        CallCount++;
        LastScript = script;
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(NextResult);
    }
}