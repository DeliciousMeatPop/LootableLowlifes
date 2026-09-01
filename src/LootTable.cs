using System.Collections.Generic;
using UnityEngine;

namespace LootableLowlifes
{
    /// <summary>
    /// Stateless helpers implementing the weighted-random selection used by the
    /// drop engine. All rolls go through <see cref="UnityEngine.Random"/> so they
    /// respect the game's frame-time RNG and stay deterministic under testing.
    /// </summary>
    public static class LootTable
    {
        /// <summary>
        /// Rolls the resolved drop plan for a single kill: which tier, which item,
        /// and how many, repeated for a random number of drops within config bounds.
        /// Returns an empty list when nothing should drop.
        /// </summary>
        public static List<ResolvedDrop> RollDrops(DropConfig config)
        {
            var results = new List<ResolvedDrop>();
            if (config == null || !config.enabled)
                return results;

            if (Random.value > config.globalDropChance)
                return results;

            int min = Mathf.Max(0, config.minDrops);
            int max = Mathf.Max(min, config.maxDrops);
            int dropCount = Random.Range(min, max + 1);

            for (int i = 0; i < dropCount; i++)
            {
                LootTier tier = SelectTier(config);
                if (tier == null)
                    continue;

                LootEntry entry = SelectEntry(tier);
                if (entry == null || string.IsNullOrEmpty(entry.itemId))
                    continue;

                int qtyMin = Mathf.Max(1, entry.minQuantity);
                int qtyMax = Mathf.Max(qtyMin, entry.maxQuantity);
                int quantity = Random.Range(qtyMin, qtyMax + 1);

                for (int q = 0; q < quantity; q++)
                    results.Add(new ResolvedDrop(entry.itemId, entry.displayName, tier.name));
            }

            return results;
        }

        /// <summary>Weighted tier pick using the configured <see cref="TierWeights"/>.</summary>
        private static LootTier SelectTier(DropConfig config)
        {
            LootTier common = FindTier(config, "common");
            LootTier rare = FindTier(config, "rare");
            LootTier legendary = FindTier(config, "legendary");

            float wCommon = common != null ? Mathf.Max(0f, config.tierWeights.common) : 0f;
            float wRare = rare != null ? Mathf.Max(0f, config.tierWeights.rare) : 0f;
            float wLegendary = legendary != null ? Mathf.Max(0f, config.tierWeights.legendary) : 0f;

            float total = wCommon + wRare + wLegendary;
            if (total <= 0f)
                return common ?? rare ?? legendary;

            float roll = Random.value * total;
            if (roll < wCommon) return common;
            roll -= wCommon;
            if (roll < wRare) return rare;
            return legendary;
        }

        /// <summary>Weighted item pick within a tier's pool.</summary>
        private static LootEntry SelectEntry(LootTier tier)
        {
            if (tier.entries == null || tier.entries.Count == 0)
                return null;

            float total = 0f;
            foreach (var e in tier.entries)
                total += Mathf.Max(0f, e.weight);

            if (total <= 0f)
                return tier.entries[0];

            float roll = Random.value * total;
            foreach (var e in tier.entries)
            {
                float w = Mathf.Max(0f, e.weight);
                if (roll < w)
                    return e;
                roll -= w;
            }

            return tier.entries[tier.entries.Count - 1];
        }

        private static LootTier FindTier(DropConfig config, string name)
        {
            if (config.tiers == null)
                return null;
            foreach (var t in config.tiers)
            {
                if (t != null && t.name == name)
                    return t;
            }
            return null;
        }
    }

    /// <summary>A concrete, ready-to-spawn drop resolved from the loot tables.</summary>
    public struct ResolvedDrop
    {
        public readonly string itemId;
        public readonly string displayName;
        public readonly string tier;

        public ResolvedDrop(string itemId, string displayName, string tier)
        {
            this.itemId = itemId;
            this.displayName = displayName;
            this.tier = tier;
        }
    }
}
