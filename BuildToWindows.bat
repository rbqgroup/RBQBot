@Echo off
set base_dir=%~dp0
dotnet publish -r win-x64 -c Release -p:PublishTrimmed=false -p:PublishReadyToRun=false -p:PublishSingleFile=true -p:SelfContained=true
Echo. Done!
pause.
