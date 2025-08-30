using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace VariedBodySizes;

public class VariedBodySizes_GameComponent : GameComponent
{
    // ReSharper disable once ChangeFieldTypeToSystemThreadingLock
    private readonly object dictLock = new();
    internal readonly TimedCache<float> SizeCache = new(36);
    public Dictionary<int, float> VariedBodySizesDictionary;

    // ReSharper disable once UnusedParameter.Local
    public VariedBodySizes_GameComponent(Game game)
    {
        Main.CurrentComponent = this;
    }

    // This way others can hook and modify while benefiting from our cache
    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.PreserveSig)]
    private static float OnCalculateBodySize(float bodySize, Pawn pawn)
    {
        return pawn == null ? 1f : bodySize;
    }

    public float GetVariedBodySize(Pawn pawn)
    {
        if (pawn == null)
        {
            return 1f;
        }

        if (Scribe.mode != LoadSaveMode.Inactive)
        {
            return 1f;
        }

        // cached value, or calculate, cache and return
        if (SizeCache.TryGet(pawn, out var cachedSize))
        {
            return cachedSize;
        }

        float bodySize;
        var pawnId = pawn.thingIDNumber;

        lock (dictLock)
        {
            VariedBodySizesDictionary ??= new Dictionary<int, float>();

            if (!VariedBodySizesDictionary.TryGetValue(pawnId, out bodySize))
            {
                bodySize = Main.GetPawnVariation(pawn);
                VariedBodySizesDictionary[pawnId] = bodySize;

                if (VariedBodySizesMod.Instance.Settings.VerboseLogging)
                {
                    Main.LogMessage($"Setting size of {pawn.NameFullColored} ({pawn.ThingID}) to {bodySize}");
                }
            }
        }

        // Apply any registered modifiers when storing
        bodySize = OnCalculateBodySize(bodySize, pawn);
        SizeCache.Set(pawn, bodySize);

        return bodySize;
    }

    public void SetVariedBodySize(Pawn pawn, float size)
    {
        if (pawn == null)
        {
            return;
        }

        lock (dictLock)
        {
            VariedBodySizesDictionary ??= new Dictionary<int, float>();
            VariedBodySizesDictionary[pawn.thingIDNumber] = size;
        }

        SizeCache.Set(pawn, size);
    }

    /// <summary>
    ///     Load the body size dict from XML and convert it from &lt;Pawn, float&gt; to &lt;int, float&gt;
    /// </summary>
    /// <returns>bool - whether the dictionary was migrated</returns>
    private bool MigrateDictionaryFormat()
    {
        var dictPath =
            "/savegame/game/components/li[@Class=\"VariedBodySizes.VariedBodySizes_GameComponent\"]/VariedBodySizesDictionary";
        var document = Scribe.loader.curXmlParent?.OwnerDocument;
        var bodySizeDict = document?.SelectSingleNode(dictPath);
        var keys = bodySizeDict?["keys"]?.ChildNodes;
        var values = bodySizeDict?["values"]?.ChildNodes;

        if (bodySizeDict is null || keys is null || values is null)
        {
            return false;
        }

        var i = 0;
        lock (dictLock)
        {
            VariedBodySizesDictionary ??= new Dictionary<int, float>();

            while (true)
            {
                var key = keys[i];
                var value = values[i++]; // next iter here
                if (key is null || value is null)
                {
                    break;
                }

                var keyText = Regex.Match(key.InnerText, @"\d+$").Value;
                var hasKey = int.TryParse(keyText, out var keyInt);
                var hasValue = float.TryParse(value.InnerText, out var valueFloat);
                if (hasKey && hasValue)
                {
                    VariedBodySizesDictionary[keyInt] = valueFloat;
                }
            }
        }

        lock (dictLock)
        {
            return VariedBodySizesDictionary is not null;
        }
    }

    public override void ExposeData()
    {
        base.ExposeData();
        if (Scribe.mode == LoadSaveMode.LoadingVars && MigrateDictionaryFormat())
        {
            return;
        }

        lock (dictLock)
        {
            Scribe_Collections.Look(ref VariedBodySizesDictionary, "VariedBodySizesData", LookMode.Value,
                LookMode.Value);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                VariedBodySizesDictionary ??= new Dictionary<int, float>();
            }
        }
    }
}