using Microsoft.VisualStudio.Extensibility;

namespace DiscoveryDeviceAndroid;

[VisualStudioContribution]
internal sealed class ExtensionEntrypoint : Extension
{
    public override ExtensionConfiguration ExtensionConfiguration => new()
    {
        Metadata = new("DiscoveryDeviceAndroid.362a398c-e451-4040-8af7-644682ce12ea",
            ExtensionAssemblyVersion, "afernandes", "Android via Wi-Fi",
            "Descubra, pareie e conecte dispositivos Android pela rede sem fio usando o Android SDK instalado.")
        {
            Icon = "Distribution/icon.png",
            PreviewImage = "Distribution/preview.png",
            ReleaseNotes = "Distribution/RELEASE-NOTES.txt",
            License = @"Distribution\LICENSE.txt",
            Tags = ["Android", "ADB", "Wi-Fi", "MAUI", "debugging", "QR code"],
            InstallationTargetVersion = "[17.14,19.0)",
            InstallationTargetArchitecture = VisualStudioArchitecture.Amd64,
            Preview = true,
        },
    };
}
