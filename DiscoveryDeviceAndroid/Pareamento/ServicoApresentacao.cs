using System.Runtime.Serialization;

namespace DiscoveryDeviceAndroid.Pareamento;

[DataContract]
internal sealed record ServicoApresentacao(
    [property: DataMember] string Nome,
    [property: DataMember] string Tipo,
    [property: DataMember] string Endereco,
    [property: DataMember] string Api,
    [property: DataMember] string Versao,
    [property: DataMember] string Instancia);
