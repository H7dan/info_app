# Himi — handbook for immigrants (Poland)

Himi is a cross‑platform **offline-first** mobile app that helps immigrants in Poland quickly find essential, practical information (first steps, documents, contacts, checklists, and short articles).

## What’s inside

- **Handbook**: categorized articles (currently `pl`, `ru`, `ua`) stored in the repo and shipped with the app.
- **Checklists**: track progress through common “first steps”.
- **Contacts**: quick access to important numbers and places.

## Tech stack

- **.NET MAUI**: cross‑platform UI (Android / iOS / Windows / MacCatalyst)
- **C#** with **MVVM** view models
- **XAML** pages and styles
- **Markdig**: Markdown parsing/rendering for the handbook content
- **Local app storage**: persist checklist progress on device

## Repository structure

- `HimiMaui/`: the MAUI application source code
- `HimiMaui/Content/`: handbook content (`index.json` + Markdown articles)
- `tools/`: helper scripts (e.g. content validation)

## Run

See `HimiMaui/RUN_ANDROID_RU.md` for Android build/run instructions.
