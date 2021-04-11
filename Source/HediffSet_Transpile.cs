using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using Verse;
using System.Reflection.Emit;
using RimWorld;
using System.Reflection;
using System;
using static HarmonyLib.AccessTools;
using static RimThreaded.RimThreadedHarmony;

namespace RimThreaded
{
    public class HediffSet_Transpile
    {
        public static IEnumerable<CodeInstruction> PartIsMissing(IEnumerable<CodeInstruction> instructions, ILGenerator iLGenerator)
        {
            List<CodeInstruction> searchInstructions = new List<CodeInstruction>
            {
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldfld, Field(typeof(HediffSet), "hediffs")),
                new CodeInstruction(OpCodes.Ldloc_0),
                new CodeInstruction(OpCodes.Callvirt, Method(typeof(List<Hediff>), "get_Item")),
                new CodeInstruction(OpCodes.Callvirt, Method(typeof(Hediff), "get_Part")),
                new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Bne_Un_S),
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldfld, Field(typeof(HediffSet), "hediffs")),
                new CodeInstruction(OpCodes.Ldloc_0),
                new CodeInstruction(OpCodes.Callvirt, Method(typeof(List<Hediff>), "get_Item")),
                new CodeInstruction(OpCodes.Isinst, typeof(Hediff_MissingPart)),
                new CodeInstruction(OpCodes.Brfalse_S),
                new CodeInstruction(OpCodes.Ldc_I4_1),
                new CodeInstruction(OpCodes.Ret),
            };
            List<CodeInstruction> instructionsList = instructions.ToList();
            int currentInstructionIndex = 0;
            bool matchFound = false;
            while (currentInstructionIndex < instructionsList.Count)
            {
                if (IsCodeInstructionsMatching(searchInstructions, instructionsList, currentInstructionIndex))
                {
                    matchFound = true;
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, Field(typeof(HediffSet), "hediffs"));
                    yield return new CodeInstruction(OpCodes.Ldloc_0);
                    yield return new CodeInstruction(OpCodes.Callvirt, Method(typeof(List<Hediff>), "get_Item"));
                    yield return new CodeInstruction(OpCodes.Brfalse, instructionsList[currentInstructionIndex+6].operand);
                    foreach (CodeInstruction codeInstruction in UpdateTryCatchCodeInstructions(
                        iLGenerator, instructionsList, currentInstructionIndex, searchInstructions.Count))
                    {
                        yield return codeInstruction;
                    }
                    currentInstructionIndex += searchInstructions.Count;
                }
                else
                {
                    yield return instructionsList[currentInstructionIndex];
                    currentInstructionIndex++;
                }
            }
            if (!matchFound)
            {
                Log.Error("IL code instructions not found");
            }
        }
        public static IEnumerable<CodeInstruction> HasDirectlyAddedPartFor(IEnumerable<CodeInstruction> instructions, ILGenerator iLGenerator)
        {
            List<CodeInstruction> searchInstructions = new List<CodeInstruction>
            {
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldfld, Field(typeof(HediffSet), "hediffs")),
                new CodeInstruction(OpCodes.Ldloc_0),
                new CodeInstruction(OpCodes.Callvirt, Method(typeof(List<Hediff>), "get_Item")),
                new CodeInstruction(OpCodes.Callvirt, Method(typeof(Hediff), "get_Part")),
                new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Bne_Un_S),
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldfld, Field(typeof(HediffSet), "hediffs")),
                new CodeInstruction(OpCodes.Ldloc_0),
                new CodeInstruction(OpCodes.Callvirt, Method(typeof(List<Hediff>), "get_Item")),
                new CodeInstruction(OpCodes.Isinst, typeof(Hediff_AddedPart)),
                new CodeInstruction(OpCodes.Brfalse_S),
                new CodeInstruction(OpCodes.Ldc_I4_1),
                new CodeInstruction(OpCodes.Ret),
            };
            List<CodeInstruction> instructionsList = instructions.ToList();
            int currentInstructionIndex = 0;
            bool matchFound = false;
            while (currentInstructionIndex < instructionsList.Count)
            {
                if (IsCodeInstructionsMatching(searchInstructions, instructionsList, currentInstructionIndex))
                {
                    matchFound = true;
                    foreach (CodeInstruction codeInstruction in UpdateTryCatchCodeInstructions(
                        iLGenerator, instructionsList, currentInstructionIndex, searchInstructions.Count))
                    {
                        yield return codeInstruction;
                    }
                    currentInstructionIndex += searchInstructions.Count;
                }
                else
                {
                    yield return instructionsList[currentInstructionIndex];
                    currentInstructionIndex++;
                }
            }
            if (!matchFound)
            {
                Log.Error("IL code instructions not found");
            }
        }
        public static IEnumerable<CodeInstruction> AddDirect(IEnumerable<CodeInstruction> instructions, ILGenerator iLGenerator)
        {
            List<CodeInstruction> searchInstructions = new List<CodeInstruction>
            {
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldfld, Field(typeof(HediffSet), "hediffs")),
                new CodeInstruction(OpCodes.Ldloc_2),
                new CodeInstruction(OpCodes.Callvirt, Method(typeof(List<Hediff>), "get_Item")),
                new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Callvirt, Method(typeof(Hediff), "TryMergeWith")),
                new CodeInstruction(OpCodes.Brfalse_S)
            };
            List<CodeInstruction> instructionsList = instructions.ToList();
            int currentInstructionIndex = 0;
            bool matchFound = false;
            while (currentInstructionIndex < instructionsList.Count)
            {
                if (IsCodeInstructionsMatching(searchInstructions, instructionsList, currentInstructionIndex))
                {
                    matchFound = true;
                    for (int i = 0; i < 4; i++)
                    {
                        CodeInstruction codeInstruction = instructionsList[currentInstructionIndex + i];
                        yield return new CodeInstruction(codeInstruction.opcode, codeInstruction.operand);
                    }
                    yield return new CodeInstruction(OpCodes.Brfalse_S, instructionsList[currentInstructionIndex + 6].operand);
                    yield return instructionsList[currentInstructionIndex];
                    currentInstructionIndex++;
                }
                else
                {
                    yield return instructionsList[currentInstructionIndex];
                    currentInstructionIndex++;
                }
            }
            if (!matchFound)
            {
                Log.Error("IL code instructions not found");
            }
        }
        public static IEnumerable<CodeInstruction> MoveNext(IEnumerable<CodeInstruction> instructions, ILGenerator iLGenerator)
        {

            List<CodeInstruction> instructionsList = instructions.ToList();
            int currentInstructionIndex = 0;
            int matchFound = 0;
            while (currentInstructionIndex < instructionsList.Count)
            {
                if (currentInstructionIndex + 2 < instructionsList.Count &&
                    instructionsList[currentInstructionIndex + 2].opcode == OpCodes.Call &&
                    (MethodInfo)instructionsList[currentInstructionIndex + 2].operand == Method(typeof(HediffSet), "PartIsMissing")) 
                {
                    matchFound++;
                    yield return new CodeInstruction(OpCodes.Ldloc_1);
                    yield return new CodeInstruction(OpCodes.Brfalse, instructionsList[currentInstructionIndex + 3].operand);
                    yield return instructionsList[currentInstructionIndex];
                    currentInstructionIndex++;
                }
                else
                {
                    yield return instructionsList[currentInstructionIndex];
                    currentInstructionIndex++;
                }
            }
            if (matchFound < 1)
            {
                Log.Error("IL code instructions not found");
            }
        }
        public static IEnumerable<CodeInstruction> GetPartHealth(IEnumerable<CodeInstruction> instructions, ILGenerator iLGenerator)
        {
            int[] matchesFound = new int[3];
            List<CodeInstruction> instructionsList = instructions.ToList();
            int i = 0;
            Label breakDestinationLabel = iLGenerator.DefineLabel();
            while (i < instructionsList.Count)
            {
                int matchIndex = 0;
                if (
                    i - 2 >= 0 &&
                    instructionsList[i - 2].opcode == OpCodes.Ldarg_1 &&
                    instructionsList[i - 1].opcode == OpCodes.Brtrue_S
                    )
                {
                    matchesFound[matchIndex]++;
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Ldfld, Field(typeof(BodyPartRecord), "def"));
                    yield return new CodeInstruction(instructionsList[i - 1].opcode, instructionsList[i - 1].operand);
                }
                matchIndex++;
                if (
                        i + 2 < instructionsList.Count &&
                        instructionsList[i + 2].opcode == OpCodes.Callvirt &&
                        (MethodInfo)instructionsList[i + 2].operand == Method(typeof(List<Hediff>), "get_Item")
                    )
                {
                    breakDestinationLabel = iLGenerator.DefineLabel();
                    StartTryAndAddBreakDestinationLabel(instructionsList, ref i, breakDestinationLabel);
                    matchesFound[matchIndex]++;
                }
                matchIndex++;
                if (
                    i - 1 >= 0 &&
                    instructionsList[i - 1].opcode == OpCodes.Callvirt &&
                    (MethodInfo)instructionsList[i - 1].operand == Method(typeof(List<Hediff>), "get_Item")
                )
                {
                    EndTryStartCatchArgumentExceptionOutOfRange(instructionsList, ref i, iLGenerator, breakDestinationLabel);
                    matchesFound[matchIndex]++;
                }
                yield return instructionsList[i++];
            }
            for (int mIndex = 0; mIndex < matchesFound.Length; mIndex++)
            {
                if (matchesFound[mIndex] < 1)
                    Log.Error("IL code instruction set " + mIndex + " not found");
            }
        }
        public static IEnumerable<CodeInstruction> CacheMissingPartsCommonAncestors ( IEnumerable<CodeInstruction> instructions, ILGenerator iLGenerator )
        {
            List<CodeInstruction> instructionsList = instructions.ToList();

            LocalBuilder newCachedMPCA = iLGenerator.DeclareLocal( typeof( List<Hediff_MissingPart> ) );
            LocalBuilder lockObject = iLGenerator.DeclareLocal( typeof( Queue<BodyPartRecord> ) );
            LocalBuilder lockTaken = iLGenerator.DeclareLocal( typeof( bool ) );

            List<CodeInstruction> MPCAQueue = new List<CodeInstruction>
            {
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldfld, Field(typeof(HediffSet), "missingPartsCommonAncestorsQueue"))
            };

            List<CodeInstruction> cachedMPCA = new List<CodeInstruction>
            {
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldfld, Field(typeof(HediffSet), "cachedMissingPartsCommonAncestors"))
            };


            // find first instruction after if-else and remove code before it
            int i = 0;
            while ( i < instructionsList.Count && !IsCodeInstructionsMatching( MPCAQueue, instructionsList, i ) )
            {
                i++;
            }
            instructionsList.RemoveRange( 0, i );

            // init local variable newCachedMPCA
            instructionsList.InsertRange( 0, new CodeInstruction[]
            {
                new CodeInstruction( OpCodes.Newobj, Constructor( typeof( List<Hediff_MissingPart> ) ) ),
                new CodeInstruction( OpCodes.Stloc, newCachedMPCA.LocalIndex )
            } );

            // replace remaining this.cachedMissingPartsCommonAncestors references with newCachedMPCA
            i = 0;
            while ( i < instructionsList.Count )
            {
                if ( IsCodeInstructionsMatching( cachedMPCA, instructionsList, i ) )
                {
                    instructionsList[i] = new CodeInstruction( OpCodes.Ldloc, newCachedMPCA.LocalIndex );
                    instructionsList.RemoveRange( i + 1, cachedMPCA.Count - 1 );
                }
                i++;
            }

            // set cachedMissingPartsCommonAncestors at the end
            instructionsList.InsertRange( instructionsList.Count - 1, new CodeInstruction[]
            {
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldloc, newCachedMPCA.LocalIndex),
                new CodeInstruction(OpCodes.Stfld, Field(typeof(HediffSet), "cachedMissingPartsCommonAncestors") )
            } );

            // enter lock
            i = 0;
            instructionsList.InsertRange( i, EnterLock( lockObject, lockTaken, MPCAQueue, instructionsList, ref i ) );

            // exit lock
            i = instructionsList.Count - 1;
            instructionsList.InsertRange( i, ExitLock( iLGenerator, lockObject, lockTaken, instructionsList, ref i ) );

            // Log.Message( string.Join( "\n", instructionsList.Select( ( ci, l ) => l + " " + ci ) ) );

            return instructionsList;
        }
        // equivalent change:
        /*
        private void CacheMissingPartsCommonAncestors()
        {
            lock(this.missingPartsCommonAncestorsQueue)
            {
                var cmpca = new List<Hediff_MissingPart>();

                // ------- REMOVE FROM HERE
	            if (this.cachedMissingPartsCommonAncestors == null)
	            {
		            this.cachedMissingPartsCommonAncestors = new List<Hediff_MissingPart>();
	            }
	            else
	            {
		            this.cachedMissingPartsCommonAncestors.Clear();
	            }
                // ------- REMOVE TO HERE
                // ============ SAME
	            this.missingPartsCommonAncestorsQueue.Clear();
	            this.missingPartsCommonAncestorsQueue.Enqueue(this.pawn.def.race.body.corePart);
	            while (this.missingPartsCommonAncestorsQueue.Count != 0)
	            {
		            BodyPartRecord node = this.missingPartsCommonAncestorsQueue.Dequeue();
		            if (!this.PartOrAnyAncestorHasDirectlyAddedParts(node))
		            {
			            Hediff_MissingPart hediffMissingPart = (from x in this.GetHediffs<Hediff_MissingPart>()
			            where x.Part == node
			            select x).FirstOrDefault<Hediff_MissingPart>();
			            if (hediffMissingPart != null)
			            {
                            // ***** replace *********************
				            this.cachedMissingPartsCommonAncestors  .Add(hediffMissingPart);
                            // *****  with   *********************
                            cmpca                                   .Add(hediffMissingPart);
                            // ***********************************
                            
			            }
			            else
			            {
				            for (int index = 0; index < node.parts.Count; index++)
				            {
					            this.missingPartsCommonAncestorsQueue.Enqueue(node.parts[index]);
				            }
			            }
		            }
	            }
                // ============ SAME

                this.cachedMissingPartsCommonAncestors = cmpca;
            }

        }
         
         */
    }
}
