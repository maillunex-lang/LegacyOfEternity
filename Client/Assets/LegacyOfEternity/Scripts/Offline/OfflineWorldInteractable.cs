using UnityEngine;

namespace LegacyOfEternity.Offline
{
    public enum OfflineInteractKind
    {
        Guide,
        GatherScrap
    }

    /// <summary>World NPC / gather point for the offline Haitan quest loop.</summary>
    public sealed class OfflineWorldInteractable : MonoBehaviour
    {
        public OfflineInteractKind Kind;
        public string DisplayName = "Объект";
        public float Radius = 3.2f;
        public bool Consumed;

        Transform hero;
        TextMesh label;
        GameObject marker;
        GameObject body;

        public void Setup(OfflineInteractKind kind, string displayName, Transform heroTransform, Color color)
        {
            Kind = kind;
            DisplayName = displayName;
            hero = heroTransform;
            BuildVisual(color);
        }

        void BuildVisual(Color color)
        {
            body = GameObject.CreatePrimitive(Kind == OfflineInteractKind.GatherScrap
                ? PrimitiveType.Cube
                : PrimitiveType.Capsule);
            body.name = "Body";
            body.transform.SetParent(transform, false);
            body.transform.localScale = Kind == OfflineInteractKind.GatherScrap
                ? new Vector3(0.85f, 0.45f, 0.85f)
                : new Vector3(0.85f, 1.0f, 0.85f);
            body.transform.localPosition = Kind == OfflineInteractKind.GatherScrap
                ? Vector3.up * 0.25f
                : Vector3.up * 1.0f;
            Object.Destroy(body.GetComponent<Collider>());
            var shader = Shader.Find("Standard") ?? Shader.Find("Mobile/Diffuse");
            body.GetComponent<Renderer>().sharedMaterial = new Material(shader) { color = color };

            marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            marker.name = "Marker";
            marker.transform.SetParent(transform, false);
            marker.transform.localScale = Vector3.one * 0.35f;
            marker.transform.localPosition = Vector3.up * (Kind == OfflineInteractKind.GatherScrap ? 1.35f : 2.6f);
            Object.Destroy(marker.GetComponent<Collider>());
            marker.GetComponent<Renderer>().sharedMaterial = new Material(shader)
            {
                color = new Color(1f, 0.85f, 0.25f, 1f)
            };

            var labelGo = new GameObject("Label");
            labelGo.transform.SetParent(transform, false);
            labelGo.transform.localPosition = Vector3.up * (Kind == OfflineInteractKind.GatherScrap ? 1.8f : 3.05f);
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
            if (!OfflineSession.Ready || OfflineSession.Hero == null || Consumed)
                return false;
            int q = OfflineSession.Hero.questId;
            if (Kind == OfflineInteractKind.Guide)
                return q == 1001701 || q == 1001704;
            if (Kind == OfflineInteractKind.GatherScrap)
                return q == 1001702;
            return false;
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
            if (Kind == OfflineInteractKind.GatherScrap)
                return "Собрать";
            if (OfflineSession.Hero != null && OfflineSession.Hero.questId == 1001704)
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

            if (Kind == OfflineInteractKind.GatherScrap)
            {
                OfflineDemoStore.ApplyQuestEvent(heroData, "gather");
                Consumed = true;
                return true;
            }

            if (heroData.questId == 1001701)
            {
                OfflineDemoStore.ApplyQuestEvent(heroData, "talk");
                return true;
            }

            if (heroData.questId == 1001704)
            {
                OfflineDemoStore.ApplyQuestEvent(heroData, "turnin");
                return true;
            }

            return false;
        }
    }
}
