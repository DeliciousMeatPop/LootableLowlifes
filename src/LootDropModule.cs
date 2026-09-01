using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using ThunderRoad;
using UnityEngine;

namespace LootableLowlifes
{
    /// <summary>
    /// Lootable Lowlifes — main ThunderScript.
    ///
    /// Subscribes to <see cref="EventManager.onCreatureKill"/> and, when a
    /// non-player creature dies, rolls the tiered loot tables and spawns
    /// persistent, player-storable base-game items at the ragdoll's location.
    /// Because the items are freshly spawned copies from the Catalog (rather than
    /// the NPC's own non-storable gear) they survive and can be kept/holstered.
    /// </summary>
    public class LootDropModule : ThunderScript
    {
        private const string ConfigFileName = "Item_LootTables.json";

        private DropConfig config;
        private bool subscribed;

        // ---- Lifecycle -------------------------------------------------------

        public override void ScriptLoaded(ModManager.ModData modData)
        {
            base.ScriptLoaded(modData);

            config = LoadConfig(modData);

            if (config == null || !config.enabled)
            {
                Debug.Log("[LootableLowlifes] Disabled via config; no hooks installed.");
                return;
            }

            EventManager.onCreatureKill += OnCreatureKill;
            subscribed = true;

            Debug.Log("[LootableLowlifes] Loaded. Listening for creature kills.");
        }

        public override void ScriptUnload()
        {
            if (subscribed)
            {
                EventManager.onCreatureKill -= OnCreatureKill;
                subscribed = false;
            }

            base.ScriptUnload();
        }

        // ---- Event handling --------------------------------------------------

        private void OnCreatureKill(Creature creature, Player player, CollisionInstance collisionInstance, EventTime eventTime)
        {
            try
            {
                // Only act once the creature is fully dead, and never on the player.
                if (eventTime != EventTime.OnEnd)
                    return;
                if (creature == null || creature.isPlayer)
                    return;

                Vector3 origin;
                if (!TryGetDropOrigin(creature, out origin))
                    return;

                List<ResolvedDrop> drops = LootTable.RollDrops(config);
                if (drops.Count == 0)
                    return;

                for (int i = 0; i < drops.Count; i++)
                    SpawnDrop(drops[i], origin);
            }
            catch (Exception e)
            {
                // Never let a loot error interrupt the game's kill pipeline.
                Debug.LogError("[LootableLowlifes] Error handling creature kill: " + e);
            }
        }

        // ---- Positioning -----------------------------------------------------

        /// <summary>
        /// Resolves a safe spawn origin from the creature's torso (falling back to
        /// the pelvis, then the base transform). Returns false if no part is usable.
        /// </summary>
        private bool TryGetDropOrigin(Creature creature, out Vector3 origin)
        {
            origin = Vector3.zero;

            Transform part = null;
            if (creature.ragdoll != null)
            {
                part = GetPartTransform(creature, RagdollPart.Type.Torso)
                       ?? GetPartTransform(creature, RagdollPart.Type.LeftLeg)
                       ?? GetPartTransform(creature, RagdollPart.Type.Head);
            }

            if (part == null)
                part = creature.transform;

            if (part == null)
                return false;

            origin = part.position + Vector3.up * config.spawnHeightOffset;
            return true;
        }

        private Transform GetPartTransform(Creature creature, RagdollPart.Type type)
        {
            RagdollPart part = creature.ragdoll.GetPart(type);
            return part != null ? part.transform : null;
        }

        // ---- Spawning --------------------------------------------------------

        private void SpawnDrop(ResolvedDrop drop, Vector3 origin)
        {
            ItemData itemData = Catalog.GetData<ItemData>(drop.itemId);
            if (itemData == null)
            {
                Debug.LogWarning($"[LootableLowlifes] Unknown item id '{drop.itemId}' ({drop.displayName}); skipping.");
                return;
            }

            // Horizontal scatter so multiple drops don't stack into one another
            // or clip through the ragdoll/terrain.
            Vector2 offset = UnityEngine.Random.insideUnitCircle * config.scatterRadius;
            Vector3 spawnPos = origin + new Vector3(offset.x, 0f, offset.y);
            Quaternion spawnRot = UnityEngine.Random.rotationUniform;

            itemData.SpawnAsync(item =>
            {
                if (item == null)
                    return;

                try
                {
                    ApplyPop(item);
                    Debug.Log($"[LootableLowlifes] Dropped [{drop.tier}] {drop.displayName} ({drop.itemId}).");
                }
                catch (Exception e)
                {
                    Debug.LogError("[LootableLowlifes] Error finalising spawned item: " + e);
                }
            }, spawnPos, spawnRot, null, false);
        }

        /// <summary>Applies a gentle upward-and-outward impulse to the fresh item.</summary>
        private void ApplyPop(Item item)
        {
            Rigidbody rb = item.physicBody != null ? item.physicBody.rigidBody : null;
            if (rb == null)
                return;

            Vector2 scatter = UnityEngine.Random.insideUnitCircle * config.popScatterForce;
            Vector3 impulse = Vector3.up * config.popForce + new Vector3(scatter.x, 0f, scatter.y);
            rb.AddForce(impulse, ForceMode.VelocityChange);
        }

        // ---- Config loading --------------------------------------------------

        /// <summary>
        /// Loads <c>Item_LootTables.json</c> from the mod folder if present,
        /// otherwise falls back to the built-in defaults so the mod always works.
        /// </summary>
        private DropConfig LoadConfig(ModManager.ModData modData)
        {
            try
            {
                string folder = modData != null ? modData.fullPath : null;
                if (!string.IsNullOrEmpty(folder))
                {
                    string path = Path.Combine(folder, ConfigFileName);
                    if (File.Exists(path))
                    {
                        string json = File.ReadAllText(path);
                        DropConfig loaded = JsonConvert.DeserializeObject<DropConfig>(json);
                        if (loaded != null)
                        {
                            Debug.Log("[LootableLowlifes] Loaded loot tables from " + path);
                            return loaded;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("[LootableLowlifes] Failed to read loot table JSON, using defaults: " + e);
            }

            Debug.Log("[LootableLowlifes] Using built-in default loot tables.");
            return DropConfig.Default();
        }
    }
}
