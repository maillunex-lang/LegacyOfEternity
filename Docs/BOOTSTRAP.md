# Восстановление разработки на новой машине или VPS

## Windows: Unity-клиент

1. Установите Git и Git LFS; `git lfs install`.
2. `git clone https://github.com/maillunex-lang/LegacyOfEternity.git` и `cd LegacyOfEternity && git lfs pull`.
3. Установите Unity Editor **6000.3.27f1** и Android Build Support, SDK, NDK, OpenJDK через Unity Hub.
4. В Unity Hub откройте `Client/`. Дождитесь первого импорта (создание `Library/`).
5. Проверьте сцену `Assets/LegacyOfEternity/Scenes/CoastalPrototype.unity`. Для сборки откройте Build Profiles или вызовите editor method `LegacyOfEternity.Editor.AndroidPreviewBuilder.Build`.
6. В редакторном скрипте `AndroidPreviewBuilder.cs` могут присутствовать локальные пути к Android toolchain. На других Windows-машинах использовать локальную установку Unity, не копировать чужой SDK. Проверить имена путей и переподключить через Preferences > External Tools перед автоматизацией.
7. Проверяйте результат на физическом Android-устройстве, только наличие APK не гарантирует исправления визуальных багов.

## Ubuntu VPS: сервер

```bash
git clone https://github.com/maillunex-lang/LegacyOfEternity.git
cd LegacyOfEternity
git lfs install && git lfs pull
cp Deploy/.env.example Deploy/.env
# задайте свои уникальные секреты и пароли только в .env
docker compose -f Deploy/docker-compose.yml --env-file Deploy/.env up -d --build
curl --fail http://127.0.0.1:18780/health
```

Это поднимает **демонстрационный Go health API**, а не полноценный MMO-сервер. Запускать отдельно от существующих сервисов VPS, не занимать их порты, не публиковать БД наружу.

## Что хранит GitHub

Включено: Unity `Assets/` с соответствующими `.meta`, `Packages/`, `ProjectSettings/`, игровой код, модели/анимации из текущего рабочего Unity-проекта, документация, инструменты импорта и серверный каркас. Большие файлы следует хранить в Git LFS.

Не включено: Unity `Library/`, `Temp/`, `Logs/`, `.utmp/`, Android SDK/NDK, секреты, локальные кэши, APK, исходные отдельные архивы Darklord за пределами данного Unity-проекта.

При переезде **проверьте Git LFS quota и наличие всех объектов**. `git clone` без `git lfs pull` может оставить вместо моделей маленькие файлы-указатели.

## Важные ограничения

Домен и IP сервера не должны быть жестко зашиты в клиент. Для production понадобятся TLS, authoritative MMO протокол, авторизация, бэкапы PostgreSQL, CI и секреты. Сегодня они ещё не закончены.
