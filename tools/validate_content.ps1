param(
  [Parameter(Mandatory = $false)]
  [string] $RepoRoot = "",

  [Parameter(Mandatory = $false)]
  [string] $IndexPath = "HimiMaui/Content/index.json",

  [Parameter(Mandatory = $false)]
  [string[]] $Languages = @("ua", "pl", "ru"),

  [Parameter(Mandatory = $false)]
  [switch] $AllowMissingTranslations
)

$ErrorActionPreference = "Stop"

function Write-Err([string] $msg) { Write-Host ("ERROR: " + $msg) -ForegroundColor Red }
function Write-WarnMsg([string] $msg) { Write-Host ("WARN:  " + $msg) -ForegroundColor Yellow }
function Write-Ok([string] $msg) { Write-Host ("OK:    " + $msg) -ForegroundColor Green }

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
if ([string]::IsNullOrWhiteSpace($RepoRoot)) {
  $RepoRoot = (Resolve-Path (Join-Path $scriptDir "..")).Path
}

$indexFullPath = Join-Path $RepoRoot $IndexPath
if (-not (Test-Path -LiteralPath $indexFullPath)) {
  Write-Err "Index not found: $indexFullPath"
  exit 2
}

$index = Get-Content -LiteralPath $indexFullPath -Raw | ConvertFrom-Json

$errors = New-Object System.Collections.Generic.List[string]
$warnings = New-Object System.Collections.Generic.List[string]

function Add-Error([string] $msg) { $errors.Add($msg) | Out-Null }
function Add-Warn([string] $msg) { $warnings.Add($msg) | Out-Null }

# ---- Categories referenced by articles
$categoryIds = @{}
foreach ($c in $index.categories) { $categoryIds[$c.id] = $true }

foreach ($a in $index.articles) {
  if (-not $categoryIds.ContainsKey($a.categoryId)) {
    Add-Error "Article '$($a.id)'[$($a.lang)] references missing categoryId '$($a.categoryId)'."
  }
}

# ---- Body files exist
foreach ($a in $index.articles) {
  $bodyRel = $a.bodyPath
  if ([string]::IsNullOrWhiteSpace($bodyRel)) {
    Add-Error "Article '$($a.id)'[$($a.lang)] has empty bodyPath."
    continue
  }

  $bodyFull = Join-Path $RepoRoot ("HimiMaui/" + $bodyRel.TrimStart("/").Replace("\\", "/"))
  if (-not (Test-Path -LiteralPath $bodyFull)) {
    Add-Error "Missing body file for '$($a.id)'[$($a.lang)]: $($a.bodyPath) (expected at '$bodyFull')."
  }
}

# ---- Translation completeness by article id
$articlesById = $index.articles | Group-Object -Property id
foreach ($g in $articlesById) {
  $id = $g.Name
  $langsPresent = @($g.Group | ForEach-Object { $_.lang }) | Sort-Object -Unique

  foreach ($lang in $Languages) {
    if (-not ($langsPresent -contains $lang)) {
      $msg = "Missing translation: article '$id' has no '$lang' entry in index.json."
      if ($AllowMissingTranslations) { Add-Warn $msg } else { Add-Error $msg }
    }
  }
}

# ---- Checklist links point to existing articles (per language)
$articlesByLang = @{}
foreach ($lang in $Languages) {
  $articlesByLang[$lang] = @{}
}
foreach ($a in $index.articles) {
  if (-not $articlesByLang.ContainsKey($a.lang)) { continue }
  $articlesByLang[$a.lang][$a.id] = $true
}

foreach ($cl in $index.checklists) {
  $lang = $cl.lang
  if (-not $articlesByLang.ContainsKey($lang)) { continue }

  foreach ($sec in $cl.sections) {
    foreach ($item in $sec.items) {
      $target = $item.linksToArticleId
      if ([string]::IsNullOrWhiteSpace($target)) { continue }

      if (-not $articlesByLang[$lang].ContainsKey($target)) {
        Add-Error "Checklist '$($cl.id)'[$lang] item '$($item.id)' links to missing articleId '$target' for language '$lang'."
      }
    }
  }
}

# ---- Print report
foreach ($w in $warnings) { Write-WarnMsg $w }
foreach ($e in $errors) { Write-Err $e }

if ($errors.Count -gt 0) {
  Write-Host ""
  Write-Err ("Validation failed with {0} error(s), {1} warning(s)." -f $errors.Count, $warnings.Count)
  exit 1
}

Write-Host ""
Write-Ok ("Validation passed with {0} warning(s)." -f $warnings.Count)
exit 0

