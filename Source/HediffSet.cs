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

    public class HediffSet_Patch
	{
		public static bool PartIsMissing(HediffSet __instance, ref bool __result, BodyPartRecord part)
		{
			Hediff hediff;
			for (int i = 0; i < __instance.hediffs.Count; i++)
			{
				try {
					hediff = __instance.hediffs[i];
				} catch (ArgumentOutOfRangeException) { break; }
				if (hediff is Hediff_MissingPart && hediff?.Part == part)
				{
					__result = true;
					return false;
				}
			}
			__result = false;
			return false;
		}

	}
}