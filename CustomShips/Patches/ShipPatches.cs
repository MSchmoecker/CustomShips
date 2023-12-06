using CustomShips.Pieces;
using HarmonyLib;

namespace CustomShips.Patches {
    [HarmonyPatch]
    public static class ShipPatches {
        [HarmonyPatch(typeof(Ship), nameof(Ship.GetWindAngle)), HarmonyPostfix]
        public static void GetWindAnglePatch(Ship __instance, ref float __result) {
            if (__instance.TryGetComponent(out CustomShip customShip)) {
                __result = -Utils.YawFromDirection(customShip.InverseTransformDirection(EnvMan.instance.GetWindDir()));
            }
        }
    }
}
