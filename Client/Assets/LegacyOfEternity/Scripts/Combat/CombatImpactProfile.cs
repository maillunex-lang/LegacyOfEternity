using UnityEngine;
using LegacyOfEternity.Player;

namespace LegacyOfEternity.Combat
{
    /// <summary>
    /// Hit-time markers transcribed from original SkillClassDisplayDesc.xml.
    /// Mapping uses candidate class skill IDs and preserves total MVP damage.
    /// Archer timing remains prototype fallback: class-specific table not identified.
    /// </summary>
    public static class CombatImpactProfile
    {
        private static readonly float[][][] ClassSlotHitTimes = new float[][][]
        {
            new float[][] {new float[] {0.15f}, new float[] {0.6f}, new float[] {0.6f}, new float[] {0.5f, 0.7f, 0.9f, 1.1f, 1.3f}, new float[] {0.3f, 0.67f, 1.7f}}, // Warrior
            new float[][] {new float[] {0.4f}, new float[] {0.6f}, new float[] {0.6f}, new float[] {0.2f, 0.7f, 0.9f}, new float[] {0.131f, 0.525f, 1.05f}}, // Assassin
            new float[][] {new float[] {0.5f}, new float[] {0.57f}, new float[] {0.54f}, new float[] {0.5f, 0.7f, 0.9f, 1.1f}, new float[] {0.45f, 0.8f, 1.4f}}, // Mage
            new float[][] {new float[] {0.28f}, new float[] {0.5f}, new float[] {0.5f}, new float[] {0.5f}, new float[] {0.5f}}, // Archer
        };
        public static float[] HitTimes(CharacterClass klass, int slot)
        {
            int index = (int)klass;
            if (index < 0 || index >= ClassSlotHitTimes.Length || slot < 0 || slot > 4)
                return new[] { 0.5f };
            return ClassSlotHitTimes[index][slot];
        }
    }
}
