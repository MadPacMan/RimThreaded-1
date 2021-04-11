﻿using System;
using System.Collections.Generic;
using Verse;

namespace RimThreaded
{
    class GenRadial_Patch
    {
        [ThreadStatic] public static List<IntVec3> tmpCells;
        [ThreadStatic] public static bool working;

        public static void InitializeThreadStatics()
        {
            tmpCells = new List<IntVec3>();
            working = false;
        }

        internal static void RunNonDestructivePatches()
        {
            Type original = typeof(GenRadial);
            Type patched = typeof(GenRadial_Patch);
            RimThreadedHarmony.AddAllMatchingFields(original, patched);
            RimThreadedHarmony.TranspileFieldReplacements(original, "ProcessEquidistantCells");
        }
    }
}