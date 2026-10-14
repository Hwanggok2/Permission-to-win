using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace StageGame.Editor
{
    // One-time, narrowly scoped migration of the existing saved prefabs. Never recreates screens.
    public static class LocalizationBuilder
    {
        const string Root = ProjectBuilder.Root;
        const string Screens = Root + "Prefabs/UI/Screens/";
        const string Common = Root + "Prefabs/UI/Common/";

        [MenuItem("StageGame/Apply localization and touch layout")]
        public static void Apply()
        {
            var table = StringTable.Load();
            Edit(Common + "CloseButton.prefab", go => Size(go.transform, 100, 100));
            foreach (string name in new[] { "ResultModal", "SettingsModal", "HelpModal", "ConfirmModal", "NoticesModal" })
                Edit(Screens + name + ".prefab", go =>
                {
                    Size(go.transform.Find("Frame/CloseButton"), 100, 100);
                    Size(go.transform.Find("Frame/Title"), 400, 80);
                });
            Edit(Screens + "LobbyScreen.prefab", go =>
            {
                foreach (string name in new[] { "Previous", "Following", "Play", "Help", "Settings" })
                    Size(go.transform.Find(name), ((RectTransform)go.transform.Find(name)).sizeDelta.x, 100);
                Position(go.transform.Find("Help"), -164, -360);
                Position(go.transform.Find("Settings"), 164, -360);
                Position(go.transform.Find("Notice"), 0, -465);
                Get<Text>(go, "Notice").fontSize = 24;
                Get<Text>(go, "StageCard/Detail").fontSize = 26;
                // The longest English titles wrap at the original 38px / 550px single-line slot.
                Get<Text>(go, "StageCard/Title").fontSize = 32;
                Size(go.transform.Find("StageCard/Title"), 580, 65);
            });
            Edit(Screens + "GameScreen.prefab", go =>
            {
                Size(go.transform.Find("Pause"), 110, 100);
                Position(go.transform.Find("Pause"), 265, 560);
                Size(go.transform.Find("Title"), 530, 80);
                Position(go.transform.Find("Title"), -55, 492);
                Get<Text>(go, "Title").fontSize = 28;
                Size(go.transform.Find("Hint"), 624, 100);
                Get<Text>(go, "Notice").fontSize = 26;
            });
            Edit(Screens + "ResultModal.prefab", go =>
            {
                Layout(go.transform.Find("Frame/Body"), 0, 110, 535, 150);
                Layout(go.transform.Find("Frame/Next"), 0, -95, 535, 100);
                Layout(go.transform.Find("Frame/Retry"), 0, -212, 535, 100);
                Layout(go.transform.Find("Frame/Lobby"), 0, -329, 535, 100);
            });
            Edit(Screens + "ConfirmModal.prefab", go =>
            {
                Size(go.transform.Find("Frame/Resume"), 535, 100);
                Position(go.transform.Find("Frame/Leave"), 0, -285);
                Size(go.transform.Find("Frame/Leave"), 535, 100);
            });
            Edit(Screens + "SettingsModal.prefab", go =>
            {
                var frame = go.transform.Find("Frame");
                Layout(frame.Find("Body"), 0, 160, 535, 70);
                var language = frame.Find("Language");
                if (language == null)
                {
                    // Copy an existing authored Text component only in the editor, then save the prefab.
                    language = UnityEngine.Object.Instantiate(frame.Find("Body").gameObject, frame).transform;
                    language.name = "Language";
                }
                Layout(language, 0, 70, 535, 40);
                AddButton(frame, "Korean", -140, -10, 255);
                AddButton(frame, "English", 140, -10, 255);
                Layout(frame.Find("Sound"), 0, -135, 535, 100);
                Layout(frame.Find("Notices"), 0, -260, 535, 100);
            });
            Edit(Root + "Prefabs/AppRoot.prefab", root =>
            {
                var app = root.GetComponent<GameApp>();
                app.korean = Get<Button>(app.settings, "Frame/Korean");
                app.english = Get<Button>(app.settings, "Frame/English");
                app.koreanLabel = Get<Text>(app.settings, "Frame/Korean/Label");
                app.englishLabel = Get<Text>(app.settings, "Frame/English/Label");
                app.playLabel = Get<Text>(app.lobby, "Play/Label");
                app.hintLabel = Get<Text>(app.match, "Hint/Label");
                app.columnLabels = new Text[7];
                for (int i = 0; i < 7; i++) app.columnLabels[i] = Get<Text>(app.match, "Marker_" + i + "/Label");
                var labels = new List<LocalizedLabel>();
                void Bind(GameObject screen, string path, string key)
                {
                    var text = Get<Text>(screen, path);
                    text.text = table.Text(key);
                    labels.Add(new LocalizedLabel { target = text, key = key });
                }
                Bind(app.lobby, "Brand", "game_brand");
                Bind(app.lobby, "Title", "client_game_title");
                Bind(app.lobby, "Subtitle", "game_subtitle");
                Bind(app.lobby, "Previous/Label", "ui_previous_page");
                Bind(app.lobby, "Following/Label", "ui_next_page");
                Bind(app.lobby, "Help/Label", "client_ui_help");
                Bind(app.lobby, "Settings/Label", "ui_settings");
                Bind(app.lobby, "Banner/Label", "ad_unavailable");
                Bind(app.match, "Eyebrow", "match_eyebrow");
                Bind(app.match, "Pause/Label", "ui_pause");
                Bind(app.match, "Legend", "match_legend");
                Bind(app.result, "Frame/Next/Label", "ui_next");
                Bind(app.result, "Frame/Retry/Label", "ui_retry");
                Bind(app.result, "Frame/Lobby/Label", "ui_lobby");
                Bind(app.result, "Banner/Label", "ad_unavailable");
                Bind(app.settings, "Frame/Title", "ui_settings");
                Bind(app.settings, "Frame/Language", "settings_language");
                Bind(app.settings, "Frame/Notices/Label", "legal_title");
                Bind(app.help, "Frame/Title", "client_ui_help");
                Bind(app.help, "Frame/Body", "client_help_rules");
                Bind(app.confirm, "Frame/Title", "confirm_title");
                Bind(app.confirm, "Frame/Body", "confirm_body");
                Bind(app.confirm, "Frame/Resume/Label", "client_ui_continue");
                Bind(app.confirm, "Frame/Leave/Label", "confirm_leave");
                Bind(app.notices, "Frame/Title", "legal_title");
                foreach (var screen in new[] { app.result, app.settings, app.help, app.confirm, app.notices })
                    Bind(screen, "Frame/Eyebrow", "game_brand");
                app.localizedLabels = labels.ToArray();
                // Default saved values also come from Excel. Dynamic values are refreshed by GameApp.
                app.playLabel.text = table.Text("client_ui_start");
                app.hintLabel.text = table.Text("client_hint_request");
                app.koreanLabel.text = table.Text("language_ko");
                app.englishLabel.text = table.Text("language_en");
                app.lobbyBanner.SetActive(false); app.resultBanner.SetActive(false);
            });
            AssetDatabase.SaveAssets();
            ProjectBuilder.Validate();
            Debug.Log("[Localization] Saved prefab bindings and touch layout updated.");
        }

        [Serializable] sealed class WebStrings { public WebLocale[] languages; }
        [Serializable] sealed class WebLocale { public string code, title, loading, error, canvasLabel; }
        public static void ExportWebStrings()
        {
            var table = StringTable.Load();
            var locales = new List<WebLocale>();
            foreach (string language in new[] { "ko", "en" })
            {
                table.SetLanguage(language);
                locales.Add(new WebLocale { code = language, title = table.Text("client_game_title"), loading = table.Text("web_loading"), error = table.Text("web_error"), canvasLabel = table.Text("web_canvas_label") });
            }
            File.WriteAllText("Assets/WebGLTemplates/StageGamePortrait/ui-text.js", "// Generated from the Excel-exported string table. Do not edit.\nwindow.stageGameUiText = " + JsonUtility.ToJson(new WebStrings { languages = locales.ToArray() }) + ";\n");
            AssetDatabase.Refresh();
        }

        static void Edit(string path, Action<GameObject> edit)
        {
            if (!File.Exists(path)) throw new FileNotFoundException("Expected existing prefab", path);
            var go = PrefabUtility.LoadPrefabContents(path);
            try { edit(go); PrefabUtility.SaveAsPrefabAsset(go, path); }
            finally { PrefabUtility.UnloadPrefabContents(go); }
        }
        static void AddButton(Transform parent, string name, float x, float y, float width)
        {
            var existing = parent.Find(name);
            if (existing == null)
            {
                var go = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Common + "PrimaryButton.prefab"), parent);
                go.name = name; existing = go.transform;
            }
            Layout(existing, x, y, width, 100);
            var text = existing.GetComponentInChildren<Text>();
            text.rectTransform.anchorMin = Vector2.zero; text.rectTransform.anchorMax = Vector2.one;
            text.rectTransform.offsetMin = new Vector2(8, 3); text.rectTransform.offsetMax = new Vector2(-8, -3);
            text.fontSize = 26;
        }
        static void Layout(Transform target, float x, float y, float width, float height) { Position(target, x, y); Size(target, width, height); }
        static void Position(Transform target, float x, float y) { ((RectTransform)target).anchoredPosition = new Vector2(x, y); }
        static void Size(Transform target, float width, float height) { ((RectTransform)target).sizeDelta = new Vector2(width, height); }
        static T Get<T>(GameObject go, string path) where T : Component => go.transform.Find(path).GetComponent<T>();
    }
}
