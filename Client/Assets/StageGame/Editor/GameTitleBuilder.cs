using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace StageGame.Editor
{
    public static class GameTitleBuilder
    {
        [MenuItem("StageGame/Apply game title")]
        public static void Apply()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || PrefabStageUtility.GetCurrentPrefabStage() != null)
                throw new InvalidOperationException("Leave Play/prefab mode before applying the title.");
            for (int i=0;i<UnityEngine.SceneManagement.SceneManager.sceneCount;i++)
                if (UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).isDirty)
                    throw new InvalidOperationException("Save open scenes before applying the title.");
            AssetDatabase.Refresh();
            string title=StringTable.Load().Text("game_title");
            int changed=0;
            foreach (string path in Directory.GetFiles(ProjectBuilder.Root+"Prefabs","*.prefab",SearchOption.AllDirectories))
            {
                var root=PrefabUtility.LoadPrefabContents(path);
                try
                {
                    bool dirty=false;
                    foreach (var text in root.GetComponentsInChildren<Text>(true))
                        if (text.text=="4목" || text.text=="Four in a Row")
                        { text.text=title;dirty=true;changed++; }
                    if (dirty) PrefabUtility.SaveAsPrefabAsset(root,path);
                }
                finally { PrefabUtility.UnloadPrefabContents(root); }
            }
            PlayerSettings.productName=title;
            LocalizationBuilder.ExportWebStrings();
            AssetDatabase.SaveAssets();
            RankUiBuilder.Validate();
            Debug.Log("[Game title] Saved "+changed+" title labels and product/web titles: "+title);
        }
    }
}
