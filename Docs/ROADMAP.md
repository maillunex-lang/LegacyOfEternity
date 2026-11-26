# Legacy of Eternity: дорожная карта

Дата актуализации: 26 ноября 2026. Владелец: LunexUfa (ранее Darklord).

## Принципы проекта

* **Unity 6 / Android landscape**, PC для редактора и проверок.
* Полноценная MMO потребует отдельного бэкенда (Go), PostgreSQL и серверной авторитетности.
* Любые игровые ресурсы, сцены и метафайлы Unity хранятся версионированно. Большие двоичные ресурсы в Git LFS.
* Разработка и доставка воспроизводимы на новой Windows-машине; сервер поднимается на Ubuntu VPS. Не привязывать код к конкретному IP и домену.
* Каждая веха должна завершаться проверками, артефактом, changelog и реальным рабочим сценарием, не только «успешной сборкой».

## Вехи (статусы отражают реальность, не обещания)

| ID | Этап | Текущее состояние | Критерий завершения |
| --- | --- | --- | --- |
| M00 | Структура проекта, GDD, CI, GitHub | **В работе** | Чистый clone+LFS+Unity import, серверный smoke, публичный репозиторий без секретов |
| M01 | Аудит лицензий, ассетов и сцены | **Частично** | Таблица ассетов и зависимостей, отсутствие missing GUID/scripts в боевой сцене |
| M02 | Библиотека анимаций и персонажей | **Частично** | Аудит циклов, скелетов, путей костей, визуальное тестирование всех 4 классов |
| M03 | Мир и визуализация | **Частично** | Стабильная сцена, свет, вода, коллизии, камера, тени, FPS на Android |
| M04 | Управление и UI | **Частично** | Safe areas, управление касанием, вращение камеры, локализация и доступность |
| M05 | Прототип боя | **Частично** | 4 класса, атаки и навыки; затем враги, попадания, кулдауны, VFX и баланс |
| M06 | MMO backend | **Запланировано** | Авторизация, сессии, tick, authoritative position/combat, репликация |
| M07 | Мир и прогрессия | **Запланировано** | NPC, квесты, инвентарь, лут, уровни, таланты, экипировка |
| M08 | Социальные механики | **Запланировано** | Чат, друзья, группы, гильдии, мировые события |
| M09 | Экономика и сервисы | **Запланировано** | Торговля, аукцион, почта, крафт, админка, аудит операций |
| M10 | Оптимизация и защита | **Запланировано** | Android performance, память, сетевое нагрузочное тестирование, резервные копии |
| M11 | Закрытая альфа | **Запланировано** | Авторазвёртывание, телеметрия без секретов, краш-логи, ограниченное тестирование |
| M12 | Бета и релиз | **Запланировано** | Обновления APK, поддержка сервера, миграции, мониторинг, продуктовые KPI |

## Активная очередь P0: 0.3.0 Alpha

1. **M05.2 — Оружие**: аудит оригинальных моделей и текстур, оружие каждого из четырех классов, крепление к костям, проверка Idle/Run/Attack/Skill/Death. Не выдумывать соответствия ассетов.
2. **M05.3 — Умения и VFX**: проверить и подключить оригинальные клипы и эффекты четырех навыков каждого класса, снаряды, звуки при наличии, синхронизация попадания и урона, без повторного урона.
3. **M05.4 — Автобой**: кнопка АВТО вкл/выкл, поиск живой цели в ограниченном радиусе, подход на дистанцию класса, поворот, атака и навыки с общими кулдаунами, смена цели, остановка при смерти или ручном движении. По умолчанию выключен; без фонового фарма. Проверить препятствия и отсутствие зацикливания.
4. **M05.5 — Полировка боя**: иконки, кулдауны, выделение цели, полоски HP, всплывающий урон; исправить выявленные проблемы анимаций и управления.
5. **M05.6 — Выпуск 0.3.0**: Windows GUI QA всех классов, Android ARM64 APK, проверка подписи и SHA256, GitHub Release, CDN, обновление документации. Физический Android QA отмечать отдельно.

Базовая версия 0.2.0 Alpha: коммит 4f799ec, локальный PvE проверен в Windows, APK опубликован. M05 остается в работе. 172 несовпавшие привязки анимационных кривых и 5 предупреждений Android требуют проверки. Серверная авторитетность, награды и прогрессия остаются следующими этапами M06/M07.

## Остальные задачи P0

1. **ANIM-01 [ГОТОВО 0.1.1]**: устранить махание в Idle и «полет» при беге во всех 4 классах, проверка клипов `stand/run`, их loop import и переходов.
2. **ANIM-02**: устройство Android: записать 4 теста стояния/движения, проверить высоту над землёй, анимации после боя, смешивание состояний.
3. **BUILD-01 [ГОТОВО]**: Android v0.1.1, подпись, версия, контрольная сумма, ссылка на APK без перезаписи v0.1.0.
4. **REPO-01**: публичный GitHub, Git LFS для бинарных ресурсов, smoke clone, исключение кэшей, документация о правах.
5. **SERVER-01 [SCAFFOLD ПРОВЕРЕН]**: минимальный health и Docker Compose на Ubuntu. Проверить сборку, контейнер и отказоустойчивое восстановление на тестовом порту.
6. **QUALITY-01**: автоматический аудит missing scripts, клипов, UI, shader compatibility, FPS профили на устройстве.

## Затем: P1

* **COMBAT-01 [ЛОКАЛЬНЫЙ PvE ГОТОВ 0.2.0]**: gameplay state machine, защита от некорректного прерывания анимаций, локальные dummy-враги.
* **NET-01**: протокол сетевых сообщений, heartbeat, reconnect, rate limits, ограничение рассинхронизации.
* **DATA-01**: PostgreSQL migrations, персонажи/аккаунты, сохранение, серверный инвентарь.
* **MAP-01**: загрузка чанков и LOD, навигация, коллизии, мобильные уровни качества.
* **UI-01**: настоящее окно выбора персонажа, экран настроек, интерфейс заданий и сумки.

## Затем: P2

Социальные системы, города и подземелья, NPC и истории, PVP/PVE, крафт и торговля, масштабирование нескольких серверов, релизная система обновлений.

## Контрольные критерии

* **Клиент**: Unity открывает проект из чистого клона без отсутствующих LFS-объектов, редактор успешно импортирует все сериализованные `.meta`; Android APK собирается и запускается на физическом устройстве.
* **Сервер**: `go test ./...`, контейнерный healthcheck, отсоединение/подключение БД, миграции с восстановлением, конфиги через env, журналы без токенов.
* **Совместимость**: клиент и сервер фиксируют версии протокола и имеют автоматические проверки назад/вперёд.
* **Безопасность**: никаких токенов, приватных ключей, базы игроков или паролей в репозитории.
* **Дистрибуция**: артефакты APK хранятся в GitHub Releases / собственном CDN, не в Git; источник и LFS полностью восстановимы.

### Известные ограничения

Сегодняшний Unity-клиент это прототип без сетевого боя. Список M00–M05 описывает **частично сделанные** функции, а не готовые системы. Многопользовательский сервер, экономическая модель и реальный online ещё не реализованы.

- **BUILD-02 [ЗАПЛАНИРОВАНО]**: стабильный релизный Android keystore, GitHub Actions Secrets, бэкапы подписывающего ключа вне репозитория. Сегодняшний APK 0.1.1 использует стандартную подпись Unity; при смене ПК обновление без переустановки пока не гарантируется.

### M05 локальный PvE (26.11.2026)

Добавлены 3 монстра, выбор цели, урон, 4 навыка с кулдаунами, AI преследования, смерть и возрождение врагов. GUI-тест Windows успешен, см. Docs/COMBAT-PVE-QA.md. Сетевой authoritative PvE, опыт и предметы ещё впереди.

### 0.3.0 Alpha implementation checkpoint (2026-11-26)

- M05.2: original class weapon meshes/textures/materials imported, static SHA256 parity 4/4 passed. `M052WeaponBuilder` prepared; requires Unity compile, bone/pose visual QA. **IN PROGRESS**.
- M05.3: original ability clip inventory identified for all four classes; authentic VFX and audio wiring still pending.
- M05.4: local auto-combat runtime/controller/HUD builder coded to call the existing damage path; Unity tests pending. **IN PROGRESS**.
- M05.5 and M05.6: not started; no APK 0.3.0 artifact.

### M05.2/M05.4/M05.5 Unity editor audit update (2026-11-26)

- M05.2 imported and attached original weapon FBXs to all four class rig anchors; Unity audit 4/4, static SHA256 parity 4/4, .meta collision check clear. Needs five-state GUI pose visual checks, weapon geometry normalization and future equipment switching regression.
- M05.3 researched original sixteen skill FBXs and asset-effect inventory, unique VFX/projectiles/icons/audio/damage impact synchronization still not implemented.
- M05.4 AUTO toggle installed default OFF and editor audit passes; gameplay control/collision/kill/death/manual interrupt smoke still pending.
- M05.5 five cooldown labels, three enemy health bars and damage event text installed/audited; original icons and device testing pending.
- M05.6 WIP Windows build succeeded (0 errors, 7 warnings); GUI launch in Windows RDP failed due to display mode selection, Android 0.3.0 and GitHub release NOT available.

## 2026-11-26: 0.3.0 Alpha APK test build (verified)

- Built via Unity 6000.3.27f1 and `LegacyOfEternity.Editor.BuildM05Android.Run`, without rebuilding older scenes. Unity build **Succeeded**, errors=0, warnings=5.
- Editor checks: weapon sockets 4/4; AUTO HUD and default OFF; combat HUD 5 cooldowns and 3 monster HP bars; PvE audit three monsters, 15 clips, 4920 animation curves (4748 matched, 172 known optional unmatched), zero critical issues.
- APK: `LegacyOfEternity-0.3.0-alpha.apk`, **38,054,500 bytes**, SHA-256 `56EAEE24519FDCEA0CBA3DB3FD017BD29F1B910A51A79BF3EF9C9237A0364CF7`.
- Android package `com.lunex.legacyofeternity`, versionName `0.3.0`, versionCode `4`, minSdk `26`, targetSdk `36`, ARM64.
- Android `apksigner verify`: v2 signature valid; certificate SHA-256 `68d2d8e7ea76c53b010f5f9e8e9c58efa83202625f1ed05a6e732288b52e9a50` identical to 0.2.0, permitting an in-place update when all other install conditions permit.
- Uploaded to VPS `/var/www/MMORPG/downloads/LegacyOfEternity-0.3.0-alpha.apk`, SHA-256 verified after transfer and installation; direct HTTPS HEAD response 200, Content-Length 38054500.
- Public download: https://rikagro.ru/MMORPG/downloads/LegacyOfEternity-0.3.0-alpha.apk
- **Not tested on a physical Android device**: Windows ADB had no device connected. This is an **APK for device testing**, not a fully QA-approved release. M05.3 authentic skill VFX/audio and complete runtime auto-battle evaluation are still pending.

## M05.3.1 weapon correction and Android 0.3.1 Alpha (2026-11-26)

2026-11-26: M05.3.1 original weapon correction, Android 0.3.1 Alpha built and published. Warrior equipped ZS_M_wuqi_01 FBX instead of incorrect L_ZS_wuqi_01. Assassin right hand CK_W_wuqi_01_a and left hand CK_W_wuqi_01_b on original DM_R_Hand/DM_L_Hand anchors. Original source C:\Source\Darklord-Working remained unchanged; Mage and Archer unchanged. Unity M052 audit 4/4, M054 AUTO audit pass, M055 HUD audit pass, PvE 3 enemies and 15 clips, zero critical issues. Unity BuildM05Android succeeded, 0 errors, 5 warnings, APK 38014240 bytes, versionName 0.3.1, versionCode 5, package com.lunex.legacyofeternity, minSdk 26, targetSdk 36, arm64. APK SHA256 F70C3BFD92D952666D23C712691712EFB60889D2D211D02E1A015FEB8B67479D. Signature v2 verified, signing cert matches 0.3.0. Published https://rikagro.ru/MMORPG/downloads/LegacyOfEternity-0.3.1-alpha.apk; HTTP 200, 38014240 bytes. Android physical gameplay inspection for positioning and both blades still pending; no GitHub sync yet.

## 2026-11-26: M05.3 original visual effects, audio and timing; 0.4.0 Alpha test APK

- **Real source-based effects**: imported low-footprint historical effect prefabs from `Resources/Effect/skill/player/{ZS_M,CK_W,FS_W,GJS_M}`, separately for basic attack and four class skills. Copied originals remain byte-identical; Unity6 working variants strip **24 incompatible legacy MonoBehaviour components** without changing the originals. Original effect import required **121 supporting mesh/material assets**, all copied with original .meta GUIDs; one referenced original GUID was not found.
- **Unity installed-effects audit**: **20/20 VFX**, **20/20 original audio mappings**, **168 particle systems**, **0 remaining missing scripts** in cleaned variants.
- **Combat hit synchronization**: original `SkillClassDisplayDesc.xml` supplies hit times for **15/20** class-action combinations. Multiple hits distribute the same existing MVP damage budget, not multiplied damage. Five archer actions still use the existing fallback timing because their original skill IDs were not established. Assassin's fourth skill is normalized from source hits [0.2, 0.8, 1.6] to [0.131, 0.525, 1.05] to fit imported 1.167-second animation.
- **Audio**: original Warrior, Assassin, Mage skill files imported; archer/shooter `SS_` sound assignment for slots 1-3 remains provisional. No paid APIs or third-party new resources.
- **Unity Android build**: Unity 6000.3.27f1 BuildPipeline Succeeded, **0 errors, 36 warnings**, APK `LegacyOfEternity-0.4.0-alpha.apk`, **40,068,189 bytes**, SHA-256 `86FC89FBB8519883F19559A1DD139B926DE523EB13C97FD721C5556F5C7CB8AE`. Some older shaders produce macro redefinition warnings; device graphics QA still needed.
- **Android package** `com.lunex.legacyofeternity`, versionName `0.4.0`, versionCode `6`, ARM64, minSdk 26, targetSdk 36. APK v2 signature verified, certificate SHA-256 same as 0.3.1, allowing upgrade without uninstalling when install policy permits.
- **Download**: https://rikagro.ru/MMORPG/downloads/LegacyOfEternity-0.4.0-alpha.apk , VPS SHA-256 checked and HTTP 200 (Content-Length 40068189).
- **Honest constraints**: no physical Android smoke test (Windows ADB had no device), no complete in-game pose/VFX or sound verification, no M06 redesigned skill icons yet, network combat still MVP prototype, and final signed production keystore not established.

## VFX texture/shader dependency restoration: 0.4.1 Alpha (2026-11-26)

2026-11-26 VFX restoration 0.4.1 Alpha: audit of 20 original VFX Unity6 prefabs identified 412 material slots, including 37 empty material slots, 68 unsupported/error-shader uses and 365 material slots without a resolvable usable texture at the initial audit. Indexed 31715 .meta GUIDs from the original Darklord-Working assets in read-only fashion; re-imported 108 original missing GUID dependencies (12,205,927 bytes) including textures and historical shader sources with matching original .meta GUIDs and SHA256 checks. After dependency restoration, Unity audit: 0 unsupported shaders, 59 optional _mask:NONE entries, 37 empty source material slots (including 2 primary blank slots present in original assassin skill004 prefab). One GUID ce47f77d4c07edb4884aed22d1665409 used by two source materials is absent from the original source .meta index, so it could not be reconstructed without inventing an asset. Original prefabs are preserved, as are the originally copied resources; 24 missing old script components were previously stripped from Unity6 working copies and still require functional replacement if parity depends on their behavior. Unity Editor M053OriginalVfxBuilder.Audit: 20 effects/20 sounds/168 ParticleSystems/0 missing scripts. M053TimingAudit: 20 actions overClip=0. Unity Android BuildM05Android.Run 0.4.1 Succeeded with 0 errors, 40 warnings (original legacy shader macro redefinitions and other nonfatal warnings). APK LegacyOfEternity-0.4.1-alpha.apk 41,897,159 bytes SHA256 D5BB1CDD47AEC844FF7FBD7B99CEF0CA7C2E6D6580F2BE4BB1C46AFB8CC443E8, versionName 0.4.1 versionCode 7 arm64 minSdk26 targetSdk36, signature v2 certificate same as 0.4.0. Published https://rikagro.ru/MMORPG/downloads/LegacyOfEternity-0.4.1-alpha.apk HTTP 200 matching SHA256/Content-Length. Physical Android frame, material alpha, audio and original behavioral parity are NOT yet verified (ADB devices empty). No new original source data invented.
