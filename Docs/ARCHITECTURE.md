# Architecture
Proposed: Unity 6 LTS, C#, URP after compatibility test; Go server, PostgreSQL. Server authoritative. Separate client and server. No legacy gameplay scripts.
# M01 coastal import (2026-11-26)
- Tool: Tools/import_coastal_scene.py, original Assets read-only.
- Coastal scene: Client/Assets/LegacyAssets/Scene/allMap/cs_yw_haitan01/cs_yw_haitan01.unity
- Imported 204 assets (58,304,703 bytes), including 63 FBX and 65 prefabs.
- Seven legacy C# scripts excluded. Twelve GUID references missing in original source assets.
- Import report: AssetLibrary/coastal-import-report.json
- Unity 6000.3.27f1 batchmode import exited 0, but FBCamera.prefab reports missing component; scene visuals and shaders still need inspection.
- URP not yet configured; defer pipeline migration until shader compatibility has been evaluated.
- Next: verify scene in editor / inspect missing reference impact; replace old camera with new C# system; configure URP with explicit material conversions.
