﻿using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using static HarmonyLib.AccessTools;

namespace RimThreaded
{
    public class ThingGrid_Patch
    {
        public static FieldRef<ThingGrid, Map> map = FieldRefAccess<ThingGrid, Map>("map");
        public static FieldRef<ThingGrid, List<Thing>[]> thingGrid = FieldRefAccess<ThingGrid, List<Thing>[]>("thingGrid");
        public static Dictionary<Map, Dictionary<WorkGiver_Scanner, Dictionary<float, List<HashSet<Thing>[]>>>> mapIngredientDict =
            new Dictionary<Map, Dictionary<WorkGiver_Scanner, Dictionary<float, List<HashSet<Thing>[]>>>>();
        // Map, Scanner, points, (jumbo cell zoom level, #0 item=zoom 2x2, #1 item=4x4), jumbo cell index converted from x,z coord, HashSet<Thing>
        public static Dictionary<ThingDef, Dictionary<WorkGiver_Scanner, float>> thingBillPoints = new Dictionary<ThingDef, Dictionary<WorkGiver_Scanner, float>>();
        // loop through all WorkGiver_Scanners on map and add ThingDefs on map points
        public static int[] power2array = new int[] { 2, 4, 8, 16, 32, 64, 128, 256, 512, 1024, 2048, 4096, 8192, 16384 }; // a 16384x16384 map is probably too big

        private static int CellToIndexCustom(IntVec3 c, int mapSizeX, int cellSize)
        {
            return (mapSizeX * c.z + c.x) / cellSize;
        }
        private static int NumGridCellsCustom(int mapSizeX, int mapSizeZ, int cellSize)
        {
            return Mathf.CeilToInt((mapSizeX * mapSizeZ) / (float)cellSize);
        }

        public static void RunDestructivePatches()
        {
            Type original = typeof(ThingGrid);
            Type patched = typeof(ThingGrid_Patch);
            RimThreadedHarmony.Prefix(original, patched, "RegisterInCell");
            RimThreadedHarmony.Prefix(original, patched, "DeregisterInCell");
        }

        public static bool RegisterInCell(ThingGrid __instance, Thing t, IntVec3 c)
        {
            Map this_map = map(__instance);
            if (!c.InBounds(this_map))
            {
                Log.Warning(t.ToString() + " tried to register out of bounds at " + c + ". Destroying.", false);
                t.Destroy(DestroyMode.Vanish);
            }
            else
            {
                int index = this_map.cellIndices.CellToIndex(c);

                int mapSizeX = this_map.Size.x;
                int mapSizeZ = this_map.Size.z;

                lock (__instance)
                {
                    thingGrid(__instance)[index].Add(t);
                    if (!thingBillPoints.TryGetValue(t.def, out Dictionary<WorkGiver_Scanner, float> billPointsDict))
                    {
                        billPointsDict = new Dictionary<WorkGiver_Scanner, float>();
                        thingBillPoints[t.def] = billPointsDict;
                    }
                    if (!mapIngredientDict.TryGetValue(this_map, out Dictionary<WorkGiver_Scanner, Dictionary<float, List<HashSet<Thing>[]>>> ingredientDict))
                    {
                        ingredientDict = new Dictionary<WorkGiver_Scanner, Dictionary<float, List<HashSet<Thing>[]>>>();
                        mapIngredientDict[this_map] = ingredientDict;
                    }
                    foreach (KeyValuePair<WorkGiver_Scanner, float> billPoints in billPointsDict)
                    {
                        int i = 0;
                        int power2;
                        do
                        {
                            power2 = power2array[i];
                            ingredientDict[billPoints.Key][billPoints.Value][i][CellToIndexCustom(c, mapSizeX, power2)].Add(t);
                            i++;
                        } while (power2 < mapSizeX || power2 < mapSizeZ);
                    }
                }
            }
            return false;
        }

        public static bool DeregisterInCell(ThingGrid __instance, Thing t, IntVec3 c)
        {
            Map this_map = map(__instance);
            if (!c.InBounds(this_map))
            {
                Log.Error(t.ToString() + " tried to de-register out of bounds at " + c, false);
                return false;
            }

            int index = this_map.cellIndices.CellToIndex(c);
            List<Thing>[] thingGridInstance = thingGrid(__instance);
            List<Thing> thingList = thingGridInstance[index];
            if (thingList.Contains(t))
            {
                lock (__instance)
                {
                    thingList = thingGridInstance[index];
                    if (thingList.Contains(t))
                    {
                        List<Thing> newThingList = new List<Thing>(thingList);
                        newThingList.Remove(t);
                        thingGridInstance[index] = newThingList;

                        int mapSizeX = this_map.Size.x;
                        int mapSizeZ = this_map.Size.z;

                        if (!thingBillPoints.TryGetValue(t.def, out Dictionary<WorkGiver_Scanner, float> billPointsDict))
                        {
                            billPointsDict = new Dictionary<WorkGiver_Scanner, float>();
                            thingBillPoints[t.def] = billPointsDict;
                        }
                        if (!mapIngredientDict.TryGetValue(this_map, out Dictionary<WorkGiver_Scanner, Dictionary<float, List<HashSet<Thing>[]>>> ingredientDict))
                        {
                            ingredientDict = new Dictionary<WorkGiver_Scanner, Dictionary<float, List<HashSet<Thing>[]>>>();
                            mapIngredientDict[this_map] = ingredientDict;
                        }
                        foreach (KeyValuePair<WorkGiver_Scanner, float> billPoints in billPointsDict)
                        {
                            int i = 0;
                            int power2;
                            do
                            {
                                power2 = power2array[i];
                                HashSet<Thing> newHashSet = new HashSet<Thing>(ingredientDict[billPoints.Key][billPoints.Value][i][CellToIndexCustom(c, mapSizeX, power2)]);
                                newHashSet.Remove(t);
                                ingredientDict[billPoints.Key][billPoints.Value][i][CellToIndexCustom(c, mapSizeX, power2)] = newHashSet;
                                i++;
                            } while (power2 < mapSizeX || power2 < mapSizeZ);
                        }
                    }
                }
            }

            return false;
        }



    }

}
