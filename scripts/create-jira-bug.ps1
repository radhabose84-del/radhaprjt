# =============================================================================
# BSOFT QA — Auto Create JIRA Bug per Failed Test
# Called by run-all-qa-tests.ps1 when tests fail
#
# Usage:
#   .\create-jira-bug.ps1 -FailedTests $failedList -RunDate "2026-05-29 15:30"
# =============================================================================

param(
    [Parameter(Mandatory)] [array]  $FailedTests,
    [Parameter(Mandatory)] [string] $RunDate,
    [string] $ConfigFile = "$PSScriptRoot\..\src\tests\qa-config.local.json"
)

# ── Load config ───────────────────────────────────────────────────────────────
if (-not (Test-Path $ConfigFile)) {
    Write-Host "[JIRA] Config file not found: $ConfigFile" -ForegroundColor Yellow
    return
}

$config = Get-Content $ConfigFile | ConvertFrom-Json
$jira   = $config.Jira
$qa     = $config.QAServer

if ($jira.ApiToken -like "FILL_IN*") {
    Write-Host "[JIRA] API token not configured — skipping bug creation." -ForegroundColor Yellow
    return
}

# ── Auth header ───────────────────────────────────────────────────────────────
$authBytes  = [System.Text.Encoding]::ASCII.GetBytes("$($jira.Email):$($jira.ApiToken)")
$authBase64 = [Convert]::ToBase64String($authBytes)
$headers    = @{
    "Authorization" = "Basic $authBase64"
    "Content-Type"  = "application/json"
    "Accept"        = "application/json"
}

$createdKeys = @()

foreach ($test in $FailedTests) {

    $summary = "[QA AUTO] $($test.Module) — $($test.Entity): $($test.TestName) FAILED"

    # ── Check for existing open bug (avoid duplicates) ────────────────────────
    $jqlQuery   = [Uri]::EscapeDataString(
        "project = $($jira.ProjectKey) AND summary ~ `"$summary`" AND status != Done")
    $searchUrl  = "$($jira.BaseUrl)/rest/api/3/issue/picker?currentJQL=$jqlQuery"

    $existing = $null
    try {
        $searchResp = Invoke-RestMethod -Uri "$($jira.BaseUrl)/rest/api/3/issue/picker" `
            -Method Get -Headers $headers `
            -Body @{ currentJQL = "project = $($jira.ProjectKey) AND summary ~ `"[QA AUTO] $($test.Module) — $($test.Entity): $($test.TestName)`" AND status != Done" }
    } catch { }

    # ── Build issue payload ───────────────────────────────────────────────────
    $description = @{
        version = 1
        type    = "doc"
        content = @(
            @{
                type    = "heading"
                attrs   = @{ level = 3 }
                content = @(@{ type = "text"; text = "🤖 Auto-created by BSOFT QA Automation Pipeline" })
            },
            @{
                type    = "paragraph"
                content = @(
                    @{ type = "text"; text = "Module    : "; marks = @(@{ type = "strong" }) },
                    @{ type = "text"; text = $test.Module },
                    @{ type = "hardBreak" },
                    @{ type = "text"; text = "Entity    : "; marks = @(@{ type = "strong" }) },
                    @{ type = "text"; text = $test.Entity },
                    @{ type = "hardBreak" },
                    @{ type = "text"; text = "Test Case : "; marks = @(@{ type = "strong" }) },
                    @{ type = "text"; text = $test.TestName },
                    @{ type = "hardBreak" },
                    @{ type = "text"; text = "Expected  : "; marks = @(@{ type = "strong" }) },
                    @{ type = "text"; text = $test.Expected },
                    @{ type = "hardBreak" },
                    @{ type = "text"; text = "Actual    : "; marks = @(@{ type = "strong" }) },
                    @{ type = "text"; text = $test.Actual }
                )
            },
            @{
                type    = "paragraph"
                content = @(
                    @{ type = "hardBreak" },
                    @{ type = "text"; text = "Environment : "; marks = @(@{ type = "strong" }) },
                    @{ type = "text"; text = "$($qa.Environment) ($($qa.BaseUrl))" },
                    @{ type = "hardBreak" },
                    @{ type = "text"; text = "Date        : "; marks = @(@{ type = "strong" }) },
                    @{ type = "text"; text = $RunDate }
                )
            },
            @{
                type    = "heading"
                attrs   = @{ level = 4 }
                content = @(@{ type = "text"; text = "Steps to Reproduce" })
            },
            @{
                type    = "orderedList"
                content = @(
                    @{ type = "listItem"; content = @(@{ type = "paragraph"; content = @(@{ type = "text"; text = "Deploy to QA server ($($qa.BaseUrl))" }) }) },
                    @{ type = "listItem"; content = @(@{ type = "paragraph"; content = @(@{ type = "text"; text = "Run: dotnet test $($test.Module).QATests --filter `"$($test.TestName)`"" }) }) },
                    @{ type = "listItem"; content = @(@{ type = "paragraph"; content = @(@{ type = "text"; text = "Observe test failure" }) }) }
                )
            }
        )
    }

    $issuePayload = @{
        fields = @{
            project     = @{ key = $jira.ProjectKey }
            summary     = $summary
            description = $description
            issuetype   = @{ name = $jira.IssueType }
            priority    = @{ name = $jira.Priority }
            labels      = @($jira.Label, $test.Module)
            environment = "$($qa.Environment) — $($qa.BaseUrl)"
        }
    } | ConvertTo-Json -Depth 15

    # ── Create JIRA bug ───────────────────────────────────────────────────────
    try {
        $response = Invoke-RestMethod `
            -Uri "$($jira.BaseUrl)/rest/api/3/issue" `
            -Method Post `
            -Headers $headers `
            -Body $issuePayload

        $key = $response.key
        $createdKeys += $key
        Write-Host "[JIRA] ✅ Bug created: $key — $summary" -ForegroundColor Green
    }
    catch {
        Write-Host "[JIRA] ❌ Failed to create bug for $($test.TestName): $_" -ForegroundColor Red
    }
}

# Return created keys for use in email/slack notifications
return $createdKeys
