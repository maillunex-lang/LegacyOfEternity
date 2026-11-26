from pathlib import Path
r=Path(r"C:\Source\LegacyOfEternity")
p=r/"Client/Assets/LegacyOfEternity/Editor/BuildM05Android.cs"
code=p.read_text(encoding="utf-8-sig")
changes={
'M058OriginalCombatHud.Audit();':'M058OriginalCombatHud.Audit();\n            M059OriginalCamera.Audit();',
'PlayerSettings.bundleVersion = "0.5.1";':'PlayerSettings.bundleVersion = "0.5.2";',
'PlayerSettings.Android.bundleVersionCode = 10;':'PlayerSettings.Android.bundleVersionCode = 11;',
'LegacyOfEternity-0.5.1-alpha.apk':'LegacyOfEternity-0.5.2-alpha.apk',
}
for old,new in changes.items():
 assert code.count(old)==1,(old,code.count(old))
 code=code.replace(old,new)
p.write_text(code,encoding="utf-8",newline="\n")
print("M059_ANDROID_052_READY")
