# Legacy of Eternity — дорожная карта для исполнителей

**Дата актуализации:** 27.11.2026 (hotfix visibility)  
**Владелец:** LunexUfa  
**Канонический файл:** этот документ (`Docs/ROADMAP.md`). Любой ИИ-исполнитель **обязан** читать его в начале задачи и **обновлять** в конце (см. §0).

### Политика недочётов vs фич (решение владельца 27.11)

- **Блокеры приёмки** (не видно героя, сплошные фиолетовые/белые квадраты VFX, краш, нельзя пройти квест) → **сразу короткий патч-APK**, потом снова фичи.
- **Косметика / UX / NPC-арт / диалоги** → копим в долг этапа, чиним отдельно или в M05.19 / GATE-ART.
- Не останавливать дорожную карту на бесконечной полировке VFX parity.

Связанные файлы:

| Файл | Назначение |
| --- | --- |
| `Docs/CURRENT-STATUS.md` | Факт «что сейчас на диске / какой APK» |
| `Docs/BOOTSTRAP.md` | Подъём машины, Unity, Docker health |
| `Docs/DECISIONS.md` | Юридические / продуктовые решения |
| `Docs/ARCHITECTURE.md` | Архитектура (если расходится с ROADMAP — править оба) |
| `C:\SOURCE\LunexUfa\ROADMAP.md` | Указатель: лаборатория, не целевой клиент |

---

## §0. Обязательный протокол для ИИ-исполнителя

### 0.1 Перед работой

1. Прочитать **этот** `ROADMAP.md` и `CURRENT-STATUS.md`.
2. Определить **активную веху** (статус `В работе` или первая `Запланировано` в P0).
3. Не начинать импорт Codex/`C:\Source\Resources` и не трогать M06 online, пока владелец явно не сказал или веха не открыта в P0.
4. Рабочая копия клиента: `C:\LegacyOfEternity`. Ветка: `cursor/<кратко>-270c` от актуальной рабочей ветки / `main`.
5. Не использовать `C:\SOURCE\LunexUfa\Client` как целевой Android-клиент.

### 0.2 Во время работы

1. Крупный этап = одна логическая поставка + один APK на приёмку. Мелкие фиксы внутри этапа — без отдельной приёмки.
2. Сцену `Assets/LegacyOfEternity/Scenes/CoastalPrototype.unity` не перегенерировать без нужды; новые системы — runtime (`RuntimeInitializeOnLoad` / AddComponent), Editor-установщики — только если уже есть паттерн M05x.
3. APK **не коммитить**. Большие бинарники — Git LFS.
4. Android toolchain на этой машине: `LOE_ANDROID_TOOLCHAIN_ROOT=C:\asdk`, `LOE_SKIP_ANDROID_AUDITS=1` при холодном Library; у `C:\asdk\SDK\platform-tools` не должно быть `package.xml` (оставить `source.properties`).
5. Сборка оффлайн-APK:  
   `Unity.exe -batchmode -nographics -quit -projectPath C:\LegacyOfEternity\Client -executeMethod LegacyOfEternity.Editor.BuildOfflineAndroid.Run -logFile <log>`  
   Успех: в логе `LOE APK OK` и `result=Succeeded errors=0`.
6. Версии: поднимать `bundleVersion` / `bundleVersionCode` / имя APK в `Editor/BuildOfflineAndroid.cs` на каждый этапный билд.

### 0.3 После работы (обязательная актуализация)

Исполнитель **в том же PR/коммите этапа** обновляет:

1. **`Docs/ROADMAP.md`**
   - Статус вехи: `Запланировано` → `В работе` → `Готово (версия APK)`.
   - Чекбоксы подзадач `[x]` / `[ ]`.
   - Блок «Активная очередь P0».
   - «Дата актуализации» в шапке.
2. **`Docs/CURRENT-STATUS.md`**
   - Что в коде, путь к APK, чеклист приёмки для владельца, «Дальше».
3. Коммит с понятным сообщением; push ветки `cursor/*-270c`.
4. Если изменились продуктовые решения — строка в `Docs/DECISIONS.md`.

**Не закрывать этап**, пока не обновлены ROADMAP + CURRENT-STATUS + путь к APK (если этап с поставкой).

### 0.4 Статусы вех (словарь)

| Статус | Значение |
| --- | --- |
| `Готово (x.y.z)` | Код + APK собраны; владелец может принимать |
| `В работе` | Текущий фокус исполнителя |
| `Запланировано` | Не начинать, пока не в P0 |
| `Блокер` | Ждёт внешнего входа (владелец / Codex / железо) |
| `Отложено` | Сознательно не делаем |

---

## §1. Продуктовые решения (зафиксировано)

1. **Целевой клиент:** `C:\LegacyOfEternity`, Unity **6000.3.x**, Android landscape ARM64.
2. **`C:\SOURCE\LunexUfa`:** эталон Darklord 2017, Server/Admin лаборатория, Content/Baseline — **не** основной APK.
3. **Приёмка владельца:** APK после **каждого крупного этапа**, не после каждой мелкой правки.
4. **Codex / `C:\Source\Resources`:** адаптация арта оригинала под Unity 6. **Импорт в LoE только по явной команде владельца** («можно импортировать Resources»).
5. **Сеть (M06):** после стабильного оффлайн-цикла на телефоне; не параллелить с незакрытым оффлайн-контентом без указания владельца.
6. **Права на ресурсы:** подтверждены владельцем (см. `DECISIONS.md` / `ASSET-RIGHTS.md`).

---

## §2. Карта репозитория (куда лезть)

```
C:\LegacyOfEternity\
  Client\                          Unity 6 проект
    Assets\LegacyOfEternity\
      Scenes\CoastalPrototype.unity
      Scripts\Offline\             оффлайн вход/квест/мир
      Scripts\Combat\              PvE, VFX trigger
      Scripts\Player\              мотор, классы, joystick
      Scripts\Camera\
      Effects\                     префабы навыков (Unity6)
      Editor\BuildOfflineAndroid.cs
    Assets\LegacyAssets\           перенесённый legacy арт/шейдеры
    Assets\Resources\OfflineUI\    splash/login спрайты
  Builds\Android\                  локальные APK (не в git)
  Docs\                            ROADMAP, STATUS, аудиты M05x
  Server\                          Go health scaffold (не полный MMO)
  Deploy\                          docker-compose demo

C:\Source\Resources\               Codex-адаптация (импорт по команде)
C:\SOURCE\LunexUfa\                эталон / Server:8080 Admin:8090
C:\SOURCE\LunexUfa\Content\HaitanPrototypeQuests.json
C:\SOURCE\LunexUfa\Baseline\       механики Haitan, скрины
```

Ключевые оффлайн-скрипты:

| Скрипт | Роль |
| --- | --- |
| `OfflineDemoStore.cs` | JSON-сейв аккаунтов/героев/квеста |
| `OfflineLoginGate.cs` | UI входа (splash), разблокировка геймплея |
| `OfflineSession.cs` | Текущий токен/герой в рантайме |
| `OfflineQuestHud.cs` | Прогресс квеста (HUD) |
| `OfflineWorldBootstrap.cs` | Спавн Тиаса + gather |
| `OfflineWorldInteractable.cs` | Интеракты в мире |
| `OfflineInteractHud.cs` | Кнопка Поговорить/Собрать/Сдать |
| `OriginalCombatEffects.cs` | VFX навыков + sanitize magenta |
| `CombatEnemy.cs` | `Killed` → прогресс kill-квеста |

Сейв оффлайн: `Application.persistentDataPath/loe_offline_demo.json` (не в git).

---

## §3. Сводная таблица вех

| ID | Этап | Статус | APK / критерий |
| --- | --- | --- | --- |
| M00–M04 | Каркас, ассеты, Coastal, UI/камера | Готово частично (0.5.x) | Сцена играбельна |
| M05.0–M05.15 | Бой, VFX, HUD, камера, контролы | Готово частично (≤0.5.5) | 4 класса, PvE |
| M05.16 | Оффлайн вход + квест | Готово (0.6.x) | login→hero→quest |
| M05.16b | Login UI «как в оригинале» | Готово (0.6.2) | splash + крупные поля |
| M05.16c | Magenta VFX на Android | Готово (0.6.3) | GLES3 + null mats |
| M05.17 | Мировой оффлайн-квест Haitan | Готово (0.7.0) | Тиас/gather/kill/сдача |
| **M05.17b** | **Патч видимости Android (герой+VFX)** | **Готово (0.7.1)** | Unlit герои, soft-particle fallback |
| **M05.18** | **Оффлайн лут + инвентарь-заготовка** | **Запланировано** | дроп/сумка, APK 0.8.0 |
| M05.19 | Полировка оффлайн UX / маркеры квеста | Запланировано | радар/стрелка, диалоги |
| GATE-ART | Импорт `C:\Source\Resources` | Блокер (ждёт команду) | каталог в LoE |
| M06 | Online auth + сессии (Go) | Запланировано | клиент↔Server |
| M07 | Серверные квесты/инвентарь/лут | Запланировано | authoritative |
| M08–M12 | Соц./экономика/оптимизация/релизы | Запланировано | по мере M06–M07 |

---

## §4. Детальные вехи и подзадачи

### M05.16 — Оффлайн вход + базовый квест — `Готово (0.6.0–0.6.2)`

- [x] `OfflineDemoStore`: register/login, миры, герои, 4 класса
- [x] `OfflineLoginGate`: блок геймплея до входа
- [x] Квест 1001701–1001703 (talk/gather/kill) + награды XP/money
- [x] Persist в `loe_offline_demo.json`
- [x] Крупный uGUI login + `Resources/OfflineUI` (splash `bg_login`, кнопки)
- [x] APK 0.6.0–0.6.2

**Не делать снова:** не откатывать на OnGUI-логин; не удалять Offline store.

---

### M05.16c — Magenta skill VFX — `Готово (0.6.3)`

- [x] Убрать `gles3` из `#pragma exclude_renderers` в `LD_glow`, `LD_uv_blend_depth`, `LD_water`
- [x] `M053AndroidVfxMaterialsFix`: заполнить 307 null material slots
- [x] Runtime `SanitizeMagentaMaterials` в `OriginalCombatEffects`
- [x] Graphics API: Vulkan → OpenGLES3 fallback
- [x] APK 0.6.3

**Регрессия:** при новых VFX-префабах снова гонять Fix/Audit перед APK.

---

### M05.17 — Мировой оффлайн-квест на Coastal — `Готово (0.7.0)`

Цепочка: **1001701 talk → 1001702 gather → 1001703 kill×3 → 1001704 turn-in**.

- [x] Спавн NPC **Тиас** (`OfflineWorldBootstrap` / `OfflineWorldInteractable` kind Guide)
- [x] Точки **прибрежный хлам** (GatherScrap), одна сбора достаточно для цели
- [x] `OfflineInteractHud`: Поговорить / Собрать / Сдать + подсказки
- [x] Kill через `CombatEnemy.Killed` → `ApplyQuestEvent(..., "kill")`
- [x] После 3 киллов → 1001704; сдача Тиасу → `questId=0` + награда
- [x] Тексты как в эталоне «Близкая опасность (n)»
- [x] HUD прогресса без старых кнопок talk/gather
- [x] APK `Builds/Android/LegacyOfEternity-0.7.0-alpha.apk`

**Приёмка владельца (чеклист):**

1. Регистрация / вход → выбор/создание героя  
2. Подойти к синему Тиасу → Поговорить  
3. Собрать хлам (золотые маркеры)  
4. Убить 3 монстров  
5. Вернуться к Тиасу → Сдать  
6. Перезапуск приложения: XP/монеты/квест сохранены  

**Известный долг:** визуалы NPC/хлам — примитивы (капсула/куб), ждут GATE-ART.

---

### M05.18 — Оффлайн лут + инвентарь-заготовка — `Запланировано` (**следующий крупный этап**)

**Цель этапа:** после убийства монстра игрок получает лут; есть простой экран сумки; всё в оффлайн JSON; APK **0.8.0** на приёмку.

#### Подзадачи (исполнять по порядку)

**A. Данные**

- [ ] Расширить `OfflineDemoStore.Hero`: массив слотов инвентаря (id, count) или список `ItemStack`
- [ ] Таблица предметов-заготовка (минимум 5–8 id): монета уже есть; добавить scrap, potion, soft weapon/mat из простых id (`offline_scrap`, `offline_fang`, …)
- [ ] Миграция сейва: старые JSON без инвентаря → пустой инвентарь, не ломать логин
- [ ] `ApplyLoot(hero, itemId, count)` + `SaveHero`

**B. Дроп с монстров**

- [ ] На `CombatEnemy.Killed` (или рядом в offline-слое): ролл лута 1–2 предмета + небольшие монеты
- [ ] Таблица дропа по «типу» врага (пока один пул на всех Coastal)
- [ ] Короткий feedback в HUD («+Fang x1») 2–3 сек — OnGUI или uGUI, крупный шрифт под мобильный

**C. UI сумки**

- [ ] Кнопка «Сумка» / «I» на HUD (не перекрывать джойстик и скиллы; якорь top-right или left под квест-боксом)
- [ ] Панель: список предметов имя × count, закрытие
- [ ] Пока **без** экипировки и без использования зелий (только просмотр) — если успеваете: use potion = +dummy heal feedback

**D. Связь с квестом**

- [ ] Gather 1001702 может класть `offline_scrap` в инвентарь при сборе (синхрон с прогрессом квеста)
- [ ] Не ломать цепочку 1001701–1001704

**E. Документация и поставка**

- [ ] `BuildOfflineAndroid`: version **0.8.0**, versionCode +1
- [ ] Собрать APK, путь в `CURRENT-STATUS.md`
- [ ] Обновить эту секцию чекбоксами; статус → `Готово (0.8.0)`
- [ ] Чеклист приёмки в CURRENT-STATUS: вход → квест → килл с лутом → открыть сумку → перезапуск

**Критерий готовности M05.18:** на телефоне видно дроп после килла, сумка показывает предметы после рестарта приложения.

**Не входит в M05.18:** серверный инвентарь, аукцион, крафт, полный UI оригинала.

---

### M05.19 — Полировка оффлайн UX — `Запланировано`

- [ ] Стрелка/компас к текущей цели квеста (Тиас / gather / ближайший враг)
- [ ] Простой диалог Тиаса (2–3 строки текста) вместо мгновенного complete
- [ ] Имена/HP врагов уже есть — проверить читаемость на телефоне
- [ ] Заменить примитивы NPC на временные меши из уже импортированных `LegacyAssets` (если есть подходящий NPC), **не** полный Codex-импорт
- [ ] APK этапный (например 0.8.1 / 0.9.0) только если объём тянет на «крупный этап»

---

### GATE-ART — Импорт Codex Resources — `Блокер` (только по команде владельца)

**Триггер:** фраза владельца вроде «можно импортировать Resources» / «Codex готов».

Источник: `C:\Source\Resources\` (каталоги `01_Characters` … `18_UnityPackages`, `14_Catalog\*.json`).

#### Подзадачи (когда откроют)

- [ ] Прочитать `14_Catalog` JSON (классы, skills VFX, animations) — план маппинга в LoE
- [ ] Импорт **порциями** (не всё сразу): сначала UI/characters OR VFX — согласовать с владельцем одну порцию
- [ ] Сохранять `.meta` GUID где каталог это требует; SHA256 сверка при наличии отчётов Codex
- [ ] Не затирать рабочие `Effects/*_Unity6.prefab` без backup/ветки
- [ ] После порции: Editor audit (M053 / skill icons) + APK smoke
- [ ] Обновить ROADMAP: какие папки импортированы, какие нет
- [ ] Отдельная ветка `cursor/import-resources-<slice>-270c`

**Пока блокер:** не копировать `C:\Source\Resources` в `Client/Assets` самостоятельно.

---

### M06 — Online backend — `Запланировано` (после оффлайн-приёмок)

Ориентиры: `C:\LegacyOfEternity\Server` (Go health), идеи API из `C:\SOURCE\LunexUfa\Server`.

#### Подзадачи (когда откроют)

- [ ] Зафиксировать контракт: register/login/worlds/heroes (совместимый с OfflineDemoStore полями где возможно)
- [ ] PostgreSQL schema (accounts, heroes, sessions)
- [ ] Go: auth + session token; health уже есть — расширить
- [ ] Docker compose в `Deploy/` с реальными сервисами (не только health)
- [ ] Клиент: режим Online рядом с Offline (флаг/экран), не ломая оффлайн-вход
- [ ] TLS / секреты — не хардкодить домен в клиент (`BOOTSTRAP.md`)
- [ ] Smoke: два клиента / один сервер
- [ ] APK + server runbook в CURRENT-STATUS

**Не копипастить** клиент Unity 2017; переносить идеи протокола и данные.

---

### M07 — Серверные квесты / инвентарь / лут — `Запланировано`

- [ ] Перенос цепочки Haitan 1001701+ на серверные цели
- [ ] Authoritative kill/gather validation
- [ ] Инвентарь на сервере; клиент отображает
- [ ] Согласование с GATE-ART NPC/точками сбора на карте

---

### M08–M12 — Дальше — `Запланировано` (кратко)

| ID | Содержание | Когда |
| --- | --- | --- |
| M08 | Соц. (партия/чат минимум) | После M06–M07 |
| M09 | Экономика / магазин | После инвентаря |
| M10 | Оптимизация Android (draw calls, атласы, шейдеры) | После GATE-ART |
| M11 | CI сборка APK / checks | По возможности раньше |
| M12 | Релизный keystore, store listing | После стабильных альф |

---

## §5. Активная очередь P0 (сейчас)

1. **M05.17b** — патч видимости по видео приёмки 0.7.0 → APK **0.7.1**.  
2. Затем **M05.18** — оффлайн лут + сумка → APK **0.8.0**.  
3. Не стартовать **GATE-ART** / **M06** без команды владельца.

---

## §6. Осознанно отложено

- Полный порт сцены Haitan 2017 и всех NPC/blocker-геометрии.
- Слияние git LunexUfa ↔ LegacyOfEternity.
- Релизный keystore / store.
- Параллельный Android-клиент из Unity 2017.

---

## §7. Шаблон обновления (копировать в конец коммита этапа)

```markdown
### Changelog этапа <ID> (<дата>)
- Сделано: …
- APK: Builds/Android/LegacyOfEternity-x.y.z-alpha.apk
- Статус вехи: Готово (x.y.z)
- Следующая P0: …
- Блокеры: нет | …
```

Вписать краткий блок также в `CURRENT-STATUS.md`.

---

## §8. Быстрые команды (эта машина)

```powershell
# Сборка этапного APK
$env:LOE_ANDROID_TOOLCHAIN_ROOT='C:\asdk'
$env:LOE_SKIP_ANDROID_AUDITS='1'
$env:JAVA_HOME='C:\asdk\OpenJDK'
# при необходимости:
# Move-Item C:\asdk\SDK\platform-tools\package.xml C:\asdk\SDK\platform-tools\package.xml.bak -Force
& 'C:\SOURCE\LunexUfa\Toolchain\Unity6000.3.29f1\Editor\Unity.exe' `
  -batchmode -nographics -quit `
  -projectPath 'C:\LegacyOfEternity\Client' `
  -executeMethod LegacyOfEternity.Editor.BuildOfflineAndroid.Run `
  -logFile 'C:\LegacyOfEternity\Builds\Android\Unity-build.log'
```

Unity Editor version на машине может быть **6000.3.29f1** (см. фактический путь Toolchain); в BOOTSTRAP указан 6000.3.27f1 — при расхождении править BOOTSTRAP под установленный Editor.

---

*Конец дорожной карты. Исполнитель: не оставляй статусы «В работе» на уже сданных вехах; не оставляй P0 на закрытом этапе.*
