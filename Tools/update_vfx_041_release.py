from pathlib import Path
import json
ROOT=Path(r"C:\Source\LegacyOfEternity")
summary=(
"2026-11-26 VFX restoration 0.4.1 Alpha: audit of 20 original VFX Unity6 prefabs identified "
"412 material slots, including 37 empty material slots, 68 unsupported/error-shader uses "
"and 365 material slots without a resolvable usable texture at the initial audit. "
"Indexed 31715 .meta GUIDs from the original Darklord-Working assets in read-only fashion; "
"re-imported 108 original missing GUID dependencies (12,205,927 bytes) including textures and "
"historical shader sources with matching original .meta GUIDs and SHA256 checks. "
"After dependency restoration, Unity audit: 0 unsupported shaders, 59 optional _mask:NONE entries, "
"37 empty source material slots (including 2 primary blank slots present in original assassin skill004 prefab). "
"One GUID ce47f77d4c07edb4884aed22d1665409 used by two source materials is absent from the original "
"source .meta index, so it could not be reconstructed without inventing an asset. "
"Original prefabs are preserved, as are the originally copied resources; 24 missing old script "
"components were previously stripped from Unity6 working copies and still require functional replacement "
"if parity depends on their behavior. "
"Unity Editor M053OriginalVfxBuilder.Audit: 20 effects/20 sounds/168 ParticleSystems/0 missing scripts. "
"M053TimingAudit: 20 actions overClip=0. "
"Unity Android BuildM05Android.Run 0.4.1 Succeeded with 0 errors, 40 warnings "
"(original legacy shader macro redefinitions and other nonfatal warnings). "
"APK LegacyOfEternity-0.4.1-alpha.apk 41,897,159 bytes SHA256 "
"D5BB1CDD47AEC844FF7FBD7B99CEF0CA7C2E6D6580F2BE4BB1C46AFB8CC443E8, "
"versionName 0.4.1 versionCode 7 arm64 minSdk26 targetSdk36, signature v2 certificate same as 0.4.0. "
"Published https://rikagro.ru/MMORPG/downloads/LegacyOfEternity-0.4.1-alpha.apk "
"HTTP 200 matching SHA256/Content-Length. "
"Physical Android frame, material alpha, audio and original behavioral parity are NOT yet verified "
"(ADB devices empty). No new original source data invented."
)
progress=ROOT/'.ai'/'PROGRESS.json'
data=json.loads(progress.read_text(encoding='utf-8-sig'))
data['M05.3 VFX Fix 0.4.1']=summary
progress.write_text(json.dumps(data,indent=2,ensure_ascii=False)+'\n',encoding='utf-8')
heading='## VFX texture/shader dependency restoration: 0.4.1 Alpha (2026-11-26)'
for rel in ['Docs/CURRENT-STATUS.md','Docs/ROADMAP.md','Docs/COMBAT-PVE-QA.md']:
    p=ROOT/rel
    original=p.read_text(encoding='utf-8-sig')
    if heading not in original:
        p.write_text(original.rstrip()+'\n\n'+heading+'\n\n'+summary+'\n',encoding='utf-8')
print('UPDATED ROADMAP, STATUS, PvE QA and PROGRESS with verified 0.4.1 results')
