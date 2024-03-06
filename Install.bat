@echo off

pushd "%~dp0"

set TARGET=%LOCALAPPDATA%\CrealityKE2MQTT

net stop CrealityKE2MQTT
sc create CrealityKE2MQTT binpath= "%TARGET%\Bin\CrealityKE2MQTT.exe" start= auto
net start CrealityKE2MQTT

popd

pause
