using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace StageGame.Editor
{
    // Targeted migration after SoftUiBuilder. Creates missing saved objects; never rebuilds a screen.
    public static class StageMapBuilder
    {
        const string Root = ProjectBuilder.Root;
        const string Common = Root + "Prefabs/UI/Common/";
        const string Screens = Root + "Prefabs/UI/Screens/";
        static readonly Color Ink = new Color32(15, 27, 43, 255), Surface = new Color32(27, 42, 64, 255);
        static readonly Color Mint = new Color32(108, 237, 199, 255), Coral = new Color32(255, 137, 121, 255);
        static readonly Color White = new Color32(241, 245, 249, 255), Muted = new Color32(155, 171, 191, 255);
        static Font font;
        static Sprite rounded, disc;

        [MenuItem("StageGame/Apply chapter map and illustrated help")]
        public static void Apply()
        {
            if (!string.Equals(Path.GetFullPath(Application.dataPath), Path.GetFullPath("C:/Github/StageGame/Client/Assets"), StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Only the StageGame Client project is allowed.");
            font = AssetDatabase.LoadAssetAtPath<Font>(Root + "Fonts/NotoSansCJKkr-Regular.otf");
            rounded = AssetDatabase.LoadAssetAtPath<Sprite>(SoftUiBuilder.Art + "RoundRect96.png");
            disc = AssetDatabase.LoadAssetAtPath<Sprite>(SoftUiBuilder.Art + "SoftDisc128.png");
            if (font == null || rounded == null || disc == null) throw new InvalidOperationException("Existing licensed font/soft sprites are required.");
            var strings = StringTable.Load();
            string nodePath = Common + "StageNode.prefab";
            if (!File.Exists(nodePath))
            {
                var node = new GameObject("StageNode", typeof(RectTransform));
                try
                {
                    Layout(node.transform, 0, 0, 120, 120);
                    var image = node.AddComponent<Image>(); image.sprite = rounded; image.type = Image.Type.Sliced; image.color = Surface;
                    node.AddComponent<Button>().targetGraphic = image;
                    Label(node.transform, "Number", "01", 32, White, 0, 23, 116, 50);
                    Label(node.transform, "State", strings.Text("stage_state_current"), 28, White, 0, -30, 116, 42);
                    PrefabUtility.SaveAsPrefabAsset(node, nodePath);
                }
                finally { UnityEngine.Object.DestroyImmediate(node); }
            }
            var nodePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(nodePath);
            if (nodePrefab == null || nodePrefab.GetComponent<Button>() == null || nodePrefab.transform.Find("Number") == null || nodePrefab.transform.Find("State") == null)
                throw new InvalidOperationException("Existing StageNode asset is unrecognized and protected.");

            Edit(Screens + "LobbyScreen.prefab", lobby =>
            {
                foreach (string name in new[] { "Brand", "Subtitle", "ConnectedStone0", "ConnectedStone1", "ConnectedStone2", "ConnectedStone3" })
                    lobby.transform.Find(name).gameObject.SetActive(false);
                Layout(lobby.transform.Find("Title"), 0, 438, 650, 70);
                lobby.transform.Find("Title").GetComponent<Text>().fontSize = 46;
                Layout(lobby.transform.Find("Previous"), -260, 340, 134, 110);
                Layout(lobby.transform.Find("Following"), 260, 340, 134, 110);
                Label(lobby.transform, "Chapter", strings.Format("stage_map_chapter", 1, 3), 28, Muted, 0, 365, 360, 42);
                Label(lobby.transform, "Tier", strings.Text("stage_tier_1"), 32, White, 0, 315, 360, 50);
                var map = Rect(lobby.transform, "StageMap", 0, 0, 720, 480);
                for (int i = 0; i < 10; i++)
                {
                    string name = "StageNode_" + i;
                    var node = map.Find(name);
                    if (node == null)
                    {
                        node = ((GameObject)PrefabUtility.InstantiatePrefab(nodePrefab, map)).transform;
                        node.name = name;
                    }
                    Layout(node, (i % 5 - 2) * 132, i < 5 ? 210 : 78, 120, 120);
                }
                var card = lobby.transform.Find("StageCard");
                Layout(card, 0, -80, 648, 176);
                Layout(card.Find("Number"), 0, 60, 600, 30);
                card.Find("Number").GetComponent<Text>().fontSize = 20;
                Layout(card.Find("Title"), 0, 15, 610, 55);
                card.Find("Title").GetComponent<Text>().fontSize = 30;
                Layout(card.Find("Detail"), 0, -46, 610, 46);
                card.Find("Detail").GetComponent<Text>().fontSize = 28;
                Layout(lobby.transform.Find("Progress"), 0, -190, 640, 42);
                lobby.transform.Find("Progress").GetComponent<Text>().fontSize = 28;
                Layout(lobby.transform.Find("Notice"), 0, -190, 640, 42);
                Layout(lobby.transform.Find("Play"), 0, -270, 648, 110);
                Layout(lobby.transform.Find("Help"), -170, -392, 308, 110);
                Layout(lobby.transform.Find("Settings"), 170, -392, 308, 110);
                Layout(lobby.transform.Find("Banner"), 0, -463, 648, 30);
            });

            Edit(Screens + "HelpModal.prefab", help =>
            {
                var frame = help.transform.Find("Frame");
                frame.Find("Body").gameObject.SetActive(false); // Preserve the earlier prose asset/reference.
                Label(frame, "DropTitle", strings.Text("help_drop_title"), 28, White, 0, 202, 560, 50);
                Label(frame, "DropCaption", strings.Text("help_drop_caption"), 28, Muted, 126, 40, 270, 175);
                var gravity = Panel(frame, "GravityExample", -170, 40, 190, 220, Surface);
                Panel(gravity, "ChosenColumn", 0, 0, 54, 208, new Color(1, 1, 1, .09f));
                for (int row = 0; row < 4; row++) for (int col = 0; col < 3; col++)
                    Dot(gravity, $"Cell_{row}_{col}", (col - 1) * 60, (row - 1.5f) * 50, 38,
                        col == 1 && row == 0 ? Coral : col == 1 && row == 1 ? Mint : Ink);
                var arrow = Rect(gravity, "Arrow", 0, 49, 40, 70);
                Bar(arrow, "Shaft", 0, 8, 6, 50, 0);
                Bar(arrow, "Left", -7, -15, 6, 22, 40);
                Bar(arrow, "Right", 7, -15, 6, 22, -40);
                Label(frame, "ConnectTitle", strings.Text("help_connect_title"), 28, White, 0, -114, 560, 50);
                for (int example = 0; example < 3; example++)
                {
                    var diagram = Panel(frame, "ConnectExample_" + example, (example - 1) * 190, -240, 174, 164, Ink);
                    // Abstract direction patterns, not floating stones on a gravity board.
                    for (int i = 0; i < 4; i++)
                    {
                        float offset = (i - 1.5f) * 36;
                        Dot(diagram, "Stone_" + i, example == 1 ? 0 : offset, example == 0 ? 0 : offset, 28, Mint);
                    }
                }
                Label(frame, "ConnectCaption", strings.Text("help_connect_caption"), 28, Muted, 0, -365, 560, 55);
            });
            Edit(Screens + "ResultModal.prefab", result =>
            {
                var frame = result.transform.Find("Frame");
                Rect(frame, "PrimarySlot", 0, -90, 535, 110);
                Rect(frame, "SecondarySlot", 0, -215, 535, 110);
            });
            Edit(Root + "Prefabs/AppRoot.prefab", root =>
            {
                var app = root.GetComponent<GameApp>();
                app.stageNodes = new Button[10]; app.stageNodeNumbers = new Text[10]; app.stageNodeStates = new Text[10];
                for (int i = 0; i < 10; i++)
                {
                    var node = app.lobby.transform.Find("StageMap/StageNode_" + i);
                    app.stageNodes[i] = node.GetComponent<Button>();
                    app.stageNodeNumbers[i] = node.Find("Number").GetComponent<Text>();
                    app.stageNodeStates[i] = node.Find("State").GetComponent<Text>();
                }
                app.chapterLabel = app.lobby.transform.Find("Chapter").GetComponent<Text>();
                app.chapterTier = app.lobby.transform.Find("Tier").GetComponent<Text>();
                app.retryLabel = app.retry.GetComponentInChildren<Text>();
                app.nextStageLabel = app.nextStage.GetComponentInChildren<Text>();
                app.resultLobbyLabel = app.resultLobby.GetComponentInChildren<Text>();
                app.resultPrimarySlot = (RectTransform)app.result.transform.Find("Frame/PrimarySlot");
                app.resultSecondarySlot = (RectTransform)app.result.transform.Find("Frame/SecondarySlot");
                var labels = new List<LocalizedLabel>(app.localizedLabels);
                foreach (var pair in new[] { new[] { "DropTitle", "help_drop_title" }, new[] { "DropCaption", "help_drop_caption" }, new[] { "ConnectTitle", "help_connect_title" }, new[] { "ConnectCaption", "help_connect_caption" } })
                {
                    var target = app.help.transform.Find("Frame/" + pair[0]).GetComponent<Text>();
                    var existing = labels.Find(item => item.target == target);
                    if (existing != null) existing.key = pair[1];
                    else labels.Add(new LocalizedLabel { target = target, key = pair[1] });
                }
                app.localizedLabels = labels.ToArray();
            });
            AssetDatabase.SaveAssets();
            ProjectBuilder.Validate();
            Debug.Log("[Stage map] Saved10 nested nodes, chapter bindings, illustrated help and result action slots.");
        }

        static void Edit(string path, Action<GameObject> edit)
        {
            var root = PrefabUtility.LoadPrefabContents(path);
            try { edit(root); PrefabUtility.SaveAsPrefabAsset(root, path); }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }
        static void Layout(Transform target, float x, float y, float width, float height)
        {
            var rect = (RectTransform)target;
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(.5f, .5f);
            rect.anchoredPosition = new Vector2(x, y); rect.sizeDelta = new Vector2(width, height);
        }
        static RectTransform Rect(Transform parent, string name, float x, float y, float width, float height)
        {
            var existing = parent.Find(name);
            var target = existing != null ? existing : new GameObject(name, typeof(RectTransform)).transform;
            if (existing == null) target.SetParent(parent, false);
            Layout(target, x, y, width, height);
            return (RectTransform)target;
        }
        static RectTransform Panel(Transform parent, string name, float x, float y, float width, float height, Color color)
        {
            var rect = Rect(parent, name, x, y, width, height);
            var image = rect.GetComponent<Image>() ?? rect.gameObject.AddComponent<Image>();
            image.sprite = rounded; image.type = Image.Type.Sliced; image.color = color; image.raycastTarget = false;
            return rect;
        }
        static void Dot(Transform parent, string name, float x, float y, float size, Color color)
        {
            var rect = Panel(parent, name, x, y, size, size, color);
            var image = rect.GetComponent<Image>(); image.sprite = disc; image.type = Image.Type.Simple;
        }
        static void Bar(Transform parent, string name, float x, float y, float width, float height, float rotation)
        {
            var rect = Panel(parent, name, x, y, width, height, Mint);
            rect.localEulerAngles = new Vector3(0, 0, rotation);
        }
        static void Label(Transform parent, string name, string value, int size, Color color, float x, float y, float width, float height)
        {
            var rect = Rect(parent, name, x, y, width, height);
            var text = rect.GetComponent<Text>() ?? rect.gameObject.AddComponent<Text>();
            text.font = font; text.text = value; text.fontSize = size; text.color = color;
            text.alignment = TextAnchor.MiddleCenter; text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Wrap; text.verticalOverflow = VerticalWrapMode.Truncate;
        }
    }
}
