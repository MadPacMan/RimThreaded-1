﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using static HarmonyLib.AccessTools;

namespace RimThreaded
{
    public class RegionDirtyer_Patch
    {
        //public static Dictionary<RegionDirtyer, ConcurrentQueue<IntVec3>> dirtyCellsDict = new Dictionary<RegionDirtyer, ConcurrentQueue<IntVec3>>();
        public static Dictionary<RegionDirtyer, List<IntVec3>> dirtyCellsDict = new Dictionary<RegionDirtyer, List<IntVec3>>();

        public static FieldRef<RegionDirtyer, Map> map = FieldRefAccess<RegionDirtyer, Map>("map");
        public static object regionDirtyerLock = new object();

        public static bool SetAllClean(RegionDirtyer __instance)
        {
            lock (regionDirtyerLock)
            {
                List<IntVec3> dirtyCells = get_DirtyCells(__instance);
                //lock (dirtyCells)
                //{
                    foreach (IntVec3 dirtyCell in dirtyCells)
                    {
                        map(__instance).temperatureCache.ResetCachedCellInfo(dirtyCell);
                    }
                    dirtyCells.Clear();
                //}
            }
            return false;
        }

        public static List<IntVec3> get_DirtyCells(RegionDirtyer __instance)
        {
            List<IntVec3> dirtyCells;
            lock (regionDirtyerLock)
            {
                if (!dirtyCellsDict.TryGetValue(__instance, out dirtyCells))
                {
                    dirtyCells = new List<IntVec3>();
                    //lock(dirtyCellsDict)
                    //{
                    dirtyCellsDict.SetOrAdd(__instance, dirtyCells);
                    //}
                }
            }
            return dirtyCells;
        }

        public static bool Notify_WalkabilityChanged(RegionDirtyer __instance, IntVec3 c)
        {
            lock (regionDirtyerLock)
            {
                List<Region> regionsToDirty = new List<Region>();
                //regionsToDirty.Clear();
                for (int i = 0; i < 9; i++)
                {
                    IntVec3 c2 = c + GenAdj.AdjacentCellsAndInside[i];
                    if (c2.InBounds(map(__instance)))
                    {
                        Region regionAt_NoRebuild_InvalidAllowed = map(__instance).regionGrid.GetRegionAt_NoRebuild_InvalidAllowed(c2);
                        if (regionAt_NoRebuild_InvalidAllowed != null && regionAt_NoRebuild_InvalidAllowed.valid)
                        {
                            map(__instance).temperatureCache.TryCacheRegionTempInfo(c, regionAt_NoRebuild_InvalidAllowed);
                            regionsToDirty.Add(regionAt_NoRebuild_InvalidAllowed);
                        }
                    }
                }

                for (int j = 0; j < regionsToDirty.Count; j++)
                {
                    SetRegionDirty(__instance, regionsToDirty[j]);
                }

                //regionsToDirty.Clear();
                List<IntVec3> dirtyCells = get_DirtyCells(__instance);
                //lock (dirtyCells)
                //{
                if (c.Walkable(map(__instance)) && !dirtyCells.Contains(c))
                {
                    dirtyCells.Add(c);
                }
                //}
            }
            return false;
        }

        public static bool Notify_ThingAffectingRegionsSpawned(RegionDirtyer __instance, Thing b)
        {
            lock (regionDirtyerLock)
            {
                //regionsToDirty.Clear();
                List<Region> regionsToDirty = new List<Region>();
                foreach (IntVec3 item in b.OccupiedRect().ExpandedBy(1).ClipInsideMap(b.Map))
                {
                    Region validRegionAt_NoRebuild = b.Map.regionGrid.GetValidRegionAt_NoRebuild(item);
                    if (validRegionAt_NoRebuild != null)
                    {
                        b.Map.temperatureCache.TryCacheRegionTempInfo(item, validRegionAt_NoRebuild);
                        regionsToDirty.Add(validRegionAt_NoRebuild);
                    }
                }

                for (int i = 0; i < regionsToDirty.Count; i++)
                {
                    SetRegionDirty(__instance, regionsToDirty[i]);
                }
            }
            //regionsToDirty.Clear();
            return false;
        }


        public static bool Notify_ThingAffectingRegionsDespawned(RegionDirtyer __instance, Thing b)
        {
            lock (regionDirtyerLock)
            {
                //regionsToDirty.Clear();
                List<Region> regionsToDirty = new List<Region>();
                Region validRegionAt_NoRebuild = map(__instance).regionGrid.GetValidRegionAt_NoRebuild(b.Position);
                if (validRegionAt_NoRebuild != null)
                {
                    map(__instance).temperatureCache.TryCacheRegionTempInfo(b.Position, validRegionAt_NoRebuild);
                    regionsToDirty.Add(validRegionAt_NoRebuild);
                }

                foreach (IntVec3 item2 in GenAdj.CellsAdjacent8Way(b))
                {
                    if (item2.InBounds(map(__instance)))
                    {
                        Region validRegionAt_NoRebuild2 = map(__instance).regionGrid.GetValidRegionAt_NoRebuild(item2);
                        if (validRegionAt_NoRebuild2 != null)
                        {
                            map(__instance).temperatureCache.TryCacheRegionTempInfo(item2, validRegionAt_NoRebuild2);
                            regionsToDirty.Add(validRegionAt_NoRebuild2);
                        }
                    }
                }

                for (int i = 0; i < regionsToDirty.Count; i++)
                {
                    SetRegionDirty(__instance, regionsToDirty[i]);
                }

                regionsToDirty.Clear();
                List<IntVec3> dirtyCells = get_DirtyCells(__instance);
                //lock (dirtyCells)
                //{
                    if (b.def.size.x == 1 && b.def.size.z == 1)
                    {
                        dirtyCells.Add(b.Position);
                        return false;
                    }

                    CellRect cellRect = b.OccupiedRect();
                    for (int j = cellRect.minZ; j <= cellRect.maxZ; j++)
                    {
                        for (int k = cellRect.minX; k <= cellRect.maxX; k++)
                        {
                            IntVec3 item = new IntVec3(k, 0, j);
                            dirtyCells.Add(item);
                        }
                    }
                //}
            }
            return false;
        }

        public static bool SetAllDirty(RegionDirtyer __instance)
        {
            List<IntVec3> dirtyCells = new List<IntVec3>();

            lock (regionDirtyerLock)
            {
                //lock (dirtyCells)
                //{
                foreach (IntVec3 item in map(__instance))
                {
                    dirtyCells.Add(item);
                }
                //}
                //lock (dirtyCellsDict) {
                    dirtyCellsDict.SetOrAdd(__instance, dirtyCells);
                //}
                foreach (Region item2 in map(__instance).regionGrid.AllRegions_NoRebuild_InvalidAllowed)
                {
                    SetRegionDirty(__instance, item2, addCellsToDirtyCells: false);
                }
            }
            
            return false;
        }


        public static bool SetRegionDirty(RegionDirtyer __instance, Region reg, bool addCellsToDirtyCells = true)
        {
            lock (regionDirtyerLock)
            {
                if (!reg.valid)
                {
                    return false;
                }

                reg.valid = false;
                reg.Room = null;
                for (int i = 0; i < reg.links.Count; i++)
                {
                    reg.links[i].Deregister(reg);
                }

                reg.links.Clear();
                if (!addCellsToDirtyCells)
                {
                    return false;
                }
                List<IntVec3> dirtyCells = get_DirtyCells(__instance);
                //lock (dirtyCells)
                //{
                    foreach (IntVec3 cell in reg.Cells)
                    {
                        dirtyCells.Add(cell);
                        if (DebugViewSettings.drawRegionDirties)
                        {
                            map(__instance).debugDrawer.FlashCell(cell);
                        }
                    }
                //}
            }
            return false;
        }

    }
}
