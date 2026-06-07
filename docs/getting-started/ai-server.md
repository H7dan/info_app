# AI server (local demo)

Optional backend for **chat** and **news translation**. Runs on your PC via Docker; the MAUI app connects over HTTP.

Full API details and curl examples: [ai-server/README.md](../../ai-server/README.md).

## Prerequisites

- Docker Desktop (or Docker + Docker Compose)

## Start

```bash
cd ai-server
docker compose up --build -d
```

Pull the model (first time only, may take several minutes):

```bash
docker exec -it ai-server-ollama-1 ollama pull qwen2.5:7b
```

Container name may differ — check with `docker ps`.

## Health check

```bash
curl http://localhost:8000/health
```

Expected: `{"ok":true,"ollama":true,"model":"qwen2.5:7b"}` (ollama may be `false` until the model is pulled).

## Connect the MAUI app

| Environment | Base URL |
|-------------|----------|
| Windows / iOS simulator | `http://localhost:8000` |
| Android emulator | `http://10.0.2.2:8000` |
| Physical device | `http://<your-pc-lan-ip>:8000` |

URLs are configured in `AiSettingsService` (Android uses `10.0.2.2` automatically).

## Stub mode (no server)

On the Home screen, enable the **AI stub** switch. Translation and chat return placeholder text without network — useful for UI testing offline.

## Stop

```bash
cd ai-server
docker compose down
```
