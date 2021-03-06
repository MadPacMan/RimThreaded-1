﻿using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Verse;
using static HarmonyLib.AccessTools;

namespace RimThreaded
{
    class childrenHarmonyHediffComp_Discoverable_CheckDiscovered_Patch_Transpile
    {
		private static readonly FieldRef<HediffComp_Discoverable, bool> discoveredRef = FieldRefAccess<HediffComp_Discoverable, bool>("discovered");

		public static bool getDiscovered(HediffComp_Discoverable hediffComp_Discoverable)
		{
			return discoveredRef(hediffComp_Discoverable);
		}
		public static void setDiscovered(HediffComp_Discoverable hediffComp_Discoverable, bool value)
		{
			discoveredRef(hediffComp_Discoverable) = value;
		}

		public static IEnumerable<CodeInstruction> CheckDiscovered_Pre(IEnumerable<CodeInstruction> instructions, ILGenerator iLGenerator)
		{
			int[] matchesFound = new int[2];
			List<CodeInstruction> instructionsList = instructions.ToList();
			int i = 0;
			while (i < instructionsList.Count)
			{
				int matchIndex = 0;
				if (
					i + 3 < instructionsList.Count &&
					(instructionsList[i + 3].opcode == OpCodes.Callvirt) &&
					instructionsList[i + 3].operand.ToString().Contains("GetValue")
					)
				{
					matchesFound[matchIndex]++;
					instructionsList[i].opcode = OpCodes.Call;
					instructionsList[i].operand = Method(typeof(childrenHarmonyHediffComp_Discoverable_CheckDiscovered_Patch_Transpile), "getDiscovered");
					yield return instructionsList[i++];
					i += 3;
				}
				matchIndex++;
				if (
					i + 5 < instructionsList.Count &&
					instructionsList[i + 5].opcode == OpCodes.Callvirt &&
					instructionsList[i + 5].operand.ToString().Contains("SetValue")
					)
				{
					matchesFound[matchIndex]++;
					instructionsList[i].opcode = OpCodes.Ldloc_0;
					instructionsList[i].operand = null;
					yield return instructionsList[i++];
					//instructionsList[i].opcode = OpCodes.Box;
					//instructionsList[i].operand = typeof(bool);
					//yield return instructionsList[i++];
					instructionsList[i].opcode = OpCodes.Call;
					instructionsList[i].operand = Method(typeof(childrenHarmonyHediffComp_Discoverable_CheckDiscovered_Patch_Transpile), "setDiscovered");
					yield return instructionsList[i++];
					i += 5;
				}
				yield return instructionsList[i++];
			}
			for (int mIndex = 0; mIndex < matchesFound.Length; mIndex++)
			{
				if (matchesFound[mIndex] < 1)
					Log.Error("IL code instruction set " + mIndex + " not found");
			}
		}
	}
}
