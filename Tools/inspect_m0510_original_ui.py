from pathlib import Path
import re
p=Path(r"C:\Source\Darklord-Working\Client\Assets\Resources\UI\Prefabs\UIMain\UIMain.prefab")
s=p.read_text(encoding="utf-8-sig")
blocks=re.split(r"(?=^--- !u!)",s,flags=re.M)
name_by_gameid={}
components={}
for b in blocks:
 gm=re.search(r"^--- !u!1 &(\d+)",b,re.M)
 if gm:
  x=re.search(r"^\s*m_Name:\s*(.*?)$",b,re.M)
  if x:name_by_gameid[gm.group(1)]=x.group(1)
 for key in ("m_Sprite","m_AnchoredPosition","m_SizeDelta"):
  pass
for b in blocks:
 gm=re.search(r"^\s*m_GameObject: \{fileID: (\d+)\}",b,re.M)
 if not gm:continue
 obj=gm.group(1)
 rec=components.setdefault(obj,[])
 props=[]
 for key in ("m_Sprite","m_AnchoredPosition","m_SizeDelta","m_Color"):
  a=re.search(r"^\s*"+key+r": (.*)$",b,re.M)
  if a:props.append(key+"="+a.group(1))
 if props:rec.append("; ".join(props))
interesting=[(k,v) for k,v in name_by_gameid.items() if any(x in v.lower() for x in ("guaji","auto","target","switch","battle","xun","lock","select","skillbutton"))]
for k,v in interesting:
 print(f"{v!r} gameid={k}")
 for c in components.get(k,[]): print("  ",c[:240])
