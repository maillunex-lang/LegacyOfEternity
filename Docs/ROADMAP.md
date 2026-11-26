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

## Original VFX relative size correction (0.4.2 Alpha)

2026-11-26: Original VFX relative-scale correction, APK 0.4.2 Alpha. Original source SkillBase and SkillMgr use effect prefab localScale=Vector3.one, optional attach to character transform/bone and reset local position; original FX prefab roots are 1. Our Unity6 FourClassesBuilder normalized each class to world height 1.8 units and applied different visual.localScale factors: Warrior 0.8138, Assassin 0.8036, Mage 0.8350, Archer 0.8338 (editor scale audit; exact scale audit logged to Docs/M053-original-relative-scale.txt). OriginalCombatEffects now multiplies source FX root scale by class visual.lossyScale.x at instantiation, preserving original prefab particle size, shape, textures, material blend and relative class scaling; this corrects the previous effect-vs-hero scale discrepancy. Original Darklord-Working untouched. Unity Android 6000.3.27f1 BuildM05Android.Run success 0 errors 40 warnings, 20 effects, 20 original sounds, 168 particle systems, 0 missing MonoBehaviour scripts, 0 unsupported shader assignments, 20 impact timing checks passed. APK LegacyOfEternity-0.4.2-alpha.apk bytes=41896679 SHA256 AB71B8B4190D8D8EFFC8E9710A9ED2CB1319BC4CF6656DF7D6B6150CE55614B9, versionName 0.4.2 versionCode 8 ARM64, same cert as 0.4.1. Published https://rikagro.ru/MMORPG/downloads/LegacyOfEternity-0.4.2-alpha.apk with matching SHA256 and HTTP 200. Physical Android VFX scale is not yet visually confirmed, and per-particle EffectPointAttach runtime binding equivalence remains pending; test APK is not a final parity claim. GitHub sync not confirmed: previous push of 0.4.1 failed with SSH publickey and HTTPS credential store not available.

## M05.7 Original skill icons: 0.5.0 Alpha (2026-11-26)

Completed M05.7 original class skill icon mapping. Original sources read-only: C:\Source\Darklord-Working\Client\Assets\Resources\Lua\Data\t_skill.lua.txt and Assets\Atlas\UIMain.png / UIMain2.png (original .meta GUIDs preserved). Source mappings verified for all 20 class/slot combinations: 5 Warrior (skill IDs 3000001,3001001,3002001,3005001,3006001); 5 Assassin (4000001..4004001), 5 Mage (1000001..1004001), 5 Archer (2000001..2004001). The prior lack of identified archer skill IDs is resolved for icon mapping; original hit timing mapping still requires independent verification in M05.8. Icon scene uses OriginalSkillIconHud to swap 20 class-specific sprites on 5 existing action buttons, without changes to combat cooldowns, buttons, auto battle, VFX or damage. Assets/LegacyAssets/Atlas two PNG files copied byte-identically with original .meta and checked by SHA256, no original source modified. Unity Editor M057OriginalSkillIcons.Run installed successfully, M057OriginalSkillIcons.Audit passed 20/20 icons across four classes, all five buttons and cooldown overlays. Unity BuildM05Android 0.5.0 Alpha succeeded 0 errors, 40 warnings; APK 42,940,627 bytes SHA256 7DD60E72BE9F02C56F22F34D8B5EAB50A115B1228B784F0BEFA5B9532847EA0E versionCode 9, arm64-v8a Android 26..36, same signing certificate as 0.4.2. Published https://rikagro.ru/MMORPG/downloads/LegacyOfEternity-0.5.0-alpha.apk HTTP 200 and verified matching hash/size. Physical Android gameplay and screenshots pending. Next M05.8: source-accurate skill behavior, including original attachment nodes and projectile movement.

## M05.8 Original Combat HUD layout / Android 0.5.1 Alpha (2026-11-26)

Original input HUD port based on user's own historical source prefabs JoystickPanel.prefab and SkillUIParent.prefab under original read-only Darklord-Working. Current Unity 6 HUD preserves gameplay input handlers rather than importing incompatible legacy MonoBehaviours. Joystick uses original UIMain joystick_back and joystick_thumb sprite slices, source 151x151 background and 80x80 thumb; pointer hit area remains 208x208 for mobile. SkillUIParent/SillUI source positions bottom-right slot01 main attack (-75,80), size110; skill02 (-185,50), skill03 (-180,130), skill04 (-120,180), skill05 (-44,190), sizes70. Unity reference CanvasScaler 1920x1080, original source layout authored for historical 1136x640, so runtime mobile aspect testing and safe-area adjustments are still pending. Original sprite frames back_skillbig_a / back_skillcd retained from previously imported UIMain Atlas. 20 original icon sprites and their class mapping preserved, 5 button event callbacks and cooldown overlays remain intact, AUTO toggle preserved. Editor install log LOE_M058_ORIGINAL_LAYOUT_INSTALLED, independent audit LOE_M058_LAYOUT_AUDIT PASS originalStick=2 originalFrames=5 anchors=5 eventsPreserved=5 auto=1. Full Unity Android BuildPipeline Succeeded (0 errors, 40 warnings) and both M057/M058 audits passed. Android test APK versionName 0.5.1, versionCode 10, ARM64, v2 signed with same certificate as 0.5.0, size 42,926,771 bytes, SHA256 38DDE5689FA1E2D74C7F649D8EA42D59E6C538DCDC1C5B52600B4DAF34581A49. Published https://rikagro.ru/MMORPG/downloads/LegacyOfEternity-0.5.1-alpha.apk HTTP 200 hash confirmed on VPS. Original input prefab file not modified. No physical Android screenshot/joystick drag and multitouch QA yet; the port is a layout-and-visual adaptation, not proof of pixel-perfect original gameplay UI parity.

## M05.9 Original camera + touch gestures / 0.5.2 Alpha (2026-11-26)

Camera port of original Coastal scene camera. Source read-only Assets/Scene/allMap/cs_yw_haitan01/cs_yw_haitan01.unity has CameraFollow initDistance=12.5, height=8, yaw=50, and original CameraFollow.cs computes vertical elevation height * (distance / 11), offset from target, LookAt. CameraControl.cs source input constants minDistance=5 maxDistance=35, minHeight=3 maxHeight=15, RotateRatio=.25 degrees/pixel, HeightRatio=.10, ScaleRatio=.05. Prototype OrbitFollowCamera now follows original camera transform, clamps zoom/height, supports one finger drag yaw/height, two finger midpoint pan yaw/height, twist yaw and pinch zoom, smoothing, terrain/building occlusion spherecast, and mouse wheel/right-drag editor controls. Touch input blocks fingers which began over UI (joystick, attack/skills, AUTO); the Unity screen-space UI preserves original combat control wiring and button callbacks. Editor M059OriginalCamera.Run installed source profile; Editor M059OriginalCamera.Audit passed distance12.5 height8 yaw50 bounds5..35 and3..15. Full Android BuildM05Android passed M057 skill icons M058 combat layout M059 camera audits: build succeeded, 0 errors, 40 warnings. APK versionName=0.5.2 versionCode=11 arm64 v2 debug signed, 42,942,139 bytes, SHA256 7E664B96DAC60499E684AEF672211E95DE30CC6AAE94BBA18E3B29E7954C8F74. Published https://rikagro.ru/MMORPG/downloads/LegacyOfEternity-0.5.2-alpha.apk HTTP 200, matching size and hash. Physical Android multitouch, camera collision, UI ownership and visual framing are not yet device-tested.

## M05.10 Original camera and HUD correction / Android 0.5.3 Alpha (2026-11-26)

M05.10 reprioritized by owner after phone video: restore usable original camera/UI rather than add more PvE mechanics. Read-only original sources: Resources/UI/Prefabs/UIMain/SkillUIParent.prefab and UIMain.prefab, Resources/Scene/Camera/FBCamera.prefab, Scripts/UI/CameraControl.cs, Scripts/Core/Camera/CameraFollow.cs, Assets/Scene/allMap/cs_yw_haitan01/cs_yw_haitan01.unity. Original HUD authored at 1136x640 while current CanvasScaler was 1920x1080; corrected reference to 1136x640, preserving source SillUI slot coordinates, authentic combat frame art, and 20 per-class skill icons. Overlaid attack/skill labels were hidden but existing Button event handlers and Cooldown Overlay preserved. Restored original atlas art UIMain:main_guaji.png and main_guaji_disable.png for AUTO, UIMain:btn_switch1.png for target switching, with clearly readable AUTO state and ЦЕЛЬ label; btn_switch1 is a verified original atlas sprite reused for this new targeting action, not proof of exact historical targeting icon. Callback methods Toggle and TargetNext preserved, AUTO icon follows actual AutoCombatController.IsEnabled. Both action buttons laid out separately in upper-right at source Canvas units [-69,312] and [-211,312]. Original CameraFollow.getTargetPosition returns target.position+Vector3.up*1, restored targetOffset 1 metre, camera distance 12.5, height 8, yaw 50 for coast scene. CameraControl handling restored axis-exclusive one-finger horizontal yaw or vertical height (after 25 px threshold); two-finger opposing movement pinches zoom, while parallel two-finger movement performs no orbit/height changes, matching source CheckTwo/Move/Scale distinctions. Explicit EventSystem.RaycastAll on finger start prevents camera stealing touch from any raycastable HUD element; camera stops single-finger orbit when another finger is active on UI. Camera occlusion spherecast ignores near-origin hits less than collider radius. Original zoom bounds 5..35 and height 3..15 preserved. Unity Editor installer LOE_M0510_INSTALLED, independent audit LOE_M0510_AUDIT PASS sourceCanvas=1136x640 originalArt=3 skills=5 auto=1 target=1 cameraFocus=1. Full Android build 0 errors, 40 warnings, all M057/M058/M059/M0510 checks PASS; APK versionName=0.5.3 versionCode=12 arm64 v2 debug-signed same cert, actual APK bytes=42946355 SHA256=11172F3CF6AFFBAD34061A1A21ABF13C5D0B8F4CBFBD74AF1EBB9BAA6FE761DC. Published at https://rikagro.ru/MMORPG/downloads/LegacyOfEternity-0.5.3-alpha.apk HTTP 200 and verified hash. No physical Android device QA for finger ownership, input usability, graphic scale or collisions yet.

## Active handoff (2026-11-26): 0.5.3 Alpha

Supersedes earlier next-task lists and historical sync statements. Windows, VPS and GitHub main verified at code commit 06dc41aa38aa913f5bce9e24f93e3799befbcbb2 before this documentation update. Latest APK: https://rikagro.ru/MMORPG/downloads/LegacyOfEternity-0.5.3-alpha.apk . M05.7–M05.10 implemented and editor audited; physical 0.5.3 acceptance pending. Local prototype, not yet network MMO.

Next proposed work in order:
1. M05.11 warning audit: BuildPipeline reports 40 warnings; text log explicitly shows six macro redefinitions across LD_glow, LD_uv_blend, LD_uv_add. Capture full BuildReport step messages and classify all warnings. Do not suppress diagnostics or modify original assets; compare rendering before/after.
2. M05.12 owner device QA: camera zoom/axis control, joystick, five combat buttons, auto and target switching, safe area. Verify historical target art (current btn_switch1 is a reused original sprite, not proven exact). Correct only demonstrated discrepancies.
3. M05.13 original skill parity: all four classes, attachment nodes, projectiles, impact timing, effect scale and audio; verify archer timing independently.
4. M05.14 auto combat/pathfinding and original monster behavior.
5. M05.15 final M05 regression build and owner acceptance; then M06 authoritative Go/PostgreSQL MMO backend.

Keep changes incremental, original source read-only, Git LFS and meta GUIDs intact. Each implemented milestone requires relevant audits, Android build, APK publication and verified GitHub synchronization. No claim of complete fresh clone validation for this release; check independently when needed.
