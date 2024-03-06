@echo off

pushd "%~dp0"

set TARGET=%LOCALAPPDATA%\CrealityKE2MQTT

mkdir %TARGET%
rmdir /s /q %TARGET%\Bin

copy *.bat %TARGET%

xcopy Bin %TARGET%\Bin /s /i

start %TARGET%

popd

pause
