using DiscoveryDeviceAndroid.Core;
using Xunit;

namespace DiscoveryDeviceAndroid.Tests;

public sealed class ParserAdbTests
{
    [Fact]
    public void Servicos_PortasDistintas_MantemPareamentoEConexaoSeparados()
    {
        var servicos = ParserAdb.Servicos("""
            List of discovered mdns services
            adb-tablet-abc _adb-tls-connect._tcp 192.168.15.8:39775
            adb-tablet-code _adb-tls-pairing._tcp. 192.168.15.8:41931
            printer _ipp._tcp 192.168.15.20:631
            invalid _adb-tls-pairing._tcp invalid:42
            """);
        Assert.Equal(2, servicos.Count);
        Assert.False(servicos[0].Pareamento);
        Assert.True(servicos[1].Pareamento);
        Assert.Equal("192.168.15.8:41931", servicos[1].Endereco);
    }

    [Fact]
    public void Servicos_AnuncioIpv6_PreservaEnderecoComEscopo()
    {
        var servico = Assert.Single(ParserAdb.Servicos("tablet _adb-tls-connect._tcp [fe80::1234%12]:40000"));
        Assert.Equal("[fe80::1234%12]:40000", servico.Endereco);
    }

    [Fact]
    public void Dispositivos_EstadosDiferentes_ConsideraSomenteDeviceConectado()
    {
        var dispositivos = ParserAdb.Dispositivos("""
            * daemon started successfully
            List of devices attached
            192.168.1.2:45321 device product:foo model:SM_X820 device:bar transport_id:1
            emulator-5554 offline transport_id:2
            ABC unauthorized usb:1-1 transport_id:3
            """);
        Assert.Equal(3, dispositivos.Count);
        Assert.Equal("SM X820", dispositivos[0].Modelo);
        Assert.True(dispositivos[0].Conectado);
        Assert.False(dispositivos[1].Conectado);
        Assert.False(dispositivos[2].Conectado);
    }

    [Theory]
    [InlineData("192.168.1.2:0")]
    [InlineData("192.168.1.2:65536")]
    [InlineData("192.168.1.2:5555 --help")]
    [InlineData("192.168.1.2:5555; calc")]
    [InlineData("fe80::1:5555")]
    [InlineData("--help:5555")]
    [InlineData("192.168.1.2")]
    public void Validar_EnderecoInvalido_RejeitaAntesDeExecutar(string endereco)
        => Assert.Throws<ArgumentException>(() => EnderecoAdb.Validar(endereco));

    [Theory]
    [InlineData("192.168.1.2:5555", "192.168.1.2:5555")]
    [InlineData(" [2001:db8::1]:41327 ", "[2001:db8::1]:41327")]
    public void Validar_EnderecoValido_Normaliza(string entrada, string esperado)
        => Assert.Equal(esperado, EnderecoAdb.Validar(entrada));
}
