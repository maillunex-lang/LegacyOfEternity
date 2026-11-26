# Фактический статус: 26.11.2026

- Unity 6000.3.27f1, Android landscape, Windows тест.
- Перенесено из собственного архива LunexUfa 1788 анимационных ассетов и связанные ресурсы. Текущая папка `Client/Assets/LegacyAssets` около 1.17 ГБ.
- Имеются 4 визуальных класса: Warrior, Assassin, Mage, Archer.
- Сцена `CoastalPrototype.unity`: камера, контроллер, HUD, joystick, локальные combat buttons.
- Android 0.1.0 был собран и доступен на личном CDN; физические тесты выявили ошибки Idle/Run.
- Исправление 0.1.1: для всех четырёх классов импортируются `@stand.FBX` (циклическая стойка) и `@run.FBX` (циклический бег). Результат импортного Unity-аудита: 4/4. Android APK 0.1.1 собран: 0 ошибок, 0 предупреждений, подпись v2 действительна, SHA256 e46cac4d51bb4682dd420859d8f080563d40faccc95105ca642cba5a2d9ce014. Пользователь подтвердил исправление на физическом Android: «Все работает!!!».
- Публичный репозиторий: исходный код и ресурсы, Git LFS; проверка полного клона обязательна.
- Go-сервер: пока только каркас с health endpoint; нет подключённого клиента, боевого сетевого сервера и persistent-персонажей.
- Известный build риск на Windows: Ninja может упереться в MAX_PATH к NDK. Использовать короткий путь к SDK/NDK.
- Права: LunexUfa подтверждает, что игровые ресурсы созданы им, прежнее имя Darklord. Условия лицензирования отдельно не опубликованы.

- VPS smoke: Docker Compose config OK, Go Docker build OK, GET /health -> status=ok, процесс контейнера остановлен по окончании теста.

- GitHub main: `b540589` содержит 7325 файлов; Git LFS upload 2152 уникальных объектов (~1.1 GB) прошёл; чистый clone+LFS pull+fsck на втором пути Windows завершился с кодом 0.
- Ubuntu VPS: `/home/gamedev/LegacyOfEternity` склонирован напрямую из GitHub в sparse-режиме (Server/Deploy/Docs), Go test, Compose config и Docker build проверены (exit 0).
- GitHub Actions `Server smoke` прошёл успешно; GitHub prerelease v0.1.1-alpha содержит Android APK. Unity 6000.3.27f1 открыл clean clone и прошёл боевой аудит: EXIT=0, 28 клипов, 12720 curves, 0 проблем.
- APK 0.1.1 подписан стандартным ключом Unity, так как `useCustomKeystore=false`; для обновления пользователей между разными ПК нужен постоянный релизный ключ с безопасным внешним хранением.

- M05 PvE 0.2.0: три monster_001..003, локальные HP/атаки/skills/агро/смерть/respawn и выбор цели; Windows-сборка успешно (0 errors, 7 warnings); практическая проверка атаки 65->53, skill1 53->33, уничтожение, выбор следующей цели, смерть/возрождение героя. См. COMBAT-PVE-QA.md. Android физическая проверка 0.2.0 ожидается.

- Android 0.2.0 Alpha: Unity BuildPipeline Succeeded, 0 errors, 5 warnings, APK 37766832 bytes, arm64-v8a, minSdk 26 targetSdk 36, versionCode 3. APK v2 signature verified, signing certificate SHA256 equal to 0.1.1. SHA256 eba87c88511f2e1d49bf68670eddbda53b507def2a62cec820174448f6599c85. Physical Android QA pending.

## 2026-11-26 • M05.2/M05.4 development checkpoint

- **M05.2 incomplete**: imported the original FBX/texture/material and corresponding .meta files for all 4 hero classes, without modifying the original Darklord-Working tree. Verified SHA256 identity for selected original files: 4/4 classes, 0 errors (see `Docs/M052-weapon-static-audit.json`). Original character FBXs contain `DM_R_Hand` markers.
- Added `WeaponEquipment` runtime slot and strict `M052WeaponBuilder.Run/Audit` for attaching those assets to actual original hand bones. Attachment, orientation, all five poses, and visual Unity testing are **not yet verified**.
- **M05.4 incomplete**: prepared opt-in foreground `AutoCombatController`, a toggle HUD builder, collision-aware movement through `PlayerMotor`, and shared `CombatEncounter` attack/cooldown methods. The automatic battle implementation has **not yet been compiled or playtested in Unity**.
- Local Windows Unity 6000.3.27f1 Editor executable was not found on PATH or expected installation folders; previous installation registration remained. Editor restoration / build tests pending.
- **0.3.0 Alpha is NOT released.** Android hardware QA pending; no unverified features marked complete.

## M05 0.3.0 implementation verified checkpoint (2026-11-26)

- Original weapon source files were copied without modifying `C:\Source\Darklord-Working`. SHA256 asset equality 4/4; Editor `M052WeaponBuilder.Run` attached original warrior, assassin, mage and archer FBXs to original `DM_R_Hand` skeletal anchors (4/4); `M052WeaponBuilder.Audit` passed. 4117 Unity .meta GUIDs and 0 new duplicates. Physical pose/orientation QA remains.
- Original skill animation file identity 16/16 and original FX/audio inventory complete as research. Unique skill VFX and authentic sounds have NOT been wired.
- `M054AutoCombatBuilder.Run/Audit` passed with UI AUTO toggle off by default; shared encounter damage path, manual-motion cancellation, range/LOS/stuck protection compiled but not behaviorally tested.
- `M055CombatHudBuilder.Run/Audit` passed: five skill cooldown overlays, three enemy world-space HP bars and one damage notification path.
- Unity 6000.3.27f1 and Android Build Support/SDK/NDK/JDK restored on Windows using official Unity CLI (installer reported success). `BuildCombatPreview.Run` succeeded with 0 errors and 7 warnings, size reported 179814354 bytes. Build lives in `Builds/Windows/`; WIP copy `Builds/Windows/M05-Alpha-WIP/`.
- Attempted GUI launch in separate Windows RDP session: D3D12 and D3D11 builds failed during display mode selection (graphics context), so actual interactive visual checks are STILL PENDING. No physical Android smoke. Source changes stay LOCAL WIP until further QA; 0.3.0 APK/release not published.

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
