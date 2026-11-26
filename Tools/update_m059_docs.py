from pathlib import Path
import json
r=Path(r"C:\Source\LegacyOfEternity")
title="## M05.9 Original camera + touch gestures / 0.5.2 Alpha (2026-11-26)"
description=("Camera port of original Coastal scene camera. Source read-only Assets/Scene/allMap/cs_yw_haitan01/"
"cs_yw_haitan01.unity has CameraFollow initDistance=12.5, height=8, yaw=50, and original "
"CameraFollow.cs computes vertical elevation height * (distance / 11), offset from target, LookAt. "
"CameraControl.cs source input constants minDistance=5 maxDistance=35, minHeight=3 maxHeight=15, "
"RotateRatio=.25 degrees/pixel, HeightRatio=.10, ScaleRatio=.05. "
"Prototype OrbitFollowCamera now follows original camera transform, clamps zoom/height, supports "
"one finger drag yaw/height, two finger midpoint pan yaw/height, twist yaw and pinch zoom, smoothing, "
"terrain/building occlusion spherecast, and mouse wheel/right-drag editor controls. "
"Touch input blocks fingers which began over UI (joystick, attack/skills, AUTO); "
"the Unity screen-space UI preserves original combat control wiring and button callbacks. "
"Editor M059OriginalCamera.Run installed source profile; Editor M059OriginalCamera.Audit passed "
"distance12.5 height8 yaw50 bounds5..35 and3..15. Full Android BuildM05Android passed "
"M057 skill icons M058 combat layout M059 camera audits: build succeeded, 0 errors, 40 warnings. "
"APK versionName=0.5.2 versionCode=11 arm64 v2 debug signed, 42,942,139 bytes, "
"SHA256 7E664B96DAC60499E684AEF672211E95DE30CC6AAE94BBA18E3B29E7954C8F74. "
"Published https://rikagro.ru/MMORPG/downloads/LegacyOfEternity-0.5.2-alpha.apk "
"HTTP 200, matching size and hash. Physical Android multitouch, camera collision, "
"UI ownership and visual framing are not yet device-tested.")
for name in ["ROADMAP.md","CURRENT-STATUS.md","COMBAT-PVE-QA.md"]:
 p=r/"Docs"/name
 old=p.read_text(encoding="utf-8-sig")
 if title not in old:p.write_text(old.rstrip()+"\n\n"+title+"\n\n"+description+"\n",encoding="utf-8")
p=r/".ai"/"PROGRESS.json"
obj=json.loads(p.read_text(encoding="utf-8-sig"))
obj["M05.9 Original camera and gestures 0.5.2 Alpha"]=description
p.write_text(json.dumps(obj,ensure_ascii=False,indent=2)+"\n",encoding="utf-8")
print("M059_DOCS_UPDATED")
