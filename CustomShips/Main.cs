using System.Collections;
using System.Collections.Generic;
using BepInEx;
using CustomShips.Pieces;
using HarmonyLib;
using Jotunn;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using UnityEngine;

namespace CustomShips {
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Patch)]
    public class Main : BaseUnityPlugin {
        public const string PluginGUID = "com.maxsch.valheim.CustomShips";
        public const string PluginName = "CustomShips";
        public const string PluginVersion = "0.0.6";

        private static AssetBundle assetBundle;
        private static List<CustomPiece> pieces = new List<CustomPiece>();
        private static HashSet<string> shipPieceNames = new HashSet<string>();
        private static HashSet<int> shipPieceHashes = new HashSet<int>();

        public static GameObject shipPrefab;
        private static int shipPrefabHash;

        public static CustomLocalization Localization = LocalizationManager.Instance.GetLocalization();

        private void Awake() {
            Harmony harmony = new Harmony(PluginGUID);
            harmony.PatchAll();

            Localization.AddJsonFile("English", AssetUtils.LoadTextFromResources("Localization.English.json"));

            assetBundle = AssetUtils.LoadAssetBundleFromResources("customships");

            shipPrefab = assetBundle.LoadAsset<GameObject>("MS_CustomShip");
            PrefabManager.Instance.AddPrefab(shipPrefab);
            shipPrefabHash = shipPrefab.name.GetStableHashCode();

            AddShipPiece("MS_Keel_2m", "RoundLog", 4, "BronzeNails", 2);
            AddShipPiece("MS_Keel_4m", "RoundLog", 8, "BronzeNails", 4);
            AddShipPiece("MS_Keel_Bow_1", "RoundLog", 6, "BronzeNails", 4);
            AddShipPiece("MS_Keel_Bow_2", "RoundLog", 4, "BronzeNails", 2);
            AddShipPiece("MS_Rib_1.0m", "RoundLog", 4, "BronzeNails", 2);
            AddShipPiece("MS_Rib_1.2m", "RoundLog", 4, "BronzeNails", 2);
            AddShipPiece("MS_Rib_1.4m", "RoundLog", 5, "BronzeNails", 2);
            AddShipPiece("MS_Rib_1.6m", "RoundLog", 5, "BronzeNails", 2);
            AddShipPiece("MS_Rib_1.8m", "RoundLog", 6, "BronzeNails", 2);
            AddShipPiece("MS_Rib_2.0m", "RoundLog", 6, "BronzeNails", 2);
            AddShipPiece("MS_Rib_2.2m", "RoundLog", 7, "BronzeNails", 2);
            AddShipPiece("MS_Rib_2.4m", "RoundLog", 7, "BronzeNails", 2);
            AddShipPiece("MS_Rib_2.6m", "RoundLog", 8, "BronzeNails", 2);
            AddShipPiece("MS_Hull_Dynamic", "Wood", 6, "BronzeNails", 2, "Resin", 10);
            AddShipPiece("MS_Rudder_1", "Wood", 12, "BronzeNails", 8);
            AddShipPiece("MS_Crate_1", "FineWood", 8, "BronzeNails", 6);
            AddShipPiece("MS_Barrel_1", "FineWood", 8, "BronzeNails", 6);
            AddShipPiece("MS_Ladder_1", "Wood", 4, "BronzeNails", 2);
            AddShipPiece("MS_Shield_Round_1_Style_1", "FineWood", 6, "BronzeNails", 4);
            AddShipPiece("MS_Shield_Round_1_Style_2", "FineWood", 6, "BronzeNails", 4);
            AddShipPiece("MS_Shield_Round_1_Style_3", "FineWood", 6, "BronzeNails", 4);
            AddShipPiece("MS_Shield_Round_1_Style_4", "FineWood", 6, "BronzeNails", 4);
            AddShipPiece("MS_Sail_2_White", "FineWood", 20, "LeatherScraps", 12, "Resin", 8);
            AddShipPiece("MS_Sail_2_Red_1", "FineWood", 20, "LeatherScraps", 12, "Resin", 8, "Raspberry", 4);
            AddShipPiece("MS_Sail_2_Hide_1", "FineWood", 20, "DeerHide", 12, "Resin", 8);
            AddShipPiece("MS_Sail_1_White", "FineWood", 30, "LeatherScraps", 20, "Resin", 12);
            AddShipPiece("MS_Sail_1_Red_1", "FineWood", 30, "LeatherScraps", 20, "Resin", 12, "Raspberry", 8);
            AddShipPiece("MS_Sail_1_Hide_1", "FineWood", 30, "DeerHide", 20, "Resin", 12);

            PieceManager.OnPiecesRegistered += OnPiecesRegistered;
        }

        private void OnPiecesRegistered() {
            PieceManager.OnPiecesRegistered -= OnPiecesRegistered;
            StartCoroutine(RenderSprites());
        }

        private static IEnumerator RenderSprites() {
            yield return null;

            foreach (CustomPiece piece in pieces) {
                piece.Piece.m_icon = RenderManager.Instance.Render(new RenderManager.RenderRequest(piece.PiecePrefab) {
                    Width = 64,
                    Height = 64,
                    Rotation = RenderManager.IsometricRotation * Quaternion.Euler(0, -90f, 0),
                });
            }
        }

        private void AddShipPiece(string pieceName, string ingredient1 = "", int amount1 = 0, string ingredient2 = "", int amount2 = 0, string ingredient3 = "", int amount3 = 0, string ingredient4 = "", int amount4 = 0) {
            CustomPiece piece = new CustomPiece(assetBundle, pieceName, true, ShipPartConfig(ingredient1, amount1, ingredient2, amount2, ingredient3, amount3, ingredient4, amount4));
            PieceManager.Instance.AddPiece(piece);

            if (piece.PiecePrefab.TryGetComponent(out DynamicHull dynamicHull)) {
                dynamicHull.RegenerateMesh(2, 2, 0.9f, 0, 0);
            }

            pieces.Add(piece);
            shipPieceNames.Add(pieceName);
            shipPieceHashes.Add(pieceName.GetStableHashCode());
        }

        public static bool IsShipPiece(GameObject piece) {
            return shipPieceNames.Contains(Utils.GetPrefabName(piece));
        }

        public static bool IsShipPiece(ZDO zdo) {
            return shipPieceHashes.Contains(zdo.GetPrefab());
        }

        public static bool IsCustomShip(GameObject piece) {
            return shipPrefab.name == Utils.GetPrefabName(piece);
        }

        public static bool IsCustomShip(ZDO zdo) {
            return zdo.GetPrefab() == shipPrefabHash;
        }

        private PieceConfig ShipPartConfig(string ingredient1, int amount1, string ingredient2, int amount2, string ingredient3, int amount3, string ingredient4, int amount4) {
            PieceConfig stackConfig = new PieceConfig();
            stackConfig.PieceTable = PieceTables.Hammer;
            stackConfig.CraftingStation = CraftingStations.Workbench;
            stackConfig.Category = "Ship";

            stackConfig.AddRequirement(new RequirementConfig(ingredient1, amount1, 0, true));
            stackConfig.AddRequirement(new RequirementConfig(ingredient2, amount2, 0, true));
            stackConfig.AddRequirement(new RequirementConfig(ingredient3, amount3, 0, true));
            stackConfig.AddRequirement(new RequirementConfig(ingredient4, amount4, 0, true));

            if (stackConfig.Requirements.Length == 0) {
                stackConfig.AddRequirement(new RequirementConfig("Wood", 3, 0, true));
            }

            return stackConfig;
        }
    }
}
