# Фактический статус: 27.11.2026

## Продуктовый поворот

Владелец: игра **Android-only**, клиент **LegacyOfEternity** (Unity 6).  
`C:\SOURCE\LunexUfa` — эталон/лаборатория. Codex готовит арт → импорт по команде.  
Приёмка: APK **после крупных этапов**.

## Сейчас в коде

- CoastalPrototype + локальный PvE, offline login (красивый splash), VFX magenta fix (0.6.3).
- **M05.17 (в работе):** мировые интеракты оффлайн-квеста
  - `OfflineWorldBootstrap` / `OfflineWorldInteractable` — Тиас + gather
  - `OfflineInteractHud` — кнопка «Поговорить / Собрать / Сдать»
  - цепочка 1001701→1001702→1001703→1001704 (сдача Тиасу)

## APK

| Версия | Путь | Заметка |
| --- | --- | --- |
| 0.6.3 | `Builds/Android/LegacyOfEternity-0.6.3-alpha.apk` | VFX magenta fix |
| 0.6.2 | `Builds/Android/LegacyOfEternity-0.6.2-alpha.apk` | Login splash art |
| 0.7.0 | *(собирается по завершении M05.17)* | Мировой оффлайн-квест |

## Дальше

1. Закончить M05.17 и выдать APK 0.7.0 на приёмку.
2. Следующий крупный этап — по ROADMAP (M05.18 или M06).
3. Импорт Codex Resources — только по явной команде владельца.
