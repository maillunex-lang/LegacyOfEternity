# Фактический статус: 26.11.2026

- Unity 6000.3.27f1, Android landscape, Windows тест.
- Перенесено из собственного архива LunexUfa 1788 анимационных ассетов и связанные ресурсы. Текущая папка `Client/Assets/LegacyAssets` около 1.17 ГБ.
- Имеются 4 визуальных класса: Warrior, Assassin, Mage, Archer.
- Сцена `CoastalPrototype.unity`: камера, контроллер, HUD, joystick, локальные combat buttons.
- Android 0.1.0 был собран и доступен на личном CDN; физические тесты выявили ошибки Idle/Run.
- Исправление 0.1.1: для всех четырёх классов импортируются `@stand.FBX` (циклическая стойка) и `@run.FBX` (циклический бег). Результат импортного Unity-аудита: 4/4. Android APK 0.1.1 собран: 0 ошибок, 0 предупреждений, подпись v2 действительна, SHA256 e46cac4d51bb4682dd420859d8f080563d40faccc95105ca642cba5a2d9ce014. Пользователь подтвердил исправление на физическом Android: «Все работает!!!».
- Публичный репозиторий: исходный код и ресурсы, Git LFS; проверка полного клона обязательна.
- Go-сервер: пока только каркас с health endpoint; нет подключённого клиента, боевого сетевого сервера и persistent-персонажей.
- Известный build риск на Windows: Ninja может упереться в MAX_PATH к NDK. Использовать короткий путь к SDK/NDK.
- Права: LunexUfa подтверждает, что игровые ресурсы созданы им, прежнее имя Darklord. Условия лицензирования отдельно не опубликованы.

- VPS smoke: Docker Compose config OK, Go Docker build OK, GET /health -> status=ok, процесс контейнера остановлен по окончании теста.

- GitHub main: `b540589` содержит 7325 файлов; Git LFS upload 2152 уникальных объектов (~1.1 GB) прошёл; чистый clone+LFS pull+fsck на втором пути Windows завершился с кодом 0.
- Ubuntu VPS: `/home/gamedev/LegacyOfEternity` склонирован напрямую из GitHub в sparse-режиме (Server/Deploy/Docs), Go test, Compose config и Docker build проверены (exit 0).
- GitHub Actions `Server smoke` прошёл успешно; GitHub prerelease v0.1.1-alpha содержит Android APK. Unity 6000.3.27f1 открыл clean clone и прошёл боевой аудит: EXIT=0, 28 клипов, 12720 curves, 0 проблем.
- APK 0.1.1 подписан стандартным ключом Unity, так как `useCustomKeystore=false`; для обновления пользователей между разными ПК нужен постоянный релизный ключ с безопасным внешним хранением.

- M05 PvE 0.2.0: три monster_001..003, локальные HP/атаки/skills/агро/смерть/respawn и выбор цели; Windows-сборка успешно (0 errors, 7 warnings); практическая проверка атаки 65->53, skill1 53->33, уничтожение, выбор следующей цели, смерть/возрождение героя. См. COMBAT-PVE-QA.md. Android физическая проверка 0.2.0 ожидается.

- Android 0.2.0 Alpha: Unity BuildPipeline Succeeded, 0 errors, 5 warnings, APK 37766832 bytes, arm64-v8a, minSdk 26 targetSdk 36, versionCode 3. APK v2 signature verified, signing certificate SHA256 equal to 0.1.1. SHA256 eba87c88511f2e1d49bf68670eddbda53b507def2a62cec820174448f6599c85. Physical Android QA pending.
