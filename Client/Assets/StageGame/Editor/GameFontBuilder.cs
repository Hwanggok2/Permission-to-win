using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace StageGame.Editor
{
    public static class GameFontBuilder
    {
        [MenuItem("StageGame/Apply Pretendard font")]
        public static void Apply()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || PrefabStageUtility.GetCurrentPrefabStage() != null)
                throw new InvalidOperationException("Leave Play/prefab mode before applying fonts.");
            for (int i=0;i<UnityEngine.SceneManagement.SceneManager.sceneCount;i++)
                if (UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).isDirty)
                    throw new InvalidOperationException("Save open scenes before applying fonts.");
            AssetDatabase.Refresh();
            var previous=AssetDatabase.LoadAssetAtPath<Font>(ProjectBuilder.Root+"Fonts/NotoSansCJKkr-Regular.otf");
            var font=AssetDatabase.LoadAssetAtPath<Font>(ProjectBuilder.Root+"Fonts/Pretendard-Regular.ttf");
            if (previous==null || font==null || !font.dynamic)
                throw new InvalidOperationException("The existing font and licensed dynamic Pretendard font are required.");
            int changed=0;
            // Save dependencies first so parent prefabs inherit changes without redundant overrides.
            var paths=Directory.GetFiles(ProjectBuilder.Root+"Prefabs","*.prefab",SearchOption.AllDirectories)
                .Select(path=>path.Replace('\\','/')).OrderBy(path=>AssetDatabase.GetDependencies(path,true).Length).ToArray();
            foreach (string path in paths)
            {
                var root=PrefabUtility.LoadPrefabContents(path);
                try
                {
                    bool dirty=false;
                    foreach (var text in root.GetComponentsInChildren<Text>(true))
                        if (text.font==previous)
                        { text.font=font;dirty=true;changed++; }
                    if (dirty) PrefabUtility.SaveAsPrefabAsset(root,path);
                }
                finally { PrefabUtility.UnloadPrefabContents(root); }
            }
            LocalizationBuilder.ExportLegalNotices();
            AssetDatabase.SaveAssets();
            RankUiBuilder.Validate();
            foreach (string path in paths)
                foreach (var text in AssetDatabase.LoadAssetAtPath<GameObject>(path).GetComponentsInChildren<Text>(true))
                    if (text.font!=font) throw new InvalidDataException("Unexpected UI font: "+path+" / "+text.name);
            Debug.Log("[Game font] Saved Pretendard on "+changed+" labels; all saved UI font references and legal notices validated.");
        }
    }
}
