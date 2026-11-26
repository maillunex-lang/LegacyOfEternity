from pathlib import Path
p=Path(r"C:\Source\LegacyOfEternity\Client\Assets\LegacyOfEternity\Editor\BuildM05Android.cs")
s=p.read_text(encoding='utf-8-sig')
for old,new in {
'PlayerSettings.bundleVersion = "0.4.1";':'PlayerSettings.bundleVersion = "0.4.2";',
'PlayerSettings.Android.bundleVersionCode = 7;':'PlayerSettings.Android.bundleVersionCode = 8;',
'LegacyOfEternity-0.4.1-alpha.apk':'LegacyOfEternity-0.4.2-alpha.apk'
}.items():
 assert s.count(old)==1,(old,s.count(old))
 s=s.replace(old,new)
p.write_text(s,encoding='utf-8',newline='\n')
print('READY_ANDROID 0.4.2 code 8')
