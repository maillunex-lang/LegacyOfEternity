# OriginalPort — импорт оригинала в Unity 6

Сюда копируются срезы из `C:\Source\Resources\` (и при необходимости точечно из Darklord),  
**с сохранением `.meta` GUID**. Оригинал на диске не изменять.

## Правила

1. Источник read-only: `C:\Source\Resources\`, `C:\Source\Darklord-Final\`, `Baseline\Darklord-Android\`.
2. Перед копированием читать `C:\Source\Resources\14_Catalog\UNITY6_NOTES.md` и `MISSING_GUIDS.md`.
3. Класть файлы зеркально по смыслу каталога Resources (`01_Characters` → `OriginalPort/01_Characters`, …).
4. Не коммитить сломанные NGUI/XLua MB без замены LoE-скриптом.
5. После импорта — smoke Unity + при игровом диффе APK; обновить `Docs/PORT-MANIFEST.md` и `Docs/ROADMAP.md`.

## Срезы

См. `Docs/ROADMAP.md` §3–§5 (PORT-00…).
