﻿using Mlie;

namespace VariedBodySizes;

internal class VariedBodySizesMod : Mod
{
    public const float MinimumSize = 0.25f;
    public const float MaximumSize = 4f;

    /// <summary>
    ///     The instance of the settings to be read by the mod
    /// </summary>
    public static VariedBodySizesMod instance;

    private static string currentVersion;
    private static readonly Vector2 buttonSize = new Vector2(120f, 25f);
    private static readonly Vector2 iconSize = new Vector2(58f, 58f);
    private static string searchText;
    private static Vector2 scrollPosition;
    private static readonly Color alternateBackground = new Color(0.2f, 0.2f, 0.2f, 0.5f);


    /// <summary>
    ///     The private settings
    /// </summary>
    public readonly VariedBodySizesModSettings Settings;

    /// <summary>
    ///     Constructor
    /// </summary>
    /// <param name="content"></param>
    public VariedBodySizesMod(ModContentPack content)
        : base(content)
    {
        instance = this;
        searchText = string.Empty;
        currentVersion = VersionFromManifest.GetVersionFromModMetaData(content.ModMetaData);
        Settings = GetSettings<VariedBodySizesModSettings>();
        Settings.VariedBodySizes ??= new Dictionary<string, FloatRange>();
    }

    public override string SettingsCategory()
    {
        return "Varied Body Sizes";
    }

    /// <summary>
    ///     The settings-window
    /// </summary>
    /// <param name="rect"></param>
    public override void DoSettingsWindowContents(Rect rect)
    {
        base.DoSettingsWindowContents(rect);
        var originalAnchor = Text.Anchor;
        var genderAddon = "";
        if (Settings.SeparateFemale)
        {
            genderAddon = ".male";
        }

        var listing_Standard = new Listing_Standard();
        listing_Standard.Begin(rect);
        listing_Standard.ColumnWidth = rect.width / 2.1f;

        var variationRect = listing_Standard.GetRect(30f);
        Widgets.FloatRange(variationRect, $"DefaultVariation{genderAddon}".GetHashCode(),
            ref Settings.DefaultVariation,
            MinimumSize, MaximumSize, $"VariedBodySizes.defaultvariation.label{genderAddon}", ToStringStyle.PercentOne);
        Text.Anchor = TextAnchor.UpperLeft;
        Widgets.Label(variationRect, Settings.DefaultVariation.min.ToStringPercent());
        Text.Anchor = TextAnchor.UpperRight;
        Widgets.Label(variationRect, Settings.DefaultVariation.max.ToStringPercent());

        if (Settings.SeparateFemale)
        {
            genderAddon = ".female";
            variationRect = listing_Standard.GetRect(30f);
            Widgets.FloatRange(variationRect, $"DefaultVariation{genderAddon}".GetHashCode(),
                ref Settings.DefaultVariationFemale,
                MinimumSize, MaximumSize, $"VariedBodySizes.defaultvariation.label{genderAddon}",
                ToStringStyle.PercentOne);
            Text.Anchor = TextAnchor.UpperLeft;
            Widgets.Label(variationRect, Settings.DefaultVariationFemale.min.ToStringPercent());
            Text.Anchor = TextAnchor.UpperRight;
            Widgets.Label(variationRect, Settings.DefaultVariationFemale.max.ToStringPercent());
        }

        Text.Anchor = originalAnchor;


        listing_Standard.Gap();
        genderAddon = "";
        if (Settings.SeparateFemale)
        {
            genderAddon = " ♂";
        }

        var dividerRect = listing_Standard.GetRect(30f);
        Settings.StandardDeviationDivider = Widgets.HorizontalSlider(dividerRect,
            Settings.StandardDeviationDivider, 2f, 20f, false,
            "VariedBodySizes.StandardDeviationDivider".Translate() + genderAddon,
            "VariedBodySizes.StandardDeviationDivider.Spread".Translate(),
            "VariedBodySizes.StandardDeviationDivider.Middle".Translate());
        TooltipHandler.TipRegion(dividerRect, "VariedBodySizes.StandardDeviationDividerTT".Translate());
        if (Settings.SeparateFemale)
        {
            genderAddon = " ♀";
            dividerRect = listing_Standard.GetRect(30f);
            Settings.StandardDeviationDividerFemale = Widgets.HorizontalSlider(dividerRect,
                Settings.StandardDeviationDividerFemale, 2f, 20f, false,
                "VariedBodySizes.StandardDeviationDivider".Translate() + genderAddon,
                "VariedBodySizes.StandardDeviationDivider.Spread".Translate(),
                "VariedBodySizes.StandardDeviationDivider.Middle".Translate());
            TooltipHandler.TipRegion(dividerRect, "VariedBodySizes.StandardDeviationDividerTT".Translate());
        }

        if (Current.Game != null && Main.CurrentComponent != null)
        {
            var resetLabel = listing_Standard.Label("VariedBodySizes.resetgame".Translate());
            if (Widgets.ButtonText(
                    new Rect(resetLabel.position + new Vector2(resetLabel.width - buttonSize.x, 0),
                        buttonSize),
                    "VariedBodySizes.reset".Translate()))
            {
                Find.WindowStack.Add(new Dialog_MessageBox(
                    "VariedBodySizes.resetgame.dialog".Translate(),
                    "VariedBodySizes.no".Translate(), null, "VariedBodySizes.yes".Translate(),
                    delegate
                    {
                        Main.CurrentComponent.VariedBodySizesDictionary = new Dictionary<int, float>();
                        Current.Game.CurrentMap.mapPawns.AllPawns.ForEach(delegate(Pawn pawn)
                        {
                            PortraitsCache.SetDirty(pawn);
                            GlobalTextureAtlasManager.TryMarkPawnFrameSetDirty(pawn);
                        });
                    }));
            }
        }

        listing_Standard.CheckboxLabeled("VariedBodySizes.ignoreMechs.label".Translate(), ref Settings.IgnoreMechs,
            "VariedBodySizes.ignoreMechs.tooltip".Translate());
        listing_Standard.CheckboxLabeled("VariedBodySizes.separateFemale.label".Translate(),
            ref Settings.SeparateFemale,
            "VariedBodySizes.separateFemale.tooltip".Translate());

        listing_Standard.NewColumn();

        listing_Standard.CheckboxLabeled("VariedBodySizes.logging.label".Translate(), ref Settings.VerboseLogging,
            "VariedBodySizes.logging.tooltip".Translate());
        listing_Standard.CheckboxLabeled("VariedBodySizes.realbodysize.label".Translate(),
            ref Settings.AffectRealBodySize,
            "VariedBodySizes.realbodysize.tooltip".Translate());
        listing_Standard.CheckboxLabeled("VariedBodySizes.realhealthscale.label".Translate(),
            ref Settings.AffectRealHealthScale,
            "VariedBodySizes.realhealthscale.tooltip".Translate());
        listing_Standard.CheckboxLabeled("VariedBodySizes.realhungerrate.label".Translate(),
            ref Settings.AffectRealHungerRate,
            "VariedBodySizes.realhungerrate.tooltip".Translate());
        listing_Standard.CheckboxLabeled("VariedBodySizes.meleedamage.label".Translate(),
            ref Settings.AffectMeleeDamage,
            "VariedBodySizes.meleedamage.tooltip".Translate());
        listing_Standard.CheckboxLabeled("VariedBodySizes.meleedodgechance.label".Translate(),
            ref Settings.AffectMeleeDodgeChance,
            "VariedBodySizes.meleedodgechance.tooltip".Translate());
        listing_Standard.CheckboxLabeled("VariedBodySizes.harvestyield.label".Translate(),
            ref Settings.AffectHarvestYield,
            "VariedBodySizes.harvestyield.tooltip".Translate());
        if (ModLister.BiotechInstalled)
        {
            listing_Standard.CheckboxLabeled("VariedBodySizes.lactating.label".Translate(),
                ref Settings.AffectLactating,
                "VariedBodySizes.lactating.tooltip".Translate());
        }
        else
        {
            Settings.AffectLactating = false;
        }

        if (Main.VehiclesLoaded)
        {
            listing_Standard.CheckboxLabeled("VariedBodySizes.ignoreVehicles.label".Translate(),
                ref Settings.IgnoreVehicles,
                "VariedBodySizes.ignoreVehicles.tooltip".Translate());
        }
        else
        {
            Settings.IgnoreVehicles = false;
        }

        if (currentVersion != null)
        {
            GUI.contentColor = Color.gray;
            listing_Standard.Label("VariedBodySizes.version.label".Translate(currentVersion));
            GUI.contentColor = Color.white;
        }

        listing_Standard.End();
        var listing_Second = new Listing_Standard();
        var secondRect = rect;
        secondRect.height -= listing_Standard.CurHeight;
        secondRect.y += listing_Standard.CurHeight;
        listing_Second.Begin(secondRect);

        listing_Second.GapLine();
        Text.Font = GameFont.Medium;
        var titleRect = listing_Second.Label("VariedBodySizes.variations.label".Translate());
        Text.Font = GameFont.Small;
        if (Widgets.ButtonText(titleRect.LeftHalf().RightHalf().RightHalf(), "VariedBodySizes.reset".Translate()))
        {
            Settings.ResetSettings();
        }

        searchText = Widgets.TextField(titleRect.RightHalf().RightHalf(), searchText);
        Widgets.Label(titleRect.RightHalf().LeftHalf().RightHalf(), "VariedBodySizes.search".Translate());

        var allPawnTypes = Main.AllPawnTypes;
        if (!string.IsNullOrEmpty(searchText))
        {
            allPawnTypes = Main.AllPawnTypes.Where(def =>
                    def.label.ToLower().Contains(searchText.ToLower()) || def.modContentPack?.Name.ToLower()
                        .Contains(searchText.ToLower()) == true)
                .ToList();
        }

        listing_Second.End();

        var borderRect = rect;
        borderRect.height -= listing_Standard.CurHeight + listing_Second.CurHeight;
        borderRect.y += listing_Standard.CurHeight + listing_Second.CurHeight;
        var scrollContentRect = borderRect;
        scrollContentRect.height = allPawnTypes.Count * 61f;
        scrollContentRect.width -= 20;
        scrollContentRect.x = 0;
        scrollContentRect.y = 0;

        var scrollListing = new Listing_Standard();
        Widgets.BeginScrollView(borderRect, ref scrollPosition, scrollContentRect);
        scrollListing.Begin(scrollContentRect);
        var alternate = false;
        foreach (var pawnType in allPawnTypes)
        {
            var locked = Settings.IgnoreMechs && pawnType.race.IsMechanoid ||
                         Settings.IgnoreVehicles && pawnType.thingClass.Name.EndsWith("VehiclePawn");

            var modInfo = pawnType.modContentPack?.Name;
            var rowRect = scrollListing.GetRect(60);
            alternate = !alternate;
            if (alternate)
            {
                Widgets.DrawBoxSolid(rowRect.ExpandedBy(10, 0), alternateBackground);
            }

            var currentValue = Settings.DefaultVariation;
            var originalColor = GUI.contentColor;
            var pawnTypeDefName = pawnType.defName;

            if (instance.Settings.VariedBodySizes.TryGetValue(pawnTypeDefName, out var bodySize))
            {
                if (locked)
                {
                    instance.Settings.VariedBodySizes.Remove(pawnTypeDefName);
                }
                else
                {
                    currentValue = bodySize;
                    GUI.contentColor = Color.green;
                }
            }

            genderAddon = "";
            if (Settings.SeparateFemale && pawnType.race.hasGenders)
            {
                genderAddon = "♂ ";
            }

            var raceLabel = $"{genderAddon}{pawnType.label.CapitalizeFirst()} ({pawnType.defName}) - {modInfo}";
            DrawIcon(pawnType,
                new Rect(rowRect.position, iconSize));
            var nameRect = new Rect(rowRect.position + new Vector2(iconSize.x, 3f),
                rowRect.size - new Vector2(iconSize.x, (rowRect.height / 2) + 3f));
            var sliderRect = new Rect(rowRect.position + new Vector2(iconSize.x, rowRect.height / 2),
                rowRect.size - new Vector2(iconSize.x, (rowRect.height / 2) + 3f));

            Widgets.Label(nameRect, raceLabel);

            if (locked)
            {
                Widgets.Label(sliderRect, "VariedBodySizes.pawnLocked".Translate());
                continue;
            }

            Widgets.FloatRange(sliderRect, pawnTypeDefName.GetHashCode(), ref currentValue, MinimumSize, MaximumSize,
                null,
                ToStringStyle.PercentOne);
            GUI.contentColor = originalColor;
            if (currentValue != Settings.DefaultVariation)
            {
                Settings.VariedBodySizes[pawnTypeDefName] = currentValue;
                continue;
            }

            if (Settings.VariedBodySizes.ContainsKey(pawnTypeDefName))
            {
                Settings.VariedBodySizes.Remove(pawnTypeDefName);
            }

            if (!Settings.SeparateFemale || !pawnType.race.hasGenders)
            {
                continue;
            }

            pawnTypeDefName += Main.FemaleSuffix;
            locked = Settings.IgnoreMechs && pawnType.race.IsMechanoid ||
                     Settings.IgnoreVehicles && pawnType.thingClass.Name.EndsWith("VehiclePawn");

            modInfo = pawnType.modContentPack?.Name;
            rowRect = scrollListing.GetRect(60);
            alternate = !alternate;
            if (alternate)
            {
                Widgets.DrawBoxSolid(rowRect.ExpandedBy(10, 0), alternateBackground);
            }

            currentValue = Settings.DefaultVariationFemale;
            originalColor = GUI.contentColor;
            if (Settings.VariedBodySizes.TryGetValue(pawnTypeDefName, out bodySize))
            {
                if (locked)
                {
                    Settings.VariedBodySizes.Remove(pawnTypeDefName);
                }
                else
                {
                    currentValue = bodySize;
                    GUI.contentColor = Color.green;
                }
            }

            genderAddon = "♀ ";
            raceLabel = $"{genderAddon}{pawnType.label.CapitalizeFirst()} ({pawnType.defName}) - {modInfo}";
            DrawIcon(pawnType,
                new Rect(rowRect.position, iconSize));
            nameRect = new Rect(rowRect.position + new Vector2(iconSize.x, 3f),
                rowRect.size - new Vector2(iconSize.x, (rowRect.height / 2) + 3f));
            sliderRect = new Rect(rowRect.position + new Vector2(iconSize.x, rowRect.height / 2),
                rowRect.size - new Vector2(iconSize.x, (rowRect.height / 2) + 3f));

            Widgets.Label(nameRect, raceLabel);

            if (locked)
            {
                Widgets.Label(sliderRect, "VariedBodySizes.pawnLocked".Translate());
                continue;
            }

            Widgets.FloatRange(sliderRect, pawnTypeDefName.GetHashCode(), ref currentValue, MinimumSize,
                MaximumSize,
                null,
                ToStringStyle.PercentOne);
            GUI.contentColor = originalColor;
            if (currentValue != Settings.DefaultVariationFemale)
            {
                Settings.VariedBodySizes[pawnTypeDefName] = currentValue;
                continue;
            }

            if (Settings.VariedBodySizes.ContainsKey(pawnTypeDefName))
            {
                Settings.VariedBodySizes.Remove(pawnTypeDefName);
            }
        }

        scrollListing.End();
        Widgets.EndScrollView();
    }

    public override void WriteSettings()
    {
        base.WriteSettings();
        Main.ResetAllCaches();
    }

    private void DrawIcon(ThingDef pawn, Rect rect)
    {
        rect = rect.ContractedBy(3f);
        var pawnKind = DefDatabase<PawnKindDef>.GetNamedSilentFail(pawn.defName);
        if (pawnKind == null)
        {
            TooltipHandler.TipRegion(rect, $"{pawn.LabelCap}\n{pawn.description}");
            GUI.DrawTexture(rect, BaseContent.BadTex);
            return;
        }

        Widgets.DefIcon(rect, pawnKind);
        TooltipHandler.TipRegion(rect, $"{pawnKind.LabelCap}\n{pawnKind.race?.description}");
    }
}