namespace DiscoveryDeviceAndroid.Core;

public interface IAdbSnapshotReader
{
    Task<string?> ReadAsync(string executable, CancellationToken cancellationToken);
}
