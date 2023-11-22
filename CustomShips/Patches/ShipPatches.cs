using HarmonyLib;

namespace CustomShips.Patches {
    [HarmonyPatch]
    public static class ShipPatches {
        [HarmonyPatch(typeof(Ship), nameof(Ship.CustomFixedUpdate)), HarmonyPrefix]
        public static void ShipCustomFixedUpdate(Ship __instance, ref bool __runOriginal) {
            __runOriginal = !Main.IsCustomShip(__instance.gameObject);
        }
    }
}
