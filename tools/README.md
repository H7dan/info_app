# Tools

Helper scripts for repository maintenance.

## validate_content.ps1

Validates `HimiMaui/Content/index.json` against Markdown files on disk.

### Usage

From the repository root:

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\validate_content.ps1
```

### Exit codes

| Code | Meaning |
|------|---------|
| 0 | All checks passed |
| 2 | Index file missing |
| 3 | Validation errors (see `ERROR:` lines) |

### When to run

- Before committing handbook changes
- Before a release (also see [smoke test](../docs/testing/smoke-test.md))
- In CI (`.github/workflows/validate.yml`)

### Options

```powershell
# Custom repo root
powershell -ExecutionPolicy Bypass -File .\tools\validate_content.ps1 -RepoRoot "D:\path\to\repo"

# Allow missing translation files (warnings only)
powershell -ExecutionPolicy Bypass -File .\tools\validate_content.ps1 -AllowMissingTranslations
```

See also: [Handbook content guide](../docs/content/handbook.md)
