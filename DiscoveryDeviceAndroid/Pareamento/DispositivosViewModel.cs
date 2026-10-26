using System.Runtime.Serialization;
using DiscoveryDeviceAndroid.Core;
using Microsoft.VisualStudio.Extensibility.UI;

namespace DiscoveryDeviceAndroid.Pareamento;

[DataContract]
internal sealed class DispositivosViewModel : NotifyPropertyChangedObject, IDisposable
{
    private readonly CancellationTokenSource encerramento = new();
    private readonly SemaphoreSlim operacao = new(1, 1);
    private readonly Task monitor;
    private readonly object filtroLock = new();
    private int descartado;
    private ClienteAdb? cliente;
    private string caminhoAtual = "";
    private IReadOnlyList<ServicoAdb> encontrados = [];
    private SessaoQr? sessaoQr;
    private string caminhoAdb = "", filtro = "", enderecoPareamento = "", codigo = "", enderecoConexao = "";
    private string status = "Procurando o Android SDK instalado…", descoberta = "Aguardando a primeira consulta.", resumo = "";
    private string statusQr = "Gere um QR code para iniciar uma sessão de três minutos.";
    private bool disponivel = true;

    public DispositivosViewModel()
    {
        AtualizarCommand = new AsyncCommand((_, ct) => ExecutarAsync(AtualizarAsync, ct));
        ParearCommand = new AsyncCommand((_, ct) => ExecutarAsync(ParearCodigoAsync, ct));
        ConectarCommand = new AsyncCommand((_, ct) => ExecutarAsync(ConectarAsync, ct));
        DesconectarCommand = new AsyncCommand((p, ct) => ExecutarAsync(c => DesconectarAsync(p, c), ct));
        SelecionarCommand = new AsyncCommand((p, ct) => ExecutarAsync(c => SelecionarAsync(p, c), ct));
        GerarQrCommand = new AsyncCommand((_, ct) => ExecutarAsync(GerarQrAsync, ct));
        CancelarQrCommand = new AsyncCommand((_, ct) => ExecutarAsync(c => { EncerrarQr("Sessão encerrada."); return Task.CompletedTask; }, ct));
        monitor = MonitorarAsync(encerramento.Token);
    }

    [DataMember] public IAsyncCommand AtualizarCommand { get; }
    [DataMember] public IAsyncCommand ParearCommand { get; }
    [DataMember] public IAsyncCommand ConectarCommand { get; }
    [DataMember] public IAsyncCommand DesconectarCommand { get; }
    [DataMember] public IAsyncCommand SelecionarCommand { get; }
    [DataMember] public IAsyncCommand GerarQrCommand { get; }
    [DataMember] public IAsyncCommand CancelarQrCommand { get; }
    [DataMember] public ObservableList<ServicoApresentacao> Servicos { get; } = new();
    [DataMember] public ObservableList<DispositivoApresentacao> Dispositivos { get; } = new();
    [DataMember] public string CaminhoAdb { get => caminhoAdb; set => SetProperty(ref caminhoAdb, value); }
    [DataMember] public string Filtro { get => filtro; set { SetProperty(ref filtro, value); Filtrar(); } }
    [DataMember] public string EnderecoPareamento { get => enderecoPareamento; set => SetProperty(ref enderecoPareamento, value); }
    [DataMember] public string Codigo { get => codigo; set => SetProperty(ref codigo, value); }
    [DataMember] public string EnderecoConexao { get => enderecoConexao; set => SetProperty(ref enderecoConexao, value); }
    [DataMember] public string Status { get => status; private set => SetProperty(ref status, value); }
    [DataMember] public string Descoberta { get => descoberta; private set => SetProperty(ref descoberta, value); }
    [DataMember] public string Resumo { get => resumo; private set => SetProperty(ref resumo, value); }
    [DataMember] public string StatusQr { get => statusQr; private set => SetProperty(ref statusQr, value); }
    [DataMember] public bool Disponivel { get => disponivel; private set => SetProperty(ref disponivel, value); }
    [DataMember] public QrViewModel Qr { get; } = new();

    private ClienteAdb ObterCliente()
    {
        var caminho = LocalizadorAdb.Localizar(CaminhoAdb);
        if (cliente is null || caminhoAtual != caminho)
        {
            cliente = new ClienteAdb(caminho);
            caminhoAtual = caminho;
            EncerrarQr("Gere um novo QR code após configurar o ADB.");
            Resumo = "ADB: " + caminho;
        }
        return cliente;
    }

    private async Task ExecutarAsync(Func<CancellationToken, Task> acao, CancellationToken ct)
    {
        using var vinculado = CancellationTokenSource.CreateLinkedTokenSource(ct, encerramento.Token);
        try
        {
            await operacao.WaitAsync(vinculado.Token);
            try { Disponivel = false; await acao(vinculado.Token); }
            finally { Disponivel = true; operacao.Release(); }
        }
        catch (OperationCanceledException) { if (!encerramento.IsCancellationRequested) Status = "Operação cancelada."; }
        catch (Exception ex) { Status = ex.Message; }
    }

    private async Task MonitorarAsync(CancellationToken ct)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(4));
        try
        {
            await ExecutarAsync(AtualizarAsync, ct);
            while (await timer.WaitForNextTickAsync(ct))
            {
                if (!await operacao.WaitAsync(0, ct)) continue;
                try { await AtualizarAsync(ct); }
                catch (OperationCanceledException) when (ct.IsCancellationRequested) { break; }
                catch (Exception ex) { Descoberta = ex.Message; }
                finally { operacao.Release(); }
            }
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { }
    }

    private async Task AtualizarAsync(CancellationToken ct)
    {
        var adb = ObterCliente();
        var dispositivos = await adb.ListarDispositivosAsync(ct);
        Dispositivos.Clear();
        foreach (var d in dispositivos) Dispositivos.Add(new(d.Modelo, d.Serial, d.Estado));
        if (Status == "Procurando o Android SDK instalado…") Status = "Pronto. Selecione um serviço ou inicie o pareamento por QR code.";
        try
        {
            encontrados = await adb.DescobrirServicosAsync(ct);
            Filtrar();
            Descoberta = encontrados.Count == 0
                ? "Nenhum serviço encontrado. Ative a depuração sem fio; confirme a mesma rede Wi-Fi. A rede pode bloquear mDNS. Use os campos manuais abaixo."
                : $"{encontrados.Count} serviço(s) na rede • atualização automática a cada 4 segundos";
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            encontrados = []; Filtrar();
            Descoberta = "Descoberta mDNS indisponível: " + ex.Message + " Use o endereço manual.";
        }

        if (sessaoQr is { } qr)
        {
            if (DateTimeOffset.UtcNow >= qr.ExpiraEm) { EncerrarQr("QR code expirado. Gere uma nova sessão."); return; }
            var servico = encontrados.FirstOrDefault(s => s.Pareamento && s.Nome == qr.Nome);
            if (servico is not null)
            {
                sessaoQr = null;
                Qr.Limpar();
                StatusQr = "Pareando o dispositivo que leu este QR code…";
                try
                {
                    Status = await adb.ParearAsync(servico.Endereco, qr.Senha, ct);
                    StatusQr = "Pareamento concluído. Aguarde a conexão automática do ADB ou selecione um serviço de conexão.";
                }
                catch { StatusQr = "Falha no pareamento. Gere outro QR code para tentar novamente."; throw; }
            }
            else StatusQr = $"Aguardando leitura no Android • expira em {Math.Ceiling((qr.ExpiraEm - DateTimeOffset.UtcNow).TotalSeconds)} s";
        }
    }

    private void Filtrar()
    {
        lock (filtroLock)
        {
            var lista = encontrados;
            Servicos.Clear();
            foreach (var s in lista.Where(s => string.IsNullOrWhiteSpace(Filtro)
                || s.Nome.Contains(Filtro, StringComparison.OrdinalIgnoreCase)
                || (s.NomeModelo?.Contains(Filtro, StringComparison.OrdinalIgnoreCase) ?? false)
                || s.Endereco.Contains(Filtro, StringComparison.OrdinalIgnoreCase)))
            {
                Servicos.Add(new(s.NomeModelo ?? s.Nome, s.Pareamento ? "Pareamento" : "Conexão", s.Endereco,
                    s.Api ?? "Não informado", s.VersaoMdns is { } versao ? "v" + versao + ".0" : "Não informada", s.Nome));
            }
        }
    }

    private async Task ParearCodigoAsync(CancellationToken ct)
    {
        var segredo = Codigo.Trim();
        Codigo = "";
        if (segredo.Length != 6 || segredo.Any(c => c < '0' || c > '9')) throw new ArgumentException("Informe o código de seis dígitos mostrado no Android.");
        Status = "Pareando…";
        Status = await ObterCliente().ParearAsync(EnderecoPareamento.Trim(), segredo, ct);
        await AtualizarAsync(ct);
        Status += " Se o dispositivo não estiver online, use o IP e a porta de conexão exibidos na tela principal de Depuração sem fio.";
    }

    private async Task ConectarAsync(CancellationToken ct)
    {
        Status = "Conectando…";
        Status = await ObterCliente().ConectarAsync(EnderecoConexao.Trim(), ct);
        await AtualizarAsync(ct);
    }

    private async Task DesconectarAsync(object? parametro, CancellationToken ct)
    {
        var serial = parametro as string ?? EnderecoConexao.Trim();
        Status = await ObterCliente().DesconectarAsync(serial, ct);
        await AtualizarAsync(ct);
    }

    private Task SelecionarAsync(object? parametro, CancellationToken ct)
    {
        if (parametro is not string endereco) return Task.CompletedTask;
        var s = encontrados.FirstOrDefault(s => s.Endereco == endereco);
        if (s is null) throw new InvalidOperationException("O serviço expirou. Atualize a descoberta.");
        if (s.Pareamento) { EnderecoPareamento = s.Endereco; Status = "Digite o código de pareamento mostrado no Android."; }
        else { EnderecoConexao = s.Endereco; Status = "Endereço selecionado. Clique em Conectar."; }
        return Task.CompletedTask;
    }

    private Task GerarQrAsync(CancellationToken ct)
    {
        ObterCliente();
        sessaoQr = new SessaoQr();
        Qr.Exibir(sessaoQr);
        StatusQr = "No Android: Depuração sem fio → Parear dispositivo com QR code.";
        return Task.CompletedTask;
    }

    private void EncerrarQr(string mensagem) { sessaoQr = null; Qr.Limpar(); StatusQr = mensagem; }

    public void Dispose()
    {
        if (Interlocked.Exchange(ref descartado, 1) != 0) return;
        encerramento.Cancel();
        sessaoQr = null;
        Codigo = "";
        _ = monitor.ContinueWith(_ => encerramento.Dispose(), TaskScheduler.Default);
    }
}
