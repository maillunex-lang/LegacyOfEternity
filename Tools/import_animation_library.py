"""Import all original character/creature animation asset families, without touching source.
Copies exact bytes and .meta files, including base FBX, textures and materials in
each folder that contains animated FBX or .anim clips. Idempotent and conflict safe.
Outputs manifest for runtime-addressable animation catalogue.
"""
from __future__ import annotations
from pathlib import Path
from collections import Counter, defaultdict
from hashlib import sha256
import json, os, shutil, sys, datetime

source_root=Path(r"C:\Source\Darklord-Working\Client\Assets\AssetsRaw\Animation")
target_root=Path(r"C:\Source\LegacyOfEternity\Client\Assets\LegacyAssets\AssetsRaw\Animation")
assetlibrary=Path(r"C:\Source\LegacyOfEternity\AssetLibrary")
source_parent=source_root.parent
target_parent=target_root.parent
assert source_root.is_dir() and target_parent.is_dir()
assetlibrary.mkdir(parents=True,exist_ok=True)
manifest_path=assetlibrary/"animation-catalog.json"
report_path=assetlibrary/"animation-import-report.json"

def norm(s:Path):return s.as_posix()
def sha(f:Path): 
    h=sha256()
    with f.open("rb") as fh:
        for c in iter(lambda:fh.read(1<<20),b""):h.update(c)
    return h.hexdigest()
def is_clip(f:Path):return f.suffix.lower()==".anim" or (f.suffix.lower()==".fbx" and "@" in f.stem)
def is_asset(f:Path):return f.suffix.lower() in (".fbx",".anim",".mat",".tga",".png",".shader",".jpg",".jpeg",".asset",".prefab",".controller",".overridecontroller")
all_anim=sorted(p for p in source_root.rglob("*") if p.is_file() and is_clip(p))
clip_dirs={p.parent for p in all_anim}
# Copy whole relevant asset family folders so animation FBXs retain geometry/textures/material assets.
families=set()
for folder in sorted(clip_dirs):
    families.add(folder)
    families.update(parent for parent in folder.parents if parent!=source_root and source_root in parent.parents)
copy_assets=set()
for folder in sorted(clip_dirs):
    # Include files in all ancestor folders. Select model assets, materials, texture files, clips.
    for f in folder.rglob("*"):
        if f.is_file() and (is_asset(f) or f.name.endswith(".meta")):copy_assets.add(f)
    for ancestor in [folder,*[x for x in folder.parents if x!=source_root and source_root in x.parents]]:
        for f in ancestor.iterdir():
            if f.is_file() and (is_asset(f) or f.name.endswith(".meta")):copy_assets.add(f)
# Include all sibling FBX/animation/texture assets that live in top-level branch files if relevant.
# Animation folder meta may be shared with other assets; copy already-present folder metadata, never parent AssetsRaw.meta.
for d in sorted({p.parent for p in copy_assets}):
    for parent in [d,*[x for x in d.parents if x!=source_root and source_root in x.parents]]:
        meta=parent.with_name(parent.name+".meta")
        if meta.exists() and source_root in meta.parents:copy_assets.add(meta)
# Inventory conflicts before mutation so no partial updates on known differences.
conflicts=[]
for src in sorted(copy_assets):
    if not src.is_file():continue
    rel=src.relative_to(source_parent)
    dst=target_parent/rel
    if dst.is_file() and src.suffix.lower() != ".meta" and (src.stat().st_size!=dst.stat().st_size or sha(src)!=sha(dst)):conflicts.append(rel.as_posix())
if conflicts:
    print("ABORT_DIFFERING_EXISTING_FILES",len(conflicts),conflicts[:15],flush=True)
    sys.exit(5)
print("SOURCE_CLIPS",len(all_anim),"FAMILIES",len(clip_dirs),"ASSETS_AND_META",len(copy_assets),"EXISTING_META_PRESERVED",flush=True)
copied=0;reused=0;bytes_copied=0
for src in sorted(copy_assets):
    dst=target_parent/src.relative_to(source_parent)
    if dst.exists():
        reused+=1
    else:
        dst.parent.mkdir(parents=True,exist_ok=True)
        shutil.copy2(src,dst)
        copied+=1;bytes_copied+=src.stat().st_size
    if copied and copied%500==0 and not (copied%500-1):print("COPY_PROGRESS",copied,flush=True)
missing=[];counts=Counter()
for src in all_anim:
    dst=target_parent/src.relative_to(source_parent)
    if not dst.is_file() or src.stat().st_size!=dst.stat().st_size or sha(src)!=sha(dst):
        missing.append(str(src.relative_to(source_root)))
    else:counts[src.relative_to(source_root).parts[0]]+=1
items=[]
for src in all_anim:
    rel=src.relative_to(source_root)
    parts=rel.parts
    model=src.stem.split("@")[0] if "@" in src.stem else src.stem
    meta=src.with_name(src.name+".meta")
    guid=None
    if meta.is_file():
        for line in meta.open(encoding="utf-8-sig",errors="replace"):
            if line.startswith("guid: "):
                guid=line[6:].strip();break
    items.append({"path":rel.as_posix(),"family":parts[0],"model":model,"clip":src.stem.split("@",1)[1] if "@" in src.stem else src.stem,"kind":src.suffix.lower(),"guid":guid,"bytes":src.stat().st_size})
catalog={"schema":1,"source_root":str(source_root),"imported_root":str(target_root),"clip_total":len(items),"counts_by_family":dict(sorted(counts.items())),"animations":items}
report={"created":datetime.datetime.now(datetime.timezone.utc).isoformat(),"source_count":len(all_anim),"copied_files":copied,"reused_files":reused,"copied_bytes":bytes_copied,"checked_clip_hashes":len(all_anim)-len(missing),"missing_or_mismatch":missing,"catalog_path":str(manifest_path),"family_dirs":len(clip_dirs)}
manifest_path.write_text(json.dumps(catalog,ensure_ascii=False,indent=2),encoding="utf-8")
report_path.write_text(json.dumps(report,ensure_ascii=False,indent=2),encoding="utf-8")
print("ANIMATION_IMPORT_RESULT",json.dumps({k:v for k,v in report.items() if k!="missing_or_mismatch"}),flush=True)
print("FAMILY_COUNTS",dict(counts),flush=True)
if missing:print("FAIL_MISMATCH",missing[:10],flush=True);sys.exit(8)
print("ANIMATION_COPY_VERIFIED",len(all_anim),flush=True)
