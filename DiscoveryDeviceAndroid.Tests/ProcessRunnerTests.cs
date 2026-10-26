using DiscoveryDeviceAndroid.Core;
using Xunit;

namespace DiscoveryDeviceAndroid.Tests;

public sealed class ProcessRunnerTests
{
    private static string PowerShell => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System),
        "WindowsPowerShell", "v1.0", "powershell.exe");

    [Fact]
    public async Task RunAsync_EntradaPadraoESaidaDeErro_LeAmbosSemShellIntermediario()
    {
        if (!OperatingSystem.IsWindows()) return;
        var resultado = await new ProcessRunner().RunAsync(PowerShell,
            ["-NoProfile", "-NonInteractive", "-Command", "$value = [Console]::ReadLine(); [Console]::Out.WriteLine($value); [Console]::Error.WriteLine('diagnostico'); exit 7"],
            "123456", TimeSpan.FromSeconds(10), CancellationToken.None);
        Assert.Equal(7, resultado.ExitCode);
        Assert.Equal("123456", resultado.StandardOutput.Trim());
        Assert.Equal("diagnostico", resultado.StandardError.Trim());
    }

    [Fact]
    public async Task RunAsync_ProcessoNaoTermina_EncerraPorTimeout()
    {
        if (!OperatingSystem.IsWindows()) return;
        await Assert.ThrowsAsync<TimeoutException>(() => new ProcessRunner().RunAsync(PowerShell,
            ["-NoProfile", "-NonInteractive", "-Command", "[System.Threading.Tasks.Task]::Delay(30000).Wait()"],
            null, TimeSpan.FromMilliseconds(300), CancellationToken.None));
    }

    [Fact]
    public async Task RunAsync_CancelamentoDuranteExecucao_EncerraEPropagaCancelamento()
    {
        if (!OperatingSystem.IsWindows()) return;
        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(300));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => new ProcessRunner().RunAsync(PowerShell,
            ["-NoProfile", "-NonInteractive", "-Command", "[System.Threading.Tasks.Task]::Delay(30000).Wait()"],
            null, TimeSpan.FromSeconds(10), cts.Token));
    }
}
