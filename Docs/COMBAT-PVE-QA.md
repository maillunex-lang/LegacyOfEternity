# M05: первый локальный PvE-бой (версия 0.2.0 Alpha)

Дата: 2026-11-26. Статус: **проверен в Windows-приложении**; тестирование APK 0.2.0 на реальном Android ещё требуется.

## Реализовано

- Три противника из библиотеки LunexUfa: monster_001 (Лесной зверь), monster_002 (Пещерный страж), monster_003 (Дикий хищник).
- Расстановка на существующей поверхности локации через проверку коллайдеров и уклона; у каждого монстра отдельный CharacterController, текстура, Animator.
- Для каждого монстра: Stand/Run в цикле, Attack/Hit/Death по одноразовым клипам, перерождение после 12 секунд.
- Базовая локальная логика агрессии, преследования, ближней атаки и урона по герою. Дистанция атаки и интервал регулируются параметрами компонента.
- Выбор цели: кнопкой, кликом по монстру или касанием, автоматический поиск ближайшей живой цели.
- Здоровье цели и героя на HUD; обычная атака и четыре активных навыка; дистанции по классам, индивидуальные кулдауны, задержка попадания, навык с уроном по области.
- Смерть персонажа блокирует его движение; возрождение восстанавливает 100 HP.
- По завершении боя противник проигрывает анимацию смерти, а затем появляется снова.

## Практические проверки

- Unity BuildPveArena.Run: **ExitCode 0**, создано **3 монстра**, HUD и кнопка цели.
- Unity PveArenaAudit.Run: **ExitCode 0**, проверено **15** клипов, **4920** анимационных кривых, **4748** совпавших и **172** несовпавших привязок путей, **0 критических проблем**. Не совпали по два пути дополнительных оконечных костей в отдельных клипах; это не подтверждённая ошибка игровой анимации, но остаётся предметом визуального QA.
- Windows64 Development Build: **Succeeded**, **0 errors**, **7 warnings**; размер сборки Unity report 179346950 байт.
- Реальный GUI-тест Windows (Termalin RDP): 3 монстра одновременно видны на каменном мосту, нападают на героя.
- Кнопка АТАКА: лесной зверь **65 → 53 HP** (12 урона).
- Навык 1: **53 → 33 HP** (20 урона).
- Сильный навык: уничтожил противника при 33 HP; персонаж переключил выбор на следующую живую цель; смерть монстра видна.
- Кнопка ВЫБРАТЬ ЦЕЛЬ: переключение на Дикого хищника, отображение **115/115 HP**.
- Кнопка СМЕРТЬ: здоровье героя **0/100**, анимация падения.
- Кнопка ОЖИТЬ: здоровье героя **100/100**, возвращение в стойку.

## Ограничения и следующая работа

Это **локальная PvE-демоверсия**, не MMO-бой. Все данные здоровья и урон сейчас рассчитываются клиентом, и не защищены от изменения. После M06 требуется перенести authority на Go-сервер и добавить протокол боевых команд.

Нет наград, опыта, инвентаря, сохранения убийств, полноценного поиска пути, уклонения или расчёта коллизий навыков. Урон навыков пока фиксирован, без характеристик и баланса. Анимации боевых эффектов/VFX не добавлены.

Потребуется отдельная проверка всех четырёх классов на физическом Android, производительности, кастов, доступности HUD и 172 кривых с отсутствующими оконечными костями. Текущая подпись Unity не является постоянным релизным ключом.

## Android v0.2.0 Alpha: автоматическая сборка

- Unity batchmode ExitCode 0; Succeeded, 0 errors, 5 warnings. Файл APK: 37 766 832 байта.
- Подпись Android APK Signature Scheme v2 проверена. Отпечаток сертификата совпадает с APK v0.1.1, поэтому установка обновления на том же устройстве должна быть совместима с подписью.
- applicationId: com.lunex.legacyofeternity; versionName 0.2.0, versionCode 3; arm64-v8a, minSdk 26, targetSdk 36.
- SHA256: EBA87C88511F2E1D49BF68670EDDBDA53B507DEF2A62CEC820174448F6599C85.
- Предупреждения сборки ещё не классифицированы; запуск и частота кадров на физическом Android 0.2.0 ещё не подтверждены.

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
