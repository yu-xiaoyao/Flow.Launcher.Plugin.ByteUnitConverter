dotnet publish Flow.Launcher.Plugin.ByteUnitConverter -c Release -r win-x64 --no-self-contained
Compress-Archive -LiteralPath Flow.Launcher.Plugin.ByteUnitConverter/bin/Release/win-x64/publish -DestinationPath Flow.Launcher.Plugin.ByteUnitConverter/bin/ByteUnitConverter.zip -Force