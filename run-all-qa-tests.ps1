# =============================================================================
# BSOFT — Run ALL Module QA Tests + Email + Slack + JIRA Bug Creation
#
# Usage:
#   .\run-all-qa-tests.ps1              # run all modules
#   .\run-all-qa-tests.ps1 -Module "UserManagement"  # single module
# =============================================================================

param(
    [string] $Module = "All"
)

$SolutionRoot  = $PSScriptRoot
$TestsRoot     = "$SolutionRoot\src\tests"
$ReportRoot    = "$SolutionRoot\qa-reports"
$ScriptsRoot   = "$SolutionRoot\scripts"
$Timestamp     = Get-Date -Format "yyyyMMdd_HHmmss"
$RunDate       = Get-Date -Format "dd-MM-yyyy HH:mm"
$ReportDir     = "$ReportRoot\$Timestamp"

# ── All modules ───────────────────────────────────────────────────────────────
$AllModules = @(
    "UserManagement",
    "SalesManagement",
    "PurchaseManagement",
    "InventoryManagement",
    "PartyManagement",
    "ProjectManagement",
    "WarehouseManagement",
    "BudgetManagement",
    "FixedAssetManagement",
    "MaintenanceManagement",
    "FinanceManagement",
    "GateEntryManagement",
    "LogisticsManagement",
    "ProductionManagement",
    "QCManagement"
)

$ModulesToRun = if ($Module -eq "All") { $AllModules } else { @($Module) }

New-Item -ItemType Directory -Force -Path $ReportDir | Out-Null

function Write-Step($msg)    { Write-Host "`n==> $msg" -ForegroundColor Cyan }
function Write-Success($msg) { Write-Host "    ✅ $msg" -ForegroundColor Green }
function Write-Fail($msg)    { Write-Host "    ❌ $msg" -ForegroundColor Red }
function Write-Skip($msg)    { Write-Host "    ⏭️  $msg" -ForegroundColor Gray }

# ── Run tests per module ──────────────────────────────────────────────────────
$moduleResults = @()
$allFailedTests = @()

Write-Step "BSOFT QA Test Run — $RunDate"
Write-Host "Modules: $($ModulesToRun -join ', ')" -ForegroundColor White

foreach ($mod in $ModulesToRun) {

    $projectPath = "$TestsRoot\$mod.QATests"
    $reportFile  = "$ReportDir\$mod.html"
    $trxFile     = "$ReportDir\$mod.trx"

    if (-not (Test-Path $projectPath)) {
        Write-Skip "$mod — no QA tests yet"
        continue
    }

    Write-Host "`n▶ Running $mod..." -ForegroundColor Cyan

    dotnet test $projectPath `
        --logger "html;LogFileName=$reportFile" `
        --logger "trx;LogFileName=$trxFile" `
        --logger "console;verbosity=quiet" `
        --results-directory $ReportDir

    $exitCode = $LASTEXITCODE

    # ── Parse TRX for failed test details ─────────────────────────────────────
    $failedInModule = @()
    if ($exitCode -ne 0 -and (Test-Path $trxFile)) {
        [xml]$trx = Get-Content $trxFile
        $failedNodes = $trx.TestRun.Results.UnitTestResult |
            Where-Object { $_.outcome -eq "Failed" }

        foreach ($node in $failedNodes) {
            $testName  = $node.testName -replace ".*\.", ""
            $entityName = ($testName -split "_")[0..1] -join ""
            $message   = $node.Output.ErrorInfo.Message

            $failedInModule += [PSCustomObject]@{
                Module   = $mod
                Entity   = $entityName
                TestName = $testName
                Expected = "see report"
                Actual   = ($message -split "`n")[0] -replace ".*Expected.*", "see report"
            }
        }
        $allFailedTests += $failedInModule
    }

    $moduleResults += [PSCustomObject]@{
        Module    = $mod
        Status    = if ($exitCode -eq 0) { "PASS" } else { "FAIL" }
        FailCount = $failedInModule.Count
        Report    = $reportFile
    }

    if ($exitCode -eq 0) {
        Write-Success "$mod — ALL PASSED"
    } else {
        Write-Fail "$mod — $($failedInModule.Count) FAILED"
    }
}

# ── Print Summary ─────────────────────────────────────────────────────────────
Write-Host "`n============================================================" -ForegroundColor White
Write-Host "  BSOFT QA TEST SUMMARY — $RunDate" -ForegroundColor White
Write-Host "============================================================" -ForegroundColor White

foreach ($r in $moduleResults) {
    $color = if ($r.Status -eq "PASS") { "Green" } else { "Red" }
    $icon  = if ($r.Status -eq "PASS") { "✅" } else { "❌" }
    Write-Host ("  $icon  {0,-35} {1}" -f $r.Module, $r.Status) -ForegroundColor $color
}

$passCount = ($moduleResults | Where-Object { $_.Status -eq "PASS" }).Count
$failCount = ($moduleResults | Where-Object { $_.Status -eq "FAIL" }).Count

Write-Host "============================================================" -ForegroundColor White
Write-Host "  Passed: $passCount   Failed: $failCount   Total Tests Failed: $($allFailedTests.Count)" -ForegroundColor White
Write-Host "  Reports: $ReportDir" -ForegroundColor White
Write-Host "============================================================`n" -ForegroundColor White

# ── Open reports for failed modules ──────────────────────────────────────────
$moduleResults | Where-Object { $_.Status -eq "FAIL" } | ForEach-Object {
    if (Test-Path $_.Report) { Start-Process $_.Report }
}

# ── Notifications + JIRA (only on failures) ───────────────────────────────────
if ($allFailedTests.Count -gt 0) {

    Write-Step "Creating JIRA bugs..."
    $jiraKeys = & "$ScriptsRoot\create-jira-bug.ps1" `
        -FailedTests $allFailedTests `
        -RunDate $RunDate

    Write-Step "Sending Email notification..."
    & "$ScriptsRoot\notify-email.ps1" `
        -FailedTests $allFailedTests `
        -RunDate $RunDate

    Write-Step "Sending Slack notification..."
    & "$ScriptsRoot\notify-slack.ps1" `
        -FailedTests $allFailedTests `
        -RunDate $RunDate

    Write-Host "`nJIRA Bugs Created: $($jiraKeys -join ', ')" -ForegroundColor Yellow
}

# ── Exit code ─────────────────────────────────────────────────────────────────
exit $failCount
