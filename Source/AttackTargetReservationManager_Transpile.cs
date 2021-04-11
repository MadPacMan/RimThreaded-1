﻿using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using Verse;
using System.Reflection.Emit;
using System.Reflection;
using static Verse.AI.AttackTargetReservationManager;
using Verse.AI;
using System;

namespace RimThreaded
{
    public class AttackTargetReservationManager_Transpile
    {
        public static IEnumerable<CodeInstruction> IsReservedBy(IEnumerable<CodeInstruction> instructions, ILGenerator iLGenerator)
        {
            List<CodeInstruction> searchInstructions = new List<CodeInstruction>
            {
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(AttackTargetReservationManager), "reservations")),
                new CodeInstruction(OpCodes.Ldloc_0),
                new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(List<AttackTargetReservation>), "get_Item")),
                new CodeInstruction(OpCodes.Stloc_1)
            };
            List<CodeInstruction> instructionsList = instructions.ToList();
            int currentInstructionIndex = 0;
            int matchFound = 0;
            while (currentInstructionIndex < instructionsList.Count)
            {
                if(RimThreadedHarmony.IsCodeInstructionsMatching(searchInstructions, instructionsList, currentInstructionIndex))
                {
                    matchFound++;
                    foreach (CodeInstruction codeInstruction in RimThreadedHarmony.UpdateTryCatchCodeInstructions(
                        iLGenerator, instructionsList, currentInstructionIndex, searchInstructions.Count))
                    {
                        yield return codeInstruction;
                    }
                    currentInstructionIndex += searchInstructions.Count;
                }
                else if(
                    instructionsList[currentInstructionIndex].opcode == OpCodes.Ldfld &&
                    (FieldInfo)instructionsList[currentInstructionIndex].operand == AccessTools.Field(typeof(AttackTargetReservation), "target")
                    )
                {
                    matchFound++;
                    yield return new CodeInstruction(OpCodes.Brfalse, instructionsList[currentInstructionIndex + 2].operand);
                    yield return new CodeInstruction(OpCodes.Ldloc_1);
                    yield return instructionsList[currentInstructionIndex];
                    currentInstructionIndex++;
                }
                else
                {
                    yield return instructionsList[currentInstructionIndex];
                    currentInstructionIndex++;
                }
            }
            if(matchFound < 2)
            {
                Log.Error("IL code instructions not found");
            }
        }

        public static IEnumerable<CodeInstruction> Reserve(IEnumerable<CodeInstruction> instructions, ILGenerator iLGenerator)
        {
            List<CodeInstruction> instructionsList = instructions.ToList();
            Type loadLockObjectType = typeof(List<AttackTargetReservation>);
            List<CodeInstruction> loadLockObjectInstructions = new List<CodeInstruction>
            {
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(AttackTargetReservationManager), "reservations"))
            };
            List<CodeInstruction> searchInstructions = loadLockObjectInstructions.ListFullCopy();
            searchInstructions.Add(new CodeInstruction(OpCodes.Ldloc_0));
            searchInstructions.Add(new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(loadLockObjectType, "Add")));

            int i = 0;
            int matchesFound = 0;

            while (i < instructionsList.Count)
            {
                if (RimThreadedHarmony.IsCodeInstructionsMatching(searchInstructions, instructionsList, i))
                {
                    matchesFound++;
                    foreach (CodeInstruction codeInstruction in RimThreadedHarmony.GetLockCodeInstructions(
                        iLGenerator, instructionsList, i, searchInstructions.Count, loadLockObjectInstructions, loadLockObjectType))
                    {
                        yield return codeInstruction;
                    }
                    i += searchInstructions.Count;
                }
                else
                {
                    yield return instructionsList[i];
                    i++;
                }
            }
            if (matchesFound < 1)
            {
                Log.Error("IL code instructions not found");
            }
        }
    }
}
