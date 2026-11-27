using LegacyOfEternity.Combat;
using LegacyOfEternity.Player;
using LegacyOfEternity.World;
using UnityEngine;

namespace LegacyOfEternity.Offline
{
    /// <summary>Tap active quest → auto-run toward objective (NPC / gather / monster / dungeon door).</summary>
    public static class OfflineQuestNavigate
    {
        public static bool TryGo()
        {
            if (!OfflineSession.Ready || OfflineSession.Hero == null)
                return false;
            var heroGo = GameObject.Find("LOE Prototype Hero");
            if (heroGo == null)
                return false;
            var motor = heroGo.GetComponent<PlayerMotor>();
            if (motor == null)
                return false;

            var q = OriginalQuestCatalog.Get(OfflineSession.Hero.questId);
            if (q == null)
                return false;

            Vector3? target = ResolveTarget(q, heroGo.transform.position);
            if (!target.HasValue)
                return false;

            motor.SetQuestTarget(target);
            Debug.Log("LOE_QUEST_NAV target=" + target.Value + " quest=" + q.id);
            return true;
        }

        static Vector3? ResolveTarget(OriginalQuestDef q, Vector3 from)
        {
            // Turn-in / talk NPC first when goals done or talk kinds.
            bool needNpc = OriginalQuestCatalog.IsTalkKind(q)
                            || (q.finishNpcId != 0 && OriginalQuestCatalog.GoalComplete(OfflineSession.Hero, q));
            if (needNpc && q.finishNpcId != 0)
            {
                var npc = FindInteractable(q.finishNpcId);
                if (npc != null)
                    return npc.position;
            }

            if (q.kind == OriginalQuestKind.Gather && q.goalId != 0)
            {
                var gather = FindInteractable(q.goalId);
                if (gather != null)
                    return gather.position;
            }

            if ((q.kind == OriginalQuestKind.KillMonster || q.kind == OriginalQuestKind.KillDrop) && q.goalId != 0)
            {
                var enemy = FindNearestEnemy(q.goalId, from);
                if (enemy != null)
                    return enemy.transform.position;
            }

            if (q.kind == OriginalQuestKind.Dungeon || q.kind == OriginalQuestKind.MountDungeon)
            {
                // Local arena: just a short forward run then player presses enter.
                return from + Vector3.forward * 2f;
            }

            if (q.finishNpcId != 0)
            {
                var npc = FindInteractable(q.finishNpcId);
                if (npc != null)
                    return npc.position;
            }

            return null;
        }

        static Transform FindInteractable(long objectId)
        {
            var all = UnityEngine.Object.FindObjectsByType<OfflineWorldInteractable>(FindObjectsSortMode.None);
            for (int i = 0; i < all.Length; i++)
            {
                if (all[i] != null && all[i].ObjectId == objectId && !all[i].Consumed)
                    return all[i].transform;
            }
            return null;
        }

        static CombatEnemy FindNearestEnemy(long templateId, Vector3 from)
        {
            CombatEnemy best = null;
            float bestDist = float.MaxValue;
            var all = UnityEngine.Object.FindObjectsByType<CombatEnemy>(FindObjectsSortMode.None);
            for (int i = 0; i < all.Length; i++)
            {
                var e = all[i];
                if (e == null || e.IsDead)
                    continue;
                if (templateId != 0 && e.TemplateId != templateId)
                    continue;
                float d = Vector3.Distance(from, e.transform.position);
                if (d < bestDist)
                {
                    bestDist = d;
                    best = e;
                }
            }
            return best;
        }
    }
}
