# Legacy of Eternity

Независимая 3D fantasy MMORPG от **LunexUfa** (ранее Darklord).

**Текущий этап:** технический альфа-прототип, **не готовая MMORPG**. Unity-клиент уже показывает четыре класса (воин, ассасин, маг и лучник), прибрежную локацию, перемещение на Android/Windows и локальный демонстрационный бой. Онлайн-мир, авторитетный сервер и полноценная экономика ещё не реализованы.

## Структура репозитория

- `Client/`: **Unity 6000.3.27f1**, сцены, C#-скрипты, импортированные модели, материалы, анимации.
- `Server/`: отдельный минимальный Go HTTP-сервер и точка роста MMO-бэкенда. **Не подключён к игровому клиенту.**
- `Database/`: схема/миграции (по мере реализации).
- `Deploy/`: Docker Compose и шаблоны развёртывания Ubuntu VPS.
- `AssetLibrary/`: каталоги импортированных ресурсов и отчёты.
- `Docs/`: GDD, архитектура, решения, [дорожная карта](Docs/ROADMAP.md), [восстановление на новой машине](Docs/BOOTSTRAP.md), [статус](Docs/CURRENT-STATUS.md).
- `Tools/`: инструменты импорта и проверки; `Tests/`: тесты.
- `.ai/`: машинно-читаемые планы и прогресс.

## Начало работы

```bash
git clone https://github.com/maillunex-lang/LegacyOfEternity.git
cd LegacyOfEternity
git lfs install
git lfs pull
```

Откройте **папку `Client`**, а не корень репозитория, в Unity Hub с редактором **6000.3.27f1**. Целевая тестовая сцена: `Client/Assets/LegacyOfEternity/Scenes/CoastalPrototype.unity`. Для Android нужны Unity Android Build Support, SDK, NDK, OpenJDK. Сборка пока завязана на редакторный инструмент `LegacyOfEternity.Editor.AndroidPreviewBuilder.Build`.

Полные инструкции, ограничения исходной альфы и настройки VPS: [Docs/BOOTSTRAP.md](Docs/BOOTSTRAP.md).

## Как работать над игрой

1. Склонировать с Git LFS, открыть `Client` и проверить сборку.
2. Реализовывать отдельные механики из [ROADMAP](Docs/ROADMAP.md); держать клиент и будущую серверную логику раздельно.
3. Подтвердить локальные тесты и делать обычные коммиты в GitHub. Не коммитить кэши Unity, секреты, локальные Android SDK, готовые APK и персональные ключи.
4. Ubuntu VPS использовать для серверных сервисов и PostgreSQL; **Unity Editor не требуется на VPS для работы запущенного MMO-сервера**.

## Права

Автор проекта и игровых ресурсов по его заявлению: **LunexUfa (ранее Darklord)**. Публичный исходный код не означает автоматически разрешение на перепродажу, повторное использование ассетов или выпуск чужих производных игр. Формальные условия лицензирования должны быть опубликованы владельцем отдельно.

**Важно:** скелет Go-сервера в репозитории только предоставляет health endpoint; серверные MMO-механики не готовы.

## Android APK 0.1.1 (26.11.2026)

Тестовая сборка со спокойной стойкой и зацикленным бегом для всех 4 классов: [скачать APK](https://rikagro.ru/MMORPG/downloads/LegacyOfEternity-0.1.1-alpha.apk). Пользователь подтвердил работу исправления на физическом Android.

[GitHub Issues](https://github.com/maillunex-lang/LegacyOfEternity/issues) содержит задачи разработки. Все исходники доступны в main, большие игровые ресурсы скачиваются с Git LFS.

GitHub Release: https://github.com/maillunex-lang/LegacyOfEternity/releases/tag/v0.1.1-alpha
