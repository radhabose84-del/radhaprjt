# run-all-qa.ps1 — Run every *.QATests assembly STRICTLY ONE AT A TIME.
#
# Why this exists: the QA suites all authenticate as the single `testsales` account
# (single-session-per-user). If two QA assemblies are live at once, they force-
# deactivate each other's session → mid-run 401 "Session is invalid or expired" and
# 400 on the login call. `qa.runsettings` (MaxCpuCount=1) only serializes assemblies
# WITHIN one `dotnet test` call; it does nothing when assemblies run as separate
# processes — and the IDE Test Explorer runs them in PARALLEL by default, which is the
# usual cause of the 401 storm. This script runs them sequentially in ONE process so
# only one `testsales` session is ever active.
#
# PREREQUISITES (do these first, in this order):
#   1. API up against the clone:   .\run-qa.ps1   (separate terminal, leave running)
#   2. Seed baseline data:         .\qa-seed.ps1  (once, after the API is up)
#   3. Close the IDE Test Explorer / stop any other `dotnet test`. Nothing else may
#      authenticate as testsales while this runs.
#
# USAGE:   .\run-all-qa.ps1                 # all QA suites, smoke+full
#          .\run-all-qa.ps1 -Smoke          # only the Layer=Smoke slice (fast gate)
#          .\run-all-qa.ps1 -Filter Finance # only assemblies whose path matches

param(
    [switch] $Smoke,
    [string] $Filter = ''
)

$ErrorActionPreference = 'Continue'
Set-Location $PSScriptRoot

$projects = Get-ChildItem -Path 'src/tests' -Recurse -Filter '*.QATests.csproj' |
    Where-Object { $_.FullName -like "*$Filter*" } |
    Sort-Object FullName

if (-not $projects) { Write-Host "No *.QATests.csproj matched filter '$Filter'." -ForegroundColor Yellow; exit 1 }

$results = @()
foreach ($p in $projects) {
    $name = $p.Directory.Name
    Write-Host "`n=== $name ===" -ForegroundColor Cyan
    $dargs = @('test', $p.FullName, '--nologo')
    if ($Smoke) { $dargs += @('--filter', 'Layer=Smoke') }
    & dotnet @dargs
    $results += [pscustomobject]@{ Suite = $name; ExitCode = $LASTEXITCODE }
}

Write-Host "`n================ SUMMARY ================" -ForegroundColor Cyan
foreach ($r in $results) {
    $ok = $r.ExitCode -eq 0
    Write-Host ("{0,-34} {1}" -f $r.Suite, $(if ($ok) { 'PASS' } else { "FAIL ($($r.ExitCode))" })) `
        -ForegroundColor $(if ($ok) { 'Green' } else { 'Red' })
}
$failed = ($results | Where-Object { $_.ExitCode -ne 0 }).Count
exit $(if ($failed -gt 0) { 1 } else { 0 })
