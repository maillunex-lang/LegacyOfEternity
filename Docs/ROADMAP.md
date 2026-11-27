# Legacy of Eternity — дорожная карта полного паритета с оригиналом (Unity 6)

**Дата актуализации:** 27.11.2026 (стратегия: full original → Unity 6)  
**Владелец:** LunexUfa  
**Канон:** этот файл. Любой ИИ-исполнитель **читает в начале** и **обновляет в конце** (§0).

Связанные файлы: `Docs/CURRENT-STATUS.md`, `Docs/DECISIONS.md`, `Docs/BOOTSTRAP.md`,  
`C:\Source\Resources\14_Catalog\` (каталог Codex), `C:\SOURCE\LunexUfa\ROADMAP.md` (указатель).

---

## §0. Протокол исполнителя (обязателен)

### 0.1 Цель продукта (с 27.11.2026)

Сделать **ту же игру, что оригинал Darklord / Legacy of Eternity**, на **Unity 6000.3 + Android**,  
перенося **возможности, механики, графику, текстуры, объекты, анимации, VFX, UI, локации, квесты, предметы (включая зелья), бой, прогрессию** — без упрощённых «заглушек» как конечной цели.

Упрощённый оффлайн-прототип (login/квест/сумка 0.6–0.8) — **временные леса**, уже заменяются срезами PORT.  
**Запрет:** новые демо-арены, спавн «рядом с героем», выдуманная раскладка мира. Только оригинал 1:1.

### 0.2 Источники (только чтение оригинала / запись в LoE)

| Роль | Путь |
| --- | --- |
| **Целевой клиент** | `C:\LegacyOfEternity` (Unity 6, Built-in RP) |
| **Codex-адаптация под U6** | `C:\Source\Resources\` (`01_…`–`18_…`, каталог `14_Catalog`) |
| **Оригинал клиент+конфиг** | `C:\Source\Darklord-Final\` |
| **Оригинал Android baseline** | `C:\SOURCE\LunexUfa\Baseline\Darklord-Android\` |
| **Квесты/спавны эталон** | `C:\SOURCE\LunexUfa\Content\`, `Baseline\Haitan-*.md` |
| **Лаборатория Server/Admin** | `C:\SOURCE\LunexUfa\Server`, `Admin` — не целевой APK |

Правила импорта:

1. **Оригинал и Resources — read-only** (не портить GUID/файлы на месте).  
2. Копировать в LoE в `Client/Assets/OriginalPort/` (или согласованный префикс), **сохраняя `.meta` GUID** где есть в каталоге.  
3. Сверять `14_Catalog\guid-mapping.json`, `compatibility-report.json`, `UNITY6_NOTES.md`, `MISSING_GUIDS.md`.  
4. Legacy MonoBehaviour / NGUI / XLua **не тащить слепым копированием** — заменить эквивалентом uGUI/C# LoE, **поведение как в оригинале**.  
5. Каждый крупный срез = ветка `cursor/port-<slice>-270c` + APK + обновление ROADMAP/STATUS.

### 0.3 Перед работой

1. Прочитать ROADMAP + CURRENT-STATUS + актуальный `14_Catalog` README/NOTES.  
2. Взять **только** активный P0-срез (не «всю игру сразу» одним коммитом).  
3. Рабочая копия: `C:\LegacyOfEternity`, ветка `cursor/<имя>-270c`.  
4. Не собирать основной APK из Unity 2017 LunexUfa.

### 0.4 Во время работы

1. Крупный этап → один APK на приёмку.  
2. CoastalPrototype можно расширять/заменять порциями; полный wipe сцены — только в срезе PORT-LOC с бэкапом/веткой.  
3. APK не в git; LFS для крупных бинарников.  
4. Toolchain: `LOE_ANDROID_TOOLCHAIN_ROOT=C:\asdk`, при необходимости `LOE_SKIP_ANDROID_AUDITS=1`; `platform-tools` без ломающего `package.xml`.  
5. Сборка: `BuildOfflineAndroid.Run` (позже — отдельный `BuildParityAndroid` когда offline-gate станет опционален).  
6. **Блокеры приёмки** (краш, не видно героя, сплошные error-квадраты) → сразу патч-APK.  
7. Косметический долг вне текущего среза — в чеклист среза как «остаток», не останавливает следующий срез без указания владельца.

### 0.5 После работы

Обязательно обновить:

1. Этот ROADMAP (статусы, чекбоксы, P0, дата).  
2. `CURRENT-STATUS.md` (APK, чеклист приёмки).  
3. При смене стратегии — `DECISIONS.md`.  
4. Commit + push.

### 0.6 Статусы

`Готово (x.y.z)` | `В работе` | `Запланировано` | `Блокер` | `Отложено`

---

## §1. Продуктовые решения

1. **Паритет с оригиналом на Unity 6 / Android** — главный критерий готовности систем.  
2. **Целевой клиент только LoE** (`C:\LegacyOfEternity`).  
3. **Перенос всего** (графика + механики + контент); упрощения допустимы только как промежуточный шаг внутри среза с явной заменой.  
4. **`C:\Source\Resources` открыт к импорту** (стратегия 27.11: не ждать отдельной фразы «можно» — владелец поручил полный перенос).  
5. Приёмка: APK после **каждого PORT-среза**.  
6. Online (M06+) — после того как оффлайн-паритет ключевых систем (бой, сумка/зелья, квесты локации) играет на телефоне; сервер сверки с оригинальными конфигами.

---

## §2. Карта переноса (что откуда)

```
Оригинал (Darklord-Final / Darklord-Android)
  ├── Characters / Animations / Weapons
  ├── Effects (VFX) + Shaders (LD/*, Custom-Character)
  ├── UI (атласы UIMain, Login, Bag, SkillUI, Joystick)
  ├── Scenes / Maps (cs_yw_haitan01, …)
  ├── Lua Data: t_skill, t_item, QuestConfig, PotionIDs, t_map, …
  └── Audio / Prefabs / Equipment
        │
        ▼  Codex adaptation
C:\Source\Resources\01_…18_ + 14_Catalog\*.json
        │
        ▼  Port slices (GUID-safe)
C:\LegacyOfEternity\Client\Assets\OriginalPort\…
        + Scripts parity (C# instead of XLua/NGUI)
        + APK на Android
```

Уже в LoE (частично, не полный паритет): CoastalPrototype, 4 класса, VFX-префабы Unity6, offline login/квест/лут-заготовка, original HUD icons/camera/controls (M05.x).

---

## §3. Сводная таблица вех PORT / M

| ID | Срез | Статус | Критерий / APK |
| --- | --- | --- | --- |
| LEGACY-0.8 | Offline scaffold (login/квест/лут) | Готово (0.8.0) | Временные леса |
| **PORT-00** | **Инвентаризация + пайплайн импорта** | **Готово** | Манифест, OriginalPort, Port-FromResources.ps1 |
| **PORT-01** | **Шейдеры + материалы персонажей** | **Готово (0.9.0)** | Custom/Character + CustomLight; APK собран; ждёт приёмку на телефоне |
| PORT-02 | Персонажи + анимации + оружие (4 класса) | Запланировано | Ориг. меши/клипы/сокеты |
| PORT-03 | VFX навыков (все классы) по каталогу | Запланировано | Без error-листов; тайминги как t_skill |
| PORT-04 | UI бой + логин + сумка (атласы/префабы) | Запланировано | Как оригинал 1136×640 |
| PORT-05 | Предметы + зелья + использование (ItemConfig/PotionIDs) | Запланировано | Реальное лечение/автоюз по конфигу |
| PORT-06 | Локация Haitan (карта, террейн, пропы, спавны) | **В работе** | cs_haitan уже в CoastalPrototype; MapData spawns; без APK до полной локации |
| PORT-07 | Квесты Haitan полный поток | **В работе** | 99 квестов QuestConfig 1001701–1001799 в runtime |
| PORT-08 | Остальные локации по t_map (очередь) | Запланировано | По одной карте = этап |
| PORT-09 | Боевая механика (скиллы, CD, хиты, AI) | Запланировано | По SkillConfig / TitanMagic эталон |
| PORT-10 | Прогрессия (уровни, эквип, усиления) | Запланировано | Как оригинальные таблицы |
| M06 | Online auth/сессии (Go) | Запланировано | После PORT-05…07 на телефоне |
| M07 | Authoritative квесты/инвентарь/лут | Запланировано | Сервер + клиент |
| M08+ | Соц./экономика/оптимизация/релиз | Запланировано | По мере паритета |

Старые M05.16–M05.18 остаются в истории как scaffold; новые работы идут под **PORT-***.

---

## §4. Детальные срезы

### LEGACY-0.8 — Offline scaffold — `Готово (0.8.0)`

- [x] Offline login/splash, квест Тиас/gather/kill/сдача  
- [x] Lут-заготовка + сумка + dummy potion  
- [x] VFX hotfix 0.6.3/0.7.1 (временный soft-particle / Unlit)  

**Долг:** заменить Unlit/soft-fallback и dummy potion срезами PORT-01 / PORT-03 / PORT-05.

---

### PORT-00 — Инвентаризация + пайплайн — `Готово`

**Цель:** любой исполнитель знает порядок срезов и куда класть файлы.

- [x] Создать `Client/Assets/OriginalPort/` (+ README с правилами GUID)  
- [x] Создать `Docs/PORT-MANIFEST.md`: список срезов, зависимости, статус файлов  
- [x] Зафиксировать скрипт/процедуру: `Docs/scripts/Port-FromResources.ps1`  
- [x] Smoke: открытие Unity + `BuildOfflineAndroid` после игрового импорта (процедура в README/скрипте)  
- [x] APK не нужен (нет игрового диффа в PORT-00)  

**Критерий:** следующий срез PORT-01 можно начать без вопросов «куда класть».

---

### PORT-01 — Шейдеры + материалы персонажей — `Готово (0.9.0)`

Источник: `Resources/12_Shaders`, mats уже в `LegacyAssets/.../player`, notes в `UNITY6_NOTES.md`.

- [x] Импорт `Custom-Character.shader` (+ meta GUID `fc39ed…`) → `LegacyAssets/Resources/Shaders/`  
- [x] Порт `CustomLight` → `Scripts/Rendering/CustomLight.cs` + глобали `_SLight*`  
- [x] `HeroAndroidVisibility`: предпочитать `Custom/Character`, Unlit только emergency  
- [x] `ramp_mask06.png` (ссылка из warrior mat)  
- [x] Compile Android: `LOE_M0511_HERO_SHADER=Custom/Character`; APK **0.9.0**  
- [ ] Приёмка на телефоне: 4 класса читаемы, текстуры верные (владелец)  
- [ ] LD_* GLES-исключения — хвост PORT-03  

**APK:** `Builds/Android/LegacyOfEternity-0.9.0-alpha.apk`  

### Changelog PORT-01 (27.11.2026)
- Источник: `C:\Source\Resources\12_Shaders` + Darklord `CustomLight.cs`
- Перенесено: Custom/Character, CustomLight, ramp_mask06; снят Unlit-форс
- Поведение: mats 4 классов → оригинальный шейдер + `_SLight*` globals
- APK: `LegacyOfEternity-0.9.0-alpha.apk`
- Остаток: телефонная приёмка; LD_* / soft-particle → PORT-03
- Следующая P0: **PORT-02** (персонажи/аним/оружие)

---

### PORT-02 — Персонажи / анимации / оружие — `Запланировано`

- [ ] Импорт FBX/контроллеров 4 классов из `01_Characters` / `02_Animations` / `03_Weapons`  
- [ ] Сокеты оружия (`weapon-socket-probe.json`)  
- [ ] Animator transitions по каталогу; не ломать root motion без сверки  
- [ ] APK  

---

### PORT-03 — VFX навыков — `Запланировано`

- [ ] Импорт по `03-class-skills-vfx*.json` / `09_VFX`  
- [ ] Материалы/текстуры; запрет «пустой additive = фиолетовый квадрат»  
- [ ] Привязка к `OriginalCombatEffects` / оригинальным таймингам `t_skill`  
- [ ] APK  

---

### PORT-04 — UI (логин, бой, сумка) — `Запланировано`

- [ ] Атласы UIMain/UIMain2/Login → Sprite; сверка rect/pivot с original meta  
- [ ] LoginUI как оригинал (splash + поля + кнопка старта)  
- [ ] SkillUIParent / Joystick / Bag layout 1136×640  
- [ ] Замена NGUI MB на uGUI, **внешний вид и позиции как оригинал**  
- [ ] APK  

---

### PORT-05 — Предметы и зелья как в оригинале — `Запланировано`

Эталон: `ItemConfig`, `ConstsConfig.PotionIDs` (`101010001`…`101010009`), BagModel auto-potion, UseItem.

- [ ] Таблица предметов из оригинала (не `offline_*` заглушки)  
- [ ] Зелья: хил по конфигу, требования уровня  
- [ ] Использование из сумки + автоюз в бою (как autofight BuyPotion/Use)  
- [ ] Миграция/замена offline inventory  
- [ ] APK  

---

### PORT-06 — Локация Haitan — `В работе`

- [x] `Resources/OriginalMap/110200011.json` из MapData XML (103 mob / 17 NPC / 9 gather / birth)  
- [x] `OriginalHaitanBootstrap`: birth, NPC, gather, monsters; demo PvE-арена сносится  
- [x] Подтверждено: `CoastalPrototype` уже содержит `cs_haitan` / dixing / walksurface / пропы (как оригинал)  
- [x] Убраны interim walk-slabs (используется сцена)  
- [x] modelId меши: `HaitanModels.json` + `Resources/Animation/*` (t_model/t_npc); fallback 880210118→108  
- [ ] APK — **только после полной локации + квесты + графика** (указание владельца)

### PORT-07 — Квесты Haitan — `В работе`

- [x] Экспорт `HaitanQuests.json` — **99** квестов `1001701`–`1001799` из `QuestConfig.lua`  
- [x] Runtime по kind (gossip/gather/kill) + `finishNpcId` / `questGoals` / награды / `afterQuest`  
- [x] Kill засчитывает только нужный `templateId`; gather/NPC — по ObjectId  
- [x] item rewards `prof*` / `otherReward` при CompleteQuest  
- [x] Модели NPC по look model из t_npc  
- [x] Диалоги/stories: оригинал → RU (`HaitanDialogues.json`, `OfflineDialogueHud`)

### Changelog PORT-06/07 (27.11.2026)
- Источник: MapData `110200011.xml` + `QuestConfig.lua` + `t_doc_quest` / `t_doc_dialogue`
- Перенесено: spawn graph + полная квестовая таблица Haitan + RU-диалоги (перевод с оригинала)
- APK: не собираем до готовности локации (owner)

---

### PORT-07 — Квесты Haitan полный паритет — `Запланировано`

- [ ] NPC Тиас и gather-объекты — оригинальные модели/точки из MapData `110200011`  
- [ ] Диалоги/accept/finish как QuestConfig (не только кнопка)  
- [ ] Цепочки после 1001703 по конфигу оригинала  
- [ ] APK  

---

### PORT-08 — Прочие локации — `Запланировано`

Очередь строить из `t_map` / каталога environments; **одна карта = один этап + APK**.

- [ ] Составить упорядоченный список карт после Haitan  
- [ ] Повторять PORT-06 паттерн  

---

### PORT-09 — Боевая механика — `Запланировано`

- [ ] Скиллы: дальность, CD, мультикиты, эффекты статусов по SkillConfig  
- [ ] Мобы: AI/агро/респавн как на карте  
- [ ] Убрать расхождения prototype combat  

---

### PORT-10 — Прогрессия / эквип — `Запланировано`

- [ ] Уровни, статы, экипировка из equipment/weapons каталогов  
- [ ] UI эквипа как оригинал  

---

### M06+ Online — `Запланировано`

После играбельного оффлайн-паритета Haitan (PORT-05…07 минимум):

- [ ] Контракт auth/heroes ближе к оригинальному протоколу / текущему LunexUfa Server  
- [ ] Go + PostgreSQL в `LegacyOfEternity/Server`  
- [ ] Клиент online mode рядом с offline  

---

## §5. Активная очередь P0 (сейчас)

1. **PORT-06/07** — добить modelId меши мобов/NPC + item rewards/диалоги; сцена `cs_haitan` уже на месте.  
2. **PORT-02/03** — modelId / VFX по каталогу.  
3. **APK** — один билд после полной локации Haitan (квесты+графика), не промежуточные.  
4. Не начинать M06, пока Haitan не принят.

---

## §6. Осознанно не делаем

- Параллельный «другой» геймдизайн вместо оригинала.  
- Основной APK из Unity 2017.  
- Слепой импорт XLua/NGUI без замены поведения.  
- Релизный keystore до стабильного паритета.

---

## §7. Шаблон changelog среза

```markdown
### Changelog PORT-XX (<дата>)
- Источник: C:\Source\Resources\… / Darklord-Final\…
- Перенесено: …
- Поведение сверено с: <файл конфига / сцена>
- APK: Builds/Android/LegacyOfEternity-x.y.z-alpha.apk
- Остаток долга: …
- Следующая P0: …
```

---

## §8. Команды сборки (эта машина)

```powershell
$env:LOE_ANDROID_TOOLCHAIN_ROOT='C:\asdk'
$env:LOE_SKIP_ANDROID_AUDITS='1'
$env:JAVA_HOME='C:\asdk\OpenJDK'
& 'C:\SOURCE\LunexUfa\Toolchain\Unity6000.3.29f1\Editor\Unity.exe' `
  -batchmode -nographics -quit `
  -projectPath 'C:\LegacyOfEternity\Client' `
  -executeMethod LegacyOfEternity.Editor.BuildOfflineAndroid.Run `
  -logFile 'C:\LegacyOfEternity\Builds\Android\Unity-build.log'
```

---

*Конец. Исполнитель не оставляет P0 на закрытом срезе и не подменяет оригинал выдуманными механиками.*
