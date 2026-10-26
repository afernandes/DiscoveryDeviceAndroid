using System.Runtime.Serialization;
using Microsoft.VisualStudio.Extensibility.UI;

namespace DiscoveryDeviceAndroid.Pareamento;

[DataContract]
internal sealed class QrViewModel : NotifyPropertyChangedObject
{
    private int tamanho = 1;
    private string geometria = "";

    [DataMember]
    public int Tamanho { get => tamanho; private set => SetProperty(ref tamanho, value); }

    [DataMember]
    public string Geometria { get => geometria; private set => SetProperty(ref geometria, value); }

    public void Exibir(SessaoQr sessao)
    {
        var imagem = sessao.CriarImagem();
        Tamanho = imagem.Tamanho;
        Geometria = imagem.Geometria;
    }

    public void Limpar()
    {
        Geometria = "";
        Tamanho = 1;
    }
}
