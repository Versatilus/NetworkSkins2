﻿using Harmony;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace NetworkSkins.Patches
{
    /// <summary>
    /// Used by lane props, wires
    /// </summary>
    [HarmonyPatch]
    public static class NetSegmentRenderInstancePatch
    {
        private const byte InfoArgIndex = 4;

        public static MethodBase TargetMethod()
        {
            // RenderInstance(RenderManager.CameraInfo cameraInfo, ushort segmentID, int layerMask, NetInfo info, ref RenderManager.Instance data)
            return typeof(NetSegment).GetMethod("RenderInstance", BindingFlags.NonPublic | BindingFlags.Instance, Type.DefaultBinder, new[] {
                typeof(RenderManager.CameraInfo),
                typeof(ushort),
                typeof(int),
                typeof(NetInfo),
                typeof(RenderManager.Instance).MakeByRefType()
            }, null);
        }

        public static IEnumerable<CodeInstruction> Transpiler(ILGenerator il, IEnumerable<CodeInstruction> instructions)
        {
            var originalCodes = new List<CodeInstruction>(instructions);
            var codes = new List<CodeInstruction>(originalCodes);

            var index = 0;

            var infoLdInstruction = new CodeInstruction(OpCodes.Ldarg_S, InfoArgIndex);
            var segmentIdLdInstruction = new CodeInstruction(OpCodes.Ldarg_2); // segmentID is second argument

            if (!NetSegmentRenderPatch.PatchLanesAndSegments(il, codes, infoLdInstruction, segmentIdLdInstruction, ref index))
            {
                Debug.LogError("Could not apply NetSegmentRenderPatch. Cancelling transpiler!");
                return originalCodes;
            }

            return codes;

            /*
            var customLanesLocalVar = il.DeclareLocal(typeof(NetInfo.Lane[]));
            customLanesLocalVar.SetLocalSymInfo("customLanes");

            var customSegmentsLocalVar = il.DeclareLocal(typeof(NetInfo.Segment[]));
            customSegmentsLocalVar.SetLocalSymInfo("customSegments");

            var beginLabel = il.DefineLabel();
            codes[0].labels.Add(beginLabel);

            var customLanesInstructions = new[]
            {
                // NetInfo.Lane[] customLanes = info.m_lanes;
                new CodeInstruction(OpCodes.Ldarg_S, InfoArgIndex), // info
                new CodeInstruction(OpCodes.Ldfld, netInfoLanesField),
                new CodeInstruction(OpCodes.Stloc, customLanesLocalVar),

                // NetInfo.Segment[] customSegments = info.m_segments;
                new CodeInstruction(OpCodes.Ldarg_S, InfoArgIndex), // info
                new CodeInstruction(OpCodes.Ldfld, netInfoSegmentsField),
                new CodeInstruction(OpCodes.Stloc, customSegmentsLocalVar),

                // if (SegmentSkinManager.SegmentSkins[segmentID] != null)
                new CodeInstruction(OpCodes.Ldsfld, segmentSkinsField),
                new CodeInstruction(OpCodes.Ldarg_2), // segmentID
                new CodeInstruction(OpCodes.Ldelem_Ref),
                new CodeInstruction(OpCodes.Brfalse_S, beginLabel),

                // customLanes = SegmentSkinManager.SegmentSkins[segmentID].m_lanes;
                new CodeInstruction(OpCodes.Ldsfld, segmentSkinsField),
                new CodeInstruction(OpCodes.Ldarg_2), // segmentID
                new CodeInstruction(OpCodes.Ldelem_Ref),
                new CodeInstruction(OpCodes.Ldfld, networkSkinLanesField),
                new CodeInstruction(OpCodes.Stloc, customLanesLocalVar),

                // customSegments = SegmentSkinManager.SegmentSkins[segmentID].m:segments;
                new CodeInstruction(OpCodes.Ldsfld, segmentSkinsField),
                new CodeInstruction(OpCodes.Ldarg_2), // segmentID
                new CodeInstruction(OpCodes.Ldelem_Ref),
                new CodeInstruction(OpCodes.Ldfld, networkSkinSegmentsField),
                new CodeInstruction(OpCodes.Stloc, customSegmentsLocalVar),
            };
            codes.InsertRange(0, customLanesInstructions);

            // Replace all occurences of:
            // ldarg.s info
            // ldfld class NetInfo/Lane[] NetInfo::m_lanes
            // -- with --
            // ldloc.s <customLanesLocalVar>
            // and all occurences of:
            // ldarg.s info
            // ldfld class NetInfo/Segment[] NetInfo::m_segments
            // -- with --
            // ldloc.s <customSegmentsLocalVar>
            for (var i = customLanesInstructions.Length; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Ldarg_S && (byte)codes[i].operand == InfoArgIndex && codes[i + 1].opcode == OpCodes.Ldfld)
                {
                    if (codes[i + 1].operand == netInfoLanesField)
                    {
                        // It is important that we copy the labels from the existing instruction!
                        // Otherwise "Label not marked" exception
                        codes[i] = new CodeInstruction(codes[i])
                        {
                            opcode = OpCodes.Ldloc,
                            operand = customLanesLocalVar
                        };
                        codes.RemoveAt(i + 1);
                    }
                    else if (codes[i + 1].operand == netInfoSegmentsField)
                    {
                        // It is important that we copy the labels from the existing instruction!
                        // Otherwise "Label not marked" exception
                        codes[i] = new CodeInstruction(codes[i])
                        {
                            opcode = OpCodes.Ldloc,
                            operand = customSegmentsLocalVar
                        };
                        codes.RemoveAt(i + 1);
                    }
                }
            }

            return codes;
            */
        }
    }
}
