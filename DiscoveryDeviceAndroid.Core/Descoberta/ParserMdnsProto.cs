using System.Text.Json;
using System.Text.RegularExpressions;

namespace DiscoveryDeviceAndroid.Core;

/// <summary>Interpreta o subconjunto escalar/mensagens do protobuf textual anunciado pelo ADB.</summary>
public static partial class ParserMdnsProto
{
    public static IReadOnlyList<ServicoAdb> Servicos(string texto)
    {
        if (texto.Length > 65536) throw new FormatException("Quadro mDNS excedeu o limite de leitura.");
        var tokens = Tokens().Matches(texto).Select(m => m.Value).ToArray();
        var indice = 0;
        var raiz = LerMensagem(tokens, ref indice, false, 0);
        var resultado = new List<ServicoAdb>();
        foreach (var (_, anuncio) in raiz.Children)
        {
            foreach (var (_, servico) in anuncio.Children.Where(c => c.Name == "service"))
            {
                var nome = servico.Value("instance");
                var tipo = servico.Value("service")?.TrimEnd('.');
                var porta = servico.Value("port");
                if (nome is null || tipo is not ("_adb-tls-pairing._tcp" or "_adb-tls-connect._tcp") || porta is null) continue;
                var ip = servico.Value("ipv4");
                if (string.IsNullOrEmpty(ip))
                {
                    ip = servico.Value("ipv6");
                    if (!string.IsNullOrEmpty(ip)) ip = $"[{ip}]";
                }
                if (string.IsNullOrEmpty(ip)) continue;
                try
                {
                    resultado.Add(new ServicoAdb(nome, tipo, EnderecoAdb.Validar($"{ip}:{porta}"))
                    {
                        NomeModelo = servico.Value("product_model") ?? servico.Value("device_model"),
                        Api = servico.Value("build_version_sdk_full"),
                        VersaoMdns = servico.Value("mdns_service_version"),
                        DispositivoConhecido = anuncio.Value("known_device") == "true"
                    });
                }
                catch (ArgumentException) { }
            }
        }
        return resultado.Distinct().ToArray();
    }

    private static MdnsProtoNode LerMensagem(string[] tokens, ref int indice, bool aninhada, int profundidade)
    {
        if (profundidade > 16) throw new FormatException("Profundidade inválida no quadro mDNS.");
        var node = new MdnsProtoNode();
        while (indice < tokens.Length)
        {
            var nome = tokens[indice++];
            if (nome == "}")
            {
                if (!aninhada) throw new FormatException("Fechamento inesperado no quadro mDNS.");
                return node;
            }
            if (indice >= tokens.Length || nome is "{" or ":") throw new FormatException("Campo incompleto no quadro mDNS.");
            var operador = tokens[indice++];
            if (operador == "{") node.Children.Add((nome, LerMensagem(tokens, ref indice, true, profundidade + 1)));
            else if (operador == ":" && indice < tokens.Length)
            {
                var valor = tokens[indice++];
                if (valor.StartsWith('"'))
                {
                    try { valor = JsonSerializer.Deserialize<string>(valor) ?? ""; }
                    catch (JsonException ex) { throw new FormatException("Texto inválido no quadro mDNS.", ex); }
                }
                if (!node.Values.TryGetValue(nome, out var valores)) node.Values[nome] = valores = [];
                valores.Add(valor);
            }
            else throw new FormatException("Estrutura inválida no quadro mDNS.");
        }
        if (aninhada) throw new FormatException("Quadro mDNS incompleto.");
        return node;
    }

    [GeneratedRegex("\"(?:\\\\.|[^\"\\\\])*\"|[{}:]|[^\\s{}:]+", RegexOptions.CultureInvariant)]
    private static partial Regex Tokens();
}
