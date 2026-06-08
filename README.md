# Himi — handbook for immigrants (Poland)

**Himi** is an offline-first mobile app that helps immigrants in Poland find practical, everyday information: handbook articles, first-step checklists, emergency contacts, cached official news, and optional AI chat and translation.


Active development lives on the [`dev`](https://github.com/H7dan/Himi/tree/dev) branch.

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
