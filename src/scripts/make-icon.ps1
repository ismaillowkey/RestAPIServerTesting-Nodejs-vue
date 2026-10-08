Add-Type -AssemblyName System.Drawing

$imgPath = "C:\Users\ismai\.gemini\antigravity-ide\brain\290fc5d9-d712-4ef8-a537-3ba057b7c425\app_icon_1791454328753.jpg"
$outIcoPath = "d:\DATA\App software bikinan\Nodejs-vue-RestAPIServer\csharp-command-center\app.ico"

$sizes = @(16, 32, 48, 64, 128, 256)
$srcBmp = [System.Drawing.Bitmap]::FromFile($imgPath)

$pngStreams = @()
foreach ($sz in $sizes) {
    $destBmp = New-Object System.Drawing.Bitmap($sz, $sz, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $g = [System.Drawing.Graphics]::FromImage($destBmp)
    $g.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
    $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::HighQuality
    $g.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
    $g.DrawImage($srcBmp, 0, 0, $sz, $sz)
    $g.Dispose()

    $ms = New-Object System.IO.MemoryStream
    $destBmp.Save($ms, [System.Drawing.Imaging.ImageFormat]::Png)
    $destBmp.Dispose()
    $pngStreams += ,$ms.ToArray()
}
$srcBmp.Dispose()

$fs = New-Object System.IO.FileStream($outIcoPath, [System.IO.FileMode]::Create)
$bw = New-Object System.IO.BinaryWriter($fs)

# ICONDIR header
$bw.Write([uint16]0) # Reserved
$bw.Write([uint16]1) # Icon type
$bw.Write([uint16]$sizes.Length) # Count

$offset = 6 + ($sizes.Length * 16)

for ($i = 0; $i -lt $sizes.Length; $i++) {
    $sz = $sizes[$i]
    $bytes = $pngStreams[$i]

    $w = if ($sz -ge 256) { 0 } else { $sz }
    $h = if ($sz -ge 256) { 0 } else { $sz }

    $bw.Write([byte]$w)
    $bw.Write([byte]$h)
    $bw.Write([byte]0) # Colors
    $bw.Write([byte]0) # Reserved
    $bw.Write([uint16]1) # Planes
    $bw.Write([uint16]32) # Bit count
    $bw.Write([uint32]$bytes.Length) # Size in bytes
    $bw.Write([uint32]$offset) # Offset

    $offset += $bytes.Length
}

foreach ($bytes in $pngStreams) {
    $bw.Write($bytes)
}

$bw.Close()
$fs.Close()

Write-Host "Icon created successfully at: $outIcoPath"
