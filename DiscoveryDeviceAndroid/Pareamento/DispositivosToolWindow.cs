using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.ToolWindows;
using Microsoft.VisualStudio.RpcContracts.RemoteUI;

namespace DiscoveryDeviceAndroid.Pareamento;

[VisualStudioContribution]
internal sealed class DispositivosToolWindow : ToolWindow
{
    public DispositivosToolWindow() => Title = "Android via Wi-Fi";
    public override ToolWindowConfiguration ToolWindowConfiguration => new() { Placement = ToolWindowPlacement.DocumentWell };

    public override Task<IRemoteUserControl> GetContentAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<IRemoteUserControl>(new DispositivosControl(new DispositivosViewModel()));
    }
}
