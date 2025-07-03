using System.Diagnostics;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

namespace VariedBodySizes;

/// <summary>
///     Class containing Harmony patches used by the mod
/// </summary>
/// <remarks>Shared functions are defined in HarmonyPatches/HarmonyPatches.cs</remarks>
public static partial class HarmonyPatches
{
    // We have to overwrite their patches, unfortunately
    private static bool hasVef => ModsConfig.IsActive("oskarpotocki.vanillafactionsexpanded.core");

    /// <summary>
    ///     This is only called once from Main to do our patching.
    /// </summary>
    public static void ApplyAll(Harmony harmony)
    {
        // Most of the VEF render patches co-exist fine, these don't.
        var vePatchesToUndo = new[]
        {
            new KeyValuePair<Type, string>(typeof(HumanlikeMeshPoolUtility),
                nameof(HumanlikeMeshPoolUtility.GetHumanlikeBodySetForPawn)),
            new KeyValuePair<Type, string>(typeof(HumanlikeMeshPoolUtility),
                nameof(HumanlikeMeshPoolUtility.GetHumanlikeHeadSetForPawn)),
            new KeyValuePair<Type, string>(typeof(PawnRenderer), nameof(PawnRenderer.BaseHeadOffsetAt))
        };
        // Go through the problem methods, if they have a VEF-originated patch then remove it
        foreach (var targetPair in vePatchesToUndo)
        {
            var targetMethod = AccessTools.Method(targetPair.Key, targetPair.Value);
            if (targetMethod == null)
            {
                continue;
            }

            var patches = Harmony.GetPatchInfo(targetMethod);
            if (patches?.Owners.Contains("OskarPotocki.VFECore") != true)
            {
                continue;
            }

            Main.LogMessage($"Unpatching {targetMethod.DeclaringType?.Name ?? string.Empty}:{targetMethod.Name}", true);
            harmony.Unpatch(targetMethod, HarmonyPatchType.All, "OskarPotocki.VFECore");
        }

        // Do our patches after we undo theirs
        harmony.PatchAll(Assembly.GetExecutingAssembly());
    }


    public static float GetScalarForPawn(Pawn pawn)
    {
        return Main.CurrentComponent?.GetVariedBodySize(pawn) ?? 1f;
    }

    private static bool notNull(params object[] input)
    {
        if (input.All(o => o is not null))
        {
            return true;
        }

        Main.LogMessage("Signature match not found", true);
        foreach (var obj in input)
        {
            if (obj is MemberInfo memberObj)
            {
                Main.LogMessage($"\tValid entry:{memberObj}", true);
            }
        }

        return false;
    }

    /// <summary>
    ///     Returns the type that called a transpiler in a given stack trace
    /// </summary>
    /// <returns></returns>
    private static string getTranspilerStackFrame()
    {
        var trace = new StackTrace();
        foreach (var frame in trace.GetFrames()!)
        {
            var method = frame.GetMethod();
            if (method.Name == "Transpiler")
            {
                return method.DeclaringType?.FullName ?? "unknown";
            }
        }

        return "unknown";
    }

    // CodeMatcher will throw errors if we try to take actions in an invalid state (i.e. no match)
    private static CodeMatcher OnSuccess(this CodeMatcher match, Action<CodeMatcher> action, bool suppress = false)
    {
        switch (match.IsInvalid)
        {
            case true when !suppress:
                Main.LogMessage(
                    $"Transpiler did not find target @ {getTranspilerStackFrame()}",
                    true);
                break;
            case false:
                action.Invoke(match);
                break;
        }

        return match;
    }

    /// <summary>
    ///     Replaces the pattern with the replacement. This is a naive implementation - if you need labels, DIY it.
    /// </summary>
    /// <param name="match">the CodeMatcher instance to use</param>
    /// <param name="pattern">instructions to match, the beginning of this match is where the replacement begins</param>
    /// <param name="replacement">the new instructions</param>
    /// <param name="replace">
    ///     Whether we should keep labels and set the opcodes/operands instead of recreating the
    ///     CodeInstruction
    /// </param>
    /// <param name="suppress">Whether to suppress the log message on a failed match</param>
    /// <returns></returns>
    // ReSharper disable once UnusedMethodReturnValue.Local
    private static CodeMatcher replace(this CodeMatcher match, CodeMatch[] pattern, CodeInstructions replacement,
        bool replace = true, bool suppress = false)
    {
        return match.MatchStartForward(pattern).OnSuccess(matcher =>
        {
            var newOps = replacement.ToList();
            for (var i = 0; i < Math.Max(newOps.Count, pattern.Length); i++)
            {
                if (i < newOps.Count)
                {
                    var op = newOps[i];
                    if (i < pattern.Length)
                    {
                        if (replace)
                        {
                            // This keeps labels
                            matcher.SetAndAdvance(op.opcode, op.operand);
                        }
                        else
                        {
                            matcher.RemoveInstruction();
                            matcher.InsertAndAdvance(op);
                        }
                    }
                    else
                    {
                        matcher.InsertAndAdvance(op);
                    }
                }
                else
                {
                    matcher.RemoveInstruction();
                }
            }
        }, suppress);
    }

    /// <summary>
    ///     Prevents the compiler from removing a given local
    /// </summary>
    /// <param name="variable">The local to protect</param>
    [MethodImpl(MethodImplOptions.NoInlining)]
    // ReSharper disable once UnusedParameter.Local
    private static void pin<T>(ref T variable)
    {
        // Do nothing
    }

    /// <summary>
    ///     Returns the given method as a basic ILCode signature, excluding ret statements
    /// </summary>
    /// <param name="method">The method to convert</param>
    /// <returns>A set of CodeInstructions representing the method given</returns>
    /// <remarks>Declared locals need to be pinned with Pin(ref local)</remarks>
    private static CodeInstructions instructionSignature(Delegate method)
    {
        var instructions = new List<CodeInstruction>();
        var locals = method.Method.GetMethodBody()!.LocalVariables;

        // Fetch the given delegate as IL code and mutate it slightly before storing it
        foreach (var instruction in PatchProcessor.GetCurrentInstructions(method.Method))
        {
            // Returns are used as a declaration within patterns, so we drop the instruction
            if (instruction == Fish.Return)
            {
                continue;
            }

            // Nops can be used for alignment or optimization, but we don't want that here as it can mess up our matching
            if (instruction.opcode == OpCodes.Nop)
            {
                continue;
            }

            // Arg indexes get shifted by 1, as the method is made static with "this" as the 0th arg. We shift them backwards here to match.
            if (instruction.opcode.LoadsArgument() || instruction.opcode.StoresArgument())
            {
                // FishInstruction cast allows us to avoid branching on all the different starg_n/ldarg_n
                var index = new FishInstruction(instruction).GetIndex() - 1;
                // Create a copy with the proper index
                var copy = instruction.IsLdarg() ? FishTranspiler.Argument(index) : FishTranspiler.StoreArgument(index);
                // Push the changes back to the instruction. This allows us to maintain block/label attributes.
                instruction.opcode = copy.OpCode;
                instruction.operand = copy.Operand;
            }

            // For methods that just declare a local, they have to pin it using HarmonyPatches.Pin. We remove this from the match.
            // Doing so is a two-instruction process (Ldloca, Call) so we remove the last and current instructions
            if (instructions.Count > 0)
            {
                var lastInstruction = new FishInstruction(instructions.Last());
                if (lastInstruction.OpCode == OpCodes.Ldloca_S || lastInstruction.OpCode == OpCodes.Ldloca)
                {
                    // Fetch the instruction for Pin<T> where T is whatever type lastInstruction accesses...
                    var genericPin = Fish.Call(typeof(HarmonyPatches), nameof(pin), generics:
                    [
                        locals[lastInstruction.GetIndex()].LocalType
                    ]);
                    // ...and check it against our current instruction
                    if (instruction == genericPin)
                    {
                        // Remove the ldloca from our list
                        instructions.RemoveAt(instructions.Count - 1);
                        // Skip storing the pin call
                        continue;
                    }
                }
            }

            // Store to our list
            instructions.Add(instruction);
        }

        return instructions;
    }

    /// <summary>
    ///     Returns the given method as a basic ILCode signature wrapped in a CodeMatch, excluding ret statements
    /// </summary>
    /// <param name="method">The method to convert</param>
    /// <returns>A set of CodeInstructions representing the method given</returns>
    /// <remarks>Declared locals need to be pinned with Pin(ref local)</remarks>
    private static CodeMatch[] InstructionMatchSignature(Delegate method)
    {
        return instructionSignature(method).Select(i => new CodeMatch(i.opcode, i.operand)).ToArray();
    }
}