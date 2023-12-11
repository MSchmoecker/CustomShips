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
                shipPart.CustomShip = snapShip;
            }

            snapShip = null;
            return piece;
        }

        [HarmonyPatch(typeof(Player), nameof(Player.UpdatePlacementGhost)), HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> UpdatePlacementGhostRotationTranspiler(IEnumerable<CodeInstruction> instructions) {
            MethodInfo euler = AccessTools.Method(typeof(Quaternion), nameof(Quaternion.Euler), new[] { typeof(float), typeof(float), typeof(float) });

            CodeMatch[] loadPlacementRotation = {
                new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(Player), nameof(Player.m_placeRotationDegrees))),
                new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(Player), nameof(Player.m_placeRotation))),
            };

            CodeMatch[] prepareEuler = {
                new CodeMatch(OpCodes.Conv_R4),
                new CodeMatch(OpCodes.Mul),
                new CodeMatch(OpCodes.Ldc_R4),
            };

            CodeMatch[] makeAndStoreRotation = {
                new CodeMatch(OpCodes.Call, euler),
                new CodeMatch(OpCodes.Stloc_S),
            };

            return new CodeMatcher(instructions)
                .MatchForward(true, loadPlacementRotation.Concat(prepareEuler).Concat(makeAndStoreRotation).ToArray())
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
                    float placeRotation = player.m_placeRotationDegrees * player.m_placeRotation;
                    return nearest.transform.rotation * Quaternion.Euler(0f, placeRotation, 0f);
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
