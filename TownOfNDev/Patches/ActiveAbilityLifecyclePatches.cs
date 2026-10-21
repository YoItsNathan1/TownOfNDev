namespace TownOfNDev.Patches;

// beta.2: Intentionally inert.
//
// TownOfNDev no longer recreates, forces visible, or manually recovers custom
// ability buttons around meetings. MiraAPI/Town of Us owns the registered button
// lifecycle, cooldown resets, effect resets and per-round use restoration.
// Keeping this source file as a no-op lets the beta.2 repo patch safely overwrite
// beta.1.0.3 without requiring a separate delete step.
public static class ActiveAbilityLifecyclePatches
{
}
