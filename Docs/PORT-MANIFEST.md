# PORT-MANIFEST — статус переноса оригинала → LoE Unity 6

Обновлять вместе с каждым PORT-срезом. Канон порядка: `Docs/ROADMAP.md`.  
Копирование: `Docs/scripts/Port-FromResources.ps1`.

## Источники

| ID | Путь |
| --- | --- |
| RES | `C:\Source\Resources\` |
| CAT | `C:\Source\Resources\14_Catalog\` |
| DF | `C:\Source\Darklord-Final\` |
| DA | `C:\SOURCE\LunexUfa\Baseline\Darklord-Android\` |
| LOE | `C:\LegacyOfEternity\Client\Assets\` |

## Срезы

| Срез | Статус | В LoE | Примечание |
| --- | --- | --- | --- |
| PORT-00 пайплайн | Готово | `OriginalPort/`, `Docs/scripts/Port-FromResources.ps1` | — |
| PORT-01 шейдеры/mats классов | Готово (0.9.0) | `Custom-Character.shader`, `CustomLight.cs`, `HeroAndroidVisibility` | APK собран; телефонная приёмка |
| PORT-02 персонажи/аним/оружие | Запланировано | частично LegacyAssets | Полный GUID-safe импорт |
| PORT-03 VFX | Запланировано | `LegacyOfEternity/Effects/*_Unity6` | Заменить soft-fallback |
| PORT-04 UI | Запланировано | частично OfflineUI + UIMain icons | Login/Bag/Skill как оригинал |
| PORT-05 предметы/зелья | Запланировано | offline_* заглушки | PotionIDs 101010001–009 |
| PORT-06 Haitan map | В работе | `110200011.*`, `OriginalHaitanBootstrap`, сцена `cs_haitan` | modelId meshes in; APK deferred |
| PORT-07 Haitan quests | В работе | `HaitanQuests.json` (99), `HaitanDialogues.json` (RU), `OfflineDialogueHud` | Prefab deps / APK after full loc |
| PORT-08+ maps | Запланировано | — | По t_map |

## PORT-01 файлы

| Файл | GUID / роль |
| --- | --- |
| `LegacyAssets/Resources/Shaders/Custom-Character.shader` | `fc39ed277a42ab04b9921720cd614363` |
| `LegacyAssets/AssetsRaw/Effect/Textures/mask/ramp_mask06.png` | `869a55df630a2b840976b9e9b32b0ce6` |
| `LegacyOfEternity/Scripts/Rendering/CustomLight.cs` | globals `_SLight*` |
| `LegacyOfEternity/Scripts/Player/HeroAndroidVisibility.cs` | stop Unlit force |

## Запрещено как «готово»

- Считать Unlit Prototype / soft-particle / offline_potion финальным паритетом.
- Дублировать asset с тем же GUID в OriginalPort и LegacyAssets.
- Импортировать всё дерево Resources одним коммитом без среза и APK-проверки.
