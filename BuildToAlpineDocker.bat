@Echo off
set base_dir=%~dp0
del /F /S /Q .\app\
del /F /S /Q .\app\database.db
dotnet publish -r linux-musl-x64 -c release -o .\app -p:SelfContained=true -p:PublishTrimmed=false -p:PublishReadyToRun=true -p:PublishSingleFile=true
cd app
del RBQBot.pdb /Q /F
mkdir db
cd ..
copy .\Dockerfile .\app\
echo "Build Done."
pause
