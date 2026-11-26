from pathlib import Path
import json,hashlib,shutil
R=Path(r"C:\Source\LegacyOfEternity")
SOURCE=Path(r"C:\Source\Darklord-Working\Client\Assets")
DEST=R/'Client'/'Assets'/'LegacyAssets'
data=json.loads((R/'Docs'/'M053-vfx-guid-inventory.json').read_text(encoding='utf-8-sig'))
rows=data['missing']
copied=[]
folders=0
for item in rows:
    rel=Path(item['source'])
    meta=SOURCE/rel
    asset=Path(str(meta)[:-5])
    dm=DEST/rel
    da=Path(str(dm)[:-5])
    if not (meta.is_file() and asset.is_file()):raise FileNotFoundError(str(asset))
    if da.exists() or dm.exists():
        if not (da.is_file() and dm.is_file() and hashlib.sha256(da.read_bytes()).digest()==hashlib.sha256(asset.read_bytes()).digest() and hashlib.sha256(dm.read_bytes()).digest()==hashlib.sha256(meta.read_bytes()).digest()):
            raise RuntimeError('Do not overwrite conflicting existing file: '+str(da))
        continue
    da.parent.mkdir(parents=True,exist_ok=True)
    shutil.copy2(meta,dm)
    shutil.copy2(asset,da)
    if hashlib.sha256(da.read_bytes()).digest()!=hashlib.sha256(asset.read_bytes()).digest():raise RuntimeError('SHA mismatch: '+str(da))
    copied.append((str(rel),asset.stat().st_size))
    sub=rel.parent
    while str(sub)!='.':
        foldermeta=Path(str(SOURCE/sub)+'.meta')
        outmeta=Path(str(DEST/sub)+'.meta')
        if foldermeta.is_file() and not outmeta.exists():
            outmeta.parent.mkdir(parents=True,exist_ok=True)
            shutil.copy2(foldermeta,outmeta)
            folders+=1
        sub=sub.parent
summary={'copied':len(copied),'bytes':sum(i[1] for i in copied),'folderMetas':folders,'notFoundInOriginal':data['unresolved'],'sha256Verified':True,'sourcePreserved':True}
(R/'Docs'/'M053-vfx-resources-restoration.json').write_text(json.dumps(summary,indent=2),encoding='utf-8')
print('VFX_RESTORATION',summary)
