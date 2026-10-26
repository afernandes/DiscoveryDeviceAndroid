using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using DiscoveryDeviceAndroid.Pareamento;
using Xunit;
using ZXing;

namespace DiscoveryDeviceAndroid.UiTests;

public sealed class SessaoQrTests
{
    [Fact]
    public async Task Exibir_PrimeiraGeracaoAposBindingVazio_ExibeQrDecodificavelSemSegundoClique()
    {
        var conclusao = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var thread = new Thread(() =>
        {
            try
            {
                var documento = XDocument.Load(Path.Combine(AppContext.BaseDirectory, "DispositivosControl.xaml"));
                XNamespace xaml = "http://schemas.microsoft.com/winfx/2006/xaml";
                var elemento = documento.Descendants().Single(e => (string?)e.Attribute(xaml + "Name") == "QrVisual");
                var visual = Assert.IsType<Viewbox>(XamlReader.Parse(elemento.ToString()));
                var modelo = new QrViewModel();
                visual.DataContext = new { Qr = modelo };
                Assert.Null(Decodificar(visual));

                var primeira = new SessaoQr();
                modelo.Exibir(primeira);
                AssertPayload(primeira, Decodificar(visual));

                var segunda = new SessaoQr();
                modelo.Exibir(segunda);
                AssertPayload(segunda, Decodificar(visual));

                modelo.Limpar();
                Assert.Null(Decodificar(visual));
                var terceira = new SessaoQr();
                modelo.Exibir(terceira);
                AssertPayload(terceira, Decodificar(visual));
                conclusao.SetResult();
            }
            catch (Exception erro) { conclusao.SetException(erro); }
        })
        { IsBackground = true };
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        await conclusao.Task.WaitAsync(TimeSpan.FromSeconds(15));
    }

    private static Result? Decodificar(FrameworkElement visual)
    {
        visual.Measure(new Size(232, 232));
        visual.Arrange(new Rect(0, 0, 232, 232));
        visual.UpdateLayout();
        var bitmap = new RenderTargetBitmap(232, 232, 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(visual);
        var pixels = new byte[232 * 232 * 4];
        bitmap.CopyPixels(pixels, 232 * 4, 0);
        return new BarcodeReaderGeneric().Decode(new RGBLuminanceSource(pixels, 232, 232, RGBLuminanceSource.BitmapFormat.BGRA32));
    }

    private static void AssertPayload(SessaoQr sessao, Result? resultado)
    {
        Assert.NotNull(resultado);
        Assert.Equal($"WIFI:T:ADB;S:{sessao.Nome};P:{sessao.Senha};;", resultado.Text);
    }

    [Fact]
    public void SessaoQr_NovaSessao_RenovaIdentidadeESegredoComExpiracaoCurta()
    {
        var antes = DateTimeOffset.UtcNow;
        var primeira = new SessaoQr();
        var segunda = new SessaoQr();
        Assert.Matches("^studio-[a-zA-Z0-9]{10}$", primeira.Nome);
        Assert.Matches("^[a-zA-Z0-9]{12}$", primeira.Senha);
        Assert.NotEqual(primeira.Nome, segunda.Nome);
        Assert.NotEqual(primeira.Senha, segunda.Senha);
        Assert.InRange(primeira.ExpiraEm, antes.AddMinutes(3), DateTimeOffset.UtcNow.AddMinutes(3));
    }
}
