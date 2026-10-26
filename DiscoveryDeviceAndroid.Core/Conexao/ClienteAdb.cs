namespace DiscoveryDeviceAndroid.Core;

public sealed class ClienteAdb
{
    private readonly string caminhoAdb;
    private readonly IProcessRunner runner;
    private readonly IAdbSnapshotReader snapshotReader;

    public ClienteAdb(string caminhoAdb, IProcessRunner? runner = null, IAdbSnapshotReader? snapshotReader = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(caminhoAdb);
        this.caminhoAdb = caminhoAdb;
        this.runner = runner ?? new ProcessRunner();
        this.snapshotReader = snapshotReader ?? new AdbSnapshotReader();
    }

    public async Task<IReadOnlyList<ServicoAdb>> DescobrirServicosAsync(CancellationToken cancellationToken = default)
    {
        var quadro = await snapshotReader.ReadAsync(caminhoAdb, cancellationToken).ConfigureAwait(false);
        if (quadro is not null)
        {
            try { return ParserMdnsProto.Servicos(quadro); }
            catch (FormatException) { /* Platform-tools antigos ou quadro incompatível: consulta tabular. */ }
        }
        return ParserAdb.Servicos(await ExecutarAsync(["mdns", "services"], null, cancellationToken).ConfigureAwait(false));
    }

    public async Task<IReadOnlyList<DispositivoAdb>> ListarDispositivosAsync(CancellationToken cancellationToken = default)
        => ParserAdb.Dispositivos(await ExecutarAsync(["devices", "-l"], null, cancellationToken).ConfigureAwait(false));

    public async Task<string> ParearAsync(string endereco, string codigo, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(codigo) || codigo.Length > 128 || codigo.Any(char.IsControl))
            throw new ArgumentException("Informe um código de pareamento válido.", nameof(codigo));
        var resultado = await ExecutarAsync(["pair", EnderecoAdb.Validar(endereco)], codigo, cancellationToken).ConfigureAwait(false);
        if (!resultado.Contains("Successfully paired to", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("O ADB não confirmou o pareamento. Confira o código e mantenha a tela de pareamento aberta no Android.");
        return resultado;
    }

    public async Task<string> ConectarAsync(string endereco, CancellationToken cancellationToken = default)
    {
        var resultado = await ExecutarAsync(["connect", EnderecoAdb.Validar(endereco)], null, cancellationToken).ConfigureAwait(false);
        if (!resultado.Contains("connected to", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"Não foi possível conectar ao dispositivo: {resultado}");
        return resultado;
    }

    public Task<string> DesconectarAsync(string endereco, CancellationToken cancellationToken = default)
        => ExecutarAsync(["disconnect", EnderecoAdb.Validar(endereco)], null, cancellationToken);

    private async Task<string> ExecutarAsync(IReadOnlyList<string> argumentos, string? segredo, CancellationToken cancellationToken)
    {
        var resultado = await runner.RunAsync(caminhoAdb, argumentos, segredo,
            TimeSpan.FromSeconds(segredo is null ? 15 : 45), cancellationToken).ConfigureAwait(false);
        var mensagem = string.Join(Environment.NewLine, new[] { resultado.StandardOutput.Trim(), resultado.StandardError.Trim() }.Where(s => s.Length > 0));
        if (segredo is not null) mensagem = mensagem.Replace(segredo, "[código protegido]", StringComparison.Ordinal);
        if (resultado.ExitCode != 0)
            throw new InvalidOperationException($"O ADB retornou erro ({resultado.ExitCode}): {mensagem}");
        return mensagem;
    }
}
