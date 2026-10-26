using System.Diagnostics;

namespace DiscoveryDeviceAndroid.Core;

public sealed class ProcessRunner : IProcessRunner
{
    public async Task<ProcessResult> RunAsync(string executable, IReadOnlyList<string> arguments,
        string? standardInput, TimeSpan timeout, CancellationToken cancellationToken)
    {
        using var limite = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        limite.CancelAfter(timeout);
        using var processo = new Process
        {
            StartInfo = new ProcessStartInfo(executable)
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = true
            }
        };
        foreach (var argumento in arguments) processo.StartInfo.ArgumentList.Add(argumento);
        limite.Token.ThrowIfCancellationRequested();
        processo.Start();
        using var registro = limite.Token.Register(() =>
        {
            try { if (!processo.HasExited) processo.Kill(); }
            catch (InvalidOperationException) { }
            catch (System.ComponentModel.Win32Exception) { }
        });
        var saida = processo.StandardOutput.ReadToEndAsync();
        var erro = processo.StandardError.ReadToEndAsync();
        try
        {
            if (standardInput is not null)
                await processo.StandardInput.WriteLineAsync(standardInput.AsMemory(), limite.Token).ConfigureAwait(false);
            processo.StandardInput.Close();
            await processo.WaitForExitAsync(limite.Token).ConfigureAwait(false);
            return new ProcessResult(processo.ExitCode, await saida.ConfigureAwait(false), await erro.ConfigureAwait(false));
        }
        catch (Exception ex) when (limite.IsCancellationRequested)
        {
            cancellationToken.ThrowIfCancellationRequested();
            throw new TimeoutException("O ADB não respondeu no tempo esperado. Verifique a rede e o dispositivo.", ex);
        }
        finally
        {
            if (limite.IsCancellationRequested)
            {
                try { if (!processo.HasExited) processo.Kill(); }
                catch (InvalidOperationException) { }
                catch (System.ComponentModel.Win32Exception) { }
                await Task.WhenAll(saida, erro).ConfigureAwait(false);
            }
        }
    }
}
