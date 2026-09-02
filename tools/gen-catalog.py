#!/usr/bin/env python3
"""Generate the standalone ThunderRoad catalog JSON for Lootable Lowlifes.

Produces coin-pouch ItemData (base-game prefab, no dependencies), tiered
LootTables, and ContainerData overrides that place a lootable pouch on the
hip of each base-game enemy archetype.
"""
import json, os

# The installable mod folder lives at <repo>/LootableLowlifes (this script is
# in <repo>/tools). Resolve it relative to the script so it works anywhere.
ROOT = os.path.join(os.path.dirname(os.path.dirname(os.path.abspath(__file__))), "LootableLowlifes")
ITEMS = os.path.join(ROOT, "Items")
TABLES = os.path.join(ROOT, "LootTables")
CONTAINERS = os.path.join(ROOT, "Containers")

# ---------------------------------------------------------------------------
# 1. Coin-pouch items. Each is a re-skin of the base-game loot bag
#    (Bas.Item.Valuable.LootBag) with a custom gold value. Structure mirrors a
#    known-good base/GildedGoons item so ThunderRoad accepts it verbatim; only
#    id / displayName / value differ.
# ---------------------------------------------------------------------------

def animcurve(k0, k1):
    def key(t, v):
        return {
            "$type": "UnityEngine.Keyframe, UnityEngine.CoreModule",
            "time": t, "value": v, "inTangent": 0.0, "outTangent": 0.0,
            "inWeight": 0.0, "outWeight": 0.0, "weightedMode": "None", "tangentMode": 0,
        }
    return {
        "$type": "UnityEngine.AnimationCurve, UnityEngine.CoreModule",
        "keys": [key(*k0), key(*k1)],
        "length": 2, "preWrapMode": "ClampForever", "postWrapMode": "ClampForever",
    }

def make_item(item_id, display, value):
    return {
        "$type": "ThunderRoad.ItemData, ThunderRoad",
        "id": item_id,
        "sensitiveContent": "None",
        "sensitiveFilterBehaviour": "Discard",
        "version": 4,
        "localizationId": "CoinBag",
        "displayName": display,
        "description": None,
        "author": "DeliciousMeatPop",
        "valueType": "Gold",
        "value": value,
        "rewardValue": 0.0,
        "tier": 0,
        "flags": "Spinnable",
        "levelRequired": 0,
        "category": "Valuables",
        "iconEffectId": "",
        "preferredItemCenter": "Mass",
        "drainImbueWhenIdle": True,
        "prefabAddress": "Bas.Item.Valuable.LootBag",
        "iconAddress": "Bas.Item.Valuable.LootBag.Icon[LootBag]",
        "closeUpIconAddress": "",
        "pooledCount": 0,
        "androidPooledCount": 0,
        "type": "Valuable",
        "allowedStorage": "Inventory",
        "despawnOnStoredInInventory": True,
        "isStackable": False,
        "consumableId": None,
        "inventoryAudioContainerAddress": "Bas.AudioGroup.Inventory.Store.CoinsPurse",
        "inventoryAudioVolume_dB": 0.0,
        "slot": "",
        "snapAudioContainerAddress": "Bas.AudioGroup.Snap.Loot",
        "snapAudioVolume_dB": 0.0,
        "overrideMassAndDrag": True,
        "mass": 1.0, "drag": 1.0, "angularDrag": 1.0,
        "focusRegenMultiplier": 1.0,
        "spellChargeSpeedPlayerMultiplier": 1.0,
        "spellChargeSpeedNPCMultiplier": 1.0,
        "collisionMaxOverride": 0,
        "collisionEnterOnly": False,
        "collisionNoMinVelocityCheck": False,
        "forceLayer": "None",
        "diffForceLayerWhenHeld": False,
        "forceLayerHeld": "None",
        "waterHandSpringMultiplierCurve": animcurve((0.0, 0.3), (1.0, 0.15)),
        "waterDragMultiplierCurve": animcurve((0.0, 1.0), (1.0, 10.0)),
        "waterSampleMinRadius": 0.2,
        "throwMultiplier": 1.0,
        "runSpeedMultiplier": 1.0,
        "flyRotationSpeed": 2.0,
        "flyThrowAngle": 0.0,
        "allowFlyBackwards": False,
        "telekinesisSafeDistance": 1.0,
        "telekinesisThrowRatio": 1.0,
        "telekinesisAutoGrabAnyHandle": False,
        "grippable": False,
        "grabAndGripClimb": False,
        "playerGrabAndGripChangeLayer": True,
        "customSnaps": [],
        "drainImbueOnSnap": True,
        "imbueEnergyOverTimeOnSnap": animcurve((0.0, 1.0), (3.0, 0.0)),
        "modules": [
            {
                "$type": "ThunderRoad.ItemModuleValueModifier, ThunderRoad",
                "id": None, "sensitiveContent": "None", "sensitiveFilterBehaviour": "Discard",
                "version": 0, "useModeMainCurrency": False, "spawnPriceTag": True,
                "priceTagAddress": "Bas.Misc.ShopPriceTag", "groupPath": None,
            },
            {
                "$type": "ThunderRoad.ItemModuleStats, ThunderRoad",
                "id": None, "sensitiveContent": "None", "sensitiveFilterBehaviour": "Discard",
                "version": 0, "stats": [], "groupPath": None,
            },
            {
                "$type": "ThunderRoad.ItemModuleConvertToCurrency, ThunderRoad",
                "id": None, "sensitiveContent": "None", "sensitiveFilterBehaviour": "Discard",
                "version": 0, "CollectSoundEffectId": "CollectCurrency", "groupPath": None,
            },
        ],
        "colliderGroups": [
            {"$type": "ThunderRoad.ItemData+ColliderGroup, ThunderRoad",
             "transformName": "Rig", "colliderGroupId": "PropDefault"},
        ],
        "damagers": [
            {"$type": "ThunderRoad.ItemData+Damager, ThunderRoad",
             "transformName": "Blunt", "damagerID": "RockBlunt"},
        ],
        "Interactables": [
            {"$type": "ThunderRoad.ItemData+Interactable, ThunderRoad",
             "transformName": "TopHandle", "interactableId": "ObjectHandleProp"},
            {"$type": "ThunderRoad.ItemData+Interactable, ThunderRoad",
             "transformName": "BottomHandle", "interactableId": "ObjectHandlePropNoTK"},
        ],
        "effectHinges": [],
        "whooshs": [],
        "entityModules": [],
        "groupPath": None,
    }

# id, display, gold value
POUCHES = [
    ("LLPouchWorn",   "Worn Coin Pouch",     8),
    ("LLPouchLight",  "Light Coin Pouch",    15),
    ("LLPouchCommon", "Coin Pouch",          28),
    ("LLPouchHeavy",  "Heavy Coin Pouch",    55),
    ("LLPouchFat",    "Bulging Coin Pouch",  110),
    ("LLPouchLordly", "Lordly Coin Pouch",   220),
]

# ---------------------------------------------------------------------------
# 2. Tiered loot tables. Weighted picks over the pouches above.
# ---------------------------------------------------------------------------

def drop(item_id, weight, lo=1, hi=1):
    return {
        "$type": "ThunderRoad.LootTable+Drop, ThunderRoad",
        "referenceID": item_id,
        "reference": "Item",
        "randMode": "ItemCount",
        "minMaxRand": {"x": float(lo), "y": float(hi)},
        "probabilityWeight": float(weight),
    }

def make_table(table_id, drops):
    return {
        "$type": "ThunderRoad.LootTable, ThunderRoad",
        "id": table_id,
        "sensitiveContent": "None",
        "sensitiveFilterBehaviour": "Discard",
        "version": 1,
        "levelledDrops": [
            {"$type": "ThunderRoad.LootTable+DropLevel, ThunderRoad",
             "dropLevel": 0, "drops": drops},
        ],
        "groupPath": "Lootable Lowlifes",
    }

# Sellable treasure from Baron's Bounties (SOFT dependency). These are that
# mod's own Valuable item ids; we only reference them. If Baron's Bounties is
# not installed, ThunderRoad simply skips the missing reference at spawn time
# (the coin pouches, which are ours, always work). Only elites (T2/T3) roll
# treasure, and at low weight so coins stay the staple.
TREASURE_T2 = [
    drop("RelicSentariBrooch", 0.5),   # Shard Brooch
    drop("RelicSentariSpyglass", 0.5), # Oswen's Spyglass
]
TREASURE_T3 = [
    drop("RelicSentariBrooch", 0.5),
    drop("RelicSentariSpyglass", 0.5),
    drop("RelicMadluStardial", 0.5),   # Madene Stardial
    drop("RelicKhareseOron", 0.5),     # Model Oron
    drop("CrystalBag", 0.5),           # gem bag
    drop("RelicCrownAldaric", 0.5),    # King Aldaric's Crown (wearable relic)
]

TABLES_DEF = {
    "LLNpcLootT0": [drop("LLPouchWorn", 5), drop("LLPouchLight", 3), drop("LLPouchCommon", 1)],
    "LLNpcLootT1": [drop("LLPouchWorn", 3), drop("LLPouchLight", 4), drop("LLPouchCommon", 2), drop("LLPouchHeavy", 1)],
    "LLNpcLootT2": [drop("LLPouchLight", 2), drop("LLPouchCommon", 4), drop("LLPouchHeavy", 3), drop("LLPouchFat", 1)] + TREASURE_T2,
    "LLNpcLootT3": [drop("LLPouchCommon", 2), drop("LLPouchHeavy", 3), drop("LLPouchFat", 3), drop("LLPouchLordly", 1)] + TREASURE_T3,
}

# ---------------------------------------------------------------------------
# 3. Container overrides. Each base enemy archetype -> a pouch on HipsLeft
#    drawn from its tier's loot table. IDs MUST match the base creature's
#    container id (that's how the override attaches), so they are NOT prefixed.
# ---------------------------------------------------------------------------

def make_container(creature_id, table_id):
    return {
        "$type": "ThunderRoad.ContainerData, ThunderRoad",
        "id": creature_id,
        "version": 1,
        "containerContents": [
            {
                "$type": "ThunderRoad.TableContent, ThunderRoad",
                "quantity": 1,
                "state": {
                    "$type": "ThunderRoad.ContentStateHolder, ThunderRoad",
                    "holderName": "HipsLeft",
                },
                "customDataList": [],
                "referenceID": table_id,
            }
        ],
    }

CREATURE_TIERS = {
    "LLNpcLootT0": ["SoldierSerf", "SoldierThrall", "ScavengerMugger", "ScavengerCharlatan",
                    "TribalTracker", "TribalStalker", "CultistDevotee", "CultistProtege"],
    "LLNpcLootT1": ["SoldierFootman", "SoldierWatchman", "SoldierBowman", "ScavengerHunter",
                    "ScavengerFiend", "TribalRaider", "TribalWarrior", "CultistAcolyte"],
    "LLNpcLootT2": ["SoldierAdept", "ScavengerHoncho", "TribalGladiator", "TribalBerserker",
                    "CultistPartisan", "CultistSniper", "CultistPureblood"],
    "LLNpcLootT3": ["SoldierBishop", "TribalChampion", "TribalDruid", "CultistArbiter",
                    "CultistMagistrate", "CultistHighborn", "CultistHighborn2", "CultistHighborn3"],
}

def write(path, obj):
    with open(path, "w") as f:
        json.dump(obj, f, indent=2)
        f.write("\n")

def main():
    for item_id, display, value in POUCHES:
        write(os.path.join(ITEMS, f"Item_{item_id}.json"), make_item(item_id, display, value))
    for table_id, drops in TABLES_DEF.items():
        write(os.path.join(TABLES, f"LootTable_{table_id}.json"), make_table(table_id, drops))
    count = 0
    for table_id, creatures in CREATURE_TIERS.items():
        for cid in creatures:
            write(os.path.join(CONTAINERS, f"Container_{cid}.json"), make_container(cid, table_id))
            count += 1
    print(f"items={len(POUCHES)} tables={len(TABLES_DEF)} containers={count}")

if __name__ == "__main__":
    main()
