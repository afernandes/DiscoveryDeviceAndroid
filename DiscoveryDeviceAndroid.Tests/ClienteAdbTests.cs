using DiscoveryDeviceAndroid.Core;
using Xunit;

namespace DiscoveryDeviceAndroid.Tests;

public sealed class ClienteAdbTests
{
    [Fact]
    public async Task ParearAsync_CodigoValido_EnviaSegredoSomentePelaEntradaPadrao()
    {
        var runner = new FakeProcessRunner(new(0, "Successfully paired to 192.168.1.2:41931 [guid=123456]", ""));
        var cliente = new ClienteAdb("adb.exe", runner);
        var resultado = await cliente.ParearAsync("192.168.1.2:41931", "123456");
        Assert.Equal(new[] { "pair", "192.168.1.2:41931" }, runner.Arguments);
        Assert.Equal("123456", runner.Input);
        Assert.DoesNotContain("123456", resultado);
    }

    [Fact]
    public async Task ParearAsync_FalhaContendoSegredo_RemoveSegredoDaExcecao()
    {
        var runner = new FakeProcessRunner(new(1, "pairing 654321 failed", "error 654321"));
        var erro = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            new ClienteAdb("adb.exe", runner).ParearAsync("192.168.1.2:41931", "654321"));
        Assert.DoesNotContain("654321", erro.ToString());
    }

    [Fact]
    public async Task ConectarAsync_AdbRetornaExitZeroComFalha_NaoDeclaraSucesso()
    {
        var runner = new FakeProcessRunner(new(0, "failed to connect to 192.168.1.2:40000", ""));
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            new ClienteAdb("adb.exe", runner).ConectarAsync("192.168.1.2:40000"));
    }

    [Fact]
    public async Task ParearAsync_QuebraDeLinhaNoSegredo_NaoExecutaAdb()
    {
        var runner = new FakeProcessRunner(new(0, "", ""));
        await Assert.ThrowsAsync<ArgumentException>(() =>
            new ClienteAdb("adb.exe", runner).ParearAsync("192.168.1.2:41931", "123456\nextra"));
        Assert.Equal(0, runner.Calls);
    }

    [Fact]
    public async Task ConectarAsync_CancelamentoSolicitado_PropagaCancelamento()
    {
        var runner = new FakeProcessRunner(new(0, "connected to 192.168.1.2:40000", ""));
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            new ClienteAdb("adb.exe", runner).ConectarAsync("192.168.1.2:40000", cts.Token));
    }
}
