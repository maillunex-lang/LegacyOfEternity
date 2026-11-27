using LegacyOfEternity.World;
using UnityEngine;

namespace LegacyOfEternity.Offline
{
    public enum OfflineInteractKind
    {
        Guide,
        GatherScrap,
        WorldProp
    }

    /// <summary>Original MapData NPC / gather point driven by QuestConfig finishNpc / goals.</summary>
    public sealed class OfflineWorldInteractable : MonoBehaviour
    {
        public OfflineInteractKind Kind;
        public string DisplayName = "Объект";
        public long ObjectId;
        public float Radius = 3.2f;
        public bool Consumed;

        Transform hero;
        TextMesh label;
        GameObject marker;
        GameObject body;

        public void Setup(OfflineInteractKind kind, string displayName, Transform heroTransform, Color color, long objectId = 0)
        {
            Kind = kind;
            DisplayName = displayName;
            ObjectId = objectId;
            hero = heroTransform;
            BuildVisual(color);
        }

        /// <summary>When an original model prefab is attached, hide the placeholder capsule/cube.</summary>
        public void HidePrimitiveBody()
        {
            if (body != null)
                body.SetActive(false);
        }

        void BuildVisual(Color color)
        {
            bool gather = Kind == OfflineInteractKind.GatherScrap;
            body = GameObject.CreatePrimitive(gather ? PrimitiveType.Cube : PrimitiveType.Capsule);
            body.name = "Body";
            body.transform.SetParent(transform, false);
            body.transform.localScale = gather
                ? new Vector3(0.85f, 0.45f, 0.85f)
                : new Vector3(0.85f, 1.0f, 0.85f);
            body.transform.localPosition = gather
                ? Vector3.up * 0.25f
                : Vector3.up * 1.0f;
            Object.Destroy(body.GetComponent<Collider>());
            var shader = Shader.Find("Standard") ?? Shader.Find("Mobile/Diffuse");
            body.GetComponent<Renderer>().sharedMaterial = new Material(shader) { color = color };

            marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            marker.name = "Marker";
            marker.transform.SetParent(transform, false);
            marker.transform.localScale = Vector3.one * 0.35f;
            marker.transform.localPosition = Vector3.up * (gather ? 1.35f : 2.6f);
            Object.Destroy(marker.GetComponent<Collider>());
            marker.GetComponent<Renderer>().sharedMaterial = new Material(shader)
            {
                color = new Color(1f, 0.85f, 0.25f, 1f)
            };

            var labelGo = new GameObject("Label");
            labelGo.transform.SetParent(transform, false);
            labelGo.transform.localPosition = Vector3.up * (gather ? 1.8f : 3.05f);
            label = labelGo.AddComponent<TextMesh>();
            label.text = DisplayName;
            label.fontSize = 42;
            label.characterSize = 0.08f;
            label.anchor = TextAnchor.LowerCenter;
            label.alignment = TextAlignment.Center;
            label.color = Color.white;
            label.fontStyle = FontStyle.Bold;
        }

        void LateUpdate()
        {
            if (label != null && Camera.main != null)
                label.transform.rotation = Quaternion.LookRotation(
                    label.transform.position - Camera.main.transform.position);

            bool relevant = IsRelevant();
            if (marker != null)
                marker.SetActive(relevant && !Consumed);
            if (label != null)
            {
                label.gameObject.SetActive(relevant && !Consumed);
                if (relevant)
                    label.text = DisplayName;
            }
            if (Consumed && Kind == OfflineInteractKind.GatherScrap)
                gameObject.SetActive(false);
        }

        public bool IsRelevant()
        {
            if (!OfflineSession.Ready || OfflineSession.Hero == null || Consumed || ObjectId == 0)
                return false;
            var heroData = OfflineSession.Hero;
            var q = OriginalQuestCatalog.Get(heroData.questId);
            if (q == null)
                return false;

            if (Kind == OfflineInteractKind.GatherScrap)
            {
                if (q.kind != OriginalQuestKind.Gather)
                    return false;
                return q.goalId == ObjectId && heroData.questProgress < Mathf.Max(1, q.goalCount);
            }

            // NPC
            if (q.finishNpcId != ObjectId)
                return false;
            if (OriginalQuestCatalog.IsTalkKind(q))
                return true;
            return OriginalQuestCatalog.GoalComplete(heroData, q);
        }

        public bool InRange()
        {
            if (hero == null || Consumed || !IsRelevant())
                return false;
            Vector3 a = transform.position;
            Vector3 b = hero.position;
            a.y = 0f;
            b.y = 0f;
            return Vector3.Distance(a, b) <= Radius;
        }

        public string ActionLabel()
        {
            var q = OfflineSession.Hero != null
                ? OriginalQuestCatalog.Get(OfflineSession.Hero.questId)
                : null;
            if (q != null && q.kind == OriginalQuestKind.Gather)
                return "Собрать";
            if (q != null && !OriginalQuestCatalog.IsTalkKind(q) && OriginalQuestCatalog.GoalComplete(OfflineSession.Hero, q))
                return "Сдать задание";
            return "Поговорить";
        }

        public bool TryInteract()
        {
            if (!InRange())
                return false;
            var heroData = OfflineSession.Hero;
            if (heroData == null)
                return false;

            var q = OriginalQuestCatalog.Get(heroData.questId);
            if (q == null)
                return false;

            if (q.kind == OriginalQuestKind.Gather && q.goalId == ObjectId)
            {
                OfflineDemoStore.ApplyQuestEvent(heroData, "gather", ObjectId);
                Consumed = true;
                return true;
            }

            if (q.finishNpcId == ObjectId)
            {
                // Show original dialogue (RU) before completing gossip / turn-in.
                OfflineDialogueHud.Ensure().OpenQuestTalk(DisplayName, ObjectId, q.id);
                return true;
            }

            return false;
        }
    }
}
