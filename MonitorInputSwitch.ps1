[CmdletBinding(DefaultParameterSetName = 'Hotkey')]
param(
    [Parameter(ParameterSetName = 'List', Mandatory)]
    [switch]$List,

    [Parameter(ParameterSetName = 'Select', Mandatory)]
    [ValidateSet('Laptop', 'Desktop')]
    [string]$Target,

    [string]$ConfigPath
)

$ErrorActionPreference = 'Stop'

try {
    $sourceFiles = @(
        (Join-Path $PSScriptRoot 'src\DisplayInput.cs')
        (Join-Path $PSScriptRoot 'src\InputProfiles.cs')
        (Join-Path $PSScriptRoot 'src\HotkeyApp.cs')
    )
    if (-not ('MonitorInputSwitch.HotkeyApp' -as [type])) {
        Add-Type -Path $sourceFiles -ReferencedAssemblies System, System.Core, System.Drawing, System.Windows.Forms
    }

    if ($List) {
        [MonitorInputSwitch.InputProfiles]::ListInputs()
        return
    }

    if (-not $ConfigPath) {
        $ConfigPath = Join-Path $PSScriptRoot 'profiles.json'
    }

    $config = Get-Content -LiteralPath $ConfigPath -Raw | ConvertFrom-Json
    if (-not $config.Monitors) {
        throw 'Configure at least one monitor in profiles.json. See profiles.example.json.'
    }

    $modelIds = @{}
    $routes = foreach ($monitor in $config.Monitors) {
        if ([string]::IsNullOrWhiteSpace($monitor.Name) -or $monitor.ModelId -notmatch '^[A-Z]{3}[0-9A-F]{4}$') {
            throw 'Each monitor needs a name and the model ID reported by -List.'
        }
        if ($modelIds.ContainsKey($monitor.ModelId)) {
            throw "Model ID $($monitor.ModelId) is configured more than once."
        }

        foreach ($profile in 'Laptop', 'Desktop') {
            $value = $monitor.$profile
            if ($value -isnot [int] -or $value -lt 0 -or $value -gt 255) {
                throw "Use an integer from 0 to 255 for $($monitor.Name)'s $profile input."
            }
        }
        if ($monitor.Laptop -eq $monitor.Desktop) {
            throw "The Laptop and Desktop inputs must differ for $($monitor.Name)."
        }

        $modelIds[$monitor.ModelId] = $true
        [MonitorInputSwitch.MonitorRoute]::new($monitor.Name, $monitor.ModelId, $monitor.Laptop, $monitor.Desktop)
    }
    $profiles = [MonitorInputSwitch.InputProfiles]::new([MonitorInputSwitch.MonitorRoute[]]@($routes))

    if ($Target) {
        $profiles.Select([MonitorInputSwitch.Profile]$Target)
        Write-Output "Verified profile: $Target"
        return
    }

    [MonitorInputSwitch.HotkeyApp]::Run($profiles)
}
catch {
    if ('MonitorInputSwitch.SwitchLog' -as [type]) {
        [MonitorInputSwitch.SwitchLog]::Write($_.Exception.GetBaseException().Message)
    }
    throw
}
