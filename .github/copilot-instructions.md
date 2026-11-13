# GitHub Copilot Instructions for Varied Body Sizes Mod

## Mod Overview and Purpose

**Varied Body Sizes** is a RimWorld mod that introduces the ability to set custom size ranges for all pawns and animals. The primary purpose of this mod is to enhance cosmetic diversity within the game, allowing players to create unique and varied character appearances. Additionally, the mod offers the option to modify actual body sizes, which can affect various in-game statistics such as carry capacity, meat amount, and health.

## Key Features and Systems

- **Custom Size Ranges**: Allows individual customization of body sizes for pawns and animals, ranging from 10% to 200% of the normal size.
- **Functional Changes**: Options to modify the "body size" stat, health scaling, hunger rate, melee damage factor, melee dodge chance, and animal resource yield.
- **Compatibility**: Works with most pawn and animal mods, and is designed to be load order independent.
- **Debug Options**: Includes features for manually changing sizes of pawns and a search filter for specific pawns or mod names.
- **Mod-Specific Settings**: The mod provides a configuration screen to toggle various effects and set default settings.

## Coding Patterns and Conventions

- **Modular Design**: The mod is structured into multiple C# files, each handling specific functionalities. For example, `HarmonyPatches` contain patches for modifying game stats and rendering.
- **Harmony Patching**: Utilizes Harmony for modifying existing RimWorld methods to introduce new behaviors or modify existing ones.
- **XML Integration**: Although this mod does not currently use XML definitions, it is designed to integrate seamlessly with other mods that do.
- **Caching Mechanisms**: Implements caching for frequently accessed data to improve performance.
- **Localization Support**: Uses translation methods to support different languages within the mod.

## XML Integration

The Varied Body Sizes mod does not currently utilize XML definitions. However, it is designed to work with XML-based mods and integrate seamlessly with them. If future updates include XML definitions, they will follow standard RimWorld XML conventions.

## Harmony Patching

The mod extensively uses Harmony patches to modify game behavior. Some key areas patched include:

- **Pawn Stats**: Patches for calculating body size, health scaling, and resource yields.
- **Rendering**: Patches for graphical representations of pawns, including hair and beard sizes.
- **Game Mechanics**: Patches for melee damage calculation and dodge chance adjustments.

Example Harmony Patch:

csharp
[HarmonyPatch(typeof(Pawn_BodySizePatch), "GetBodySize")]
public static class Pawn_BodySizePatch
{
    [HarmonyPostfix]
    public static void Postfix(Pawn pawn, ref float __result)
    {
        float variedSize = GetVariedBodySize(pawn);
        __result *= variedSize;
    }
}


## Suggestions for Copilot

- **Code Snippets**: Provide examples of Harmony patches and caching mechanisms used in the mod.
- **Mod Configuration**: Offer snippets for creating and managing mod settings screens.
- **Localization**: Provide examples of translation methods for supporting multiple languages.
- **XML Integration**: If XML definitions are added in future updates, provide guidance on integrating them with the existing mod structure.
- **Performance Optimization**: Suggest best practices for optimizing performance, especially in areas where caching is used.

## Conclusion

The Varied Body Sizes mod is a powerful tool for enhancing diversity and customization in RimWorld. By leveraging Harmony patches and modular design, it achieves its goals of providing unique and varied body sizes for pawns and animals. As the mod evolves, it will continue to integrate seamlessly with other RimWorld mods and provide a rich experience for players.



This .github/copilot-instructions.md file provides detailed guidelines and examples for GitHub Copilot to assist developers working on the Varied Body Sizes mod. It covers the mod's purpose, key features, coding patterns, and future integration possibilities, ensuring that Copilot can provide relevant and useful suggestions.

## Project Solution Guidelines
- Relevant mod XML files are included as Solution Items under the solution folder named XML, these can be read and modified from within the solution.
- Use these in-solution XML files as the primary files for reference and modification.
- The `.github/copilot-instructions.md` file is included in the solution under the `.github` solution folder, so it should be read/modified from within the solution instead of using paths outside the solution. Update this file once only, as it and the parent-path solution reference point to the same file in this workspace.
- When making functional changes in this mod, ensure the documented features stay in sync with implementation; use the in-solution `.github` copy as the primary file.
- In the solution is also a project called Assembly-CSharp, containing a read-only version of the decompiled game source, for reference and debugging purposes.
- For any new documentation, update this copilot-instructions.md file rather than creating separate documentation files.


## Hard rules (must follow)
- Do NOT run commands that modify the repo (no git commit, git apply, dotnet format) unless explicitly asked.
- Prefer minimal reads: read only the smallest code region needed (around the suspicious lines).
- When mentioning SonarQube issues, automatically use the SonarQube MCP service to fetch and address issues instead of making inferred fixes without querying SonarQube first.
- When mentioning the rimworld log, automatically use the Rimworld MCP service to fetch the log.

