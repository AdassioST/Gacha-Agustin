param(
    [Parameter(Mandatory=$true)][string]$Source,
    [Parameter(Mandatory=$true)][string]$Id,
    [switch]$Enemy,
    [string]$Revision = ''
)
$ErrorActionPreference = 'Stop'
$root = 'C:\Arcanoria Master\Gusi Gaming\gacha-fase3'
if ($Id -notmatch '^(amb_[a-z_]+|mapa_anillo|enemigo_[a-z_]+)$') { throw 'Nombre fuera del contrato de arte' }
$kind = if ($Enemy) { 'enemigos' } else { 'ambientes' }
$destination = if ($Enemy) { 'Enemigos' } else { 'Ambientes' }
$suffix = if($Revision) { if($Revision -notmatch '^r[1-9][0-9]*$') { throw 'Revision invalida' }; "_$Revision" } else { '' }
$original = Join-Path $root "art\$kind\fuentes\${Id}_fuente$suffix.png"
$target = Join-Path $root "Assets\SalYAlmas\Art\$destination\$Id.png"
if ((Test-Path -LiteralPath $target) -and -not $Revision) { throw "El archivo ya existe: $Id" }
New-Item -ItemType Directory -Path (Split-Path -Parent $original) -Force | Out-Null
New-Item -ItemType Directory -Path (Split-Path -Parent $target) -Force | Out-Null
if (-not (Test-Path -LiteralPath $original)) { Copy-Item -LiteralPath $Source -Destination $original }
Add-Type -AssemblyName System.Drawing
$inputImage = [System.Drawing.Bitmap]::FromFile($original)
$width = if ($Enemy) { 1024 } else { 1920 }
$height = if ($Enemy) { 1024 } else { 1080 }
$canvas = [System.Drawing.Bitmap]::new($width,$height,[System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
$graphics = [System.Drawing.Graphics]::FromImage($canvas)
$graphics.Clear([System.Drawing.Color]::Transparent)
$graphics.CompositingMode = [System.Drawing.Drawing2D.CompositingMode]::SourceCopy
$graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
$graphics.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
if ($Enemy) {
    $minX=$inputImage.Width; $minY=$inputImage.Height; $maxX=-1; $maxY=-1
    for($y=0; $y -lt $inputImage.Height; $y++) {
        for($x=0; $x -lt $inputImage.Width; $x++) {
            if($inputImage.GetPixel($x,$y).A -gt 2) {
                $minX=[Math]::Min($minX,$x); $minY=[Math]::Min($minY,$y)
                $maxX=[Math]::Max($maxX,$x); $maxY=[Math]::Max($maxY,$y)
            }
        }
    }
    if ($maxX -lt 0) { throw 'Fuente sin contenido visible' }
    if ($minX -eq 0 -and $minY -eq 0 -and $maxX -eq $inputImage.Width-1 -and $maxY -eq $inputImage.Height-1) { throw 'Revisar transparencia: contenido ocupa los cuatro bordes' }
    $bw=$maxX-$minX+1; $bh=$maxY-$minY+1
    $scale=[Math]::Min(896.0/$bw,896.0/$bh)
    $dw=[int][Math]::Floor($bw*$scale); $dh=[int][Math]::Floor($bh*$scale)
    $destRect=[System.Drawing.Rectangle]::new([int](($width-$dw)/2),[int](($height-$dh)/2),$dw,$dh)
    $srcRect=[System.Drawing.Rectangle]::new($minX,$minY,$bw,$bh)
} else {
    $ratio=1920.0/1080.0
    $sw=[double]$inputImage.Width; $sh=[double]$inputImage.Height
    if($sw/$sh -gt $ratio) { $sw=$sh*$ratio } else { $sh=$sw/$ratio }
    $srcRect=[System.Drawing.RectangleF]::new([single](($inputImage.Width-$sw)/2),[single](($inputImage.Height-$sh)/2),[single]$sw,[single]$sh)
    $destRect=[System.Drawing.RectangleF]::new(0,0,$width,$height)
}
$graphics.DrawImage($inputImage,$destRect,$srcRect,[System.Drawing.GraphicsUnit]::Pixel)
$canvas.Save($target,[System.Drawing.Imaging.ImageFormat]::Png)
$graphics.Dispose(); $canvas.Dispose(); $inputImage.Dispose()
Write-Output $target
