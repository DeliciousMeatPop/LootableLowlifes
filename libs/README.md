# libs/ — game reference assemblies (NOT committed)

The mod compiles against Blade & Sorcery / ThunderRoad assemblies that ship
**only with the game**. They are proprietary and must never be committed here
or redistributed. This folder is git-ignored except for these docs.

Copy the following DLLs into this folder from your install at
`<Blade & Sorcery>\BladeAndSorcery_Data\Managed\`:

- `ThunderRoad.dll`
- `UnityEngine.dll`
- `UnityEngine.CoreModule.dll`
- `UnityEngine.PhysicsModule.dll`

Then build without any extra flags:

```bash
dotnet build src/BanditLootMod.csproj -c Release
```

(Alternatively, skip this folder and point the build at your install with
`-p:BSInstallDir="...\Blade & Sorcery"` or the `BLADE_AND_SORCERY_DIR` env var.)

## For CI

The GitHub Actions workflow downloads these same DLLs into this folder from a
private archive whose URL is stored in the `BS_ASSEMBLIES_URL` repository secret.
See `.github/workflows/build.yml`.
