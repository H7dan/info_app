# Release smoke test (MVP)

Manual checklist before tagging a release or sharing a build.

## Launch

- App starts on Android emulator/device without crashing.

## Home

- Language switch cycles `ua → pl → ru` and UI texts update.
- Phone menu opens and shows emergency numbers (997/998/999).
- Tapping a phone item opens the dialer (or shows an error alert if unavailable).
- Chat icon (💬) opens the chat screen.
- AI stub switch toggles; when ON, no network is required for AI actions.

## Handbook

- Categories load.
- If a category has one article, it opens directly.
- If a category has multiple articles, the list opens and search works.
- Articles open and render correctly (markdown → HTML).

## First steps

- Checklists list loads.
- Checklist opens, checkboxes toggle.
- Progress persists after app restart.
- “Open details” links open the referenced article.

## News

- News list loads (cached or after refresh).
- Opening a news article renders content.
- **Translate**: button works (stub or live server); original/translation toggle appears when cached.
- **Ask question**: navigates to chat with article footnote (title only in UI).

## AI (stub mode)

With stub ON on Home:

- Chat: send a message → `[Stub]` reply.
- Article translate → `[Stub]` title/body.

## AI (live server)

With stub OFF and [ai-server running](../getting-started/ai-server.md):

- `curl http://localhost:8000/health` returns OK.
- Chat: send a message → real reply (may take up to a few minutes on first run).
- Chat about article: footnote visible, reply uses article context.
- Cancel (■) stops an in-flight chat request.
- Disclaimer visible at bottom of chat screen.

## Content validation

From repo root:

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\validate_content.ps1
```

Must exit with code 0.
