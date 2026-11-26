from pathlib import Path
import re,json
orig=Path(r"C:\Source\Darklord-Working\Client\Assets")
out=Path(r"C:\Source\LegacyOfEternity\Docs\M058-original-hud-audit.json")
sources={"joystick":orig/"Resources/UI/Prefabs/Joystick/JoystickPanel.prefab","skill":orig/"Resources/UI/Prefabs/UIMain/SkillUIParent.prefab","main":orig/"Resources/UI/Prefabs/UIMain/UIMain.prefab"}
def parse(p):
 s=p.read_text(encoding="utf-8-sig")
 obj={}
 for m in re.finditer(r"--- !u!1 &(\d+)\s*\nGameObject:\s*\n(.*?)(?=\n--- !u!|\Z)",s,re.S):
  id=m.group(1); name=re.search(r"^\s+m_Name:\s*(.*?)\s*$",m.group(2),re.M);obj[id]={"name":name.group(1) if name else ""}
 byid={}; children={}
 for m in re.finditer(r"--- !u!224 &(\d+)\s*\nRectTransform:\s*\n(.*?)(?=\n--- !u!|\Z)",s,re.S):
  id=m.group(1); block=m.group(2); g=re.search(r"m_GameObject: \{fileID: (\d+)\}",block); par=re.search(r"m_Father: \{fileID: (\d+)\}",block)
  if not g:continue
  name=obj.get(g.group(1),{}).get("name","")
  row={"id":id,"gameObject":g.group(1),"name":name,"parent":par.group(1) if par else None}
  for key in ("m_AnchoredPosition","m_SizeDelta","m_AnchorMin","m_AnchorMax","m_Pivot"):
   n=re.search(r"^\s+"+key+r":\s*(\{[^\n]+\})",block,re.M)
   if n: row[key]=n.group(1)
  byid[id]=row
 for v in byid.values():
  parts=[]; current=v; visited=set()
  while current and current["id"] not in visited:
   visited.add(current["id"]);parts.append(current["name"]);current=byid.get(current["parent"])
  v["path"]="/".join(reversed(parts))
 return list(byid.values())
data={name:parse(p) for name,p in sources.items()}
selected={}
for name,objs in data.items():
 if name=="joystick":selected[name]=objs
 elif name=="skill":selected[name]=[x for x in objs if any(t in x["path"] for t in ("SkillUIParent","SkillCell","SillUI"))]
 else:selected[name]=[x for x in objs if any(t in x["path"] for t in ("SkillButtonHolder","Joystick","Battle"))]
out.write_text(json.dumps(selected,ensure_ascii=False,indent=2),encoding="utf8")
for name,objs in selected.items():
 print("###",name,"total",len(data[name]),"selected",len(objs))
 for row in objs[:65]:
  print(row["id"],row["path"],"pos",row.get("m_AnchoredPosition"),"size",row.get("m_SizeDelta"),"anchors",row.get("m_AnchorMin"),row.get("m_AnchorMax"))
