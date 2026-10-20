param([string]$GodotBin = "$PSScriptRoot/.tools/godot-4.7.2-win64/Godot_v4.7.2-stable_win64_console.exe")
$ErrorActionPreference = 'Stop'
$repoPath = $PSScriptRoot
$testRoot = Join-Path $repoPath '.tools/check-windows'
$logRoot = Join-Path $repoPath '.tools/logs'
New-Item -ItemType Directory -Force $testRoot,$logRoot,(Join-Path $testRoot 'Roaming'),(Join-Path $testRoot 'Local') | Out-Null
$oldAppData = $env:APPDATA
$oldLocalData = $env:LOCALAPPDATA
$oldTesting = $env:D_W_TESTING
function Invoke-Check([string]$Name, [string[]]$EngineArgs, [string]$Marker = '') {
    $logFile = Join-Path $logRoot "marco1-$Name.log"
    & $GodotBin --headless --path $repoPath --log-file $logFile @EngineArgs 2>&1 | Out-Host
    if ($LASTEXITCODE -ne 0) { throw "Godot falhou: $Name" }
    $logText = Get-Content -LiteralPath $logFile -Raw
    if ($logText -match 'SCRIPT ERROR:|Parse Error:|Assertion failed|ERROR:') { throw "Erro no log: $Name" }
    if ($Marker -and $logText -notmatch $Marker) { throw "Harness sem confirmação: $Name" }
}
try {
    $env:APPDATA = Join-Path $testRoot 'Roaming'
    $env:LOCALAPPDATA = Join-Path $testRoot 'Local'
    $env:D_W_TESTING = '1'
    Invoke-Check 'import' @('--editor','--import')
    Invoke-Check 'boot' @('--quit-after','5')
    Invoke-Check 'legacy-logic' @('--script','res://scripts/gameplay/logic_harness.gd','--quit-after','1800') 'LOGIC_OK'
    Invoke-Check 'legacy-save' @('--script','res://scripts/save/save_harness.gd','--quit-after','1800') 'SAVE_OK'
    Invoke-Check 'defense' @('--script','res://scripts/gameplay/defense_harness.gd','--quit-after','1800') 'DEFENSE_OK'
    Invoke-Check 'defense-save' @('--script','res://scripts/save/defense_save_harness.gd','--quit-after','1800') 'DEFENSE_SAVE_OK'
    Invoke-Check 'gameplay' @('--script','res://scripts/gameplay/game_harness.gd','--quit-after','1800') 'GAME_OK'
    Invoke-Check 'android-ui' @('--script','res://scripts/gameplay/android_harness.gd','--quit-after','1800') 'ANDROID_UI_OK'
    Invoke-Check 'characters' @('--script','res://scripts/visual/characters_harness.gd','--quit-after','1800') 'CHARACTERS_OK'
    Write-Output 'MARCO1_CHECKS_OK desktop_only=true'
} finally {
    $env:APPDATA = $oldAppData
    $env:LOCALAPPDATA = $oldLocalData
    $env:D_W_TESTING = $oldTesting
}
