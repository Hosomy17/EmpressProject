param(
    [ValidateSet('Recompile', 'EditModeTests', 'PlayModeTests', 'ExecuteMethod')]
    [string]$Mode = 'Recompile',

    [string]$Method = '',

    [string]$LogFile = '',

    [string]$TestResults = '',

    [string]$ProjectPath = ''
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

if ([string]::IsNullOrWhiteSpace($ProjectPath)) {
    $ProjectPath = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
}

function Resolve-UnityExecutable {
    if (-not [string]::IsNullOrWhiteSpace($env:UNITY_EDITOR_PATH) -and (Test-Path $env:UNITY_EDITOR_PATH)) {
        return $env:UNITY_EDITOR_PATH
    }

    $exactPath = Join-Path $env:ProgramFiles 'Unity\Hub\Editor\6000.3.10f1\Editor\Unity.exe'
    if (Test-Path $exactPath) {
        return $exactPath
    }

    $hubDir = Join-Path $env:ProgramFiles 'Unity\Hub\Editor'
    if (Test-Path $hubDir) {
        $candidates = Get-ChildItem -Path $hubDir -Directory | Sort-Object Name -Descending
        foreach ($dir in $candidates) {
            $exe = Join-Path $dir.FullName 'Editor\Unity.exe'
            if (Test-Path $exe) {
                Write-Warning "Project targets Unity 6000.3.10f1, using detected version '$($dir.Name)' at '$exe'."
                return $exe
            }
        }
    }

    Write-Error "Unity Editor executable not found. Set UNITY_EDITOR_PATH or install Unity 6000.3.10f1."
    exit 1
}

$unityExe = Resolve-UnityExecutable

$logsDir = Join-Path $ProjectPath 'Logs'
if (-not (Test-Path $logsDir)) {
    New-Item -ItemType Directory -Path $logsDir -Force | Out-Null
}

if ([string]::IsNullOrWhiteSpace($LogFile)) {
    $LogFile = switch ($Mode) {
        'Recompile'     { Join-Path $ProjectPath 'Logs\Recompile.log' }
        'EditModeTests' { Join-Path $ProjectPath 'Logs\EditMode.log' }
        'PlayModeTests' { Join-Path $ProjectPath 'Logs\PlayMode.log' }
        'ExecuteMethod' { Join-Path $ProjectPath 'Logs\BatchExecute.log' }
    }
}

$unityArgs = @(
    '-batchmode',
    '-nographics',
    '-projectPath', $ProjectPath
)

switch ($Mode) {
    'Recompile' {
        $unityArgs += '-quit'
    }
    'EditModeTests' {
        if ([string]::IsNullOrWhiteSpace($TestResults)) {
            $TestResults = Join-Path $ProjectPath 'Logs\EditMode-Results.xml'
        }
        $unityArgs += @('-runTests', '-testPlatform', 'editmode', '-testResults', $TestResults)
    }
    'PlayModeTests' {
        if ([string]::IsNullOrWhiteSpace($TestResults)) {
            $TestResults = Join-Path $ProjectPath 'Logs\PlayMode-Results.xml'
        }
        $unityArgs += @('-runTests', '-testPlatform', 'playmode', '-testResults', $TestResults)
    }
    'ExecuteMethod' {
        if ([string]::IsNullOrWhiteSpace($Method)) {
            Write-Error "-Method is required when -Mode ExecuteMethod is selected."
            exit 1
        }
        $unityArgs += @('-executeMethod', $Method, '-quit')
    }
}

$unityArgs += @('-logFile', $LogFile)

& $unityExe @unityArgs
exit $LASTEXITCODE
