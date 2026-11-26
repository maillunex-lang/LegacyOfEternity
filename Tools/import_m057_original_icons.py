from pathlib import Path
import re,shutil,hashlib,json
R=Path(r"C:\Source\LegacyOfEternity")
O=Path(r"C:\Source\Darklord-Working\Client\Assets")
dest=R/"Client"/"Assets"/"LegacyAssets"/"Atlas"
dest.mkdir(parents=True,exist_ok=True)
copy=[]
for name in ["UIMain.png","UIMain2.png"]:
    for ext in ("",".meta"):
        rel=Path("Atlas")/(name+ext)
        src=O/rel
        out=dest/(name+ext)
        assert src.is_file(),src
        if out.exists():
            assert hashlib.sha256(out.read_bytes()).digest()==hashlib.sha256(src.read_bytes()).digest(),out
        else:shutil.copy2(src,out)
        copy.append({"asset":str(rel),"bytes":src.stat().st_size,"sha256":hashlib.sha256(src.read_bytes()).hexdigest()})
folderMeta=O/"Atlas.meta"
if folderMeta.exists() and not Path(str(dest)+".meta").exists():
    shutil.copy2(folderMeta,Path(str(dest)+".meta"))
skilltext=(O/"Resources"/"Lua"/"Data"/"t_skill.lua.txt").read_text(encoding="utf-8-sig")
defs={"Warrior":[3000001,3001001,3002001,3005001,3006001],
"Assassin":[4000001,4001001,4002001,4003001,4004001],
"Mage":[1000001,1001001,1002001,1003001,1004001],
"Archer":[2000001,2001001,2002001,2003001,2004001]}
atlases={}
for atlas in ("UIMain","UIMain2"):
    text=(O/"Atlas"/(atlas+".png.meta")).read_text(encoding="utf-8-sig")
    names=set(re.findall(r"^\s+name:\s+([^\r\n]+)",text,re.M))
    atlases[atlas]=names
found=[]
for cls,ids in defs.items():
    for slot,id in enumerate(ids):
        m=re.search(r"^\s*\["+str(id)+r"\]=\{[^\r\n]+",skilltext,re.M)
        assert m,id
        quoted=re.findall(r'"([^"]*)"',m.group(0))
        ref=quoted[0]
        atlas,spr=ref.split(":",1)
        expected=spr+".png"
        assert atlas in atlases and expected in atlases[atlas],(id,atlas,expected)
        found.append({"class":cls,"slot":slot,"skillId":id,"atlas":atlas,"spriteName":expected,"sourceReference":ref})
report={"source":"Resources/Lua/Data/t_skill.lua.txt","spriteSources":"Assets/Atlas/UIMain.png.meta and UIMain2.png.meta","slots":found,"copiedAssets":copy,"originalUntouched":True}
(R/"Docs"/"M057-original-skill-icon-map.json").write_text(json.dumps(report,ensure_ascii=False,indent=2),encoding="utf-8")
for row in found:print(row["class"],row["slot"],row["skillId"],row["sourceReference"])
print("VALIDATED_SPRITES",len(found),"COPIED_SOURCE_FILES",len(copy))
