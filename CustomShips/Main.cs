using System.Collections;
using System.Collections.Generic;
using BepInEx;
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
    internal class Main : BaseUnityPlugin {
        public const string PluginGUID = "com.jotunn.jotunnmodstub";
        public const string PluginName = "CustomShips";
        public const string PluginVersion = "0.0.1";

        private static AssetBundle assetBundle;
        private static List<CustomPiece> pieces = new List<CustomPiece>();
        private static HashSet<string> shipPieceNames = new HashSet<string>();

        public static GameObject shipPrefab;

        // public static CustomLocalization Localization = LocalizationManager.Instance.GetLocalization();

        private void Awake() {
            assetBundle = AssetUtils.LoadAssetBundleFromResources("customships");

            shipPrefab = assetBundle.LoadAsset<GameObject>("CustomShip");
            PrefabManager.Instance.AddPrefab(shipPrefab);

            AddShipPiece("MS_Keel_4m");
            AddShipPiece("MS_Rib_2.0m");
            AddShipPiece("MS_Rib_2.2m");
            AddShipPiece("MS_Rib_2.4m");
            AddShipPiece("MS_Rib_2.6m");
            AddShipPiece("MS_Hull_2m");
            AddShipPiece("MS_HullEnd_2m");

            PieceManager.OnPiecesRegistered += OnPiecesRegistered;

            Harmony harmony = new Harmony(PluginGUID);
            harmony.PatchAll();
        }

        private void OnPiecesRegistered() {
            PieceManager.OnPiecesRegistered -= OnPiecesRegistered;

            foreach (CustomPiece piece in pieces) {
                StartCoroutine(RenderSprite(piece));
            }
        }

        private static IEnumerator RenderSprite(CustomPiece piece) {
            yield return null;

            piece.Piece.m_icon = RenderManager.Instance.Render(new RenderManager.RenderRequest(piece.PiecePrefab) {
                Width = 64,
                Height = 64,
                Rotation = RenderManager.IsometricRotation * Quaternion.Euler(0, -90f, 0),
            });
        }

        private void AddShipPiece(string pieceName) {
            CustomPiece piece = new CustomPiece(assetBundle, pieceName, true, ShipPartConfig());
            PieceManager.Instance.AddPiece(piece);

            pieces.Add(piece);
            shipPieceNames.Add(pieceName);
        }

        public static bool IsShipPiece(GameObject piece) {
            return shipPieceNames.Contains(Utils.GetPrefabName(piece));
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
