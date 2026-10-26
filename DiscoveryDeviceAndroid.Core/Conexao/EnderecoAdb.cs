using System.Globalization;
using System.Net;

namespace DiscoveryDeviceAndroid.Core;

public static class EnderecoAdb
{
    public static string Validar(string endereco)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(endereco);
        endereco = endereco.Trim();
        var separador = endereco.LastIndexOf(':');
        if (separador <= 0 || !int.TryParse(endereco.AsSpan(separador + 1), NumberStyles.None,
                CultureInfo.InvariantCulture, out var porta) || porta is < 1 or > 65535)
            throw new ArgumentException("Informe um endereço IP e uma porta entre 1 e 65535.", nameof(endereco));

        var host = endereco[..separador];
        if (host.StartsWith('[') && host.EndsWith(']'))
            host = host[1..^1];
        else if (host.Contains(':'))
            throw new ArgumentException("Use [IPv6]:porta para um endereço IPv6.", nameof(endereco));

        if (!IPAddress.TryParse(host, out var ip))
            throw new ArgumentException("O endereço deve conter um IP válido, seguido de :porta.", nameof(endereco));

        return ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6
            ? $"[{ip}]:{porta}" : $"{ip}:{porta}";
    }
}
