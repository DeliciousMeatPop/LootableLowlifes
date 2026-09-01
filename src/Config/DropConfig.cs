using System;
using System.Collections.Generic;

namespace LootableLowlifes
{
    /// <summary>
    /// Serializable configuration for the loot system. This maps 1:1 to the
    /// external <c>Item_LootTables.json</c> file so drop rates and item pools
    /// can be tuned without recompiling the DLL.
    /// </summary>
    [Serializable]
    public class DropConfig
    {
        /// <summary>Master switch. When false the mod does nothing on kill.</summary>
        public bool enabled = true;

        /// <summary>
        /// Chance (0..1) that a slain creature drops anything at all.
        /// Rolled once per kill before the tiered table is consulted.
        /// </summary>
        public float globalDropChance = 0.85f;

        /// <summary>Minimum number of item stacks to spawn when a drop occurs.</summary>
        public int minDrops = 1;

        /// <summary>Maximum number of item stacks to spawn when a drop occurs.</summary>
        public int maxDrops = 3;

        /// <summary>Vertical offset (metres) above the chosen ragdoll part to spawn at.</summary>
        public float spawnHeightOffset = 0.15f;

        /// <summary>Horizontal scatter radius (metres) so stacked drops don't clip.</summary>
        public float scatterRadius = 0.25f;

        /// <summary>Upward impulse strength for the gentle "pop" on spawn.</summary>
        public float popForce = 1.5f;

        /// <summary>Randomised horizontal component added to the pop impulse.</summary>
        public float popScatterForce = 0.6f;

        /// <summary>
        /// Relative selection weight of each tier once a drop is guaranteed.
        /// Keys must match the <see cref="LootTier.name"/> entries below.
        /// </summary>
        public TierWeights tierWeights = new TierWeights();

        /// <summary>The tiered loot tables themselves.</summary>
        public List<LootTier> tiers = new List<LootTier>();

        /// <summary>
        /// Returns a config populated with sane base-game defaults. Used when no
        /// external JSON table is found so the mod is functional out of the box.
        /// </summary>
        public static DropConfig Default()
        {
            return new DropConfig
            {
                enabled = true,
                globalDropChance = 0.85f,
                minDrops = 1,
                maxDrops = 3,
                spawnHeightOffset = 0.15f,
                scatterRadius = 0.25f,
                popForce = 1.5f,
                popScatterForce = 0.6f,
                tierWeights = new TierWeights { common = 70f, rare = 25f, legendary = 5f },
                tiers = new List<LootTier>
                {
                    new LootTier
                    {
                        name = "common",
                        entries = new List<LootEntry>
                        {
                            new LootEntry { itemId = "Currency", displayName = "Florin Pouch (Small)", weight = 60f, minQuantity = 1, maxQuantity = 1 },
                            new LootEntry { itemId = "RingSilver",  displayName = "Silver Ring",         weight = 25f, minQuantity = 1, maxQuantity = 1 },
                            new LootEntry { itemId = "Apple",        displayName = "Apple",               weight = 15f, minQuantity = 1, maxQuantity = 2 },
                        }
                    },
                    new LootTier
                    {
                        name = "rare",
                        entries = new List<LootEntry>
                        {
                            new LootEntry { itemId = "Currency",    displayName = "Florin Pouch (Large)", weight = 40f, minQuantity = 2, maxQuantity = 4 },
                            new LootEntry { itemId = "RingGold",     displayName = "Gold Ring",            weight = 30f, minQuantity = 1, maxQuantity = 1 },
                            new LootEntry { itemId = "CrystalShard", displayName = "Crystal Shard",        weight = 30f, minQuantity = 1, maxQuantity = 2 },
                        }
                    },
                    new LootTier
                    {
                        name = "legendary",
                        entries = new List<LootEntry>
                        {
                            new LootEntry { itemId = "WeaponSwordShortCommon", displayName = "Short Sword", weight = 50f, minQuantity = 1, maxQuantity = 1 },
                            new LootEntry { itemId = "WeaponAxe1H",            displayName = "War Axe",     weight = 30f, minQuantity = 1, maxQuantity = 1 },
                            new LootEntry { itemId = "WeaponMace1H",           displayName = "Mace",        weight = 20f, minQuantity = 1, maxQuantity = 1 },
                        }
                    },
                }
            };
        }
    }

    [Serializable]
    public class TierWeights
    {
        public float common = 70f;
        public float rare = 25f;
        public float legendary = 5f;
    }

    /// <summary>A named tier holding a weighted pool of possible drops.</summary>
    [Serializable]
    public class LootTier
    {
        public string name = "common";
        public List<LootEntry> entries = new List<LootEntry>();
    }

    /// <summary>A single weighted loot entry referencing a base-game ItemData id.</summary>
    [Serializable]
    public class LootEntry
    {
        /// <summary>The Catalog ItemData id (e.g. "Currency", "WeaponSwordShortCommon").</summary>
        public string itemId = "";

        /// <summary>Human readable name, for logging/debugging only.</summary>
        public string displayName = "";

        /// <summary>Relative weight within its tier.</summary>
        public float weight = 1f;

        /// <summary>Minimum stack count of this item to spawn.</summary>
        public int minQuantity = 1;

        /// <summary>Maximum stack count of this item to spawn.</summary>
        public int maxQuantity = 1;
    }
}
