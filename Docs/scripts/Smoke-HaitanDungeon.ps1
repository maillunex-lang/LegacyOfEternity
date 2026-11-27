# Smoke: Haitan offline dungeon catalog + asset wiring (no Unity Editor required).
$ErrorActionPreference = 'Stop'
$root = 'C:\LegacyOfEternity'
$fail = 0

function Ok($m) { Write-Host "OK  $m" }
function Bad($m) { Write-Host "FAIL $m"; $script:fail++ }

$dungeonJson = Join-Path $root 'Client\Assets\Resources\OriginalMap\HaitanDungeons.json'
$questsJson  = Join-Path $root 'Client\Assets\Resources\OriginalMap\HaitanQuests.json'
$modelsJson  = Join-Path $root 'Client\Assets\Resources\OriginalMap\HaitanModels.json'
$cs          = Join-Path $root 'Client\Assets\LegacyOfEternity\Scripts\Offline\OfflineQuestDungeon.cs'
$hud         = Join-Path $root 'Client\Assets\LegacyOfEternity\Scripts\Offline\OfflineInteractHud.cs'
$boot        = Join-Path $root 'Client\Assets\LegacyOfEternity\Scripts\World\OriginalHaitanBootstrap.cs'

foreach ($p in @($dungeonJson,$questsJson,$modelsJson,$cs,$hud,$boot)) {
  if (Test-Path $p) { Ok "exists $($p.Substring($root.Length+1))" } else { Bad "missing $p" }
}

$d = Get-Content $dungeonJson -Raw | ConvertFrom-Json
$ids = @($d.dungeons | ForEach-Object { $_.id })
$need = @(11,12,22,31,30)
foreach ($id in $need) {
  if ($ids -contains $id) { Ok "dungeon id=$id" } else { Bad "dungeon id=$id missing" }
}

$q = Get-Content $questsJson -Raw | ConvertFrom-Json
$dungeonQuests = @($q.quests | Where-Object { $_.kind -eq 14 -or $_.kind -eq 17 })
Ok "dungeon/mount quests=$($dungeonQuests.Count)"
foreach ($dq in $dungeonQuests) {
  $did = [int]$dq.dungeonId
  if ($ids -contains $did) { Ok "quest $($dq.id) -> dungeon $did" }
  else { Bad "quest $($dq.id) dungeonId=$did not in catalog" }
}

$m = Get-Content $modelsJson -Raw | ConvertFrom-Json
foreach ($mid in @(880211004, 880211008)) {
  $row = $m.models | Where-Object { $_.modelId -eq $mid }
  if ($null -eq $row) { Bad "modelId $mid missing"; continue }
  $prefab = Join-Path $root ("Client\Assets\Resources\" + ($row.resourcesPath -replace '/','\') + '.prefab')
  if (Test-Path $prefab) { Ok "prefab $mid -> $($row.resourcesPath)" } else { Bad "prefab missing $prefab" }
}

foreach ($tid in @(100101201,100101303,100102203,100103201)) {
  $st = $m.monsterStats | Where-Object { $_.id -eq $tid }
  if ($null -ne $st) { Ok "monsterStats $tid $($st.name)" } else { Bad "monsterStats $tid missing" }
}

$csText = Get-Content $cs -Raw
foreach ($needle in @('TryEnter','ApplyQuestEvent','CombatEnemy.Killed','HaitanDungeons','LookModelFor')) {
  if ($csText -match [regex]::Escape($needle)) { Ok "OfflineQuestDungeon has $needle" }
  else { Bad "OfflineQuestDungeon missing $needle" }
}

$hudText = Get-Content $hud -Raw
if ($hudText -match 'OfflineQuestDungeon\.TryEnter') { Ok 'InteractHud wires TryEnter' } else { Bad 'InteractHud missing TryEnter' }
$bootText = Get-Content $boot -Raw
if ($bootText -match 'OfflineQuestDungeon\.Ensure') { Ok 'Bootstrap Ensure dungeon' } else { Bad 'Bootstrap missing Ensure' }

$out = Join-Path $root 'Docs\haitan-dungeon-smoke.txt'
@(
  "Haitan dungeon smoke $(Get-Date -Format o)"
  "dungeons=$($ids -join ',')"
  "dungeonQuests=$($dungeonQuests.Count)"
  "fail=$fail"
) | Set-Content $out -Encoding UTF8
Ok "wrote $out"

if ($fail -gt 0) { Write-Host "SMOKE FAIL count=$fail"; exit 1 }
Write-Host 'SMOKE PASS'
exit 0
