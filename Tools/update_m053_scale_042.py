from pathlib import Path
import json
R=Path(r"C:\Source\LegacyOfEternity")
msg=(
"2026-11-26: Original VFX relative-scale correction, APK 0.4.2 Alpha. "
"Original source SkillBase and SkillMgr use effect prefab localScale=Vector3.one, optional attach to "
"character transform/bone and reset local position; original FX prefab roots are 1. "
"Our Unity6 FourClassesBuilder normalized each class to world height 1.8 units and applied different "
"visual.localScale factors: Warrior 0.8138, Assassin 0.8036, Mage 0.8350, Archer 0.8338 "
"(editor scale audit; exact scale audit logged to Docs/M053-original-relative-scale.txt). "
"OriginalCombatEffects now multiplies source FX root scale by class visual.lossyScale.x at instantiation, "
"preserving original prefab particle size, shape, textures, material blend and relative class scaling; "
"this corrects the previous effect-vs-hero scale discrepancy. Original Darklord-Working untouched. "
"Unity Android 6000.3.27f1 BuildM05Android.Run success 0 errors 40 warnings, "
"20 effects, 20 original sounds, 168 particle systems, 0 missing MonoBehaviour scripts, "
"0 unsupported shader assignments, 20 impact timing checks passed. "
"APK LegacyOfEternity-0.4.2-alpha.apk bytes=41896679 SHA256 "
"AB71B8B4190D8D8EFFC8E9710A9ED2CB1319BC4CF6656DF7D6B6150CE55614B9, "
"versionName 0.4.2 versionCode 8 ARM64, same cert as 0.4.1. "
"Published https://rikagro.ru/MMORPG/downloads/LegacyOfEternity-0.4.2-alpha.apk "
"with matching SHA256 and HTTP 200. "
"Physical Android VFX scale is not yet visually confirmed, and per-particle EffectPointAttach "
"runtime binding equivalence remains pending; test APK is not a final parity claim. "
"GitHub sync not confirmed: previous push of 0.4.1 failed with SSH publickey and HTTPS "
"credential store not available."
)
p=R/'.ai'/'PROGRESS.json'
d=json.loads(p.read_text(encoding='utf-8-sig'))
d['M05.3 VFX Scale 0.4.2']=msg
p.write_text(json.dumps(d,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
for rel in ['Docs/CURRENT-STATUS.md','Docs/ROADMAP.md','Docs/COMBAT-PVE-QA.md']:
 p=R/rel;s=p.read_text(encoding='utf-8-sig')
 heading="## Original VFX relative size correction (0.4.2 Alpha)"
 if heading not in s:p.write_text(s.rstrip()+'\n\n'+heading+'\n\n'+msg+'\n',encoding='utf-8')
print('M05.3 scale results documented')
