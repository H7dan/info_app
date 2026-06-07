# Himi AI Server (local demo)

Part of [Himi](../README.md). Architecture overview: [docs/architecture.md](../docs/architecture.md). Short setup guide: [docs/getting-started/ai-server.md](../docs/getting-started/ai-server.md).

Python FastAPI orchestrator + Ollama (`qwen2.5:7b`) for chat and news translation.

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

Container name may differ; list with `docker ps`.

## Health check

```bash
curl http://localhost:8000/health
```

## Test translate

```bash
curl -X POST http://localhost:8000/v1/translate ^
  -H "Content-Type: application/json" ^
  -d "{\"title\":\"Test\",\"markdown\":\"# Hello\\n\\nBody text.\",\"sourceLang\":\"pl\",\"targetLang\":\"ua\"}"
```

## Test chat

```bash
curl -X POST http://localhost:8000/v1/chat ^
  -H "Content-Type: application/json" ^
  -d "{\"message\":\"Jak uzyskać PESEL?\",\"language\":\"ua\",\"history\":[]}"
```

## MAUI client URLs

| Environment | Base URL |
|-------------|----------|
| Windows / iOS simulator | `http://localhost:8000` |
| Android emulator | `http://10.0.2.2:8000` |
| Physical device | `http://<your-pc-lan-ip>:8000` |

## Stop

```bash
docker compose down
```
