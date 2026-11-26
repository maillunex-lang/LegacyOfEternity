from pathlib import Path
r=Path(r"C:\Source\LegacyOfEternity")
p=r/"Client/Assets/LegacyOfEternity/Editor/BuildM05Android.cs"
code=p.read_text(encoding="utf-8-sig")
mods={
'M059OriginalCamera.Audit();':'M059OriginalCamera.Audit();\n            M0510OriginalControls.Audit();',
'PlayerSettings.bundleVersion = "0.5.2";':'PlayerSettings.bundleVersion = "0.5.3";',
'PlayerSettings.Android.bundleVersionCode = 11;':'PlayerSettings.Android.bundleVersionCode = 12;',
'LegacyOfEternity-0.5.2-alpha.apk':'LegacyOfEternity-0.5.3-alpha.apk'
}
for a,b in mods.items():
 assert code.count(a)==1,(a,code.count(a))
 code=code.replace(a,b)
p.write_text(code,encoding="utf8",newline="\n")
print("VERSION_053_AUDIT_PREPARED")
