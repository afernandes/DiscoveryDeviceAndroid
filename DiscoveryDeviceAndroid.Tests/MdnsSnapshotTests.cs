using DiscoveryDeviceAndroid.Core;
using Xunit;

namespace DiscoveryDeviceAndroid.Tests;

public sealed class MdnsSnapshotTests
{
    private const string Quadro = """
        tls {
          service {
            instance: "adb-EXEMPLO1234-demo"
            service: "_adb-tls-connect._tcp"
            ipv4: "192.0.2.10"
            ipv6: "2001:db8::10"
            ipv6: "fe80::10"
            port: 39775
            product_model: "SM-X820"
            build_version_sdk_full: "36.1"
            mdns_service_version: "1"
            hostname: "Android-2.local"
          }
          known_device: true
        }
        pair {
          service {
            instance: "studio-sessao"
            service: "_adb-tls-pairing._tcp"
            ipv4: "192.0.2.10"
            port: 41931
          }
        }
        """;

    [Fact]
    public void Servicos_QuadroModernoComMetadados_PreservaModeloApiVersaoEPortas()
    {
        var servicos = ParserMdnsProto.Servicos(Quadro);
        Assert.Equal(2, servicos.Count);
        Assert.Equal("SM-X820", servicos[0].NomeModelo);
        Assert.Equal("36.1", servicos[0].Api);
        Assert.Equal("1", servicos[0].VersaoMdns);
        Assert.True(servicos[0].DispositivoConhecido);
        Assert.Equal("192.0.2.10:39775", servicos[0].Endereco);
        Assert.True(servicos[1].Pareamento);
        Assert.Equal("192.0.2.10:41931", servicos[1].Endereco);
    }

    [Fact]
    public void Servicos_SomenteIpv6EMetadadosAusentes_NaoInventaModeloOuVersao()
    {
        var servico = Assert.Single(ParserMdnsProto.Servicos("""
            tls { service { instance: "tablet" service: "_adb-tls-connect._tcp" ipv6: "2001:db8::1" port: 40000 } }
            """));
        Assert.Equal("[2001:db8::1]:40000", servico.Endereco);
        Assert.Null(servico.Api);
        Assert.Null(servico.NomeModelo);
        Assert.Null(servico.VersaoMdns);
    }

    [Fact]
    public void Servicos_ModeloComChavesEAspasEscapadas_InterpretaComoTexto()
    {
        var servico = Assert.Single(ParserMdnsProto.Servicos("""
            tls { service { instance: "tablet" service: "_adb-tls-connect._tcp" ipv4: "192.168.1.2" port: 40000 device_model: "Tablet {\"Pro\"}" } }
            """));
        Assert.Equal("Tablet {\"Pro\"}", servico.NomeModelo);
    }

    [Fact]
    public async Task ReadAsync_DoisQuadros_RetornaSomentePrimeiroQuadroCompleto()
    {
        using var reader = new StringReader($"\nServices:\n{Quadro}\n\nServices:\ntls {{\n");
        var texto = await AdbSnapshotFrameReader.ReadAsync(reader, CancellationToken.None);
        Assert.NotNull(texto);
        Assert.Equal(2, ParserMdnsProto.Servicos(texto).Count);
    }

    [Fact]
    public async Task ReadAsync_QuadroCompletoSemLinhaVaziaFinal_AceitaFormatoRealDoAdb()
    {
        using var reader = new StringReader($"\r\nServices:\r\n{Quadro}\r\n");
        var texto = await AdbSnapshotFrameReader.ReadAsync(reader, CancellationToken.None);
        Assert.NotNull(texto);
        Assert.Equal(2, ParserMdnsProto.Servicos(texto).Count);
    }

    [Theory]
    [InlineData("Services:\ntls {\n\n")]
    [InlineData("Services:\ntls {\n")]
    [InlineData("unknown command\n")]
    public async Task ReadAsync_QuadroIncompletoOuIncompativel_NaoPublicaFragmento(string texto)
    {
        using var reader = new StringReader(texto);
        Assert.Null(await AdbSnapshotFrameReader.ReadAsync(reader, CancellationToken.None));
    }

    [Fact]
    public async Task DescobrirServicosAsync_ModernoDisponivel_PreservaMetadadosSemConsultaLegada()
    {
        var runner = new FakeProcessRunner(new(0, "", ""));
        var cliente = new ClienteAdb("adb.exe", runner, new FakeAdbSnapshotReader(Quadro));
        var resultado = await cliente.DescobrirServicosAsync();
        Assert.Equal("SM-X820", resultado[0].NomeModelo);
        Assert.Equal(0, runner.Calls);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("tls {")]
    public async Task DescobrirServicosAsync_ModernoIndisponivelOuIncompleto_UsaFallbackLegado(string? quadro)
    {
        var runner = new FakeProcessRunner(new(0, "tablet _adb-tls-connect._tcp 192.168.1.2:40000", ""));
        var cliente = new ClienteAdb("adb.exe", runner, new FakeAdbSnapshotReader(quadro));
        var servico = Assert.Single(await cliente.DescobrirServicosAsync());
        Assert.Equal("192.168.1.2:40000", servico.Endereco);
        Assert.Null(servico.NomeModelo);
        Assert.Equal(new[] { "mdns", "services" }, runner.Arguments);
    }

    [Fact]
    public async Task DescobrirServicosAsync_QuadroModernoVazio_RespeitaAusenciaDeAnuncios()
    {
        var runner = new FakeProcessRunner(new(0, "tablet _adb-tls-connect._tcp 192.168.1.2:40000", ""));
        var cliente = new ClienteAdb("adb.exe", runner, new FakeAdbSnapshotReader(""));
        Assert.Empty(await cliente.DescobrirServicosAsync());
        Assert.Equal(0, runner.Calls);
    }
}
