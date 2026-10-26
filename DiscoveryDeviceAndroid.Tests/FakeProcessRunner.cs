using DiscoveryDeviceAndroid.Core;

namespace DiscoveryDeviceAndroid.Tests;

internal sealed class FakeProcessRunner(ProcessResult result) : IProcessRunner
{
    public IReadOnlyList<string> Arguments { get; private set; } = [];
    public string? Input { get; private set; }
    public int Calls { get; private set; }

    public Task<ProcessResult> RunAsync(string executable, IReadOnlyList<string> arguments,
        string? standardInput, TimeSpan timeout, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Calls++;
        Arguments = arguments;
        Input = standardInput;
        return Task.FromResult(result);
    }
}
