# =============================================================================
# BSOFT QA — Slack Notification
# Called by run-all-qa-tests.ps1 when tests fail
#
# Usage:
#   .\notify-slack.ps1 -FailedTests $failedList -RunDate "2026-05-29 15:30"
# =============================================================================

param(
    [Parameter(Mandatory)] [array]  $FailedTests,
    [Parameter(Mandatory)] [string] $RunDate,
    [string] $ConfigFile = "$PSScriptRoot\..\src\tests\qa-config.local.json"
)

# ── Load config ───────────────────────────────────────────────────────────────
if (-not (Test-Path $ConfigFile)) {
    Write-Host "[Slack] Config file not found: $ConfigFile" -ForegroundColor Yellow
    return
}

$config  = Get-Content $ConfigFile | ConvertFrom-Json
$slack   = $config.Slack
$qa      = $config.QAServer

if ($slack.WebhookUrl -like "FILL_IN*") {
    Write-Host "[Slack] Webhook URL not configured — skipping Slack notification." -ForegroundColor Yellow
    return
}

# ── Build Slack message ───────────────────────────────────────────────────────
$failCount = $FailedTests.Count

$failBlocks = $FailedTests | ForEach-Object {
    @{
        type = "section"
        text = @{
            type = "mrkdwn"
            text = "❌ *$($_.Module) / $($_.Entity)*`n`t`t`tTest: `$($_.TestName)`n`t`t`tExpected: $($_.Expected)  |  Actual: $($_.Actual)"
        }
    }
}

$payload = @{
    username   = $slack.BotName
    icon_emoji = ":red_circle:"
    channel    = $slack.Channel
    blocks     = @(
        @{
            type = "header"
            text = @{
                type  = "plain_text"
                text  = "❌ BSOFT QA Tests FAILED"
                emoji = $true
            }
        },
        @{
            type = "section"
            fields = @(
                @{ type = "mrkdwn"; text = "*Date:*`n$RunDate" },
                @{ type = "mrkdwn"; text = "*Failed:*`n$failCount test(s)" },
                @{ type = "mrkdwn"; text = "*Environment:*`n$($qa.Environment)" },
                @{ type = "mrkdwn"; text = "*Server:*`n$($qa.BaseUrl)" }
            )
        },
        @{ type = "divider" }
    ) + $failBlocks + @(
        @{ type = "divider" },
        @{
            type = "section"
            text = @{
                type = "mrkdwn"
                text = "🐛 JIRA bugs created automatically — check SCRUM backlog for `[QA AUTO]` label"
            }
        }
    )
} | ConvertTo-Json -Depth 10

# ── Send to Slack ─────────────────────────────────────────────────────────────
try {
    Invoke-RestMethod -Uri $slack.WebhookUrl `
        -Method Post `
        -ContentType "application/json" `
        -Body $payload

    Write-Host "[Slack] ✅ Message sent to $($slack.Channel)" -ForegroundColor Green
}
catch {
    Write-Host "[Slack] ❌ Failed to send: $_" -ForegroundColor Red
}
