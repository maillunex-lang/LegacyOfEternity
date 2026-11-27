#!/usr/bin/env python3
"""Remove MonoBehaviour blocks whose m_Script GUID is missing from LoE Assets."""
import json
import os
import re
from pathlib import Path

loe_assets = Path(r"C:\LegacyOfEternity\Client\Assets")
models = json.loads(
    (loe_assets / "Resources/OriginalMap/HaitanModels.json").read_text(encoding="utf-8")
)
gaps = json.loads(Path(r"C:\LegacyOfEternity\Docs\haitan-prefab-guid-gaps.json").read_text(encoding="utf-8"))

loe_guids = set()
for root, _, files in os.walk(loe_assets):
    for f in files:
        if not f.endswith(".meta"):
            continue
        try:
            for line in (Path(root) / f).read_text(encoding="utf-8", errors="ignore").splitlines()[:5]:
                if line.startswith("guid:"):
                    loe_guids.add(line.split(":", 1)[1].strip())
                    break
        except Exception:
            pass

script_missing = set()
for g, p in gaps["found"].items():
    if p.lower().endswith(".cs") and g not in loe_guids:
        script_missing.add(g)
for g in gaps.get("unresolved", []):
    if g != "0000000000000000e000000000000000":
        script_missing.add(g)

prefab_paths = []
for m in models["models"]:
    p = loe_assets / "Resources" / (m["resourcesPath"] + ".prefab")
    if p.exists():
        prefab_paths.append(p)

header_re = re.compile(r"^--- !u!114 &(\d+)\s*$", re.M)
script_line_re = re.compile(r"m_Script:\s*\{fileID:\s*\d+,\s*guid:\s*([a-f0-9]{32}),\s*type:\s*3\}")

stripped = []
for prefab in prefab_paths:
    text = prefab.read_text(encoding="utf-8", errors="ignore")
    parts = re.split(r"(?=^--- !u!\d+ &)", text, flags=re.M)
    keep = []
    removed_ids = set()
    removed = 0
    for part in parts:
        if not part.startswith("--- !u!114 &"):
            keep.append(part)
            continue
        m = header_re.match(part)
        sm = script_line_re.search(part)
        if m and sm:
            guid = sm.group(1)
            if guid in script_missing or guid not in loe_guids:
                removed_ids.add(m.group(1))
                removed += 1
                continue
        keep.append(part)

    new_text = "".join(keep)
    if removed_ids:
        for fid in removed_ids:
            new_text = re.sub(rf"\n  - component: \{{fileID: {fid}\}}", "", new_text)

    if removed > 0:
        prefab.write_text(new_text, encoding="utf-8")
        stripped.append({"prefab": str(prefab), "removed": removed})

report = {
    "prefabs_touched": len(stripped),
    "details": stripped,
    "script_missing_count": len(script_missing),
}
Path(r"C:\LegacyOfEternity\Docs\haitan-prefab-script-strip.json").write_text(
    json.dumps(report, indent=2), encoding="utf-8"
)
print("touched", len(stripped), "script_guids", len(script_missing))
for d in stripped:
    print(d["removed"], Path(d["prefab"]).name)
