﻿using RimWorld.Planet;
using System;

namespace RimThreaded
{

    public class WorldComponentUtility_Patch
	{
        public static bool WorldComponentTick(World world)
        {
            RimThreaded.WorldComponents = world.components;
            RimThreaded.WorldComponentTicks = world.components.Count;
            return false;
        }

        internal static void RunDestructivePatches()
        {
            Type original = typeof(WorldComponentUtility);
            Type patched = typeof(WorldComponentUtility_Patch);
            RimThreadedHarmony.Prefix(original, patched, "WorldComponentTick");
        }
    }
}
