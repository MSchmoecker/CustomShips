using HarmonyLib;

namespace CustomShips.Patches {
    [HarmonyPatch]
    public class PlacementPatch {
        [HarmonyPatch(typeof(Player), nameof(Player.SetupPlacementGhost)), HarmonyPostfix]
        public static void SetupPlacementGhostPatch(Player __instance) {
            if (!__instance.m_placementGhost) {
                return;
            }

            foreach (ShipPart shipPart in __instance.m_placementGhost.GetComponentsInChildren<ShipPart>()) {
                UnityEngine.Object.Destroy(shipPart);
            }
        }
    }
}
