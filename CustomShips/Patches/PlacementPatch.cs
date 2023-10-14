using System.Collections.Generic;
using System.Reflection.Emit;
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

            foreach (ShipPart shipPart in __instance.m_placementGhost.GetComponentsInChildren<ShipPart>()) {
                Object.Destroy(shipPart);
            }
        }

        [HarmonyPatch(typeof(Player), nameof(Player.PlacePiece)), HarmonyPrefix]
        public static void PlacePiecePatch(Player __instance, ref bool __runOriginal, ref bool __result) {
            if (__instance.m_placementStatus == PlacementBlock.invalidRipPlacement) {
                __instance.Message(MessageHud.MessageType.Center, "Needs to be placed on a keel");
                __runOriginal = false;
                __result = false;
            }
        }

        [HarmonyPatch(typeof(Player), nameof(Player.PlacePiece)), HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> PlacePieceTranspiler(IEnumerable<CodeInstruction> instructions) {
            return new CodeMatcher(instructions)
                .MatchForward(true, new CodeMatch(i => i.IsCall(nameof(Object), nameof(Object.Instantiate))))
                .InsertAndAdvance(
                    new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PlacementPatch), nameof(BeforePlacePiece)))
                )
                .Advance(1)
                .InsertAndAdvance(
                    new CodeInstruction(OpCodes.Dup),
                    new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PlacementPatch), nameof(AfterPlacePiece)))
                )
                .Instructions();
        }

        private static void BeforePlacePiece() {
            Player player = Player.m_localPlayer;

            if (Main.IsShipPiece(player.m_placementGhost)) {
                player.FindClosestSnapPoints(player.m_placementGhost.transform, 0.5f, out Transform selfSnapPoint, out Transform otherSnapPoint, player.m_tempPieces);

                if (otherSnapPoint && otherSnapPoint.parent && otherSnapPoint.parent.TryGetComponent(out ShipPart shipPart)) {
                    snapShip = shipPart.CustomShip;
                }
            }
        }

        private static void AfterPlacePiece(GameObject piece) {
            if (piece && Main.IsShipPiece(piece) && piece.TryGetComponent(out ShipPart shipPart)) {
                shipPart.CustomShip = snapShip;
            }

            snapShip = null;
        }

        [HarmonyPatch(typeof(Player), nameof(Player.UpdatePlacementGhost)), HarmonyPostfix]
        public static void UpdatePlacementGhostPatch(Player __instance) {
            GameObject ghost = __instance.m_placementGhost;

            if (!ghost || !Main.IsShipPiece(ghost)) {
                return;
            }

            ShipPart nearest = ShipPart.FindNearest(ghost.transform.position);

            if (nearest) {
                float placeRotation = __instance.m_placeRotationDegrees * __instance.m_placeRotation;
                float shipRotation = nearest.transform.rotation.eulerAngles.y;
                float relativeRotation = (shipRotation - placeRotation) % __instance.m_placeRotationDegrees;
                ghost.transform.rotation = Quaternion.Euler(0f, placeRotation + relativeRotation, 0f);
            }

            bool isValidPlacement = __instance.m_placementStatus == Player.PlacementStatus.Valid;

            if (isValidPlacement && ghost.TryGetComponent(out PlacementBlock placementBlock)) {
                if (placementBlock.placementRule == PlacementRule.Rib) {
                    placementBlock.CheckRibPlacement(__instance);
                }
            }

            __instance.SetPlacementGhostValid(__instance.m_placementStatus == Player.PlacementStatus.Valid);
        }
    }
}
