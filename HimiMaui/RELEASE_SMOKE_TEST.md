## Release smoke test (MVP)

- **Launch**
  - App starts on Android emulator/device without crashing.

- **Home**
  - Language switch cycles `ua → pl → ru` and UI texts update.
  - Phone menu opens and shows emergency numbers (997/998/999).
  - Tapping a phone item opens the dialer (or shows an error alert if unavailable).

- **Handbook**
  - Categories load.
  - If a category has one article, it opens directly.
  - If a category has multiple articles, the list opens and search works.
  - Articles open and render correctly (markdown → HTML).

- **First steps**
  - Checklists list loads.
  - Checklist opens, checkboxes toggle.
  - Progress persists after app restart.
  - “Open details” links open the referenced article.

- **Content validation**
  - Run `tools/validate_content.ps1` and ensure it exits with code 0.

