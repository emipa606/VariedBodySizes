﻿namespace VariedBodySizes;

/// <summary>
///     Definition of the settings for the mod
/// </summary>
public class VariedBodySizesModSettings : ModSettings
{
    public bool AffectHarvestYield;
    public bool AffectLactating;
    public bool AffectMeleeDamage = ModsConfig.IsActive("mute.genebodysize");
    public bool AffectMeleeDodgeChance = ModsConfig.IsActive("mute.genebodysize");
    public bool AffectRealBodySize = ModsConfig.IsActive("mute.genebodysize");
    public bool AffectRealHealthScale = ModsConfig.IsActive("mute.genebodysize");
    public bool AffectRealHungerRate;
    public FloatRange DefaultVariation = new FloatRange(0.9f, 1.1f);
    public FloatRange DefaultVariationFemale = new FloatRange(0.9f, 1.1f);
    public bool IgnoreMechs;
    public bool IgnoreVehicles;
    public bool SeparateFemale;
    public float StandardDeviationDivider = 6f;
    public float StandardDeviationDividerFemale = 6f;
    public Dictionary<string, FloatRange> VariedBodySizes;
    private List<string> variedBodySizesKeys;

    private List<FloatRange> variedBodySizesValues;
    public bool VerboseLogging;

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref VerboseLogging, "VerboseLogging");
        Scribe_Values.Look(ref SeparateFemale, "SeparateFemale");
        Scribe_Values.Look(ref AffectRealBodySize, "AffectRealBodySize");
        Scribe_Values.Look(ref AffectRealHealthScale, "AffectRealHealthScale");
        Scribe_Values.Look(ref AffectRealHungerRate, "AffectRealHungerRate");
        Scribe_Values.Look(ref AffectMeleeDamage, "AffectMeleeDamage");
        Scribe_Values.Look(ref AffectMeleeDodgeChance, "AffectMeleeDodgeChance");
        Scribe_Values.Look(ref AffectHarvestYield, "AffectHarvestYield");
        Scribe_Values.Look(ref AffectLactating, "AffectLactating");
        Scribe_Values.Look(ref IgnoreMechs, "IgnoreMechs");
        Scribe_Values.Look(ref IgnoreVehicles, "IgnoreVehicles");
        Scribe_Values.Look(ref StandardDeviationDivider, "StandardDeviationDivider", 6f);
        Scribe_Values.Look(ref StandardDeviationDividerFemale, "StandardDeviationDividerFemale", 6f);
        Scribe_Values.Look(ref DefaultVariation, "DefaultVariation", new FloatRange(0.9f, 1.1f));
        Scribe_Values.Look(ref DefaultVariationFemale, "DefaultVariationFemale", new FloatRange(0.9f, 1.1f));
        Scribe_Collections.Look(ref VariedBodySizes, "VariedBodySizes", LookMode.Value,
            LookMode.Value,
            ref variedBodySizesKeys, ref variedBodySizesValues);
    }

    public void ResetSettings()
    {
        VariedBodySizes = new Dictionary<string, FloatRange>();
        DefaultVariation = new FloatRange(0.9f, 1.1f);
        DefaultVariationFemale = new FloatRange(0.9f, 1.1f);
        StandardDeviationDivider = 6f;
        StandardDeviationDividerFemale = 6f;
    }
}