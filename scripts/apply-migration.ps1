[CmdletBinding()]
param(
    [string]$ConnectionString
)

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
$apiProject = Join-Path $repoRoot "backend\src\BolaoCopa.Api\BolaoCopa.Api.csproj"
$infrastructureProject = Join-Path $repoRoot "backend\src\BolaoCopa.Infrastructure\BolaoCopa.Infrastructure.csproj"

function Invoke-CheckedCommand {
    param(
        [Parameter(Mandatory = $true)]
        [string]$FilePath,

        [Parameter(ValueFromRemainingArguments = $true)]
        [string[]]$Arguments
    )

    & $FilePath @Arguments

    if ($LASTEXITCODE -ne 0) {
        throw "Command failed with exit code ${LASTEXITCODE}: $FilePath $($Arguments -join ' ')"
    }
}

Push-Location $repoRoot

try {
    if ($ConnectionString) {
        $env:ConnectionStrings__DefaultConnection = $ConnectionString
    }

    Invoke-CheckedCommand dotnet ef database update --project $infrastructureProject --startup-project $apiProject
}
finally {
    Pop-Location
}
