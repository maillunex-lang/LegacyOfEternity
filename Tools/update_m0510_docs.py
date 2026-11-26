from pathlib import Path
import json
r=Path(r"C:\Source\LegacyOfEternity")
title="## M05.10 Original camera and HUD correction / Android 0.5.3 Alpha (2026-11-26)"
desc=("M05.10 reprioritized by owner after phone video: restore usable original camera/UI rather than add more PvE mechanics. "
"Read-only original sources: Resources/UI/Prefabs/UIMain/SkillUIParent.prefab and UIMain.prefab, "
"Resources/Scene/Camera/FBCamera.prefab, Scripts/UI/CameraControl.cs, "
"Scripts/Core/Camera/CameraFollow.cs, Assets/Scene/allMap/cs_yw_haitan01/cs_yw_haitan01.unity. "
"Original HUD authored at 1136x640 while current CanvasScaler was 1920x1080; corrected reference to 1136x640, "
"preserving source SillUI slot coordinates, authentic combat frame art, and 20 per-class skill icons. "
"Overlaid attack/skill labels were hidden but existing Button event handlers and Cooldown Overlay preserved. "
"Restored original atlas art UIMain:main_guaji.png and main_guaji_disable.png for AUTO, "
"UIMain:btn_switch1.png for target switching, with clearly readable AUTO state and ЦЕЛЬ label; "
"btn_switch1 is a verified original atlas sprite reused for this new targeting action, not proof of exact historical targeting icon. "
"Callback methods Toggle and TargetNext preserved, AUTO icon follows actual AutoCombatController.IsEnabled. "
"Both action buttons laid out separately in upper-right at source Canvas units [-69,312] and [-211,312]. "
"Original CameraFollow.getTargetPosition returns target.position+Vector3.up*1, restored targetOffset 1 metre, "
"camera distance 12.5, height 8, yaw 50 for coast scene. "
"CameraControl handling restored axis-exclusive one-finger horizontal yaw or vertical height (after 25 px threshold); "
"two-finger opposing movement pinches zoom, while parallel two-finger movement performs no orbit/height changes, "
"matching source CheckTwo/Move/Scale distinctions. "
"Explicit EventSystem.RaycastAll on finger start prevents camera stealing touch from any raycastable HUD element; "
"camera stops single-finger orbit when another finger is active on UI. "
"Camera occlusion spherecast ignores near-origin hits less than collider radius. "
"Original zoom bounds 5..35 and height 3..15 preserved. "
"Unity Editor installer LOE_M0510_INSTALLED, independent audit LOE_M0510_AUDIT PASS "
"sourceCanvas=1136x640 originalArt=3 skills=5 auto=1 target=1 cameraFocus=1. "
"Full Android build 0 errors, 40 warnings, all M057/M058/M059/M0510 checks PASS; "
"APK versionName=0.5.3 versionCode=12 arm64 v2 debug-signed same cert, "
"actual APK bytes=42946355 SHA256=11172F3CF6AFFBAD34061A1A21ABF13C5D0B8F4CBFBD74AF1EBB9BAA6FE761DC. "
"Published at https://rikagro.ru/MMORPG/downloads/LegacyOfEternity-0.5.3-alpha.apk HTTP 200 and verified hash. "
"No physical Android device QA for finger ownership, input usability, graphic scale or collisions yet.")
for name in ("ROADMAP.md","CURRENT-STATUS.md","COMBAT-PVE-QA.md"):
 p=r/"Docs"/name
 before=p.read_text(encoding="utf-8-sig")
 if title not in before:p.write_text(before.rstrip()+"\n\n"+title+"\n\n"+desc+"\n",encoding="utf-8")
p=r/".ai"/"PROGRESS.json"
doc=json.loads(p.read_text(encoding="utf-8-sig"))
doc["M05.10 Original controls camera and HUD correction 0.5.3 Alpha"]=desc
p.write_text(json.dumps(doc,ensure_ascii=False,indent=2)+"\n",encoding="utf-8")
source={
 "milestone":"M05.10 original controls corrected",
 "original_ui_sources":[
  "Assets/Resources/UI/Prefabs/UIMain/SkillUIParent.prefab",
  "Assets/Resources/UI/Prefabs/UIMain/UIMain.prefab",
  "Assets/Atlas/UIMain.png.meta"
 ],
 "original_camera_sources":[
  "Assets/Scripts/Core/Camera/CameraFollow.cs",
  "Assets/Scripts/UI/CameraControl.cs",
  "Assets/Scene/allMap/cs_yw_haitan01/cs_yw_haitan01.unity"
 ],
 "hud_original_reference":{"width":1136,"height":640},
 "camera_original_coastal":{"distance":12.5,"height":8,"yaw":50,"focus_offset_y":1,"distance_limit":[5,35],"height_limit":[3,15],"rotate_ratio":0.25,"height_ratio":0.10,"zoom_ratio":0.05},
 "auto_original_sprites":["UIMain:main_guaji.png","UIMain:main_guaji_disable.png"],
 "target_reused_original_atlas_sprite":"UIMain:btn_switch1.png",
 "target_historical_exact_sprite_verified":False,
 "skills":"SkillUIParent/SillUI original source positions unchanged; active 5 callbacks intact",
 "android_version":"0.5.3 Alpha",
 "android_versionCode":12,
 "apk_bytes":42946355,
 "apk_sha256":"11172F3CF6AFFBAD34061A1A21ABF13C5D0B8F4CBFBD74AF1EBB9BAA6FE761DC",
 "manual_android_QA":"pending"
}
(r/"Docs"/"M0510-original-camera-hud-map.json").write_text(json.dumps(source,ensure_ascii=False,indent=2)+"\n",encoding="utf-8")
print("M0510_DOCS_UPDATED")
