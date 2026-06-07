# Handbook content

Handbook articles are **bundled with the app** and work offline.

## Layout

```
HimiMaui/Content/
  index.json              # categories + article metadata
  articles/
    ua/*.md
    pl/*.md
    ru/*.md
```

Each logical article (e.g. `emergency_numbers`) has one Markdown file per language. Paths are referenced in `index.json` as `bodyPath`.

## Adding or editing an article

1. Add or update `.md` files under `articles/{ua,pl,ru}/`.
2. Add or update entries in `Content/index.json`:
   - `id` — stable slug (same across languages)
   - `lang` — `ua`, `pl`, or `ru`
   - `title`, `summary`, `categoryId`, `tags`, `bodyPath`
3. Run validation from the repo root:

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\validate_content.ps1
```

Fix any `ERROR:` output before committing.

## Validation rules (summary)

The script checks that:

- Every `bodyPath` in `index.json` points to an existing file
- Categories referenced by articles exist
- Expected language folders and cross-references are consistent

See [tools/README.md](../../tools/README.md) for script options.

## Rendering

Articles are converted to HTML in the app via Markdig (`MarkdownRenderer`) and shown in a `WebView` on `ArticlePage`.

News articles use a separate pipeline (`NewsStore`, cached under app data) — not the bundled `Content/` folder.
