using System.Collections;
using System.Collections.Generic;
using BepInEx;
using CustomShips.Pieces;
using HarmonyLib;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using UnityEngine;

namespace CustomShips {
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    //[NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
    public class Main : BaseUnityPlugin {
        public const string PluginGUID = "com.jotunn.jotunnmodstub";
        public const string PluginName = "CustomShips";
        public const string PluginVersion = "0.0.1";

        private static AssetBundle assetBundle;
        private static List<CustomPiece> pieces = new List<CustomPiece>();
        private static HashSet<string> shipPieceNames = new HashSet<string>();

        public static GameObject shipPrefab;

        public static CustomLocalization Localization = LocalizationManager.Instance.GetLocalization();

        private void Awake() {
            Harmony harmony = new Harmony(PluginGUID);
            harmony.PatchAll();

            Localization.AddJsonFile("English", AssetUtils.LoadTextFromResources("Localization.English.json"));

            assetBundle = AssetUtils.LoadAssetBundleFromResources("customships");

            shipPrefab = assetBundle.LoadAsset<GameObject>("MS_CustomShip");
            PrefabManager.Instance.AddPrefab(shipPrefab);

            AddShipPiece("MS_Keel_4m");
            AddShipPiece("MS_Keel_Bow_1");
            AddShipPiece("MS_Rib_1.0m");
            AddShipPiece("MS_Rib_1.2m");
            AddShipPiece("MS_Rib_1.4m");
            AddShipPiece("MS_Rib_1.6m");
            AddShipPiece("MS_Rib_1.8m");
            AddShipPiece("MS_Rib_2.0m");
            AddShipPiece("MS_Rib_2.2m");
            AddShipPiece("MS_Rib_2.4m");
            AddShipPiece("MS_Rib_2.6m");
            AddShipPiece("MS_Hull_Dynamic");
            AddShipPiece("MS_Rudder_1");

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

        private void AddShipPiece(string pieceName) {
            CustomPiece piece = new CustomPiece(assetBundle, pieceName, true, ShipPartConfig());
            PieceManager.Instance.AddPiece(piece);

            if (piece.PiecePrefab.TryGetComponent(out DynamicHull dynamicHull)) {
                dynamicHull.RegenerateMesh(2, 2, 0.9f, 0, 0);
            }

            pieces.Add(piece);
            shipPieceNames.Add(pieceName);
        }

        public static bool IsShipPiece(GameObject piece) {
            return shipPieceNames.Contains(Utils.GetPrefabName(piece));
        }

        public static bool IsCustomShip(GameObject piece) {
            return shipPrefab.name == Utils.GetPrefabName(piece);
        }

        private PieceConfig ShipPartConfig() {
            PieceConfig stackConfig = new PieceConfig();
            stackConfig.PieceTable = PieceTables.Hammer;
            stackConfig.Category = "Ship";
            stackConfig.AddRequirement(new RequirementConfig("Wood", 3, 0, true));
            return stackConfig;
        }
    }
}
