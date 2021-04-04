﻿using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace RimThreaded
{

	public class SoundSizeAggregator_Patch
	{
		public static AccessTools.FieldRef<SoundSizeAggregator, List<ISizeReporter>> reporters =
			AccessTools.FieldRefAccess<SoundSizeAggregator, List<ISizeReporter>>("reporters");
		public static AccessTools.FieldRef<SoundSizeAggregator, float> testSize =
			AccessTools.FieldRefAccess<SoundSizeAggregator, float>("testSize");
		public static bool RegisterReporter(SoundSizeAggregator __instance, ISizeReporter newRep)
		{
			lock (reporters(__instance))
			{
				reporters(__instance).Add(newRep);
			}
			return false;
		}

		public static bool RemoveReporter(SoundSizeAggregator __instance, ISizeReporter oldRep)
		{
			lock (reporters(__instance))
			{
				List<ISizeReporter> newReporters = new List<ISizeReporter>(reporters(__instance));
				newReporters.Remove(oldRep);
				reporters(__instance) = newReporters;
			}
			return false;
		}
		public static bool get_AggregateSize(SoundSizeAggregator __instance, ref float __result)
		{
			if (reporters(__instance).Count == 0)
			{
				__result = testSize(__instance);
				return false;
			}

			float num = 0f;
			for (int i = 0; i < reporters(__instance).Count; i++)
			{
				ISizeReporter reporter = reporters(__instance)[i];
				if (reporter != null)
				{
					num += reporter.CurrentSize();
				}
			}

			__result = num;
			return false;
		}

        internal static void RunDestructivePatches()
        {
			Type original = typeof(SoundSizeAggregator);
			Type patched = typeof(SoundSizeAggregator_Patch);
			RimThreadedHarmony.Prefix(original, patched, "RegisterReporter");
			RimThreadedHarmony.Prefix(original, patched, "RemoveReporter");
			RimThreadedHarmony.Prefix(original, patched, "get_AggregateSize");
		}
    }
}