<#
.SYNOPSIS
    Finish an AI Trello task — bumps patch version, commits, pushes, creates a PR,
    then moves the Trello card to Code Review and posts the PR URL as a comment.

.PARAMETER CardId
    The Trello card ID (the long hex string, e.g. 69cbf92d9aa2bae6bc674825).

.PARAMETER Summary
    One-line description of what was implemented/fixed. Used in the commit message,
    PR title, and Trello comment.

.PARAMETER CommitType
    Conventional commit prefix: fix | feat | chore. Defaults to "fix".

.EXAMPLE
    .\scripts\ai-finish.ps1 `
        -CardId  "69cbf92d9aa2bae6bc674825" `
        -Summary "Session capacity not enforced when MaxPlayerPerCourt is set"
#>
param(
    [Parameter(Mandatory = $true)]
    [string]$CardId,

    [Parameter(Mandatory = $true)]
    [string]$Summary,

    [ValidateSet("fix", "feat", "chore")]
    [string]$CommitType = "fix"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$ReviewListId = "69b053ecd1ed8f9bcd01e51e"
$BECsproj     = "Badminton_BE/Badminton_BE.csproj"
$MCPCsproj    = "Badminton_MCP/Badminton_MCP.csproj"

# -----------------------------------------------------------------------
# 1. Read and bump patch version
# -----------------------------------------------------------------------
function Get-ProjectVersion([string]$path) {
    $content = Get-Content $path -Raw
    if ($content -match '<Version>(\d+)\.(\d+)\.(\d+)</Version>') {
        return [PSCustomObject]@{ Major = $Matches[1]; Minor = $Matches[2]; Patch = [int]$Matches[3] }
    }
    throw "Could not find <Version> in $path"
}

$ver        = Get-ProjectVersion $BECsproj
$newVersion = "$($ver.Major).$($ver.Minor).$($ver.Patch + 1)"

Write-Host "Bumping version: $($ver.Major).$($ver.Minor).$($ver.Patch) -> $newVersion" -ForegroundColor Cyan

foreach ($proj in @($BECsproj, $MCPCsproj)) {
    $content = Get-Content $proj -Raw
    $content = $content -replace '<Version>\d+\.\d+\.\d+</Version>', "<Version>$newVersion</Version>"
    Set-Content $proj $content -NoNewline
}

# -----------------------------------------------------------------------
# 2. Build check
# -----------------------------------------------------------------------
Write-Host "Building solution..." -ForegroundColor Cyan
dotnet build --nologo -v q
if ($LASTEXITCODE -ne 0) {
    Write-Error "Build failed. Fix errors before finishing the task."
    exit 1
}
Write-Host "Build succeeded." -ForegroundColor Green

# -----------------------------------------------------------------------
# 3. Stage, commit, push
# -----------------------------------------------------------------------
$branchName = git rev-parse --abbrev-ref HEAD
Write-Host "Committing on branch '$branchName'..." -ForegroundColor Cyan

git add -A
git commit -m "${CommitType}: $Summary (v$newVersion)"
git push origin $branchName

# -----------------------------------------------------------------------
# 4. Create PR via GitHub CLI
# -----------------------------------------------------------------------
Write-Host "Creating pull request..." -ForegroundColor Cyan

$prBody = @"
## Summary
$Summary

## Trello Card
https://trello.com/c/$CardId

## Version
$newVersion

## Changes
See ``docs/version.md`` for detailed change notes.
"@

$prUrl = gh pr create `
    --title "${CommitType}: $Summary (v$newVersion)" `
    --body  $prBody `
    --base  master `
    --head  $branchName

if ($LASTEXITCODE -ne 0) {
    Write-Error "PR creation failed. Push succeeded — create the PR manually."
    exit 1
}

Write-Host "PR created: $prUrl" -ForegroundColor Green

# -----------------------------------------------------------------------
# 5. Update Trello card
# -----------------------------------------------------------------------
$apiKey = $env:TRELLO_API_KEY
$token  = $env:TRELLO_TOKEN

if ([string]::IsNullOrWhiteSpace($apiKey) -or [string]::IsNullOrWhiteSpace($token)) {
    Write-Warning "TRELLO_API_KEY or TRELLO_TOKEN not set — skipping Trello update."
    Write-Host "Manually post this comment on card $CardId :"
    Write-Host "  PR: $prUrl"
    Write-Host "  $Summary"
    exit 0
}

$comment = "PR: $prUrl`n`n$Summary`n`nVersion bumped to $newVersion."

# Post comment
$commentUrl = "https://api.trello.com/1/cards/$CardId/actions/comments?key=$apiKey&token=$token"
Invoke-RestMethod -Uri $commentUrl -Method Post -ContentType "application/json" `
    -Body (ConvertTo-Json @{ text = $comment } -Compress) | Out-Null

# Move card to Code Review and Testing
$moveUrl = "https://api.trello.com/1/cards/$CardId`?key=$apiKey&token=$token"
Invoke-RestMethod -Uri $moveUrl -Method Put -ContentType "application/json" `
    -Body (ConvertTo-Json @{ idList = $ReviewListId } -Compress) | Out-Null

Write-Host "Trello card $CardId moved to Code Review and Testing." -ForegroundColor Green
Write-Host ""
Write-Host "All done!" -ForegroundColor Green
Write-Host "  Branch  : $branchName"
Write-Host "  Version : $newVersion"
Write-Host "  PR      : $prUrl"
