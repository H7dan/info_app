# HimiMaui

.NET MAUI client for **Himi** — offline-first handbook, checklists, news, and optional AI features.

## Open in IDE

From the repository root, open [`For-immigrants.slnx`](../For-immigrants.slnx) in Visual Studio or Rider.

## Run

See [docs/getting-started/android.md](../docs/getting-started/android.md) (English) or [android.ru.md](../docs/getting-started/android.ru.md) (Русский).

```powershell
cd HimiMaui
dotnet build -t:Run -f net10.0-android
```

## Project layout

| Folder | Role |
|--------|------|
| `Pages/` | XAML screens |
| `ViewModels/` | MVVM logic |
| `Services/` | Data, news, AI, i18n |
| `Content/` | Bundled handbook (`index.json` + articles) |
| `Platforms/` | Android, iOS, Windows, MacCatalyst |

## Docs

- [Architecture](../docs/architecture.md)
- [Handbook content](../docs/content/handbook.md)
- [Smoke test](../docs/testing/smoke-test.md)
- [AI server](../docs/getting-started/ai-server.md)
