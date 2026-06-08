# Himi — handbook for immigrants (Poland)

**Himi** is an offline-first mobile app that helps immigrants in Poland find practical, everyday information: handbook articles, first-step checklists, emergency contacts, cached official news, and optional AI chat and translation.

> **Disclaimer:** Educational/demo project. Not legal advice.

## Where is the code?

Active development and the full project live on the **`dev`** branch:

| Branch | Contents |
|--------|----------|
| [`dev`](https://github.com/H7dan/info_app/tree/dev) | .NET MAUI app (`HimiMaui/`), AI server (`ai-server/`), docs, tools |
| `main` | Project overview (this file); legacy Flutter scaffold in `gigi/` |

To work with the app, clone the repo and switch to `dev`:

```bash
git clone https://github.com/H7dan/info_app.git
cd info_app
git checkout dev
```

See the [README on `dev`](https://github.com/H7dan/info_app/blob/dev/README.md) for setup, architecture, and contribution guides.

## Features

| Area | Description |
|------|-------------|
| **Handbook** | Categorized articles in Ukrainian, Polish, and Russian — bundled offline |
| **First steps** | Checklists with persistent progress |
| **News** | Cached articles from gov.pl sources |
| **AI** | Chat, article Q&A, news translation (local Ollama + FastAPI, or stub mode) |

## Tech stack

- **Client:** .NET MAUI, C#, MVVM, XAML
- **AI (optional):** FastAPI, Ollama, Docker Compose
- **Storage:** Bundled content + on-device app data (news cache, chat history, checklist progress)

## License

MIT (see the `dev` branch for license details).
