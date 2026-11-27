# Фактический статус: 27.11.2026

## Продуктовый поворот

Владелец подтвердил: игра **чисто под Android**, наработки брать из **LegacyOfEternity** (`C:\LegacyOfEternity`).  
`C:\SOURCE\LunexUfa` — эталон/лаборатория Server-Admin, не целевой клиент.

## Сейчас в коде

- Unity client: CoastalPrototype + локальный PvE (линия 0.5.5).
- **M05.16**: оффлайн вертикаль
  - `Scripts/Offline/OfflineDemoStore.cs` — аккаунты/герои/квест JSON
  - `Scripts/Offline/OfflineLoginGate.cs` — вход до разблокировки боя
  - `Scripts/Offline/OfflineQuestHud.cs` — talk/gather/kill 1001701–1001703
  - `CombatEnemy.Killed` → прогресс квеста
- Дорожная карта переписана: `Docs/ROADMAP.md` (LoE-first).

## APK

- **0.6.0 offline login:** `C:\LegacyOfEternity\Builds\Android\LegacyOfEternity-0.6.0-alpha.apk` (~43 MB, Unity 6000.3.29f1, ARM64). Лог: `Builds/Android/Unity-0.6.0-offline-build-6.log` (`LOE APK OK`, 0 errors).
- Предыдущий CDN без оффлайн-входа: https://rikagro.ru/MMORPG/downloads/LegacyOfEternity-0.5.5-alpha.apk

### Android SDK fix (эта машина)

Unity 6 читал `platform-tools/package.xml` (schema v4) и получал версию **0.0**, затем падал на `Failed to update Android SDK` (ещё и UAC через `RunElevatedCommand.ps1`).  
Исправление: физический SDK в `C:\asdk\SDK`, у `platform-tools` оставить `source.properties` (`Pkg.Revision=37.0.3`), проблемный `package.xml` убрать/не использовать; `cmdline-tools\latest` = классический 16.0; `LOE_ANDROID_TOOLCHAIN_ROOT=C:\asdk`.

## Дальше

1. Собрать 0.6.0 APK и отдать путь.
2. Проверка на телефоне: регистрация → герой → квест → 3 убийства → перезапуск приложения.
3. Затем M06 online (не раньше).
