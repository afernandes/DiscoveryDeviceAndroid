namespace DiscoveryDeviceAndroid.Core;

public interface IProcessRunner
{
    Task<ProcessResult> RunAsync(string executable, IReadOnlyList<string> arguments,
        string? standardInput, TimeSpan timeout, CancellationToken cancellationToken);
}
