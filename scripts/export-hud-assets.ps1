param([string]$Repository = (Split-Path $PSScriptRoot -Parent))
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$source = Get-Content -LiteralPath (Join-Path $Repository 'UnityProject/Assets/_Game/Scripts/Runtime/PrototypeGameUI.cs') -Raw
$artRoot = Join-Path $Repository 'UnityProject/Assets/_Game/Resources/Art/UI'
# Mesmos recortes do runtime; origem inferior esquerda do Unity.
$pattern = 'SliceUiAtlas\("Art/UI/(?<atlas>[^"]+)", new\[\]\s*\{(?<names>.*?)\}, new\[\]\s*\{(?<rects>.*?)\}\);'
foreach ($block in [regex]::Matches($source, $pattern, [System.Text.RegularExpressions.RegexOptions]::Singleline)) {
    $atlasName = $block.Groups['atlas'].Value
    $names = [regex]::Matches($block.Groups['names'].Value, '"([^"]+)"')
    $rects = [regex]::Matches($block.Groups['rects'].Value, 'new RectInt\((\d+), (\d+), (\d+), (\d+)\)')
    if ($names.Count -ne $rects.Count) { throw "Atlas com nomes/recortes divergentes: $atlasName" }
    $folder = if ($atlasName -eq 'dw_hud_structure_atlas') { 'Structure' } else { 'Extracted' }
    $output = Join-Path $artRoot $folder
    [System.IO.Directory]::CreateDirectory($output) | Out-Null
    $bitmap = [System.Drawing.Bitmap]::new((Join-Path $artRoot ($atlasName + '.png')))
    try {
        for ($i = 0; $i -lt $names.Count; $i++) {
            $match = $rects[$i]
            $x = [int]$match.Groups[1].Value; $y = [int]$match.Groups[2].Value
            $width = [int]$match.Groups[3].Value; $height = [int]$match.Groups[4].Value
            $rectangle = [System.Drawing.Rectangle]::new($x, $bitmap.Height - $y - $height, $width, $height)
            $crop = $bitmap.Clone($rectangle, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
            if ($names[$i].Groups[1].Value -eq 'gold_coin') {
                # Isola o medalhão circular: a carteira passa atrás dele no atlas original.
                $isolated = [System.Drawing.Bitmap]::new($width, $height, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
                $graphics = [System.Drawing.Graphics]::FromImage($isolated)
                $outline = [System.Drawing.Drawing2D.GraphicsPath]::new()
                try {
                    $outline.AddEllipse(0, 5, 161, 163)
                    $graphics.SetClip($outline)
                    $graphics.DrawImageUnscaled($crop, 0, 0)
                } finally { $outline.Dispose(); $graphics.Dispose() }
                $crop.Dispose()
                $crop = $isolated
            }
            try { $crop.Save((Join-Path $output ($names[$i].Groups[1].Value + '.png')), [System.Drawing.Imaging.ImageFormat]::Png) }
            finally { $crop.Dispose() }
        }
    } finally { $bitmap.Dispose() }
    Write-Output ("Exportado {0}: {1} peças" -f $atlasName, $names.Count)
}
