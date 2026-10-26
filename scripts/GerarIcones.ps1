$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$destino = Join-Path (Split-Path -Parent $PSScriptRoot) 'DiscoveryDeviceAndroid\Distribution'
[IO.Directory]::CreateDirectory($destino) | Out-Null
foreach ($item in @(@{ Nome = 'icon.png'; Tamanho = 128 }, @{ Nome = 'preview.png'; Tamanho = 200 })) {
    $bitmap = [Drawing.Bitmap]::new($item.Tamanho, $item.Tamanho)
    $grafico = [Drawing.Graphics]::FromImage($bitmap)
    $fundo = [Drawing.Drawing2D.LinearGradientBrush]::new([Drawing.Point]::new(0,0), [Drawing.Point]::new($item.Tamanho,$item.Tamanho), [Drawing.ColorTranslator]::FromHtml('#1656B8'), [Drawing.ColorTranslator]::FromHtml('#102B58'))
    $caneta = [Drawing.Pen]::new([Drawing.Color]::White, 5)
    $caneta.StartCap = [Drawing.Drawing2D.LineCap]::Round
    $caneta.EndCap = [Drawing.Drawing2D.LineCap]::Round
    $verde = [Drawing.SolidBrush]::new([Drawing.ColorTranslator]::FromHtml('#79F2C0'))
    try {
        $grafico.SmoothingMode = [Drawing.Drawing2D.SmoothingMode]::AntiAlias
        $grafico.FillRectangle($fundo, 0, 0, $item.Tamanho, $item.Tamanho)
        $grafico.ScaleTransform(($item.Tamanho / 128.0), ($item.Tamanho / 128.0))
        $grafico.DrawRectangle($caneta, 28, 30, 45, 72)
        $grafico.DrawLine($caneta, 44, 39, 57, 39)
        $grafico.FillEllipse([Drawing.Brushes]::White, 48, 91, 6, 6)
        $grafico.DrawArc($caneta, 65, 26, 42, 42, 222, 96)
        $grafico.DrawArc($caneta, 74, 36, 24, 24, 222, 96)
        $grafico.FillEllipse($verde, 82, 50, 8, 8)
        $bitmap.Save((Join-Path $destino $item.Nome), [Drawing.Imaging.ImageFormat]::Png)
    }
    finally { $verde.Dispose(); $caneta.Dispose(); $fundo.Dispose(); $grafico.Dispose(); $bitmap.Dispose() }
}
