using System.Security.Cryptography;
using System.Text;
using QRCoder;

namespace DiscoveryDeviceAndroid.Pareamento;

internal sealed class SessaoQr
{
    public string Nome { get; } = "studio-" + RandomNumberGenerator.GetString("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789", 10);
    public string Senha { get; } = RandomNumberGenerator.GetString("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789", 12);
    public DateTimeOffset ExpiraEm { get; } = DateTimeOffset.UtcNow.AddMinutes(3);

    public ImagemQr CriarImagem()
    {
        using var dados = QRCodeGenerator.GenerateQrCode($"WIFI:T:ADB;S:{Nome};P:{Senha};;", QRCodeGenerator.ECCLevel.M);
        var geometria = new StringBuilder();
        for (var y = 0; y < dados.ModuleMatrix.Count; y++)
            for (var x = 0; x < dados.ModuleMatrix[y].Length; x++)
                if (dados.ModuleMatrix[y][x]) geometria.Append($"M{x},{y}h1v1h-1z ");
        var tamanho = dados.ModuleMatrix.Count;
        return new ImagemQr(tamanho, geometria.ToString());
    }
}
