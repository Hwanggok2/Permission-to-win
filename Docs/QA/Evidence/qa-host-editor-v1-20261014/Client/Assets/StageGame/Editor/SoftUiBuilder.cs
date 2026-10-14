using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace StageGame.Editor
{
    // Original geometric artwork generated here. Existing artwork and prefab hierarchies are preserved.
    public static class SoftUiBuilder
    {
        const string Root = ProjectBuilder.Root;
        public const string Art = Root + "Art/SoftUI/";
        const string Common = Root + "Prefabs/UI/Common/";
        const string Screens = Root + "Prefabs/UI/Screens/";

        [MenuItem("StageGame/Apply soft geometric UI")]
        public static void Apply()
        {
            if (!string.Equals(Path.GetFullPath(Application.dataPath), Path.GetFullPath("C:/Github/StageGame/Client/Assets"), StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("This migration only runs in the StageGame Client project.");
            Directory.CreateDirectory(Art);
            var rounded = CreateSprite("RoundRect96.png", 96, 26, (x, y) =>
            {
                // Signed distance to a 96px square with 24px corners; one-pixel antialias fringe.
                float qx = Mathf.Abs(x - 48) - 23.5f, qy = Mathf.Abs(y - 48) - 23.5f;
                float distance = new Vector2(Mathf.Max(qx, 0), Mathf.Max(qy, 0)).magnitude + Mathf.Min(Mathf.Max(qx, qy), 0) - 24;
                return new Color(1, 1, 1, Mathf.Clamp01(0.5f - distance));
            });
            var disc = CreateSprite("SoftDisc128.png", 128, 0, (x, y) =>
            {
                float distance = Vector2.Distance(new Vector2(x, y), new Vector2(64, 64));
                float shade = 0.92f + 0.08f * Mathf.Clamp01(y / 128f);
                return new Color(shade, shade, shade, Mathf.Clamp01(62 - distance));
            });
            foreach (string path in new[] { Common + "PrimaryButton.prefab", Common + "CloseButton.prefab", Common + "ModalFrame.prefab", Common + "StageCard.prefab", Root + "Prefabs/Board/Board.prefab" })
                Edit(path, go => Round(go.GetComponent<Image>(), rounded));
            Edit(Common + "PrimaryButton.prefab", go => Size(go.transform, 535, 110));
            Edit(Common + "CloseButton.prefab", go => Size(go.transform, 110, 110));
            Edit(Root + "Prefabs/Board/Cell.prefab", go => go.GetComponent<Image>().sprite = disc);
            Edit(Root + "Prefabs/Board/Board.prefab", go =>
            {
                Size(go.transform, 720, 550);
                for (int row = 0; row < 6; row++) for (int col = 0; col < 7; col++)
                    Position(go.transform.Find($"Cell_{row * 7 + col:00}"), (col - 3) * (720f / 7), (row - 2.5f) * 88);
                for (int col = 0; col < 7; col++) Layout(go.transform.Find("Column_" + col), (col - 3) * (720f / 7), 0, 720f / 7, 550);
            });
            foreach (string name in new[] { "LobbyScreen", "GameScreen", "ResultModal", "SettingsModal", "HelpModal", "ConfirmModal", "NoticesModal" })
                Edit(Screens + name + ".prefab", go =>
                {
                    Size(go.transform, 720, 960);
                    foreach (var button in go.GetComponentsInChildren<Button>(true))
                    {
                        if (button.name.StartsWith("Column_")) continue;
                        var rect = (RectTransform)button.transform;
                        Size(rect, Mathf.Max(110, rect.sizeDelta.x), 110);
                        if (button.name != "CloseButton") button.GetComponentInChildren<Text>().fontSize = 28;
                    }
                    var frame = go.transform.Find("Frame");
                    if (frame == null) return;
                    Layout(frame, 0, 0, 640, name == "NoticesModal" ? 900 : 840);
                    Layout(frame.Find("Title"), 0, name == "NoticesModal" ? 300 : 280, 380, 80);
                    Position(frame.Find("CloseButton"), 255, name == "NoticesModal" ? 380 : 350);
                    Position(frame.Find("Eyebrow"), -35, name == "NoticesModal" ? 380 : 350);
                });
            Edit(Screens + "LobbyScreen.prefab", go =>
            {
                Layout(go.transform.Find("Brand"), 0, 460, 640, 28);
                Layout(go.transform.Find("Title"), 0, 395, 650, 96);
                Layout(go.transform.Find("Subtitle"), 0, 330, 630, 40);
                for (int i = 0; i < 4; i++)
                {
                    var dot = go.transform.Find("ConnectedStone" + i);
                    dot.GetComponent<Image>().sprite = disc;
                    Position(dot, (i - 1.5f) * 84, 275);
                }
                Position(go.transform.Find("StageCard"), 0, 95);
                Position(go.transform.Find("Previous"), -240, -100);
                Position(go.transform.Find("Following"), 240, -100);
                Position(go.transform.Find("Progress"), 0, -100);
                Position(go.transform.Find("Play"), 0, -215);
                Position(go.transform.Find("Help"), -164, -330);
                Position(go.transform.Find("Settings"), 164, -330);
                Layout(go.transform.Find("Notice"), 0, -405, 620, 40);
                Layout(go.transform.Find("Banner"), 0, -452, 624, 48);
                Round(go.transform.Find("Banner").GetComponent<Image>(), rounded);
            });
            Edit(Screens + "GameScreen.prefab", go =>
            {
                Layout(go.transform.Find("Eyebrow"), -35, 455, 480, 36);
                Layout(go.transform.Find("Pause"), 265, 425, 110, 110);
                Layout(go.transform.Find("Title"), -55, 395, 530, 60);
                Layout(go.transform.Find("MoveCount"), -220, 345, 220, 35);
                Layout(go.transform.Find("Legend"), 125, 345, 420, 35);
                Layout(go.transform.Find("Turn"), 0, 305, 690, 50);
                for (int i = 0; i < 7; i++)
                {
                    var marker = go.transform.Find("Marker_" + i);
                    Layout(marker, (i - 3) * (720f / 7), 255, 92, 36);
                    Round(marker.GetComponent<Image>(), rounded);
                }
                Layout(go.transform.Find("Board"), 0, -38, 720, 550);
                Layout(go.transform.Find("Notice"), 0, -340, 690, 50);
                Layout(go.transform.Find("Hint"), 0, -425, 624, 110);
            });
            Edit(Screens + "ResultModal.prefab", go =>
            {
                Position(go.transform.Find("Frame/Next"), 0, -90);
                Position(go.transform.Find("Frame/Retry"), 0, -215);
                Position(go.transform.Find("Frame/Lobby"), 0, -340);
                Layout(go.transform.Find("AdStatus"), 0, -425, 610, 30);
                Layout(go.transform.Find("Banner"), 0, -463, 624, 30);
                Round(go.transform.Find("Banner").GetComponent<Image>(), rounded);
            });
            Edit(Screens + "ConfirmModal.prefab", go => Position(go.transform.Find("Frame/Leave"), 0, -295));
            Edit(Screens + "SettingsModal.prefab", go =>
            {
                Layout(go.transform.Find("Frame/Body"), 0, 170, 535, 70);
                Position(go.transform.Find("Frame/Language"), 0, 80);
                Position(go.transform.Find("Frame/Korean"), -140, -20);
                Position(go.transform.Find("Frame/English"), 140, -20);
                Position(go.transform.Find("Frame/Sound"), 0, -160);
                Position(go.transform.Find("Frame/Notices"), 0, -300);
                var button = go.transform.Find("Frame/Notices").GetComponent<Button>();
                var background = button.GetComponent<Image>();
                background.color = Color.clear; // The rectangular hit target remains; only the label is visible.
                var label = button.GetComponentInChildren<Text>();
                label.color = new Color32(155, 171, 191, 255);
                label.fontSize = 26;
                button.targetGraphic = label;
                var colors = button.colors;
                colors.normalColor = Color.white; colors.highlightedColor = new Color32(185, 255, 236, 255);
                colors.pressedColor = new Color32(108, 237, 199, 255);
                button.colors = colors;
            });
            Edit(Screens + "NoticesModal.prefab", go =>
            {
                Layout(go.transform.Find("Frame/Scroll"), 0, -85, 560, 650);
                go.transform.Find("Frame/Scroll/Viewport/Content").GetComponent<Text>().fontSize = 24;
            });
            // Some original scene instances have explicit sprite overrides; update those references only.
            Edit(Root + "Prefabs/AppRoot.prefab", go =>
            {
                var app = go.GetComponent<GameApp>();
                foreach (var stone in app.stones) stone.sprite = disc;
                var fitter = go.GetComponentInChildren<SafeAreaFitter>(true);
                fitter.referenceSize = new Vector2(720, 960);
                Size(fitter.content, 720, 960);
                go.GetComponent<CanvasScaler>().referenceResolution = new Vector2(720, 960);
                foreach (var screen in new[] { app.lobby, app.match, app.result, app.settings, app.help, app.confirm, app.notices }) Size(screen.transform, 720, 960);
            });
            AssetDatabase.SaveAssets();
            ProjectBuilder.Validate();
            Debug.Log("[Soft UI] Original round-rectangle/disc artwork and saved prefab references applied.");
        }

        static void Round(Image image, Sprite sprite) { image.sprite = sprite; image.type = Image.Type.Sliced; }
        static void Layout(Transform target, float x, float y, float width, float height) { Position(target, x, y); Size(target, width, height); }
        static void Position(Transform target, float x, float y) { ((RectTransform)target).anchoredPosition = new Vector2(x, y); }
        static void Size(Transform target, float width, float height) { ((RectTransform)target).sizeDelta = new Vector2(width, height); }
        static void Edit(string path, Action<GameObject> edit)
        {
            if (!File.Exists(path)) throw new FileNotFoundException("Expected existing prefab", path);
            var go = PrefabUtility.LoadPrefabContents(path);
            try { edit(go); PrefabUtility.SaveAsPrefabAsset(go, path); }
            finally { PrefabUtility.UnloadPrefabContents(go); }
        }

        static Sprite CreateSprite(string name, int size, int border, Func<float, float, Color> pixel)
        {
            string path = Art + name;
            if (File.Exists(path))
            {
                var existing = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (existing == null) throw new InvalidOperationException("Existing asset is protected: " + path);
                return existing;
            }
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            for (int y = 0; y < size; y++) for (int x = 0; x < size; x++) texture.SetPixel(x, y, pixel(x + .5f, y + .5f));
            texture.Apply(); File.WriteAllBytes(path, texture.EncodeToPNG()); UnityEngine.Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(path);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single;
            importer.spriteBorder = new Vector4(border, border, border, border);
            importer.alphaIsTransparency = true; importer.mipmapEnabled = false; importer.wrapMode = TextureWrapMode.Clamp;
            importer.filterMode = FilterMode.Bilinear;
            var settings = new TextureImporterSettings(); importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect; importer.SetTextureSettings(settings);
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
    }
}
