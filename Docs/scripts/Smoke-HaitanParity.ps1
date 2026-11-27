# Smoke: Haitan RU names + dialogue talk modes + dungeon wiring.
$ErrorActionPreference = 'Stop'
$root = 'C:\LegacyOfEternity'
$fail = 0
function Ok($m){ Write-Host "OK  $m" }
function Bad($m){ Write-Host "FAIL $m"; $script:fail++ }

$models = Get-Content "$root\Client\Assets\Resources\OriginalMap\HaitanModels.json" -Raw -Encoding UTF8 | ConvertFrom-Json
$dlg = Get-Content "$root\Client\Assets\Resources\OriginalMap\HaitanDialogues.json" -Raw -Encoding UTF8 | ConvertFrom-Json
$quests = Get-Content "$root\Client\Assets\Resources\OriginalMap\HaitanQuests.json" -Raw -Encoding UTF8 | ConvertFrom-Json

$tias = $models.npcLooks | Where-Object { $_.npcId -eq 201001001 }
if ($tias.name -eq 'Тиас') { Ok 'NPC 201001001 = Тиас' } else { Bad "Tias name=$($tias.name)" }

$cols = @($models.collectionNames)
if ($cols.Count -ge 9) { Ok "collectionNames=$($cols.Count)" } else { Bad "collectionNames=$($cols.Count)" }
$c180 = $cols | Where-Object { $_.id -eq 180 }
if ($c180.name -match 'Оружие') { Ok "collection 180 $($c180.name)" } else { Bad "collection 180 missing" }

$q1 = $dlg.quests | Where-Object { $_.id -eq 1001701 }
if ($q1.finishStoryId -eq 101001 -and $q1.acceptStoryId -eq 101001) { Ok '1001701 finish/accept story' } else { Bad "1001701 stories a=$($q1.acceptStoryId) f=$($q1.finishStoryId)" }

$hq = $quests.quests | Where-Object { $_.id -eq 1001701 }
if ($null -ne $hq.finishTalkId) { Ok "HaitanQuests finishTalkId field=$($hq.finishTalkId)" } else { Bad 'finishTalkId missing on quests' }

$cs = @(
  'Client\Assets\LegacyOfEternity\Scripts\World\OriginalDialogueCatalog.cs',
  'Client\Assets\LegacyOfEternity\Scripts\Offline\OfflineDialogueHud.cs',
  'Client\Assets\LegacyOfEternity\Scripts\Offline\OfflineDemoStore.cs',
  'Client\Assets\LegacyOfEternity\Scripts\World\OriginalVisualSanitize.cs'
)
foreach ($rel in $cs) {
  $text = Get-Content (Join-Path $root $rel) -Raw
  if ($rel -match 'DialogueCatalog' -and $text -match 'QuestTalkMode' -and $text -match 'FinishStoryId') { Ok $rel }
  elseif ($rel -match 'DialogueHud' -and $text -match 'OpenAcceptIntro') { Ok $rel }
  elseif ($rel -match 'DemoStore' -and $text -match 'QueueAcceptIntro') { Ok $rel }
  elseif ($rel -match 'VisualSanitize' -and $text -match 'mainTexture') { Ok $rel }
  else { Bad $rel }
}

# dungeon smoke reuse
& "$root\Docs\scripts\Smoke-HaitanDungeon.ps1"
if ($LASTEXITCODE -ne 0) { $fail++ }

$out = Join-Path $root 'Docs\haitan-parity-smoke.txt'
"Haitan parity smoke $(Get-Date -Format o)`nfail=$fail" | Set-Content $out -Encoding UTF8
if ($fail -gt 0) { Write-Host "SMOKE FAIL count=$fail"; exit 1 }
Write-Host 'SMOKE PASS'
exit 0
