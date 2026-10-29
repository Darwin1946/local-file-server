$ErrorActionPreference = 'Stop'
Set-Location $PSScriptRoot
$compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
& $compiler /nologo /target:winexe /optimize+ /win32icon:LocalFileServer.ico /out:LocalFileServer-v2.exe /reference:System.Windows.Forms.dll /reference:System.Drawing.dll LocalFileServer.cs
if ($LASTEXITCODE -ne 0) { throw 'Build failed' }
