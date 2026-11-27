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

- Предыдущий стабильный CDN: https://rikagro.ru/MMORPG/downloads/LegacyOfEternity-0.5.5-alpha.apk
- Цель сборки после M05.16: `C:\LegacyOfEternity\Builds\Android\LegacyOfEternity-0.6.0-alpha.apk`

## Дальше

1. Собрать 0.6.0 APK и отдать путь.
2. Проверка на телефоне: регистрация → герой → квест → 3 убийства → перезапуск приложения.
3. Затем M06 online (не раньше).
