using System.Text;

namespace DiscoveryDeviceAndroid.Core;

public static class AdbSnapshotFrameReader
{
    public static async Task<string?> ReadAsync(TextReader reader, CancellationToken cancellationToken,
        bool aceitarQuadroCompletoNoLimite = false)
    {
        var quadro = new StringBuilder();
        var iniciado = false;
        var caracteres = 0;
        try
        {
            while (await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false) is { } linha)
            {
                caracteres += linha.Length;
                if (caracteres > 65536) return null;
                if (!iniciado)
                {
                    if (linha.Trim() != "Services:") continue;
                    iniciado = true;
                    continue;
                }
                if (string.IsNullOrWhiteSpace(linha) || linha.Trim() == "Services:") return ValidarQuadro(quadro);
                quadro.AppendLine(linha);
            }
        }
        catch (OperationCanceledException) when (aceitarQuadroCompletoNoLimite) { }
        return iniciado ? ValidarQuadro(quadro) : null;
    }

    private static string? ValidarQuadro(StringBuilder quadro)
    {
        try { _ = ParserMdnsProto.Servicos(quadro.ToString()); }
        catch (FormatException) { return null; }
        return quadro.ToString();
    }
}
