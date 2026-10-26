namespace DiscoveryDeviceAndroid.Core;

internal sealed class MdnsProtoNode
{
    public Dictionary<string, List<string>> Values { get; } = new(StringComparer.Ordinal);
    public List<(string Name, MdnsProtoNode Node)> Children { get; } = [];

    public string? Value(string name) => Values.TryGetValue(name, out var values) ? values.FirstOrDefault() : null;
}
