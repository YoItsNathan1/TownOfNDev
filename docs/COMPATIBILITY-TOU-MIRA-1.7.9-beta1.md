# TownOfNDev Beta.2.0.0 — ToU-Mira 1.7.9 Beta 1 compatibility candidate

This is a separate, unverified compatibility branch for ToU-Mira 1.7.9 Beta 1. It retains the Beta.2.0.0 role, ability, and network changes; the separate technical suffix prevents the two different DLLs from being mistaken for one another in multiplayer.

## Build inputs

- Fresh official [ToU-Mira 1.7.9 Beta 1 release](https://github.com/AU-Avengers/TOU-Mira/releases/tag/1.7.9-beta1), with its bundled MiraAPI 0.6.0 and dependencies.
- Among Us 2026.9.29-targeted GameLibs and Windows .NET 6 SDK.
- Do not mix a 1.7.3 game folder with 1.7.9 dependencies. The upstream release explicitly requires a complete reinstall.

NuGet's older TownOfUsMira 1.7.3 and MiraAPI 0.5.0 references are deliberately removed from this variant. `AmongUs.props` instead compiles against the DLLs installed by the official upstream release. This is a source-only overlay: third-party DLLs are **not** redistributed.

## Build from your local repository root

```powershell
.\tools\build-tou179.ps1 -InstallDir 'C:\Games\AmongUs-ToUMira-1.7.9-beta1'
```

Substitute the actual clean installation folder. The script locates exactly one `TownOfUsMira.dll` and `MiraAPI.dll` under `BepInEx\plugins`, then restores and builds without auto-installing the resulting DLL. Inspect the versions in your clean installation first: checking only the filenames is not sufficient.

The expected build output, **only if successful**, is:

`TownOfNDev\bin\Release\net6.0\TownOfNDev-0.1.0-beta.2.0.0-tou179b1.dll`

If restore or compilation fails, do not create the GitHub release. Capture the full console output and fix the compatibility errors first.

## Release distinction

- Public release title: **TownOfNDev v0.1.0-beta.2.0.0 (ToU-Mira 1.7.9 Beta 1 Compatibility)**
- Unique GitHub tag: `v0.1.0-beta.2.0.0-tou179b1`
- Technical assembly suffix: `beta.2.0.0-tou179b1`
- Original `v0.1.0-beta.2.0.0` (ToU 1.7.3) remains available and unchanged.

All lobby participants must use the same TownOfNDev compatibility DLL and matching ToU-Mira and MiraAPI versions. Never install both TownOfNDev variants together.

## Before publishing

- [ ] Fresh install of upstream 1.7.9 Beta 1, as explicitly instructed by the upstream release.
- [ ] Exact runtime DLLs identified, restore and Release compilation pass.
- [ ] Game loads with no BepInEx/IL2CPP exceptions.
- [ ] All custom roles and modifiers register and show their options.
- [ ] At least three-player host and non-host tests before and after several meetings: SUI, Tracer, Condemner, Reverser, Fungi.
- [ ] Test accepted and rejected host requests, cooldown/uses, Fungi growth and custom win conditions.
- [ ] Check host migration/role draft, lobby exit/re-entry and mixed-version rejection.

This is an engineering compatibility candidate, not a verified new release until the checklist passes.
