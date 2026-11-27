# Фактический статус: 27.11.2026

**APK:** код Haitan доведён до offline-паритета цепочки; сборка — после вашей явной команды / приёмки в Editor.

## Готово в коде

| Слой | Статус |
| --- | --- |
| Сцена / MapData / modelId | Готово |
| 99 квестов + RU диалоги + kind 3/7/8/9/14/17 | Готово |
| Accept / finish talk | `acceptStoryId` / `finishStoryId` + intro после autoAccept |
| Offline fuben 14/17 | `OfflineQuestDungeon` + BOSS_004/008 |
| NPC / gather имена | RU (`Тиас`, `Оружие по руке`, …) |
| Prefab deps + strip + sanitize | Готово (+ fallback при null `_MainTex`) |
| Collection wuqi/xiakucao | mesh GUID нет в источниках → `emptything` |

## Перед APK (приёмка)

1. Editor Play: вход → Haitan → Тиас → цепочка квестов / данж кнопка.
2. Визуально: нет критичного magenta на ключевых NPC/мобах.
3. Ваша команда «собирай APK».
