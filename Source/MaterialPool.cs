﻿using System;
using UnityEngine;
using Verse;
using static RimThreaded.RimThreaded;
using static System.Threading.Thread;

namespace RimThreaded
{    
    public class MaterialPool_Patch
    {
		static readonly Func<object[], object> safeFunction = parameters =>
			MaterialPool.MatFrom((MaterialRequest)parameters[0]);

		public static void RunDestructivePatches()
        {
			Type original = typeof(MaterialPool);
			Type patched = typeof(MaterialPool_Patch);
			RimThreadedHarmony.Prefix(original, patched, "MatFrom", new Type[] { typeof(MaterialRequest) });
		}

		public static bool MatFrom(ref Material __result, MaterialRequest req)
		{
			if (allThreads2.TryGetValue(CurrentThread, out ThreadInfo threadInfo))
			{
				threadInfo.safeFunctionRequest = new object[] { safeFunction, new object[] { req } };
				mainThreadWaitHandle.Set();
				threadInfo.eventWaitStart.WaitOne();
				__result = (Material)threadInfo.safeFunctionResult;
				return false;
			}
			return true;
		}
	}

}
