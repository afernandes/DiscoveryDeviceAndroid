using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Commands;

namespace DiscoveryDeviceAndroid.Pareamento;

[VisualStudioContribution]
internal sealed class AbrirDispositivosCommand : Command
{
    public override CommandConfiguration CommandConfiguration => new("%AndroidWifi.DisplayName%")
    {
        Placements = [CommandPlacement.KnownPlacements.ToolsMenu],
        Icon = new(ImageMoniker.KnownValues.ToolWindow, IconSettings.IconAndText),
    };

    public override Task ExecuteCommandAsync(IClientContext context, CancellationToken cancellationToken) =>
        Extensibility.Shell().ShowToolWindowAsync<DispositivosToolWindow>(activate: true, cancellationToken);
}
