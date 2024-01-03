using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using Logger = Jotunn.Logger;

namespace CustomShips.Patches {
    [HarmonyPatch]
    public static class ZDOManPatch {
        private static Dictionary<int, ZDO> ships = new Dictionary<int, ZDO>();
        private static Dictionary<int, List<ZDO>> shipPieces = new Dictionary<int, List<ZDO>>();
        private static Queue<ZDO> newZDOs = new Queue<ZDO>();

        [HarmonyPatch(typeof(ZDOMan), nameof(ZDOMan.Load)), HarmonyPostfix]
        public static void Load(ZDOMan __instance) {
            foreach (ZDO zdo in __instance.m_objectsByID.Values) {
                RegisterZDO(zdo);
            }

            foreach (var ship in ships.Keys) {
                CheckShip(ship);
            }

            __instance.m_onZDODestroyed += OnZDODestroyed;
        }

        [HarmonyPatch(typeof(ZDOMan), nameof(ZDOMan.CreateNewZDO), typeof(ZDOID), typeof(Vector3), typeof(int)), HarmonyPostfix]
        public static void OnCreateNewZDO(ref ZDO __result) {
            if (ZNet.instance.IsServer()) {
                newZDOs.Enqueue(__result);
            }
        }

        [HarmonyPatch(typeof(ZDOMan), nameof(ZDOMan.Update)), HarmonyPostfix]
        public static void ZDOManUpdate() {
            while (newZDOs.Count > 0) {
                RegisterZDO(newZDOs.Dequeue());
            }
        }

        private static void CheckShip(int targetShip) {
            if (!HasAnyPiece(targetShip) && ships.TryGetValue(targetShip, out ZDO shipZDO)) {
                Logger.LogInfo($"Deleting empty ship {targetShip}");

                if (!shipZDO.IsOwner()) {
                    shipZDO.SetOwner(ZDOMan.GetSessionID());
                }

                ZDOMan.instance.DestroyZDO(shipZDO);
            } else {
                Logger.LogInfo($"Not deleting ship {targetShip}, {shipPieces[targetShip].Count}");
            }
        }

        private static bool HasAnyPiece(int targetShip) {
            if (shipPieces.TryGetValue(targetShip, out List<ZDO> pieces)) {
                return pieces.Count > 0;
            }

            return false;
        }

        private static void OnZDODestroyed(ZDO zdo) {
            if (Main.IsShipPiece(zdo)) {
                int ship = zdo.GetInt("MS_UniqueID");
                ships.Remove(ship);
            }

            if (Main.IsShipPiece(zdo)) {
                int ship = zdo.GetInt("MS_ConnectedShip");

                if (shipPieces.TryGetValue(ship, out List<ZDO> pieces)) {
                    Logger.LogInfo($"Removing ZDO from ship {ship}");
                    pieces.Remove(zdo);
                } else {
                    Logger.LogWarning($"ship {ship} for {zdo} not registered");
                }

                CheckShip(ship);
            }
        }

        private static void RegisterZDO(ZDO zdo) {
            if (Main.IsCustomShip(zdo)) {
                ships.Add(zdo.GetInt("MS_UniqueID"), zdo);
            }

            if (Main.IsShipPiece(zdo)) {
                int ship = zdo.GetInt("MS_ConnectedShip");
                Logger.LogInfo($"Registering ZDO with ship {ship}");

                if (shipPieces.TryGetValue(ship, out List<ZDO> pieces)) {
                    pieces.Add(zdo);
                } else {
                    shipPieces[ship] = new List<ZDO>() { zdo };
                }
            }
        }
    }
}
