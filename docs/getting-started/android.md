# Run on Android (Windows)

## Prerequisites

- .NET SDK with `dotnet` in PATH
- .NET MAUI workload: `dotnet workload install maui`
- Android SDK + emulator (AVD)
- JDK with `JAVA_HOME` set; `java -version` works

## 0) Quick checks

```powershell
java -version
echo $env:JAVA_HOME
dotnet --info
```

If `java` is not found, restart the terminal and verify `JAVA_HOME` and `PATH`.

## 1) Start an Android emulator (AVD)

List AVDs:

```powershell
& "$env:LOCALAPPDATA\Android\Sdk\emulator\emulator.exe" -list-avds
```

Run an AVD (replace with a name from the list):

```powershell
& "$env:LOCALAPPDATA\Android\Sdk\emulator\emulator.exe" -avd Medium_Phone_API_36.0
```

Verify the device is visible:

```powershell
& "$env:LOCALAPPDATA\Android\Sdk\platform-tools\adb.exe" devices
```

Expected: a line like `emulator-5554 device`.

## 2) Validate offline content (recommended)

From the repository root:

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\validate_content.ps1
```

If it prints `ERROR:` lines, fix `HimiMaui/Content/index.json` or missing `.md` files and rerun.

## 3) Run the MAUI app

```powershell
cd HimiMaui
dotnet build -t:Run -f net10.0-android
```

If you get `XA5300` (Java SDK not found), pass an explicit JDK path:

```powershell
dotnet build -t:Run -f net10.0-android -p:JavaSdkDirectory="$env:JAVA_HOME"
```

## IDE

Open [`For-immigrants.slnx`](../../For-immigrants.slnx) in Visual Studio or Rider, select the Android target, and run.

Russian version: [android.ru.md](android.ru.md).
