from pathlib import Path
import re, json, shutil, hashlib, collections, sys
source = Path(r"C:\Source\Darklord-Working\Client\Assets")
dest = Path(r"C:\Source\LegacyOfEternity\Client\Assets")
docs = Path(r"C:\Source\LegacyOfEternity\AssetLibrary")
scene = Path("Scene/allMap/cs_yw_haitan01/cs_yw_haitan01.unity")
report_path = docs / "coastal-import-report.json"
guid_pat = re.compile(rb"\bguid: ([0-9a-f]{32})")
script_pat = re.compile(rb"m_Script: \{fileID: [^}]*?guid: ([0-9a-f]{32})")
is_builtin = lambda g: g.startswith(b"0000000000000000")
files = sorted(source.rglob("*.meta"))
index = {}
duplicates = {}
for meta in files:
    try:
        data = meta.read_bytes()
        m = guid_pat.search(data[:512])
        if m:
            guid = m.group(1).decode()
            if guid in index:
                duplicates.setdefault(guid, []).append(str(meta))
            else:
                index[guid] = meta.relative_to(source)
    except OSError:
        continue
print("Indexed",len(index),"guids from",len(files),"meta files",flush=True)
if not (source/scene).exists():
    raise SystemExit("SOURCE_SCENE_MISSING")
seen = set()
pending = collections.deque([scene])
copied = []
missing = collections.defaultdict(set)
skipped_scripts = []
errors = []
refs = {}
maximum_files = 4000
while pending:
    rel = pending.popleft()
    key = rel.as_posix().lower()
    if key in seen: continue
    seen.add(key)
    original = source/rel
    if not original.is_file():
        errors.append("Missing source: "+str(rel));continue
    if rel.suffix.lower() in (".cs",".dll",".js",".boo",".asmdef",".asmref"):
        skipped_scripts.append(str(rel));continue
    if len(seen)>maximum_files: raise SystemExit("Import size guard "+str(maximum_files))
    try: data = original.read_bytes()
    except OSError as ex:
        errors.append(str(rel)+": "+str(ex));continue
    ref_ids = set(v.decode() for v in guid_pat.findall(data) if not is_builtin(v))
    refs[str(rel)] = sorted(ref_ids)
    # A scene/prefab can refer to a MonoBehaviour script; leave it unresolved intentionally.
    for guid in sorted(ref_ids):
        meta_rel = index.get(guid)
        if meta_rel is None:
            missing[guid].add(str(rel)); continue
        asset_rel = Path(str(meta_rel)[:-5])
        if asset_rel.suffix.lower() in (".cs",".dll",".js",".boo",".asmdef",".asmref"):
            skipped_scripts.append(str(asset_rel)); continue
        pending.append(asset_rel)
    target = dest/"LegacyAssets"/rel
    if target.exists():
        if hashlib.sha256(target.read_bytes()).digest()!=hashlib.sha256(data).digest():
            raise SystemExit("Refusing to overwrite "+str(target))
    else:
        target.parent.mkdir(parents=True,exist_ok=True)
        target.write_bytes(data)
    meta = original.with_name(original.name+".meta")
    if meta.is_file():
        meta_target = target.with_name(target.name+".meta")
        meta_data=meta.read_bytes()
        if meta_target.exists():
            if meta_target.read_bytes()!=meta_data:raise SystemExit("Meta differs "+str(meta_target))
        else: meta_target.write_bytes(meta_data)
    copied.append(str(rel))
report = dict(source=str(source),scene=str(scene),unique_meta_guids=len(index),duplicates=duplicates,imported_files=len(copied),imported_bytes=sum((dest/"LegacyAssets"/f).stat().st_size for f in copied),skipped_code=sorted(set(skipped_scripts)),missing_guids={k:sorted(v) for k,v in missing.items()},errors=errors,files=copied,ref_counts={k:len(v) for k,v in refs.items()})
docs.mkdir(parents=True,exist_ok=True)
report_path.write_text(json.dumps(report,indent=2,ensure_ascii=False),encoding="utf-8")
print("COPIED",len(copied),"ASSET_BYTES",report["imported_bytes"],"SKIPPED_CODE",len(report["skipped_code"]),"MISSING_GUIDS",len(missing),"ERRORS",len(errors),flush=True)
print("TOP_TYPES",collections.Counter(Path(f).suffix.lower() for f in copied).most_common(16),flush=True)
print("REPORT",report_path,flush=True)
if errors: sys.exit(3)
