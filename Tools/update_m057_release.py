from pathlib import Path
import json
r=Path(r"C:\Source\LegacyOfEternity")
header="## M05.7 Original skill icons: 0.5.0 Alpha (2026-11-26)"
description=(
"Completed M05.7 original class skill icon mapping. "
"Original sources read-only: C:\\Source\\Darklord-Working\\Client\\Assets\\Resources\\Lua\\Data\\t_skill.lua.txt "
"and Assets\\Atlas\\UIMain.png / UIMain2.png (original .meta GUIDs preserved). "
"Source mappings verified for all 20 class/slot combinations: 5 Warrior (skill IDs "
"3000001,3001001,3002001,3005001,3006001); 5 Assassin (4000001..4004001), "
"5 Mage (1000001..1004001), 5 Archer (2000001..2004001). "
"The prior lack of identified archer skill IDs is resolved for icon mapping; original "
"hit timing mapping still requires independent verification in M05.8. "
"Icon scene uses OriginalSkillIconHud to swap 20 class-specific sprites on 5 existing action "
"buttons, without changes to combat cooldowns, buttons, auto battle, VFX or damage. "
"Assets/LegacyAssets/Atlas two PNG files copied byte-identically with original .meta "
"and checked by SHA256, no original source modified. Unity Editor "
"M057OriginalSkillIcons.Run installed successfully, M057OriginalSkillIcons.Audit passed "
"20/20 icons across four classes, all five buttons and cooldown overlays. "
"Unity BuildM05Android 0.5.0 Alpha succeeded 0 errors, 40 warnings; "
"APK 42,940,627 bytes SHA256 7DD60E72BE9F02C56F22F34D8B5EAB50A115B1228B784F0BEFA5B9532847EA0E "
"versionCode 9, arm64-v8a Android 26..36, same signing certificate as 0.4.2. "
"Published https://rikagro.ru/MMORPG/downloads/LegacyOfEternity-0.5.0-alpha.apk "
"HTTP 200 and verified matching hash/size. Physical Android gameplay and screenshots pending. "
"Next M05.8: source-accurate skill behavior, including original attachment nodes and projectile movement."
)
for name in ("ROADMAP.md","CURRENT-STATUS.md","COMBAT-PVE-QA.md"):
 p=r/"Docs"/name
 old=p.read_text(encoding="utf-8-sig")
 if header not in old:p.write_text(old.rstrip()+"\n\n"+header+"\n\n"+description+"\n",encoding="utf-8")
p=r/".ai"/"PROGRESS.json"
d=json.loads(p.read_text(encoding="utf-8-sig"))
d["M05.7 Original Skill Icons (0.5.0 Alpha)"]=description
p.write_text(json.dumps(d,ensure_ascii=False,indent=2)+"\n",encoding="utf-8")
print("M057_DOCS_UPDATED roadmap,status,qa,progress")
