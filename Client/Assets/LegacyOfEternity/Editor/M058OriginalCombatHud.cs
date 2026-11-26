using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using LegacyOfEternity.Mobile;
using LegacyOfEternity.Combat;
using LegacyOfEternity.Player;

namespace LegacyOfEternity.Editor
{
    public static class M058OriginalCombatHud
    {
        private const string ScenePath = "Assets/LegacyOfEternity/Scenes/CoastalPrototype.unity";
        private const string AtlasPath = "Assets/LegacyAssets/Atlas/UIMain.png";
        private static readonly string[] ButtonNames = {"Attack", "Skill 1", "Skill 2", "Skill 3", "Skill 4"};
        // Original SkillUIParent.prefab: SillUI, active basic attack and four skills,
        // coordinates relative to bottom right, original 1136x640 reference canvas.
        private static readonly Vector2[] OriginalPositions =
        {
            new Vector2(-75f, 80f),
            new Vector2(-185f, 50f),
            new Vector2(-180f, 130f),
            new Vector2(-120f, 180f),
            new Vector2(-44f, 190f)
        };
        private static readonly float[] Sizes = {110f, 70f, 70f, 70f, 70f};

        private static Sprite Original(string name)
        {
            var sprite = AssetDatabase.LoadAllAssetsAtPath(AtlasPath)
                .OfType<Sprite>().FirstOrDefault(s => s.name == name || s.name == name + ".png");
            if (sprite == null) throw new Exception("Original HUD sprite missing: " + name);
            return sprite;
        }

        private static void Place(RectTransform rect, Vector2 pos, Vector2 size, Vector2 pivot)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = pivot;
            rect.anchoredPosition = pos;
            rect.sizeDelta = size;
            rect.localScale = Vector3.one;
        }

        private static Image Decorative(Transform parent, string name, Sprite sprite, float dimension)
        {
            var child = parent.Find(name);
            if (child == null)
            {
                var obj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                obj.transform.SetParent(parent, false);
                child = obj.transform;
            }
            var rect = child.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(.5f, .5f);
            rect.pivot = new Vector2(.5f, .5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(dimension, dimension);
            var image = child.GetComponent<Image>();
            if (image == null) throw new Exception("Decorative Image missing: " + name);
            image.sprite = sprite;
            image.color = Color.white;
            image.preserveAspect = true;
            image.raycastTarget = false;
            child.SetAsFirstSibling();
            return image;
        }

        [MenuItem("Legacy of Eternity/M05.8 Install Original Combat Layout")]
        public static void Run()
        {
            var baseSprite = Original("joystick_back");
            var thumbSprite = Original("joystick_thumb");
            var skillFrame = Original("back_skillcd");
            var attackFrame = Original("back_skillbig_a");
            var scene = EditorSceneManager.OpenScene(ScenePath);
            var all = scene.GetRootGameObjects().SelectMany(x => x.GetComponentsInChildren<Transform>(true)).ToArray();
            var hud = all.FirstOrDefault(x => x.name == "LOE Mobile HUD");
            var hero = all.FirstOrDefault(x => x.name == "LOE Prototype Hero");
            if (hud == null || hero == null) throw new Exception("Hero or HUD absent");

            var stick = hud.Find("Move Joystick");
            var stickComponent = stick != null ? stick.GetComponent<MobileJoystick>() : null;
            var stickImage = stick != null ? stick.GetComponent<Image>() : null;
            var handle = stick != null ? stick.Find("Handle") as RectTransform : null;
            if (stickComponent == null || stickImage == null || handle == null)
                throw new Exception("Existing working joystick not found");
            // Original JoystickPanel: base 150x152 at (100,100), thumb 80x80.
            // Keep a larger transparent pointer surface for comfortable Android use.
            Place((RectTransform)stick, new Vector2(160, 160), new Vector2(208, 208), new Vector2(.5f,.5f));
            stickImage.sprite = baseSprite;
            stickImage.color = new Color(1f,1f,1f,.01f);
            stickImage.raycastTarget = true;
            Decorative(stick, "Original Joystick Background", baseSprite, 151f);
            var thumb = handle.GetComponent<Image>();
            if (thumb == null) throw new Exception("Joystick thumb Image missing");
            handle.anchorMin = handle.anchorMax = new Vector2(.5f, .5f);
            handle.pivot = new Vector2(.5f, .5f);
            handle.sizeDelta = new Vector2(80,80);
            thumb.sprite = thumbSprite;
            thumb.color = Color.white;
            thumb.raycastTarget = false;
            stickComponent.enabled = true;

            // Re-use existing event listeners, cooldowns, VFX icon layers and automatic combat.
            var panel = hud.Find("Combat Actions");
            if (panel == null) throw new Exception("Combat Actions absent");
            for (int i = 0; i < 5; i++)
            {
                var button = panel.Find(ButtonNames[i]);
                var btn = button?.GetComponent<Button>();
                var bg = button?.GetComponent<Image>();
                var icon = button?.Find("Original Skill Icon");
                var overlay = button?.Find("Cooldown Overlay");
                if (btn == null || bg == null || icon == null || overlay == null)
                    throw new Exception("Existing button wiring incomplete: " + ButtonNames[i]);
                if (btn.onClick.GetPersistentEventCount() == 0)
                    throw new Exception("No combat callback on " + ButtonNames[i]);
                Place((RectTransform)button, OriginalPositions[i], new Vector2(Sizes[i],Sizes[i]), new Vector2(.5f,.5f));
                bg.sprite = null;
                bg.color = new Color(1f,1f,1f,.01f);
                bg.raycastTarget = true;
                Decorative(button, "Original Button Frame",
                           i == 0 ? attackFrame : skillFrame,
                           i == 0 ? 120f : 76f);
                var iconRect = (RectTransform)icon;
                iconRect.anchorMin = iconRect.anchorMax = new Vector2(.5f,.5f);
                iconRect.pivot = new Vector2(.5f,.5f);
                iconRect.anchoredPosition = Vector2.zero;
                iconRect.sizeDelta = i == 0 ? new Vector2(90,90) : new Vector2(60,60);
                icon.SetAsLastSibling();
                // Overlay after icon, so cooldown text remains visible.
                overlay.SetAsLastSibling();
                var cooldown = (RectTransform)overlay;
                cooldown.anchorMin = new Vector2(.1f,.1f);
                cooldown.anchorMax = new Vector2(.9f,.9f);
                cooldown.offsetMin = cooldown.offsetMax = Vector2.zero;
                var cdText = overlay.GetComponent<Text>();
                if (cdText == null) throw new Exception("Cooldown missing on " + ButtonNames[i]);
                cdText.alignment = TextAnchor.MiddleCenter;
                cdText.raycastTarget = false;
            }
            var auto = hud.Find("Auto Combat Toggle");
            if (auto == null || auto.GetComponent<Button>()?.onClick.GetPersistentEventCount() != 1)
                throw new Exception("AUTO toggle missing");
            Place((RectTransform)auto,new Vector2(-175f,280f),new Vector2(150f,58f),new Vector2(.5f,.5f));
            var img = auto.GetComponent<Image>();
            img.color = new Color(.06f,.16f,.19f,.78f);
            if (!EditorSceneManager.SaveScene(scene)) throw new Exception("HUD scene save failed");
            AssetDatabase.SaveAssets();
            Debug.Log("LOE_M058_ORIGINAL_LAYOUT_INSTALLED joystickOriginal=1 classButtons=5 sourcePositions=5 autoPreserved=1");
        }

        [MenuItem("Legacy of Eternity/M05.8 Audit Original Combat Layout")]
        public static void Audit()
        {
            var scene=EditorSceneManager.OpenScene(ScenePath);
            var all=scene.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<Transform>(true)).ToArray();
            var hud=all.First(x=>x.name=="LOE Mobile HUD");
            var stick=hud.Find("Move Joystick");
            if(stick==null || stick.GetComponent<MobileJoystick>()==null ||
               stick.Find("Original Joystick Background")?.GetComponent<Image>()?.sprite!=Original("joystick_back") ||
               stick.Find("Handle")?.GetComponent<Image>()?.sprite!=Original("joystick_thumb"))
                throw new Exception("M058 joystick mismatch");
            var panel=hud.Find("Combat Actions");
            for(int i=0;i<5;i++)
            {
                var node=panel.Find(ButtonNames[i]);
                var btn=node?.GetComponent<Button>();
                var rect=node as RectTransform;
                var frame=node?.Find("Original Button Frame")?.GetComponent<Image>();
                if(btn==null || btn.onClick.GetPersistentEventCount()==0 || rect==null ||
                   Vector2.Distance(rect.anchoredPosition,OriginalPositions[i])>.01f ||
                   frame==null || frame.sprite!=Original(i==0?"back_skillbig_a":"back_skillcd") ||
                   node.Find("Original Skill Icon")?.GetComponent<Image>()?.sprite==null ||
                   node.Find("Cooldown Overlay")?.GetComponent<Text>()==null)
                    throw new Exception("M058 skill slot mismatch: "+i);
            }
            if(hud.Find("Auto Combat Toggle")?.GetComponent<Button>()?.onClick.GetPersistentEventCount()!=1)
                throw new Exception("M058 auto toggle missing");
            Debug.Log("LOE_M058_LAYOUT_AUDIT PASS originalStick=2 originalFrames=5 anchors=5 eventsPreserved=5 auto=1");
        }
    }
}
