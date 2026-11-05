param([Parameter(Mandatory=$true)][string]$Region,[ValidateSet('base','grieta','abismo')][string]$Layer='base')
$ErrorActionPreference='Stop'
$root='C:\Arcanoria Master\Gusi Gaming\gacha-fase3'
Add-Type -AssemblyName System.Drawing
$bgs=@(Get-ChildItem -LiteralPath "$root\Assets\SalYAlmas\Art\Ambientes" -Filter "amb_${Region}_*.png" | Where-Object { if($Layer -eq 'base') { $_.BaseName -notmatch '_(grieta|abismo)$' } else { $_.BaseName -match "_${Layer}$" } } | Sort-Object Name)
$enemies=@()
switch($Region) {
 'aries' {$names=@('escoria_viva','perro_fragua','soldada_crisol','ferran','carnero_hierro','ave_a_medias')}
 'cancer' {$names=@('medusa_salmuera','gaviota_hueca','cangrejo_salinero','madre_tenaza','medusa_cielo')}
 'libra' {$names=@('moneda_andante','guardia_contrapeso','tesorera_almas','duquesa_fiel','contador_sin_rostro')}
 'capricornio' {$names=@('gota_obsidiana','garza_obsidiana','sorbedor','boca_vieja','boca_nueva','serpiente_nube')}
 'invasores' {$names=@('figura_sal','cristalizado','sin_carta','huesped_menor')}
 default {$names=@(); $list="$root\art\enemigos\$Region.lst"; if(Test-Path -LiteralPath $list) { foreach($id in (Get-Content -LiteralPath $list)) { $enemies+=Get-Item -LiteralPath "$root\Assets\SalYAlmas\Art\Enemigos\$id.png" -ErrorAction SilentlyContinue } }}
}
foreach($n in $names) {$enemies+=Get-Item -LiteralPath "$root\Assets\SalYAlmas\Art\Enemigos\enemigo_$n.png" -ErrorAction SilentlyContinue}
if($Layer -ne 'base') { $enemies=@() }
$bgRows=[int][Math]::Ceiling($bgs.Count/2.0)
$enemyRows=[int][Math]::Ceiling($enemies.Count/4.0)
$height=[Math]::Max(40,40+$bgRows*385+$enemyRows*340)
$sheet=[System.Drawing.Bitmap]::new(1280,$height)
$g=[System.Drawing.Graphics]::FromImage($sheet)
$g.Clear([System.Drawing.ColorTranslator]::FromHtml('#EDE3CC'))
$g.InterpolationMode=[System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
$font=[System.Drawing.Font]::new('Consolas',13)
$brush=[System.Drawing.Brushes]::Black
$g.DrawString($Region,$font,$brush,8,6)
for($i=0;$i -lt $bgs.Count;$i++) {
 $im=[System.Drawing.Image]::FromFile($bgs[$i].FullName)
 $x=($i%2)*640; $y=40+[int][Math]::Floor($i/2)*385
 $g.DrawImage($im,[System.Drawing.Rectangle]::new($x,$y,640,360))
 $g.DrawString($bgs[$i].Name,$font,$brush,$x+5,$y+362)
 $im.Dispose()
}
for($i=0;$i -lt $enemies.Count;$i++) {
 $im=[System.Drawing.Image]::FromFile($enemies[$i].FullName)
 $x=($i%4)*320; $y=40+$bgRows*385+[int][Math]::Floor($i/4)*340
 $g.FillRectangle([System.Drawing.Brushes]::DimGray,$x+4,$y,312,285)
 $g.DrawImage($im,[System.Drawing.Rectangle]::new($x+18,$y,280,280))
 $g.FillRectangle([System.Drawing.Brushes]::Black,$x+248,$y+220,64,64)
 $g.DrawImage($im,[System.Drawing.Rectangle]::new($x+248,$y+220,64,64))
 $g.DrawString($enemies[$i].BaseName.Replace('enemigo_',''),$font,$brush,$x+5,$y+287)
 $im.Dispose()
}
$suffix=if($Layer -eq 'base'){''}else{"_$Layer"}
$output="$root\art\ambientes\hoja_$Region$suffix.png"
$sheet.Save($output,[System.Drawing.Imaging.ImageFormat]::Png)
$g.Dispose();$sheet.Dispose();$font.Dispose()
Write-Output $output
