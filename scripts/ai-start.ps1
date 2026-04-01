<#
.SYNOPSIS
    Start working on a Trello AI card — pulls master and creates the AI/ branch.

.PARAMETER CardName
    The Trello card name (used to generate the branch slug).

.EXAMPLE
    .\scripts\ai-start.ps1 -CardName "Bug - Session capacity not enforced"
    # Creates and checks out: AI/bug-session-capacity-not-enforced
#>
param(
    [Parameter(Mandatory = $true)]
    [string]$CardName
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

# --- Slugify -----------------------------------------------------------
$slug = $CardName.ToLower()
$slug = $slug -replace "[^a-z0-9\s-]", ""
$slug = $slug -replace "\s+", "-"
$slug = $slug -replace "-+", "-"
$slug = $slug.Trim("-")
$branchName = "AI/$slug"

# --- Ensure we start from a clean master --------------------------------
Write-Host "Switching to master and pulling latest..." -ForegroundColor Cyan
git checkout master
git pull origin master

# --- Create branch ------------------------------------------------------
Write-Host "Creating branch '$branchName'..." -ForegroundColor Cyan
git checkout -b $branchName

Write-Host ""
Write-Host "Ready. You are now on branch: $branchName" -ForegroundColor Green
Write-Host "Card: $CardName"
Write-Host ""
Write-Host "When done, run:" -ForegroundColor Yellow
Write-Host "  .\scripts\ai-finish.ps1 -CardId <trello-card-id> -Summary `"<what you did>`""
