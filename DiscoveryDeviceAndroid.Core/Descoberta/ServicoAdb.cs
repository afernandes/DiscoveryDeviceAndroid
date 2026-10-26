namespace DiscoveryDeviceAndroid.Core;

public sealed record ServicoAdb(string Nome, string Tipo, string Endereco)
{
    public bool Pareamento => Tipo == "_adb-tls-pairing._tcp";
    public string? NomeModelo { get; init; }
    public string? Api { get; init; }
    public string? VersaoMdns { get; init; }
    public bool DispositivoConhecido { get; init; }
}
