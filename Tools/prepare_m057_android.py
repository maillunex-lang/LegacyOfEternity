from pathlib import Path
r=Path(r"C:\Source\LegacyOfEternity")
p=r/"Client"/"Assets"/"LegacyOfEternity"/"Editor"/"BuildM05Android.cs"
s=p.read_text(encoding="utf-8-sig")
for before,after in {
"PlayerSettings.bundleVersion = \"0.4.2\";":"PlayerSettings.bundleVersion = \"0.5.0\";",
"PlayerSettings.Android.bundleVersionCode = 8;":"PlayerSettings.Android.bundleVersionCode = 9;",
"LegacyOfEternity-0.4.2-alpha.apk":"LegacyOfEternity-0.5.0-alpha.apk",
"            M053ShaderDiagnostics.Run();":"            M053ShaderDiagnostics.Run();\n            M057OriginalSkillIcons.Audit();",
}.items():
 assert s.count(before)==1,(before,s.count(before))
 s=s.replace(before,after)
p.write_text(s,encoding="utf-8",newline="\n")
print("Android 0.5.0-alpha code9 with M057 verification")
