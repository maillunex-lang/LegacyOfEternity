from pathlib import Path
import json
r=Path(r"C:\Source\LegacyOfEternity")
title="## M05.8 Original Combat HUD layout / Android 0.5.1 Alpha (2026-11-26)"
description=("Original input HUD port based on user's own historical source prefabs JoystickPanel.prefab and "
"SkillUIParent.prefab under original read-only Darklord-Working. Current Unity 6 HUD preserves gameplay "
"input handlers rather than importing incompatible legacy MonoBehaviours. Joystick uses original "
"UIMain joystick_back and joystick_thumb sprite slices, source 151x151 background and 80x80 thumb; "
"pointer hit area remains 208x208 for mobile. SkillUIParent/SillUI source positions bottom-right "
"slot01 main attack (-75,80), size110; skill02 (-185,50), skill03 (-180,130), skill04 (-120,180), "
"skill05 (-44,190), sizes70. Unity reference CanvasScaler 1920x1080, original source layout authored "
"for historical 1136x640, so runtime mobile aspect testing and safe-area adjustments are still pending. "
"Original sprite frames back_skillbig_a / back_skillcd retained from previously imported UIMain Atlas. "
"20 original icon sprites and their class mapping preserved, 5 button event callbacks and cooldown "
"overlays remain intact, AUTO toggle preserved. Editor install log LOE_M058_ORIGINAL_LAYOUT_INSTALLED, "
"independent audit LOE_M058_LAYOUT_AUDIT PASS originalStick=2 originalFrames=5 anchors=5 eventsPreserved=5 auto=1. "
"Full Unity Android BuildPipeline Succeeded (0 errors, 40 warnings) and both M057/M058 audits passed. "
"Android test APK versionName 0.5.1, versionCode 10, ARM64, v2 signed with same certificate as 0.5.0, "
"size 42,926,771 bytes, SHA256 38DDE5689FA1E2D74C7F649D8EA42D59E6C538DCDC1C5B52600B4DAF34581A49. "
"Published https://rikagro.ru/MMORPG/downloads/LegacyOfEternity-0.5.1-alpha.apk HTTP 200 hash confirmed on VPS. "
"Original input prefab file not modified. No physical Android screenshot/joystick drag and multitouch QA yet; "
"the port is a layout-and-visual adaptation, not proof of pixel-perfect original gameplay UI parity.")
for name in ["ROADMAP.md","CURRENT-STATUS.md","COMBAT-PVE-QA.md"]:
 p=r/"Docs"/name
 old=p.read_text(encoding="utf-8-sig")
 if title not in old:p.write_text(old.rstrip()+"\n\n"+title+"\n\n"+description+"\n",encoding="utf-8")
p=r/".ai"/"PROGRESS.json"
j=json.loads(p.read_text(encoding="utf-8-sig"))
j["M05.8 Original combat HUD layout 0.5.1 Alpha"]=description
p.write_text(json.dumps(j,ensure_ascii=False,indent=2)+"\n",encoding="utf-8")
print("M058_DOCS_UPDATED")
