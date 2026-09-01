# Lootable Lowlifes

A C# scripted mod for **Blade & Sorcery 1.0+** (Crystal Hunt / Sandbox). When an
enemy dies, it drops **persistent, keepable** loot — Florin pouches, rings,
crystal shards, and player-storable weapons — right at the ragdoll, bypassing the
default despawning/unstoreable behaviour of NPC-held items.

Because every drop is a *fresh copy* spawned from the game Catalog (not the NPC's
own non-storable gear), the items survive, can be picked up, holstered, and kept.

## How it works

```
[Enemy Killed]
      │
      ▼
[EventManager.onCreatureKill]
      │
      ├─► Validate  (eventTime == OnEnd, creature != player, ragdoll exists)
      ├─► Roll      (global drop chance → weighted tier → weighted item → quantity)
      └─► Spawn     Catalog.GetData<ItemData>(id).SpawnAsync(...)
                     ├─ position: torso/pelvis + height offset + scatter
                     └─ physics : gentle upward "pop" impulse
```

- **`src/LootDropModule.cs`** — the `ThunderScript`. Subscribes to
  `EventManager.onCreatureKill` in `ScriptLoaded()` and detaches in
  `ScriptUnload()` to avoid dangling handlers across map loads. Handles entity
  filtering, positioning, async spawning, and the pop impulse.
- **`src/LootTable.cs`** — stateless weighted-random selection (tier → item →
  quantity), driven entirely by config.
- **`src/Config/DropConfig.cs`** — the serializable config model plus built-in
  defaults so the mod works even with no JSON present.

## Configuring loot (no recompile needed)

Edit **`mod/Item_LootTables.json`**. It deserializes straight into `DropConfig`
via Unity's `JsonUtility`, so the JSON shape must match the field names exactly.

| Field | Meaning |
|-------|---------|
| `enabled` | Master switch. `false` installs no hooks. |
| `globalDropChance` | 0..1 chance a kill drops anything. |
| `minDrops` / `maxDrops` | Range of item stacks per drop event. |
| `spawnHeightOffset` | Metres above the ragdoll part to spawn. |
| `scatterRadius` | Horizontal spread so drops don't clip/stack. |
| `popForce` / `popScatterForce` | Upward + random impulse strength. |
| `tierWeights` | Relative odds of `common` / `rare` / `legendary`. |
| `tiers[]` | Named tiers, each a weighted pool of `entries`. |
| `entries[].itemId` | The Catalog `ItemData` id to spawn. |
| `entries[].weight` | Relative odds within its tier. |
| `entries[].minQuantity` / `maxQuantity` | Stack count range. |

> **Item IDs** must match real Catalog ids from the game version / mods you run.
> The defaults use plausible base-game ids (`Currency`, `RingSilver`, `RingGold`,
> `CrystalShard`, `WeaponSwordShortCommon`, `WeaponAxe1H`, `WeaponMace1H`,
> `Apple`). If an id doesn't exist in your install the drop is skipped and a
> warning is logged — verify against your `BladeAndSorcery_Data` catalog and
> adjust as needed.

## Building

Requires the .NET SDK and a Blade & Sorcery install for the game assemblies.

```bash
dotnet build src/BanditLootMod.csproj -c Release \
  -p:BSInstallDir="C:\Program Files (x86)\Steam\steamapps\common\Blade & Sorcery"
```

Or set the `BLADE_AND_SORCERY_DIR` environment variable instead of passing
`BSInstallDir`. The project references (but does not copy) `ThunderRoad.dll` and
the `UnityEngine.*` modules from `<install>\BladeAndSorcery_Data\Managed`.

## Installing

1. Build `LootableLowlifes.dll`.
2. Create a mod folder:
   `...\BladeAndSorcery\BladeAndSorcery_Data\StreamingAssets\Mods\LootableLowlifes\`
3. Copy into it:
   - `LootableLowlifes.dll` (build output)
   - `mod/manifest.json`
   - `mod/Item_LootTables.json`
4. Launch the game. ThunderRoad auto-loads the `ThunderScript` on startup.

## Notes & safety

- All kill-handling is wrapped in try/catch so a loot error never interrupts the
  game's death pipeline.
- Player deaths are ignored; only fully-dead (`OnEnd`) non-player creatures drop.
- Spawns are asynchronous (`SpawnAsync`) to avoid frame drops during combat.

## Repository layout

```
src/
  BanditLootMod.csproj   # build config + game assembly references
  LootDropModule.cs      # ThunderScript & event lifecycle
  LootTable.cs           # weighted probability engine
  Config/DropConfig.cs   # config model + defaults
mod/
  manifest.json          # ThunderRoad mod manifest
  Item_LootTables.json   # externalised, tunable loot tables
```
