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
