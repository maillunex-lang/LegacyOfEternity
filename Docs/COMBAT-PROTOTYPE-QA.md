# Legacy of Eternity: four-class combat prototype QA
Date: 2026-11-26

## Build
- Unity: 6000.3.27f1, Windows x64 development player.
- Run: C:\Source\LegacyOfEternity\Builds\Windows\LegacyOfEternity.exe
- Scene: Assets/LegacyOfEternity/Scenes/CoastalPrototype.unity
- Build report: Succeeded, 0 errors, 7 warnings, 178,308,131 bytes (approx 170 MiB).
- Log: Docs/unity-combat-windows-build.log

## Verified
- All four classes visibly rendered in standalone Windows player: Warrior, Assassin, Mage, Archer.
- Class selection arrows changed the model and Russian class label.
- Hit prototype reduced HP 100 -> 90.
- Death prototype set HP to 0 and changed character pose.
- Respawn restored HP 100 and normal character pose.
- Unity audit: four AnimatorControllers with 7 action clips each (Attack, Skill1-4, Hit, Death), 28 imported action clips, 12,720 animation curves.
- Skeletal path check: 1,840 matched, 0 unmatched paths.
- Eight HUD button persistent callbacks checked, 0 issues.
- Scene view rendered and preview exported to Docs/coastal-preview.png (camera render excludes UI).

## Incomplete and known limitations
- Prototype combat is local animation+HP only, without real damage, targets, cooldowns, skill effects, hitboxes, or server-authoritative logic.
- Per-frame visual quality of each individual attack/skill animation and gamepad/touch device QA are not yet established.
- Android APK is not built. Windows preview runs successfully.
- Lighting, aggressive dark shadows, sky/environment visuals and original shader fidelity need polish.
- Build reported 7 warnings; no specific warning details were classified from the batch log.
- Windows Firewall asked to permit inbound networking; request was canceled, no firewall permissions were granted.

## Keyboard test controls
- Movement: WASD/arrows; mobile prototype: left joystick.
- Combat: 1 Attack; 2/3/4/5 Skills 1-4; H Take Hit; K Die; R Respawn.
- Class selection: top-left / top-right arrows.
