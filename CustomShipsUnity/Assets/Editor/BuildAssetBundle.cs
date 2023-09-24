using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class BuildAssetBundle : MonoBehaviour {
    [MenuItem("Assets/Build AssetBundles")]
    private static void BuildAllAssetBundles() {
        const string assetBundleOutputPath = "AssetBundles/StandaloneWindows";
        Directory.CreateDirectory(assetBundleOutputPath);
        string assetBundlePath = Path.Combine(assetBundleOutputPath, "customships");

        BuildPipeline.BuildAssetBundles(assetBundleOutputPath, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows);
        FileUtil.ReplaceFile(assetBundlePath, "../CustomShips/customships");
    }
}
