#!/usr/bin/env python3
"""Copy missing mat/fbx/anim (+metas) for Haitan Animation prefabs from Darklord."""
import json
import os
import re
import shutil
from pathlib import Path

gaps = json.loads(Path(r"C:\LegacyOfEternity\Docs\haitan-prefab-guid-gaps.json").read_text(encoding="utf-8"))
found = dict(gaps["found"])
loe_assets = Path(r"C:\LegacyOfEternity\Client\Assets")
dl_assets = Path(r"C:\Source\Darklord-Final\Client\Assets")
da_assets = Path(r"C:\SOURCE\LunexUfa\Baseline\Darklord-Android\Assets")

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

guid_re = re.compile(r"guid:\s*([a-f0-9]{32})")


def rel_under_assets(src: Path):
    for root in (dl_assets, da_assets):
        try:
            return root, src.relative_to(root)
        except Exception:
            continue
    return None, None


copied = []
skipped_cs = []
queue = []
for g, p in found.items():
    src = Path(p)
    if src.suffix.lower() == ".cs":
        skipped_cs.append(str(src))
        continue
    queue.append(src)

seen = set()


def enqueue_deps_from_file(path: Path):
    if path.suffix.lower() not in (".mat", ".prefab", ".asset"):
        return
    try:
        text = path.read_text(encoding="utf-8", errors="ignore")
    except Exception:
        return
    for g in guid_re.findall(text):
        if g in loe_guids:
            continue
        if g in found:
            dep = Path(found[g])
            if dep.suffix.lower() != ".cs":
                queue.append(dep)


while queue:
    src = Path(queue.pop(0))
    key = str(src).lower()
    if key in seen:
        continue
    seen.add(key)
    if not src.exists():
        continue
    root, rel = rel_under_assets(src)
    if rel is None:
        continue
    rel_s = str(rel).replace("\\", "/")
    if rel_s.startswith("AssetsRaw/"):
        dst = loe_assets / "LegacyAssets" / rel
    elif rel_s.startswith("Resources/"):
        dst = loe_assets / rel
    else:
        dst = loe_assets / "OriginalPort" / "HaitanDeps" / rel

    dst.parent.mkdir(parents=True, exist_ok=True)
    meta_src = Path(str(src) + ".meta")
    if not dst.exists():
        shutil.copy2(src, dst)
        if meta_src.exists():
            shutil.copy2(meta_src, Path(str(dst) + ".meta"))
        copied.append(str(dst))
        # refresh loe guid cache for this meta
        if meta_src.exists():
            for line in meta_src.read_text(encoding="utf-8", errors="ignore").splitlines()[:5]:
                if line.startswith("guid:"):
                    loe_guids.add(line.split(":", 1)[1].strip())
                    break
    enqueue_deps_from_file(src)
    if dst.exists():
        enqueue_deps_from_file(dst)

# Resolve remaining mat texture GUIDs not in found
need_more = set()
for dst in list(copied):
    p = Path(dst)
    if p.suffix.lower() != ".mat":
        continue
    text = p.read_text(encoding="utf-8", errors="ignore")
    for g in guid_re.findall(text):
        if g not in loe_guids and g not in found:
            need_more.add(g)

if need_more:
    for root, _, files in os.walk(dl_assets):
        for f in files:
            if not f.endswith(".meta"):
                continue
            mp = Path(root) / f
            try:
                head = mp.read_text(encoding="utf-8", errors="ignore").splitlines()[:5]
            except Exception:
                continue
            g = None
            for line in head:
                if line.startswith("guid:"):
                    g = line.split(":", 1)[1].strip()
                    break
            if g in need_more:
                found[g] = str(mp)[:-5]
                need_more.discard(g)
                src = Path(found[g])
                if src.suffix.lower() != ".cs":
                    queue.append(src)
        if not need_more:
            break

while queue:
    src = Path(queue.pop(0))
    key = str(src).lower()
    if key in seen:
        continue
    seen.add(key)
    if not src.exists():
        continue
    root, rel = rel_under_assets(src)
    if rel is None:
        continue
    rel_s = str(rel).replace("\\", "/")
    if rel_s.startswith("AssetsRaw/"):
        dst = loe_assets / "LegacyAssets" / rel
    elif rel_s.startswith("Resources/"):
        dst = loe_assets / rel
    else:
        dst = loe_assets / "OriginalPort" / "HaitanDeps" / rel
    dst.parent.mkdir(parents=True, exist_ok=True)
    if not dst.exists():
        shutil.copy2(src, dst)
        meta_src = Path(str(src) + ".meta")
        if meta_src.exists():
            shutil.copy2(meta_src, Path(str(dst) + ".meta"))
        copied.append(str(dst))

report = {
    "copied": len(copied),
    "skipped_cs": len(skipped_cs),
    "copied_files": copied,
    "skipped_cs_sample": skipped_cs[:30],
    "need_more_unresolved": sorted(need_more),
}
Path(r"C:\LegacyOfEternity\Docs\haitan-prefab-deps-copy.json").write_text(
    json.dumps(report, indent=2), encoding="utf-8"
)
print("copied", len(copied), "skipped_cs", len(skipped_cs), "unresolved_tex", len(need_more))
