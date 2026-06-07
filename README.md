# Himi — handbook for immigrants (Poland)

Himi is a cross-platform **offline-first** mobile app that helps immigrants in Poland find practical information: handbook articles, first-step checklists, emergency contacts, cached news, and optional **AI chat/translation** via a local demo server.

> **Disclaimer:** Educational/demo project. Not legal advice.

## Features

| Area | Description |
|------|-------------|
| **Handbook** | Categorized articles in `ua`, `pl`, `ru` — bundled offline |
| **First steps** | Checklists with persistent progress |
| **News** | Cached articles from gov.pl sources |
| **AI** | Chat, article Q&A, news translation (local Ollama + FastAPI, or stub mode) |

## Quick start

```powershell
# 1) Validate handbook content (from repo root)
powershell -ExecutionPolicy Bypass -File .\tools\validate_content.ps1

# 2) Run on Android
cd HimiMaui
dotnet build -t:Run -f net10.0-android

# 3) Optional — start AI server
cd ai-server
docker compose up --build -d
```

Detailed guides: **[docs/](docs/README.md)**

## Repository map

| Path | Purpose |
|------|---------|
| [`HimiMaui/`](HimiMaui/) | .NET MAUI client (Android, iOS, Windows, Mac) |
| [`ai-server/`](ai-server/) | Python FastAPI + Ollama orchestrator |
| [`docs/`](docs/) | Architecture, getting started, testing |
| [`tools/`](tools/) | Content validation scripts |
| [`HimiMaui/Content/`](HimiMaui/Content/) | Handbook `index.json` + Markdown articles |

## Tech stack

- **Client:** .NET MAUI, C#, MVVM, XAML, Markdig
- **AI (optional):** FastAPI, Ollama (`qwen2.5:7b`), Docker Compose
- **Storage:** Bundled content + on-device app data (news cache, chat history, checklist progress)

## Development

Open [`For-immigrants.slnx`](For-immigrants.slnx) in **Visual Studio** or **Rider**, or use the CLI (see [docs/getting-started/android.md](docs/getting-started/android.md)).

Before a release, run the [smoke test checklist](docs/testing/smoke-test.md).

Contributing: [CONTRIBUTING.md](CONTRIBUTING.md)

## License

MIT — see [LICENSE](LICENSE).
