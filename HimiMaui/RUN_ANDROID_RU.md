# HimiMaui — run on Android (Windows)

## Prerequisites
- .NET SDK installed and `dotnet` available in PATH
- .NET MAUI workload installed (`dotnet workload install maui`)
- Android SDK + Emulator installed 
- JDK installed; `JAVA_HOME` set; `java -version` works

## 0) Quick checks
    java -version
    echo $env:JAVA_HOME
    dotnet --info

If `java` is not found, restart the terminal/Cursor and verify `JAVA_HOME` and `PATH`.
## 1) Start an Android emulator (AVD)

List AVDs:
    & "$env:LOCALAPPDATA\Android\Sdk\emulator\emulator.exe" -list-avds

Run an AVD (replace with a name from the list):
    & "$env:LOCALAPPDATA\Android\Sdk\emulator\emulator.exe" -avd Medium_Phone_API_36.0 (example)

Verify the device is visible:
    & "$env:LOCALAPPDATA\Android\Sdk\platform-tools\adb.exe" devices

Expected: a line like `emulator-5554 device`.
## 2) Validate offline content (recommended before running)
From repo root:
    powershell -ExecutionPolicy Bypass -File .\tools\validate_content.ps1

If it prints `ERROR:` lines, fix `HimiMaui/Content/index.json` or missing `.md` files and rerun.
## 3) Run the MAUI app on Android
    cd D:\Progs\For-immigrants\HimiMaui
    dotnet build -t:Run -f net10.0-android

If you get `XA5300` (Java SDK not found), run with an explicit JDK path:
    dotnet build -t:Run -f net10.0-android -p:JavaSdkDirectory="$env:JAVA_HOME"
