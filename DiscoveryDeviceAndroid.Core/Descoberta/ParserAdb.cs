using System.Text.RegularExpressions;

namespace DiscoveryDeviceAndroid.Core;

public static partial class ParserAdb
{
    public static IReadOnlyList<ServicoAdb> Servicos(string saida)
    {
        var resultado = new List<ServicoAdb>();
        foreach (var linha in saida.Split('\n'))
        {
            var partes = Espacos().Split(linha.Trim());
            if (partes.Length < 3) continue;
            var tipo = partes[1].TrimEnd('.');
            if (tipo is not ("_adb-tls-pairing._tcp" or "_adb-tls-connect._tcp")) continue;
            try
            {
                resultado.Add(new ServicoAdb(partes[0], tipo, EnderecoAdb.Validar(partes[2])));
            }
            catch (ArgumentException) { /* Um anúncio inválido não impede a descoberta dos demais. */ }
        }
        return resultado.Distinct().ToArray();
    }

    public static IReadOnlyList<DispositivoAdb> Dispositivos(string saida)
    {
        var resultado = new List<DispositivoAdb>();
        foreach (var linha in saida.Split('\n'))
        {
            var partes = Espacos().Split(linha.Trim());
            if (partes.Length < 2 || partes[0] == "List" || partes[0].StartsWith('*')) continue;
            if (partes[1] is not ("device" or "offline" or "unauthorized" or "recovery" or "sideload" or "bootloader" or "no")) continue;
            var modelo = partes.Skip(2).FirstOrDefault(p => p.StartsWith("model:", StringComparison.Ordinal));
            resultado.Add(new DispositivoAdb(partes[0], partes[1], modelo?[6..].Replace('_', ' ') ?? partes[0]));
        }
        return resultado;
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex Espacos();
}
