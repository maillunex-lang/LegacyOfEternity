from pathlib import Path
import re, json, collections
ROOT=Path(r"C:\Source\LegacyOfEternity")
SOURCE=Path(r"C:\Source\Darklord-Working\Client\Assets")
TARGET=ROOT/'Client'/'Assets'/'LegacyAssets'
DIAG=(ROOT/'Docs'/'M053-shader-diagnostics.txt').read_text(encoding='utf-8-sig').splitlines()
mats=sorted({r.split('|')[5] for r in DIAG if r.startswith("MAT|") and len(r.split('|'))>5})
imported_path=ROOT/'Client'/'Assets'
material_guids=collections.Counter()
for path in mats:
    p=imported_path/path.removeprefix('Assets/')
    if not p.exists():continue
    txt=p.read_text(encoding='utf-8-sig',errors='replace')
    for g in re.findall(r'guid: ([0-9a-f]{32})',txt):
        if set(g)=={'0'} or g.startswith('0000000000000000'):continue
        material_guids[g]+=1
print("UNIQUE_MATERIALS",len(mats),"UNIQUE_REFERENCED_GUIDS",len(material_guids))
source_meta={}
for file in SOURCE.rglob('*.meta'):
    try:
        with file.open('rb') as f: head=f.read(350)
        m=re.search(rb'guid:\s*([0-9a-f]{32})',head)
        if m:source_meta[m.group(1).decode('ascii')]=file
    except OSError: pass
print("SOURCE_META_INDEX",len(source_meta))
existing_meta={}
for file in imported_path.rglob('*.meta'):
    try:
        with file.open('rb') as f:head=f.read(350)
        m=re.search(rb'guid:\s*([0-9a-f]{32})',head)
        if m:existing_meta[m.group(1).decode('ascii')]=file
    except OSError:pass
missing=[]
unresolved=[]
for guid,uses in material_guids.most_common():
    if guid in existing_meta:continue
    p=source_meta.get(guid)
    if not p:unresolved.append((guid,uses))
    else:
        asset=Path(str(p)[:-5])
        missing.append({'guid':guid,'uses':uses,'source':str(p.relative_to(SOURCE)),'size':asset.stat().st_size if asset.is_file() else -1})
print('MISSING_GUIDS',len(missing),'UNRESOLVED',len(unresolved))
for row in missing[:40]:print('NEED',row)
for row in unresolved[:40]:print('UNRESOLVED',row)
report=ROOT/'Docs'/'M053-vfx-guid-inventory.json'
report.write_text(json.dumps({'sourceMeta':len(source_meta),'usedMaterials':len(mats),'materialRefGuidCount':len(material_guids),'missing':missing,'unresolved':unresolved},ensure_ascii=False,indent=2),encoding='utf-8')
print('REPORT',report)
