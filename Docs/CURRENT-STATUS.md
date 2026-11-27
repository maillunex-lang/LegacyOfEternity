# Фактический статус: 27.11.2026

**APK не собирать** до полной локации Haitan (указание владельца).

## Готово в коде

| Слой | Статус |
| --- | --- |
| Сцена / MapData / modelId | Готово |
| 99 квестов + RU диалоги + kind 3/7/8/9/14/17 | Готово |
| Prefab deps (mat/fbx/anim) для Haitan models | Скопированы GUID-safe |
| Missing scripts на Haitan Animation prefab | Сняты (18 prefab) |
| Runtime material sanitize | `OriginalVisualSanitize` |
| Offline fuben kind 14/17 | `OfflineQuestDungeon` + `HaitanDungeons.json` (11/12/22/31/30) |
| Boss visuals | BOSS_004 / BOSS_008 + modelId 880211004/008 |
| Collection wuqi/xiakucao | mesh GUID нет в Darklord → fallback `emptything` |
| APK | Отложен |

## Остаток по Haitan

- 6 residual unresolved texture/mesh GUID (built-in + отсутствующие в исходниках) — sanitize/fallback
- Приёмка графики на сцене (Editor/device)
- APK когда владелец подтвердит полноту локации
