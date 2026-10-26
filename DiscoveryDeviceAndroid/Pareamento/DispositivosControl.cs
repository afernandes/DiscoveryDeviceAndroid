using Microsoft.VisualStudio.Extensibility.UI;

namespace DiscoveryDeviceAndroid.Pareamento;

internal sealed class DispositivosControl(DispositivosViewModel modelo) : RemoteUserControl(modelo)
{
    protected override void Dispose(bool disposing)
    {
        if (disposing) modelo.Dispose();
        base.Dispose(disposing);
    }
}
