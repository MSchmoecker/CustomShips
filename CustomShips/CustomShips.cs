using BepInEx;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using UnityEngine;

namespace CustomShips {
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    //[NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
    internal class CustomShips : BaseUnityPlugin {
        public const string PluginGUID = "com.jotunn.jotunnmodstub";
        public const string PluginName = "CustomShips";
        public const string PluginVersion = "0.0.1";

        private static AssetBundle assetBundle;

        // public static CustomLocalization Localization = LocalizationManager.Instance.GetLocalization();

        private void Awake() {
            assetBundle = AssetUtils.LoadAssetBundleFromResources("customships");

            AddPiece("MS_Keel_4m");
            AddPiece("MS_Rib_2.0m");
            AddPiece("MS_Rib_2.2m");
            AddPiece("MS_Rib_2.4m");
            AddPiece("MS_Rib_2.6m");
            AddPiece("MS_Hull_2m");
            AddPiece("MS_HullEnd_2m");
        }

        private void AddPiece(string pieceName) {
            PieceManager.Instance.AddPiece(new CustomPiece(assetBundle, pieceName, true, ShipPartConfig()));
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