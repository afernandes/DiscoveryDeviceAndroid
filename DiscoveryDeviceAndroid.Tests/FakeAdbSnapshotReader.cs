using DiscoveryDeviceAndroid.Core;

namespace DiscoveryDeviceAndroid.Tests;

internal sealed class FakeAdbSnapshotReader(string? snapshot) : IAdbSnapshotReader
{
    public Task<string?> ReadAsync(string executable, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(snapshot);
    }
}
