namespace DiscoveryDeviceAndroid.Core;

public sealed record DispositivoAdb(string Serial, string Estado, string Modelo)
{
    public bool Conectado => Estado == "device";
}
