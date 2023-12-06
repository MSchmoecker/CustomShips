using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using CustomShips.Pieces;
using HarmonyLib;
using UnityEngine;

namespace CustomShips.Patches {
    [HarmonyPatch]
    public static class ShipPatches {
        [HarmonyTargetMethods]
        public static IEnumerable<MethodBase> AllShipMethods() {
            return AccessTools.GetDeclaredMethods(typeof(Ship));
        }

        private static CodeMatch[] forwardMatch = new CodeMatch[] {
            new CodeMatch(OpCodes.Ldarg_0),
            new CodeMatch(i => i.Calls(AccessTools.PropertyGetter(typeof(Component), nameof(Component.transform)))),
            new CodeMatch(i => i.Calls(AccessTools.PropertyGetter(typeof(Transform), nameof(Transform.forward))))
        };

        private static CodeMatch[] rightMatch = new CodeMatch[] {
            new CodeMatch(OpCodes.Ldarg_0),
            new CodeMatch(i => i.Calls(AccessTools.PropertyGetter(typeof(Component), nameof(Component.transform)))),
            new CodeMatch(i => i.Calls(AccessTools.PropertyGetter(typeof(Transform), nameof(Transform.right))))
        };

        private static CodeInstruction[] forwardInsert = new CodeInstruction[] {
            new CodeInstruction(OpCodes.Ldarg_0),
            new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ShipPatches), nameof(ShipForward)))
        };

        private static CodeInstruction[] rightInsert = new CodeInstruction[] {
            new CodeInstruction(OpCodes.Ldarg_0),
            new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ShipPatches), nameof(ShipRight)))
        };

        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> UpdateDirectionTranspiler(MethodBase original, IEnumerable<CodeInstruction> instructions) {
            // InverseTransformDirection?

            return new CodeMatcher(instructions)
                .MatchForward(true, forwardMatch)
                .Repeat(matcher => matcher.Advance(1).InsertAndAdvance(forwardInsert))
                .Start()
                .MatchForward(true, rightMatch)
                .Repeat(matcher => matcher.Advance(1).InsertAndAdvance(rightInsert))
                .InstructionEnumeration();
        }

        private static Vector3 ShipForward(Vector3 forward, Ship ship) {
            if (!ship.TryGetComponent(out CustomShip customShip)) {
                return forward;
            }

            return customShip.GetForward();
        }

        private static Vector3 ShipRight(Vector3 right, Ship ship) {
            if (!ship.TryGetComponent(out CustomShip customShip)) {
                return right;
            }

            return customShip.GetRight();
        }
    }
}
