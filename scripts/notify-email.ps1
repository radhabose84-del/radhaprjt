# =============================================================================
# BSOFT QA — Email Notification (Zimbra SMTP)
# Called by run-all-qa-tests.ps1 when tests fail
#
# Usage:
#   .\notify-email.ps1 -FailedTests $failedList -RunDate "2026-05-29 15:30"
# =============================================================================

param(
    [Parameter(Mandatory)] [array]  $FailedTests,
    [Parameter(Mandatory)] [string] $RunDate,
    [string] $ConfigFile = "$PSScriptRoot\..\src\tests\qa-config.local.json"
)

# ── Load config ───────────────────────────────────────────────────────────────
if (-not (Test-Path $ConfigFile)) {
    Write-Host "[Email] Config file not found: $ConfigFile" -ForegroundColor Yellow
    return
}

$config = Get-Content $ConfigFile | ConvertFrom-Json
$email  = $config.Email
$qa     = $config.QAServer

if ($email.Password -like "FILL_IN*") {
    Write-Host "[Email] Password not configured in qa-config.local.json — skipping email." -ForegroundColor Yellow
    return
}

# ── Build email body ──────────────────────────────────────────────────────────
$failCount   = $FailedTests.Count
$subject     = "❌ [QA FAIL] BSOFT — $failCount test(s) failed ($RunDate)"

$failDetails = $FailedTests | ForEach-Object {
    "  ❌ $($_.Module) / $($_.Entity)`n     $($_.TestName)`n     Expected: $($_.Expected)  Actual: $($_.Actual)`n"
}

$body = @"
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
BSOFT QA Automation — Test Failure Report
Date       : $RunDate
Environment: $($qa.Environment) ($($qa.BaseUrl))
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

FAILED TESTS ($failCount):

$($failDetails -join "`n")

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
JIRA bugs have been created automatically.
Check SCRUM backlog for: [QA AUTO] label
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
"@

# ── Send email via Zimbra SMTP ────────────────────────────────────────────────
try {
    $smtpClient = New-Object System.Net.Mail.SmtpClient($email.SmtpServer, $email.SmtpPort)
    $smtpClient.EnableSsl = $email.EnableSsl
    $smtpClient.Credentials = New-Object System.Net.NetworkCredential(
        $email.Username, $email.Password)

    $mailMessage = New-Object System.Net.Mail.MailMessage
    $mailMessage.From    = New-Object System.Net.Mail.MailAddress(
        $email.FromEmail, $email.FromName)
    $mailMessage.Subject = $subject
    $mailMessage.Body    = $body

    foreach ($to in $email.ToEmails) {
        $mailMessage.To.Add($to)
    }

    $smtpClient.Send($mailMessage)
    Write-Host "[Email] ✅ Sent to: $($email.ToEmails -join ', ')" -ForegroundColor Green
}
catch {
    Write-Host "[Email] ❌ Failed to send: $_" -ForegroundColor Red
}
