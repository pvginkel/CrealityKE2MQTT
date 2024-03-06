@echo off

pushd "%~dp0"

set TARGET=\\srvmain\Software\CrealityKE2MQTT

mkdir %TARGET%

copy Install.bat %TARGET%
copy Update.bat %TARGET%

cd CrealityKE2MQTT\bin\Release\net48

rmdir /q /s %TARGET%\Bin

xcopy . %TARGET%\Bin /s /i

start %TARGET%

popd

pause
