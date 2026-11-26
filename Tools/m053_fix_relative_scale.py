from pathlib import Path
p=Path(r"C:\Source\LegacyOfEternity\Client\Assets\LegacyOfEternity\Scripts\Combat\OriginalCombatEffects.cs")
s=p.read_text(encoding='utf-8-sig')
orig='''            GameObject instance = Instantiate(prefab, spawn, facing);
            instance.name = "Original FX " + prefab.name;'''
replacement='''            GameObject instance = Instantiate(prefab, spawn, facing);
            instance.name = "Original FX " + prefab.name;
            // The imported heroes are normalized to 1.8 world units high by FourClassesBuilder.
            // Original FX authored for the original character rig must preserve their scale
            // relative to that same rig. The visual's original scale factor varies by class.
            // Never modify the source particle size/shape curves or original prefabs.
            float originalRigScale = visual != null ? visual.lossyScale.x : 1f;
            if (originalRigScale <= 0.001f || originalRigScale > 10f)
                originalRigScale = 1f;
            instance.transform.localScale = prefab.transform.localScale * originalRigScale;'''
assert s.count(orig)==1
s=s.replace(orig,replacement)
p.write_text(s,encoding='utf-8',newline='\n')
print('FX scale now follows per-class normalized original rig scale')
