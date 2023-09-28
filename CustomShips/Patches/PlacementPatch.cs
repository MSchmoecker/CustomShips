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

        [HarmonyPatch(typeof(Player), nameof(Player.UpdatePlacementGhost)), HarmonyPostfix]
        public static void UpdatePlacementGhostPatch(Player __instance) {
            if (!__instance.m_placementGhost || !Main.IsShipPiece(__instance.m_placementGhost)) {
                return;
            }

            ShipPart nearest = ShipPart.FindNearest(__instance.m_placementGhost.transform.position);

            if (nearest) {
                float placeRotation = __instance.m_placeRotationDegrees * __instance.m_placeRotation;
                float shipRotation = nearest.transform.rotation.eulerAngles.y;
                float relativeRotation = (shipRotation - placeRotation) % __instance.m_placeRotationDegrees;
                __instance.m_placementGhost.transform.rotation = Quaternion.Euler(0f, placeRotation + relativeRotation, 0f);
            }
        }
    }
}
