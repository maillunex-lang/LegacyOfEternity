from pathlib import Path
r=Path(r"C:\Source\LegacyOfEternity")
p=r/"Client/Assets/LegacyOfEternity/Editor/BuildM05Android.cs"
s=p.read_text(encoding="utf-8-sig")
changes={
'PlayerSettings.bundleVersion = "0.5.0";':'PlayerSettings.bundleVersion = "0.5.1";',
'PlayerSettings.Android.bundleVersionCode = 9;':'PlayerSettings.Android.bundleVersionCode = 10;',
'LegacyOfEternity-0.5.0-alpha.apk':'LegacyOfEternity-0.5.1-alpha.apk',
'M057OriginalSkillIcons.Audit();':'M057OriginalSkillIcons.Audit();\n            M058OriginalCombatHud.Audit();'
}
for a,b in changes.items():
 assert s.count(a)==1,(a,s.count(a))
 s=s.replace(a,b)
p.write_text(s,encoding="utf-8",newline="\n")
print("M058_BUILD_PREPARED_ANDROID_051")
