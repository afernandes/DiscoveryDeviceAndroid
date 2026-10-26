using System.Runtime.Serialization;

namespace DiscoveryDeviceAndroid.Pareamento;

[DataContract]
internal sealed record DispositivoApresentacao(
    [property: DataMember] string Modelo,
    [property: DataMember] string Serial,
    [property: DataMember] string Estado);
