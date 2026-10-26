using System.Diagnostics;

namespace DiscoveryDeviceAndroid.Core;

/// <summary>Lê um único quadro do cliente de acompanhamento, sem encerrar o servidor ADB compartilhado.</summary>
public sealed class AdbSnapshotReader : IAdbSnapshotReader
{
    public async Task<string?> ReadAsync(string executable, CancellationToken cancellationToken)
    {
        using var limite = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        limite.CancelAfter(TimeSpan.FromSeconds(2));
        using var processo = new Process
        {
            StartInfo = new ProcessStartInfo(executable)
            {
                UseShellExecute = false, CreateNoWindow = true,
                RedirectStandardOutput = true, RedirectStandardError = true
            }
        };
        processo.StartInfo.ArgumentList.Add("mdns");
        processo.StartInfo.ArgumentList.Add("track-services");
        processo.StartInfo.ArgumentList.Add("--proto-text");
        cancellationToken.ThrowIfCancellationRequested();
        processo.Start();
        var erro = processo.StandardError.ReadToEndAsync();
        try
        {
            var quadro = await AdbSnapshotFrameReader.ReadAsync(processo.StandardOutput, limite.Token, aceitarQuadroCompletoNoLimite: true).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            return quadro;
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested) { return null; }
        finally
        {
            try { if (!processo.HasExited) processo.Kill(); }
            catch (InvalidOperationException) { }
            catch (System.ComponentModel.Win32Exception) { }
            await processo.WaitForExitAsync(CancellationToken.None).ConfigureAwait(false);
            await erro.ConfigureAwait(false);
        }
    }
}
