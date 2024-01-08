using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using CustomShips.Helper;
using CustomShips.Pieces;
using HarmonyLib;
using UnityEngine;

namespace CustomShips.Patches {
    [HarmonyPatch]
    public class PlacementPatch {
        private static CustomShip snapShip;

        [HarmonyPatch(typeof(Player), nameof(Player.SetupPlacementGhost)), HarmonyPostfix]
        public static void SetupPlacementGhostPatch(Player __instance) {
            if (!__instance.m_placementGhost) {
                return;
            }

            foreach (DynamicHull dynamicHull in __instance.m_placementGhost.GetComponentsInChildren<DynamicHull>()) {
                Object.Destroy(dynamicHull);
            }

            foreach (ShipPart shipPart in __instance.m_placementGhost.GetComponentsInChildren<ShipPart>()) {
                Object.Destroy(shipPart);
            }

            foreach (CustomShip customShip in __instance.m_placementGhost.GetComponentsInChildren<CustomShip>()) {
                Object.Destroy(customShip);
            }
        }

        [HarmonyPatch(typeof(Player), nameof(Player.PlacePiece)), HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> PlacePieceTranspiler(IEnumerable<CodeInstruction> instructions) {
            return new CodeMatcher(instructions)
                .MatchForward(true, new CodeMatch(i => i.IsCall(nameof(Object), nameof(Object.Instantiate))))
                .ThrowIfInvalid("Could not find Object.Instantiate call")
                .InsertAndAdvance(
                    new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PlacementPatch), nameof(BeforePlacePiece)))
                )
                .Advance(1)
                .InsertAndAdvance(
                    new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PlacementPatch), nameof(AfterPlacePiece)))
                )
                .Instructions();
        }

        private static void BeforePlacePiece() {
            Player player = Player.m_localPlayer;

            if (Main.IsShipPiece(player.m_placementGhost)) {
                if (player.m_hoveringPiece) {
                    snapShip = player.m_hoveringPiece.GetComponentInParent<CustomShip>();
                }

                player.FindClosestSnapPoints(player.m_placementGhost.transform, 0.5f, out Transform selfSnapPoint, out Transform otherSnapPoint, player.m_tempPieces);

                if (otherSnapPoint && otherSnapPoint.parent && otherSnapPoint.parent.TryGetComponent(out ShipPart shipPart)) {
                    snapShip = shipPart.CustomShip;
                }
            }
        }

        private static GameObject AfterPlacePiece(GameObject piece) {
            if (piece && Main.IsShipPiece(piece) && piece.TryGetComponent(out ShipPart shipPart)) {
                if (snapShip) {
                    shipPart.CustomShip = snapShip;
                } else {
                    shipPart.CreateCustomShip();
                }
            }

            snapShip = null;
            return piece;
        }

        [HarmonyPatch(typeof(Player), nameof(Player.UpdatePlacementGhost)), HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> UpdatePlacementGhostRotationTranspiler(IEnumerable<CodeInstruction> instructions) {
            MethodInfo euler = AccessTools.Method(typeof(Quaternion), nameof(Quaternion.Euler), new[] { typeof(float), typeof(float), typeof(float) });

            List<CodeInstruction> instr = instructions.ToList();
            int matchPosition = -1;

            for (var i = 0; i < instr.Count; i++) {
                if (
                    // placeRotation match
                    instr[i + 0].opcode == OpCodes.Ldarg_0 &&
                    instr[i + 1].LoadsField(AccessTools.Field(typeof(Player), nameof(Player.m_placeRotationDegrees))) &&
                    instr[i + 2].opcode == OpCodes.Ldarg_0 &&
                    instr[i + 3].LoadsField(AccessTools.Field(typeof(Player), nameof(Player.m_placeRotation))) &&
                    instr[i + 4].opcode == OpCodes.Conv_R4 &&
                    instr[i + 5].opcode == OpCodes.Mul &&
                    instr[i + 6].opcode == OpCodes.Ldc_R4 &&
                    instr[i + 7].Calls(euler)
                ) {
                    if (
                        // Vanilla or with Comfy Gizmos at shutdown
                        instr[i + 8].opcode == OpCodes.Stloc_S
                    ) {
                        matchPosition = i + 8;
                        break;
                    }

                    if (
                        // with Comfy Gizmos at startup
                        instr[i + 8].CallReturns(typeof(Quaternion)) &&
                        instr[i + 9].opcode == OpCodes.Stloc_S
                    ) {
                        matchPosition = i + 9;
                        break;
                    }
                }
            }

            if (matchPosition < 0) {
                throw new System.InvalidOperationException("Could not find rotation calculation");
            }

            return new CodeMatcher(instr)
                .Advance(matchPosition + 1)
                .GetOperand(out object localRotation)
                .Advance(1)
                .InsertAndAdvance(
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldloc_S, localRotation),
                    new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PlacementPatch), nameof(PieceRotation))),
                    new CodeInstruction(OpCodes.Stloc_S, localRotation)
                )
                .Instructions();
        }

        private static Quaternion PieceRotation(Player player, Quaternion rotation) {
            GameObject ghost = player.m_placementGhost;

            if (ghost && Main.IsShipPiece(ghost)) {
                player.PieceRayTest(out Vector3 point, out Vector3 normal, out Piece piece, out Heightmap heightmap, out Collider waterSurface, false);
                ShipPart nearest = ShipPart.FindNearest(point);

                if (nearest) {
                    return nearest.CustomShip.transform.rotation * rotation;
                }

                return rotation;
            }

            return rotation;
        }

        [HarmonyPatch(typeof(Player), nameof(Player.PieceRayTest)), HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> AllowShipRigidbodyTranspiler(IEnumerable<CodeInstruction> instructions) {
            MethodInfo getCollider = AccessTools.PropertyGetter(typeof(RaycastHit), nameof(RaycastHit.collider));
            MethodInfo getAttachedRigidbody = AccessTools.PropertyGetter(typeof(Collider), nameof(Collider.attachedRigidbody));
            MethodInfo opImplicit = AccessTools.Method(typeof(Object), "op_Implicit");

            CodeMatch[] loadRigidbody = {
                new CodeMatch(i => i.opcode == OpCodes.Ldloca_S),
                new CodeMatch(i => i.Calls(getCollider)),
                new CodeMatch(i => i.Calls(getAttachedRigidbody)),
                new CodeMatch(i => i.Calls(opImplicit)),
            };

            return new CodeMatcher(instructions)
                .MatchForward(false, loadRigidbody)
                .GetOperand(out object loadHitInfo)
                .Advance(loadRigidbody.Length)
                .InsertAndAdvance(
                    new CodeInstruction(OpCodes.Ldloca_S, loadHitInfo),
                    new CodeInstruction(OpCodes.Call, getCollider),
                    new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PlacementPatch), nameof(DenyPieceRay)))
                )
                .Instructions();
        }

        private static bool DenyPieceRay(bool hasRigidbody, Collider hit) {
            if (!Main.IsShipPiece(Player.m_localPlayer.m_placementGhost)) {
                // vanilla behavior
                return hasRigidbody;
            }

            Piece piece = hit ? hit.GetComponentInParent<Piece>() : null;
            bool isShipPiece = piece && Main.IsShipPiece(piece.gameObject);

            if (isShipPiece) {
                return false;
            }

            // vanilla behavior
            return hasRigidbody;
        }
    }
}
