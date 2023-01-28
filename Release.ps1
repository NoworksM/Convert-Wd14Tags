Param(
    [string]
    $version = ""
)

if (Test-Path "./Release")
{
    rm -R -Force ./Release
}

$runtimes = @{ win32 = "win-x64"; win64 = "win-x64"; linux64 = "linux-x64"; osx64 = "osx-x64" }

$runtimes.GetEnumerator() | ForEach-Object {
    dotnet publish Convert-Wd14Tags -c Release -r $_.Value --no-self-contained -p:PublishSingleFile=true -o "./Release/$($_.Key)"
}

if ([string]::IsNullOrWhiteSpace($version))
{
    7z a ./Release/wdtaggerfix-win32.zip ./Release/win32/Convert-Wd14Tags.exe
    7z a ./Release/wdtaggerfix-win64.zip ./Release/win64/Convert-Wd14Tags.exe
    7z a ./Release/wdtaggerfix-linux64.zip ./Release/linux64/Convert-Wd14Tags
    7z a ./Release/wdtaggerfix-osx64.zip ./Release/osx64/Convert-Wd14Tags
}
else
{
    7z a "./Release/wdtaggerfix-win32-v$($version).zip" ./Release/win32/Convert-Wd14Tags.exe
    7z a "./Release/wdtaggerfix-win64-v$($version).zip" ./Release/win64/Convert-Wd14Tags.exe
    7z a "./Release/wdtaggerfix-linux64-v$($version).zip" ./Release/linux64/Convert-Wd14Tags
    7z a "./Release/wdtaggerfix-osx64-v$($version).zip" ./Release/osx64/Convert-Wd14Tags
}

rm -R -Force ./Release/win32
rm -R -Force ./Release/win64
rm -R -Force ./Release/linux64
rm -R -Force ./Release/osx64