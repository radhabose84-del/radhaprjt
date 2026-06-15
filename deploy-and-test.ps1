# =============================================================================
# BSOFT — Deploy to QA Server + Run QA Tests Automatically
# Usage:
#   .\deploy-and-test.ps1                    # deploy + test against QA server
#   .\deploy-and-test.ps1 -TestOnly          # test against QA server (no deploy)
#   .\deploy-and-test.ps1 -TestOnly -Local   # test against localhost:5239
#   .\deploy-and-test.ps1 -DeployOnly        # deploy only, skip tests
# =============================================================================

param(
    [switch]$TestOnly,
    [switch]$DeployOnly,
    [switch]$Local       # if set, tests run against localhost instead of QA server
)

# ── CONFIG — change these for your environment ────────────────────────────────
$QAServerIP       = "198.168.1.130"
$QABaseUrl        = "http://$QAServerIP/BsoftErp"
$QADeployPath     = "\\$QAServerIP\D`$\Publish\BsoftERP"   # UNC path to IIS folder
$IISAppPool       = "BsoftERP"                              # IIS App Pool name on QA server
$SolutionRoot     = $PSScriptRoot
$PublishOutput    = "$SolutionRoot\publish\api"
$QATestProject    = "$SolutionRoot\src\tests\UserManagement.QATests\UserManagement.QATests.csproj"
$TestResultsDir   = "$SolutionRoot\qa-reports"
$ApiProject       = "$SolutionRoot\src\BSOFT.Api\BSOFT.Api.csproj"
# ─────────────────────────────────────────────────────────────────────────────

$ErrorActionPreference = "Stop"

function Write-Step($msg) {
    Write-Host "`n==> $msg" -ForegroundColor Cyan
}

function Write-Success($msg) {
    Write-Host "    [OK] $msg" -ForegroundColor Green
}

function Write-Fail($msg) {
    Write-Host "    [FAIL] $msg" -ForegroundColor Red
}

# ── STEP 1: BUILD ─────────────────────────────────────────────────────────────
if (-not $TestOnly) {
    Write-Step "Building BSOFT.Api..."
    dotnet build $ApiProject -c Release --no-restore
    if ($LASTEXITCODE -ne 0) { Write-Fail "Build failed. Aborting."; exit 1 }
    Write-Success "Build passed."

    # ── STEP 2: PUBLISH ───────────────────────────────────────────────────────
    Write-Step "Publishing to $PublishOutput..."
    if (Test-Path $PublishOutput) { Remove-Item $PublishOutput -Recurse -Force }
    dotnet publish $ApiProject -c Release -o $PublishOutput --no-build
    if ($LASTEXITCODE -ne 0) { Write-Fail "Publish failed. Aborting."; exit 1 }
    Write-Success "Publish complete."

    # ── STEP 3: STOP IIS APP POOL ─────────────────────────────────────────────
    Write-Step "Stopping IIS App Pool '$IISAppPool' on QA server..."
    try {
        Invoke-Command -ComputerName $QAServerIP -ScriptBlock {
            param($pool)
            Import-Module WebAdministration
            if ((Get-WebAppPoolState -Name $pool).Value -eq "Started") {
                Stop-WebAppPool -Name $pool
                Start-Sleep -Seconds 2
            }
        } -ArgumentList $IISAppPool
        Write-Success "App pool stopped."
    } catch {
        Write-Host "    [WARN] Could not stop app pool remotely: $_" -ForegroundColor Yellow
        Write-Host "    [INFO] Stop IIS app pool manually on QA server before proceeding." -ForegroundColor Yellow
        Read-Host "    Press ENTER after stopping app pool to continue..."
    }

    # ── STEP 4: XCOPY TO QA SERVER ────────────────────────────────────────────
    Write-Step "Copying files to QA server ($QADeployPath)..."
    robocopy $PublishOutput $QADeployPath /MIR /NFL /NDL /NJH /NJS
    if ($LASTEXITCODE -gt 7) { Write-Fail "File copy failed (robocopy exit: $LASTEXITCODE)."; exit 1 }
    Write-Success "Files copied."

    # ── STEP 5: START IIS APP POOL ────────────────────────────────────────────
    Write-Step "Starting IIS App Pool '$IISAppPool'..."
    try {
        Invoke-Command -ComputerName $QAServerIP -ScriptBlock {
            param($pool)
            Import-Module WebAdministration
            Start-WebAppPool -Name $pool
        } -ArgumentList $IISAppPool
        Write-Success "App pool started."
    } catch {
        Write-Host "    [WARN] Could not start app pool remotely: $_" -ForegroundColor Yellow
        Read-Host "    Press ENTER after starting app pool to continue..."
    }

    # ── STEP 6: WAIT FOR APP TO BE READY ─────────────────────────────────────
    Write-Step "Waiting for QA server to be ready..."
    $healthUrl = "$QABaseUrl/health"
    $maxWait   = 30
    $waited    = 0
    $ready     = $false

    while ($waited -lt $maxWait) {
        try {
            $response = Invoke-WebRequest -Uri $healthUrl -UseBasicParsing -TimeoutSec 3 -ErrorAction Stop
            if ($response.StatusCode -lt 500) { $ready = $true; break }
        } catch { }
        Start-Sleep -Seconds 2
        $waited += 2
        Write-Host "    Waiting... ($waited/$maxWait s)" -ForegroundColor Gray
    }

    if ($ready) {
        Write-Success "QA server is ready."
    } else {
        Write-Host "    [WARN] Health check timed out. Proceeding anyway..." -ForegroundColor Yellow
    }
}

if ($DeployOnly) {
    Write-Host "`nDeploy complete. Tests skipped (-DeployOnly)." -ForegroundColor Cyan
    exit 0
}

# ── STEP 7: UPDATE appsettings.QA.json ───────────────────────────────────────
Write-Step "Configuring QA test settings..."
$qaSettingsPath = "$SolutionRoot\src\tests\UserManagement.QATests\appsettings.QA.json"
$qaSettings     = Get-Content $qaSettingsPath | ConvertFrom-Json

if ($Local) {
    $targetUrl = "http://localhost:5239"
    Write-Host "    [INFO] Running against LOCAL server" -ForegroundColor Yellow
} else {
    $targetUrl = $QABaseUrl
    Write-Host "    [INFO] Running against QA SERVER ($QABaseUrl)" -ForegroundColor Cyan
}

$qaSettings.QAServer.BaseUrl = $targetUrl
$qaSettings | ConvertTo-Json -Depth 5 | Set-Content $qaSettingsPath -Encoding utf8
Write-Success "BaseUrl set to $targetUrl"

# ── STEP 8: RUN QA TESTS ──────────────────────────────────────────────────────
Write-Step "Running QA tests..."

if (-not (Test-Path $TestResultsDir)) { New-Item -ItemType Directory -Path $TestResultsDir | Out-Null }

$timestamp  = Get-Date -Format "yyyyMMdd_HHmmss"
$trxFile    = "$TestResultsDir\AccessPolicy_$timestamp.trx"
$htmlFile   = "$TestResultsDir\AccessPolicy_$timestamp.html"

dotnet test $QATestProject `
    --logger "html;LogFileName=$htmlFile" `
    --logger "console;verbosity=normal" `
    --results-directory $TestResultsDir

$testExitCode = $LASTEXITCODE

# ── STEP 9: OPEN HTML REPORT IN BROWSER ──────────────────────────────────────
Write-Step "Opening HTML report..."
if (Test-Path $htmlFile) {
    Write-Success "HTML report: $htmlFile"
    Start-Process $htmlFile   # opens in default browser automatically
} else {
    Write-Host "    [WARN] HTML report not found at $htmlFile" -ForegroundColor Yellow
}

# ── STEP 10: SUMMARY ──────────────────────────────────────────────────────────
Write-Host "`n============================================" -ForegroundColor White
if ($testExitCode -eq 0) {
    Write-Host "  QA TESTS PASSED" -ForegroundColor Green
} else {
    Write-Host "  QA TESTS FAILED — review results above" -ForegroundColor Red
}
Write-Host "  Results: $TestResultsDir" -ForegroundColor White
Write-Host "  TRX    : $trxFile" -ForegroundColor White
Write-Host "============================================`n" -ForegroundColor White

exit $testExitCode
