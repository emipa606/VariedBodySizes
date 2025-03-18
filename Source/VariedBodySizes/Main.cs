using Log = Verse.Log;

namespace VariedBodySizes;

/// <summary>
///     Contains logic that must be run after the DefDatabase is initialized (thus, StaticConstructorOnStartup)
/// </summary>
[StaticConstructorOnStartup]
public static class Main
{
    public const string FemaleSuffix = "_female";
    public static VariedBodySizes_GameComponent CurrentComponent;
    public static readonly bool VehiclesLoaded;
    public static readonly List<ThingDef> AllPawnTypes;

    static Main()
    {
        VehiclesLoaded = ModLister.GetActiveModWithIdentifier("SmashPhil.VehicleFramework") != null;
        AllPawnTypes = DefDatabase<ThingDef>.AllDefsListForReading.Where(def => def.race != null && !def.IsCorpse)
            .OrderBy(def => def.label).ToList();
        HarmonyPatches.ApplyAll(new Harmony("Mlie.VariedBodySizes"));
    }

    public static GraphicMeshSet TranslateForPawn(GraphicMeshSet baseMesh, Pawn pawn)
    {
        // North[2] is positive on both x and y-axis
        var baseVector = baseMesh.MeshAt(Rot4.North).vertices[2] * 2 * HarmonyPatches.GetScalarForPawn(pawn);
        return MeshPool.GetMeshSetForSize(baseVector.x, baseVector.z);
    }

    public static float GetPawnVariation(Pawn pawn)
    {
        if (VariedBodySizesMod.instance.Settings.IgnoreMechs && pawn.RaceProps.IsMechanoid)
        {
            return 1f;
        }

        if (VariedBodySizesMod.instance.Settings.IgnoreVehicles && pawn.def.thingClass.Name.EndsWith("VehiclePawn"))
        {
            return 1f;
        }

        var sizeRange = VariedBodySizesMod.instance.Settings.DefaultVariation;

        var pawnDefName = pawn.def.defName;
        if (VariedBodySizesMod.instance.Settings.SeparateFemale && pawn.gender == Gender.Female)
        {
            pawnDefName += FemaleSuffix;
            sizeRange = VariedBodySizesMod.instance.Settings.DefaultVariationFemale;
        }

        if (VariedBodySizesMod.instance.Settings.VariedBodySizes.TryGetValue(pawnDefName, out var bodySize))
        {
            sizeRange = bodySize;
        }

        var randomStandardNormal = Math.Sqrt(-2.0 * Math.Log(Rand.Value)) * Math.Sin(2.0 * Math.PI * Rand.Value);
        var mean = (sizeRange.min + sizeRange.max) / 2;
        var deviationDivider = VariedBodySizesMod.instance.Settings.StandardDeviationDivider;
        if (VariedBodySizesMod.instance.Settings.SeparateFemale && pawn.gender == Gender.Female)
        {
            deviationDivider = VariedBodySizesMod.instance.Settings.StandardDeviationDividerFemale;
        }

        var standardDeviation = (sizeRange.max - sizeRange.min) / deviationDivider;
        return (float)Math.Round(mean + (standardDeviation * randomStandardNormal), 2);
    }

    public static void ResetAllCaches(Pawn pawn = null)
    {
        if (pawn == null)
        {
            HarmonyPatches.FacialAnimation_GetHeadMeshSetPatch.HeadCache?.Clear();
            CurrentComponent?.sizeCache?.Clear();
            return;
        }

        HarmonyPatches.FacialAnimation_GetHeadMeshSetPatch.HeadCache.Remove(pawn);
        CurrentComponent.sizeCache.Remove(pawn);
        GlobalTextureAtlasManager.TryMarkPawnFrameSetDirty(pawn);
    }

    public static void LogMessage(string message, bool forced = false)
    {
        if (!forced && !VariedBodySizesMod.instance.Settings.VerboseLogging)
        {
            return;
        }

        Log.Message($"[VariedBodySizes]: {message}");
    }
}