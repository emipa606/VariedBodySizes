# GitHub Copilot Instructions for Varied Body Sizes Mod

## Mod Overview and Purpose

**Mod Name:** Varied Body Sizes

Varied Body Sizes is a RimWorld mod designed to provide players with the ability to customize the size of pawns and animals, both cosmetically and functionally. It enhances the game's flexibility by allowing users to set size ranges from 10% to 200% of the normal size, affecting only the cosmetic appearance by default. Additionally, players can enable functional changes that impact various game stats such as meat yield, carry capacity, and more.

## Key Features and Systems

- **Size Customization:** Adjust the cosmetic and functional size of pawns and animals.
- **Functional Changes (Optional):** Enable settings that alter stats such as meat yield, leather amount, and health scaling.
- **Stat Modifications:** Optionally modify:
  - Body size stat
  - Health factor and melee damage
  - Hunger rate and animal resource yield
  - Melee dodge chance and humanoid lactation
- **Compatibility:** Works with most pawn and animal mods without specific load order requirements.
- **Debug Options:** Manually adjust sizes of pawns during gameplay.

### Verified Compatible Mods

- Humanoid Alien Races
- Babies and Children

## Coding Patterns and Conventions

- **Class Naming:** Classes are named using PascalCase, e.g., `TimedCache`.
- **Method Naming:** Methods follow PascalCase, e.g., `Set`, `GetVariedBodySize`.
- **File Structure:** The mod is organized into specific files focusing on distinct aspects like caching (`CacheEntry.cs`, `TimedCache.cs`) and patches (`PawnStatPatches.cs`, `CombatCalculationPatches.cs`).

## XML Integration

While the primary functionality is implemented in C#, XML can be used for game object definitions and configurations within the mod. Ensure XML synchronization aligns with C# data manipulations.

## Harmony Patching

Harmony is utilized extensively to patch existing game methods to adapt them for size alterations. Notable patches include:

- **Combat Calculation Patches:** Modify melee damage and dodge chance.
- **Pawn Stat Patches:** Adjust body size, health scaling, and related stats.
- **Render Patches:** Change the graphical representation of pawns and animals.

### Harmony Patch Structure

Harmony patches are organized into partial classes within a consistent namespace, `HarmonyPatches`, allowing for modular patching:

csharp
public static partial class HarmonyPatches


Each subsystem, such as rendering or combat, has its own sub-class for specific method patches.

## Suggestions for Copilot

1. **Method Suggestions:** Assist with writing methods that interact with the game's stat system, such as `GetVariedBodySize`.
2. **Patching Assistance:** Recommend Harmony patch configurations and syntax for modifying specific game behaviors.
3. **Debug Developments:** Generate code snippets for debug functions to alter pawn attributes dynamically.
4. **Optimization Tips:** Provide guidance on optimizing cached data retrieval and utilization.

By following these detailed instructions and leveraging GitHub Copilot, contributors can effectively add new features, maintain code quality, and enhance the Varied Body Sizes mod for RimWorld.
