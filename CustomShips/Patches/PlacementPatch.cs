using HarmonyLib;
using UnityEngine;

namespace CustomShips.Patches {
    [HarmonyPatch]
    public class PlacementPatch {
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
