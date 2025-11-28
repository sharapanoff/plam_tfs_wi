<#
.SYNOPSIS
    ReadOnlyAudit.ps1 - Verifies TFS Viewer application uses only read-only operations

.DESCRIPTION
    This script analyzes the TFS Viewer codebase to ensure it only uses read-only
    TFS operations and does not perform any create, update, or delete operations.
    This validates Success Criterion SC-010: Application is strictly read-only.

.PARAMETER SourcePath
    Path to the source code directory. Defaults to current directory.

.PARAMETER ReportPath
    Path where the audit report will be saved. Defaults to "ReadOnlyAudit-Report.txt"

.EXAMPLE
    .\ReadOnlyAudit.ps1

.EXAMPLE
    .\ReadOnlyAudit.ps1 -SourcePath "C:\Projects\TfsViewer" -ReportPath "audit.txt"
#>

param(
    [string]$SourcePath = ".",
    [string]$ReportPath = "ReadOnlyAudit-Report.txt"
)

# TFS API endpoints that indicate write operations
$WriteEndpoints = @(
    "POST.*workitems",
    "PATCH.*workitems",
    "PUT.*workitems",
    "DELETE.*workitems",
    "POST.*pullRequests",
    "PATCH.*pullRequests",
    "PUT.*pullRequests",
    "DELETE.*pullRequests",
    "POST.*codeReviews",
    "PATCH.*codeReviews",
    "PUT.*codeReviews",
    "DELETE.*codeReviews",
    "POST.*_apis/wit/workitems/.*",
    "PATCH.*_apis/wit/workitems/.*",
    "PUT.*_apis/wit/workitems/.*",
    "DELETE.*_apis/wit/workitems/.*"
)

# TFS API endpoints that are read-only (allowed)
$ReadOnlyEndpoints = @(
    "GET.*workitems",
    "GET.*pullRequests",
    "GET.*pullrequests",
    "GET.*codeReviews",
    "GET.*codereviews",
    "POST.*_apis/wit/wiql",  # WIQL queries are read-only
    "GET.*_apis/connectionData",
    "GET.*_apis/projects",
    "GET.*_apis/git/repositories"
)

function Test-ReadOnlyCompliance {
    param([string]$FilePath)

    $content = Get-Content $FilePath -Raw -ErrorAction SilentlyContinue
    if ($null -eq $content) { return $null }

    $violations = @()
    $compliantEndpoints = @()

    # Check for write endpoints
    foreach ($endpoint in $WriteEndpoints) {
        if ($content -match $endpoint) {
            $violations += "Found potential write operation: $endpoint in $FilePath"
        }
    }

    # Check for read-only endpoints (these are OK)
    foreach ($endpoint in $ReadOnlyEndpoints) {
        if ($content -match $endpoint) {
            $compliantEndpoints += $endpoint
        }
    }

    return @{
        File = $FilePath
        Violations = $violations
        CompliantEndpoints = $compliantEndpoints
        IsCompliant = ($violations.Count -eq 0)
    }
}

function New-AuditReport {
    param([array]$Results)

    $totalFiles = $Results.Count
    $compliantFiles = ($Results | Where-Object { $_.IsCompliant }).Count
    $violatingFiles = $totalFiles - $compliantFiles
    $totalViolations = ($Results | ForEach-Object { $_.Violations.Count } | Measure-Object -Sum).Sum

    $report = @"
TFS Viewer Read-Only Audit Report
==================================

Generated: $(Get-Date)
Source Path: $SourcePath

SUMMARY
-------
Total Files Analyzed: $totalFiles
Compliant Files: $compliantFiles
Files with Violations: $violatingFiles
Total Violations: $totalViolations

OVERALL STATUS: $(if ($violatingFiles -eq 0) { "PASS - Application is read-only" } else { "FAIL - Write operations detected" })

DETAILS
-------

"@

    foreach ($result in $Results) {
        if (-not $result.IsCompliant) {
            $report += "`nVIOLATIONS in $($result.File):`n"
            foreach ($violation in $result.Violations) {
                $report += "  - $violation`n"
            }
        }
    }

    $report += "`nCOMPLIANT ENDPOINTS FOUND:`n"
    $uniqueEndpoints = $Results | ForEach-Object { $_.CompliantEndpoints } | Select-Object -Unique | Sort-Object
    foreach ($endpoint in $uniqueEndpoints) {
        $report += "  - $endpoint`n"
    }

    return $report
}

# Main execution
Write-Host "Starting TFS Viewer Read-Only Audit..." -ForegroundColor Cyan
Write-Host "Source Path: $SourcePath" -ForegroundColor Gray
Write-Host "Report Path: $ReportPath" -ForegroundColor Gray
Write-Host ""

# Find all relevant source files
$sourceFiles = Get-ChildItem -Path $SourcePath -Recurse -Include "*.cs","*.xaml","*.config" -ErrorAction SilentlyContinue |
    Where-Object { $_.FullName -notmatch "\\bin\\" -and $_.FullName -notmatch "\\obj\\" -and $_.FullName -notmatch "\\packages\\" }

Write-Host "Found $($sourceFiles.Count) source files to analyze..." -ForegroundColor Yellow

$results = @()
$processedCount = 0

foreach ($file in $sourceFiles) {
    $result = Test-ReadOnlyCompliance -FilePath $file.FullName
    if ($result) {
        $results += $result
    }

    $processedCount++
    if ($processedCount % 10 -eq 0) {
        Write-Progress -Activity "Analyzing files" -Status "$processedCount / $($sourceFiles.Count)" -PercentComplete (($processedCount / $sourceFiles.Count) * 100)
    }
}

Write-Progress -Activity "Analyzing files" -Completed

# Generate report
$report = New-AuditReport -Results $results
$report | Out-File -FilePath $ReportPath -Encoding UTF8

Write-Host "Audit complete!" -ForegroundColor Green
Write-Host "Report saved to: $ReportPath" -ForegroundColor Green

# Display summary
$violatingFiles = ($results | Where-Object { -not $_.IsCompliant }).Count
if ($violatingFiles -eq 0) {
    Write-Host "✅ PASS: No write operations detected. Application is read-only." -ForegroundColor Green
} else {
    Write-Host "❌ FAIL: $violatingFiles files contain potential write operations." -ForegroundColor Red
    Write-Host "See $ReportPath for details." -ForegroundColor Yellow
}

exit $violatingFiles