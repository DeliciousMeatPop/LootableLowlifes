# Lootable Lowlifes

A **standalone, code-free** loot mod for **Blade & Sorcery 1.0+** (Crystal Hunt /
Sandbox). Every enemy carries a **lootable coin pouch on their hip** that you can
grab off the body and keep — it converts to Gold when stored, just like the
game's own valuables. Payouts scale by enemy rank across four tiers.

**No DLL. No dependency on any other mod.** The whole thing is ThunderRoad
catalog JSON built on base-game assets.

## How it works (the native ThunderRoad loot chain)

```
Creature (NPC)
   └─► ContainerData   (override, same id as the base creature)
         └─► TableContent in holder "HipsLeft"
               └─► LootTable  (weighted, tiered)
                     └─► ItemData  (coin pouch → base prefab Bas.Item.Valuable.LootBag)
```

Nothing runs on death — the pouch is a real held item on the living NPC, so the
game handles persistence and looting for free. Killing the enemy just leaves the
pouch on the ragdoll for you to take.

### Why it needs no other mod

The coin pouch items point at the **base-game** prefab `Bas.Item.Valuable.LootBag`
(the `Bas.` prefix = base game), so the mod ships no art and references no other
mod's content. This is the same technique the Gilded Goons mod uses — we just do
it self-contained.

## Repository layout

```
mod/                         # <-- this IS the installable mod folder
  manifest.json
  Items/        Item_LLPouch*.json        # 6 coin pouches (8g … 220g)
  LootTables/   LootTable_LLNpcLootT*.json # 4 tiers (T0…T3)
  Containers/   Container_<Creature>.json  # 31 enemy archetype overrides
tools/
  gen-catalog.py     # regenerates everything in mod/ (edit values here)
  List-ItemIds.ps1   # scans your install/mods for real item IDs
src/                 # OPTIONAL alternative: a C# ThunderScript drop-on-death
                     # mod. Superseded by the native approach above; see below.
```

## Installing

1. Copy the **contents** of `mod/` into a new folder:
   `…\Blade & Sorcery\BladeAndSorcery_Data\StreamingAssets\Mods\LootableLowlifes\`
   (so you end up with `…\Mods\LootableLowlifes\manifest.json`, `…\Items\`, etc.)
2. Launch. Kill an enemy, grab the pouch off their hip, store it to bank the gold.

No build step. Editing the JSON just needs a game relaunch.

## Tuning

Edit `tools/gen-catalog.py` and re-run it to regenerate all the JSON:

```bash
python3 tools/gen-catalog.py
```

- **Pouch values** — the `POUCHES` list (id, display name, gold value).
- **Drop odds** — the `TABLES_DEF` weights (higher weight = more likely).
- **Which enemies drop what** — the `CREATURE_TIERS` map (archetype → tier).

You can also hand-edit any single JSON file directly; the generator is just for
bulk consistency.

### Adding weapon / ring drops

Loot tables can drop anything, not just pouches. Add a `Drop` entry referencing
any valid item id (find real ids with `tools/List-ItemIds.ps1`, or base prefab
addresses with `tools/Find-BaseLoot.ps1`).

## Sellable treasure (optional — Baron's Bounties)

The base game ships no ring/gem items to reuse (only the loot-bag prefab), so
sellable treasure comes from **Baron's Bounties** as a **soft dependency**:

- Elite enemies (tiers **T2/T3**) have a small chance to drop Baron's Bounties
  relics instead of coins — Shard Brooch, Oswen's Spyglass, Madene Stardial,
  Model Oron, a gem `CrystalBag`, and (rarely) King Aldaric's Crown.
- These are Baron's Bounties' own item ids; this mod only *references* them.
- **If Baron's Bounties isn't installed, nothing breaks** — ThunderRoad just
  skips the missing reference, and the coin pouches (which are ours) drop as
  normal. So the mod stays fully functional standalone; treasure is a bonus for
  players who also run Baron's Bounties.

Tune it in `tools/gen-catalog.py` via `TREASURE_T2` / `TREASURE_T3` (weights are
relative to the coin-pouch weights in the same table). To add treasure from a
different mod, drop its item ids in there the same way.

## Enemies covered

31 base archetypes across Soldiers, Cultists, Scavengers, and Tribals, tiered
T0 (serfs, muggers, trackers) → T3 (bishops, champions, highborn, magistrates).
See `CREATURE_TIERS` in the generator for the exact mapping.

## Caveat / verifying

Container overrides replace the base creature's container by id. This is the
same mechanism Gilded Goons uses and it does not disarm enemies (weapon loadouts
come from the creature's equipment, not this container). If you ever see an enemy
type spawn without its expected gear, tell me which archetype and we'll adjust
that container.

Item ids are prefixed `LL…` and loot-table ids `LLNpcLoot…` to avoid clashing
with other mods. Container ids intentionally match base creatures (that's how the
override attaches) — so running this **alongside** Gilded Goons means whichever
loads last wins for a given enemy. Run one or the other for predictable results.

---

## Optional: the C# drop-on-death mod (`src/`)

The repo also contains an earlier **ThunderScript** implementation that hooks
`EventManager.onCreatureKill` and spawns items at the ragdoll in code. The native
data mod above supersedes it (simpler, more update-proof, no despawn concerns),
so you don't need it. It's kept for reference / as an alternative if you want
code-driven drop logic.

Build it (needs the game assemblies — see `libs/README.md`):

```bash
dotnet build src/BanditLootMod.csproj -c Release
```

Do **not** install both the DLL and the data mod at once, or enemies will get
loot from both systems.
