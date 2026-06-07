# HimiMaui — запуск на Android (Windows)

> English version: [android.md](android.md)

## Требования

- .NET SDK, `dotnet` в PATH
- Workload MAUI: `dotnet workload install maui`
- Android SDK + эмулятор (AVD)
- JDK, `JAVA_HOME`, `java -version`

## 0) Быстрая проверка

```powershell
java -version
echo $env:JAVA_HOME
dotnet --info
```

Если `java` не найден — перезапустите терминал и проверьте `JAVA_HOME` и `PATH`.

## 1) Запуск эмулятора (AVD)

Список AVD:

```powershell
& "$env:LOCALAPPDATA\Android\Sdk\emulator\emulator.exe" -list-avds
```

Запуск (подставьте имя из списка):

```powershell
& "$env:LOCALAPPDATA\Android\Sdk\emulator\emulator.exe" -avd Medium_Phone_API_36.0
```

Проверка устройства:

```powershell
& "$env:LOCALAPPDATA\Android\Sdk\platform-tools\adb.exe" devices
```

Ожидается строка вида `emulator-5554 device`.

## 2) Валидация контента (рекомендуется)

Из корня репозитория:

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\validate_content.ps1
```

При `ERROR:` — исправьте `HimiMaui/Content/index.json` или отсутствующие `.md` и повторите.

## 3) Запуск приложения

```powershell
cd HimiMaui
dotnet build -t:Run -f net10.0-android
```

При ошибке `XA5300` (Java SDK not found):

```powershell
dotnet build -t:Run -f net10.0-android -p:JavaSdkDirectory="$env:JAVA_HOME"
```

## IDE

Откройте [`For-immigrants.slnx`](../../For-immigrants.slnx) в Visual Studio или Rider.
