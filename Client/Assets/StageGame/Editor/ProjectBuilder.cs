using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace StageGame.Editor
{
    public static class ProjectBuilder
    {
        public const string Root = "Assets/StageGame/";
        public const string ScenePath = Root + "Scenes/Main.unity";
        const string Common = Root + "Prefabs/UI/Common/";
        const string Screens = Root + "Prefabs/UI/Screens/";
        static Font font;
        static Sprite circle;
        static readonly Color Ink = Hex("101B2B"), Surface = Hex("1B2A40"), Muted = Hex("9BABBF"), Mint = Hex("6CEDC7"), Coral = Hex("FF8979"), White = Hex("F1F5F9");

        [MenuItem("StageGame/Create missing project assets")]
        public static void Create()
        {
            AssertTarget();
            foreach (string directory in new[] { Common, Screens, Root + "Prefabs/Board", Root + "Scenes", Root + "Art", Root + "Audio" }) Directory.CreateDirectory(directory);
            AssetDatabase.Refresh();
            font = AssetDatabase.LoadAssetAtPath<Font>(Root + "Fonts/NotoSansCJKkr-Regular.otf");
            if (font == null) throw new InvalidOperationException("The licensed Korean font is missing.");
            circle = CreateCircle();
            CreateSound();
            Ensure(Common + "PrimaryButton.prefab", () =>
            {
                var button = Rect("PrimaryButton", null, 560, 84);
                button.AddComponent<Image>().color = Mint;
                var control = button.AddComponent<Button>();
                control.targetGraphic = button.GetComponent<Image>();
                var colors = control.colors; colors.disabledColor = new Color(0.4f, 0.5f, 0.5f, 0.5f); control.colors = colors;
                Label("Label", button.transform, "계속", 28, Ink, 520, 80);
                return button;
            });
            Ensure(Common + "CloseButton.prefab", () =>
            {
                var go = Rect("CloseButton", null, 64, 64);
                go.AddComponent<Image>().color = Hex("34455D");
                go.AddComponent<Button>().targetGraphic = go.GetComponent<Image>();
                Label("Label", go.transform, "×", 36, White, 64, 64);
                return go;
            });
            Ensure(Common + "ModalFrame.prefab", () =>
            {
                var go = Panel("ModalFrame", null, Surface, 640, 790);
                Label("Eyebrow", go.transform, "STAGEGAME", 18, Mint, 430, 35, -35, 325);
                Label("Title", go.transform, "안내", 34, White, 470, 80, 0, 250);
                Label("Body", go.transform, "", 24, Muted, 535, 235, 0, 60);
                var close = Instance(Common + "CloseButton.prefab", go.transform, "CloseButton");
                Place(close, 255, 325, 64, 64);
                return go;
            });
            Ensure(Common + "StageCard.prefab", () =>
            {
                var go = Panel("StageCard", null, Surface, 624, 260);
                Label("Number", go.transform, "CHAPTER 01 / STAGE 01", 21, Mint, 550, 45, 0, 82);
                Label("Title", go.transform, "첫 수", 38, White, 550, 65, 0, 20);
                Label("Detail", go.transform, "나의 돌 4개를 먼저 연결하세요.", 23, Muted, 560, 85, 0, -68);
                return go;
            });
            Ensure(Root + "Prefabs/Board/Cell.prefab", () =>
            {
                var go = Rect("Cell", null, 72, 72);
                var image = go.AddComponent<Image>(); image.sprite = circle; image.color = Ink;
                Label("Label", go.transform, "", 23, Ink, 65, 60);
                return go;
            });
            Ensure(Root + "Prefabs/Board/Board.prefab", () =>
            {
                var go = Panel("Board", null, Surface, 648, 570);
                for (int row = 0; row < 6; row++)
                    for (int col = 0; col < 7; col++)
                    {
                        var cell = Instance(Root + "Prefabs/Board/Cell.prefab", go.transform, $"Cell_{row * 7 + col:00}");
                        Place(cell, (col - 3) * 88, (row - 2.5f) * 88, 72, 72);
                    }
                for (int col = 0; col < 7; col++)
                {
                    var column = Rect($"Column_{col}", go.transform, 88, 550, (col - 3) * 88, 0);
                    var image = column.AddComponent<Image>(); image.color = new Color(1, 1, 1, 0);
                    var button = column.AddComponent<Button>(); button.targetGraphic = image;
                    var colors = button.colors; colors.highlightedColor = new Color(1, 1, 1, .06f); colors.pressedColor = new Color(1, 1, 1, .13f); button.colors = colors;
                }
                return go;
            });
            Ensure(Screens + "LobbyScreen.prefab", CreateLobby);
            Ensure(Screens + "GameScreen.prefab", CreateMatch);
            Ensure(Screens + "ResultModal.prefab", () =>
            {
                var go = Modal("ResultModal", "연결 성공!", "다음 스테이지에 도전하세요.");
                Action(go.transform.Find("Frame"), "Next", "다음 스테이지", 0, -95, 535, 76);
                Action(go.transform.Find("Frame"), "Retry", "무료로 다시 도전", 0, -192, 535, 76, false);
                Action(go.transform.Find("Frame"), "Lobby", "로비로", 0, -290, 535, 65, false);
                Label("AdStatus", go.transform, "광고 미연결", 18, Muted, 610, 36, 0, -438);
                Banner(go.transform, "Banner", -515);
                return go;
            });
            Ensure(Screens + "SettingsModal.prefab", () =>
            {
                var go = Modal("SettingsModal", "설정", "");
                Action(go.transform.Find("Frame"), "Sound", "효과음 켜짐", 0, -210, 535, 80);
                Action(go.transform.Find("Frame"), "Notices", "오픈소스 라이선스", 0, -310, 535, 65, false);
                return go;
            });
            Ensure(Screens + "NoticesModal.prefab", CreateNotices);
            Ensure(Screens + "HelpModal.prefab", () => Modal("HelpModal", "플레이 방법", "1. 열을 누르면 가장 아래에 돌이 놓입니다.\n\n2. 가로·세로·대각선으로 네 개를 이으세요.\n\n3. 상대가 세 돌을 연결하면 먼저 막으세요.\n\n금색 줄은 승리를 만든 연결을 뜻합니다."));
            Ensure(Screens + "ConfirmModal.prefab", () =>
            {
                var go = Modal("ConfirmModal", "잠시 쉬어가기", "현재 대국을 나가면 이 판은 저장되지 않습니다.\n해금한 스테이지는 유지됩니다.");
                Action(go.transform.Find("Frame"), "Resume", "이어서 대국", 0, -165, 535, 80);
                Action(go.transform.Find("Frame"), "Leave", "로비로 나가기", 0, -270, 535, 80, false);
                return go;
            });
            Ensure(Root + "Prefabs/AppRoot.prefab", CreateApp);
            if (!File.Exists(ScenePath))
            {
                var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                var camera = new GameObject("Main Camera", typeof(Camera));
                camera.tag = "MainCamera";
                camera.transform.position = new Vector3(0, 0, -10);
                camera.GetComponent<Camera>().clearFlags = CameraClearFlags.SolidColor;
                camera.GetComponent<Camera>().backgroundColor = Ink;
                camera.AddComponent<AudioListener>();
                PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Root + "Prefabs/AppRoot.prefab"));
                new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
                EditorSceneManager.SaveScene(scene, ScenePath);
            }
            Configure();
            AssetDatabase.SaveAssets();
            Validate();
            Debug.Log("[StageGame] Saved scene, nested prefabs and project settings are ready.");
        }

        static GameObject CreateLobby()
        {
            var go = Screen("LobbyScreen");
            Label("Brand", go.transform, "FOUR IN A ROW   /   STAGEGAME", 19, Mint, 640, 40, 0, 558);
            Label("Title", go.transform, "네 개의 수", 61, White, 650, 90, 0, 463);
            Label("Subtitle", go.transform, "한 수씩, 연결되는 전략", 26, Muted, 630, 50, 0, 386);
            for (int i = 0; i < 4; i++)
            {
                var dot = Rect("ConnectedStone" + i, go.transform, 58, 58, (i - 1.5f) * 84, 285);
                var image = dot.AddComponent<Image>(); image.sprite = circle; image.color = Mint; image.raycastTarget = false;
            }
            var card = Instance(Common + "StageCard.prefab", go.transform, "StageCard"); Place(card, 0, 65, 624, 260);
            Action(go.transform, "Previous", "이전", -240, -125, 134, 62, false);
            Action(go.transform, "Following", "다음", 240, -125, 134, 62, false);
            Label("Progress", go.transform, "1 / 30 스테이지 해금", 21, Muted, 325, 55, 0, -125);
            Action(go.transform, "Play", "대국 시작", 0, -240, 624, 90);
            Action(go.transform, "Help", "플레이 방법", -164, -349, 296, 72, false);
            Action(go.transform, "Settings", "설정", 164, -349, 296, 72, false);
            Label("Notice", go.transform, "무료 재도전 · 진행도는 이 기기에 저장됩니다", 20, Muted, 620, 100, 0, -441);
            Banner(go.transform, "Banner", -557);
            return go;
        }

        static GameObject CreateMatch()
        {
            var go = Screen("GameScreen");
            Label("Eyebrow", go.transform, "TACTICAL STAGE", 18, Mint, 500, 36, -35, 570);
            Label("Title", go.transform, "01 / 첫 수", 32, White, 550, 65, -20, 509);
            Action(go.transform, "Pause", "쉼", 278, 563, 82, 66, false);
            Label("Turn", go.transform, "나의 차례 · 열을 선택하세요", 27, White, 640, 60, 0, 408);
            Label("Legend", go.transform, "나  MINT     /     AI  CORAL", 20, Muted, 600, 35, 0, 355);
            for (int col = 0; col < 7; col++)
            {
                var marker = Panel("Marker_" + col, go.transform, new Color(1, 1, 1, .08f), 76, 43, (col - 3) * 88, 295);
                Label("Label", marker.transform, (col + 1).ToString(), 20, White, 70, 40);
            }
            var board = Instance(Root + "Prefabs/Board/Board.prefab", go.transform, "Board"); Place(board, 0, -22, 648, 570);
            Label("MoveCount", go.transform, "00 / 42 수", 21, Muted, 620, 40, 0, -350);
            Label("Notice", go.transform, "민트색은 나, 산호색은 AI입니다.", 22, Muted, 625, 100, 0, -421);
            Action(go.transform, "Hint", "광고를 보고 힌트 · 1회", 0, -535, 624, 77, false);
            return go;
        }

        static GameObject CreateApp()
        {
            var go = new GameObject("AppRoot", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = go.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = go.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(720, 1280); scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            var backdrop = Panel("Backdrop", go.transform, Ink, 720, 1280); Stretch(backdrop);
            var safe = Rect("SafeArea", go.transform, 720, 1280); Stretch(safe); safe.AddComponent<SafeAreaFitter>();
            // CanvasScaler Expand keeps the 720x1280 composition visible even on short or wide devices.
            var content = Rect("Content", safe.transform, 720, 1280);
            safe.GetComponent<SafeAreaFitter>().content = (RectTransform)content.transform;
            var app = go.AddComponent<GameApp>();
            app.lobby = Instance(Screens + "LobbyScreen.prefab", content.transform, "Lobby");
            app.match = Instance(Screens + "GameScreen.prefab", content.transform, "Match");
            app.result = Instance(Screens + "ResultModal.prefab", content.transform, "Result");
            app.settings = Instance(Screens + "SettingsModal.prefab", content.transform, "Settings");
            app.help = Instance(Screens + "HelpModal.prefab", content.transform, "Help");
            app.confirm = Instance(Screens + "ConfirmModal.prefab", content.transform, "Confirm");
            app.notices = Instance(Screens + "NoticesModal.prefab", content.transform, "Notices");
            foreach (var screen in new[] { app.lobby, app.match, app.result, app.settings, app.help, app.confirm }) Place(screen, 0, 0, 720, 1280);
            app.stageNumber = Get<Text>(app.lobby, "StageCard/Number"); app.stageTitle = Get<Text>(app.lobby, "StageCard/Title"); app.stageDetail = Get<Text>(app.lobby, "StageCard/Detail");
            app.progressLabel = Get<Text>(app.lobby, "Progress"); app.lobbyNotice = Get<Text>(app.lobby, "Notice");
            app.previous = Get<Button>(app.lobby, "Previous"); app.following = Get<Button>(app.lobby, "Following"); app.play = Get<Button>(app.lobby, "Play");
            app.lobbySettings = Get<Button>(app.lobby, "Settings"); app.lobbyHelp = Get<Button>(app.lobby, "Help");
            app.matchTitle = Get<Text>(app.match, "Title"); app.turnLabel = Get<Text>(app.match, "Turn"); app.matchNotice = Get<Text>(app.match, "Notice"); app.moveLabel = Get<Text>(app.match, "MoveCount");
            app.pause = Get<Button>(app.match, "Pause"); app.hint = Get<Button>(app.match, "Hint");
            app.resultTitle = Get<Text>(app.result, "Frame/Title"); app.resultDetail = Get<Text>(app.result, "Frame/Body"); app.resultAdStatus = Get<Text>(app.result, "AdStatus");
            app.retry = Get<Button>(app.result, "Frame/Retry"); app.nextStage = Get<Button>(app.result, "Frame/Next"); app.resultLobby = Get<Button>(app.result, "Frame/Lobby"); app.resultClose = Get<Button>(app.result, "Frame/CloseButton");
            app.settingsDetail = Get<Text>(app.settings, "Frame/Body"); app.settingsClose = Get<Button>(app.settings, "Frame/CloseButton"); app.soundToggle = Get<Button>(app.settings, "Frame/Sound"); app.soundLabel = Get<Text>(app.settings, "Frame/Sound/Label");
            app.helpBody = Get<Text>(app.help, "Frame/Body"); app.helpClose = Get<Button>(app.help, "Frame/CloseButton");
            app.confirmClose = Get<Button>(app.confirm, "Frame/CloseButton"); app.confirmLeave = Get<Button>(app.confirm, "Frame/Leave"); app.confirmResume = Get<Button>(app.confirm, "Frame/Resume");
            app.noticesOpen = Get<Button>(app.settings, "Frame/Notices"); app.noticesClose = Get<Button>(app.notices, "Frame/CloseButton"); app.noticesBody = Get<Text>(app.notices, "Frame/Scroll/Viewport/Content");
            app.lobbyBanner = app.lobby.transform.Find("Banner").gameObject; app.resultBanner = app.result.transform.Find("Banner").gameObject;
            app.stones = new Image[42]; app.stoneLabels = new Text[42]; app.columns = new Button[7]; app.columnMarkers = new Image[7];
            for (int i = 0; i < 42; i++) { app.stones[i] = Get<Image>(app.match, $"Board/Cell_{i:00}"); app.stoneLabels[i] = Get<Text>(app.match, $"Board/Cell_{i:00}/Label"); }
            for (int i = 0; i < 7; i++) { app.columns[i] = Get<Button>(app.match, $"Board/Column_{i}"); app.columnMarkers[i] = Get<Image>(app.match, $"Marker_{i}"); }
            app.audioSource = go.AddComponent<AudioSource>(); app.audioSource.playOnAwake = false;
            app.moveSound = AssetDatabase.LoadAssetAtPath<AudioClip>(Root + "Audio/Move.wav");
            app.match.SetActive(false); app.result.SetActive(false); app.settings.SetActive(false); app.help.SetActive(false); app.confirm.SetActive(false);
            app.notices.SetActive(false);
            return go;
        }

        static GameObject CreateNotices()
        {
            var go = Modal("NoticesModal", "오픈소스 라이선스", "");
            var frame = go.transform.Find("Frame"); Place(frame.gameObject, 0, 0, 640, 1110);
            Place(frame.Find("Title").gameObject, 0, 410, 535, 80);
            Place(frame.Find("Eyebrow").gameObject, -40, 490, 430, 35);
            Place(frame.Find("CloseButton").gameObject, 255, 490, 64, 64);
            frame.Find("Body").gameObject.SetActive(false);
            var scroll = Rect("Scroll", frame, 560, 845, 0, -75);
            var view = Panel("Viewport", scroll.transform, Surface, 560, 845); Stretch(view);
            view.AddComponent<Mask>().showMaskGraphic = false;
            var content = Label("Content", view.transform, "", 18, White, 525, 0);
            content.alignment = TextAnchor.UpperLeft;
            content.rectTransform.anchorMin = new Vector2(0, 1); content.rectTransform.anchorMax = Vector2.one;
            content.rectTransform.pivot = new Vector2(.5f, 1); content.rectTransform.anchoredPosition = Vector2.zero;
            content.rectTransform.sizeDelta = new Vector2(-24, 0);
            content.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            var rect = scroll.AddComponent<ScrollRect>(); rect.viewport = (RectTransform)view.transform; rect.content = content.rectTransform;
            rect.horizontal = false; rect.movementType = ScrollRect.MovementType.Clamped; rect.scrollSensitivity = 30;
            return go;
        }

        public static void AddNotices()
        {
            AssertTarget();
            font = AssetDatabase.LoadAssetAtPath<Font>(Root + "Fonts/NotoSansCJKkr-Regular.otf");
            Ensure(Screens + "NoticesModal.prefab", CreateNotices);
            var settings = PrefabUtility.LoadPrefabContents(Screens + "SettingsModal.prefab");
            try
            {
                var frame = settings.transform.Find("Frame");
                if (frame.Find("Notices") == null) Action(frame, "Notices", "오픈소스 라이선스", 0, -310, 535, 65, false);
                PrefabUtility.SaveAsPrefabAsset(settings, Screens + "SettingsModal.prefab");
            }
            finally { PrefabUtility.UnloadPrefabContents(settings); }
            var root = PrefabUtility.LoadPrefabContents(Root + "Prefabs/AppRoot.prefab");
            try
            {
                var app = root.GetComponent<GameApp>();
                var content = root.transform.Find("SafeArea/Content");
                var existing = content.Find("Notices");
                app.notices = existing != null ? existing.gameObject : Instance(Screens + "NoticesModal.prefab", content, "Notices");
                app.noticesOpen = Get<Button>(app.settings, "Frame/Notices"); app.noticesClose = Get<Button>(app.notices, "Frame/CloseButton");
                app.noticesBody = Get<Text>(app.notices, "Frame/Scroll/Viewport/Content");
                app.notices.SetActive(false);
                PrefabUtility.SaveAsPrefabAsset(root, Root + "Prefabs/AppRoot.prefab");
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
            AssetDatabase.SaveAssets(); Validate();
        }

        [MenuItem("StageGame/Validate saved assets")]
        public static void Validate()
        {
            AssertTarget();
            var data = GameData.Load();
            var root = AssetDatabase.LoadAssetAtPath<GameObject>(Root + "Prefabs/AppRoot.prefab");
            if (root == null || !File.Exists(ScenePath)) throw new InvalidOperationException("Missing AppRoot or scene.");
            var app = root.GetComponent<GameApp>();
            var serialized = new SerializedObject(app);
            var property = serialized.GetIterator();
            while (property.NextVisible(true))
                if (property.propertyType == SerializedPropertyType.ObjectReference && property.objectReferenceValue == null)
                    throw new InvalidOperationException("Missing serialized reference: " + property.propertyPath);
            if (app.stones.Length != 42 || app.columns.Length != 7) throw new InvalidOperationException("Board view dimensions invalid.");
            if (app.stones.Any(stone => stone.sprite == null)) throw new InvalidOperationException("Board disc sprite reference is missing.");
            foreach (var close in root.GetComponentsInChildren<Button>(true).Where(b => b.name == "CloseButton"))
            {
                var source = PrefabUtility.GetCorrespondingObjectFromOriginalSource(close.gameObject);
                if (AssetDatabase.GetAssetPath(source) != Common + "CloseButton.prefab") throw new InvalidOperationException("CloseButton is not nested.");
            }
            Debug.Log($"[StageGame] Validation passed: {data.Stages.Length} stages, 42 cells, 7 columns, nested CloseButtons.");
        }

        static void Configure()
        {
            EnsurePlayerShaders();
            PlayerSettings.companyName = "StageGamePrototype"; PlayerSettings.productName = "StageGame";
            PlayerSettings.SetApplicationIdentifier(UnityEditor.Build.NamedBuildTarget.Android, "com.stagegame.prototype");
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.defaultScreenWidth = 720; PlayerSettings.defaultScreenHeight = 1280;
            PlayerSettings.bundleVersion = "0.1.0"; PlayerSettings.Android.bundleVersionCode = 1;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel25;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
            PlayerSettings.WebGL.template = "PROJECT:StageGamePortrait";
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            QualitySettings.vSyncCount = 0;
        }

        public static void BuildAndroid() { AssertTarget(); Validate(); Build(BuildTarget.Android, "Builds/Android/StageGame-development.apk"); }
        public static void BuildWebGL() { AssertTarget(); Validate(); Build(BuildTarget.WebGL, "Builds/WebGL"); }
        public static void BuildWindows() { AssertTarget(); Validate(); Build(BuildTarget.StandaloneWindows64, "Builds/Windows/StageGame.exe"); }
        static void Build(BuildTarget target, string location)
        {
            EnsurePlayerShaders();
            if (target == BuildTarget.WebGL) PlayerSettings.WebGL.template = "PROJECT:StageGamePortrait";
            AssetDatabase.SaveAssets();
            Directory.CreateDirectory(Path.GetDirectoryName(location));
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions { scenes = new[] { ScenePath }, locationPathName = location, target = target, options = BuildOptions.Development });
            File.WriteAllText("Logs/build-" + target + ".txt", $"{report.summary.result}\nErrors: {report.summary.totalErrors}\nWarnings: {report.summary.totalWarnings}\nBytes: {report.summary.totalSize}\nDuration: {report.summary.totalTime}\n");
            if (report.summary.result != BuildResult.Succeeded) throw new InvalidOperationException("Build failed: " + report.summary.result);
            Debug.Log("[StageGame] Build completed: " + report.summary.outputPath);
        }

        static void EnsurePlayerShaders()
        {
            // uGUI obtains these default materials at runtime; an empty project has no included shader list.
            var graphics = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/GraphicsSettings.asset")[0]);
            var included = graphics.FindProperty("m_AlwaysIncludedShaders");
            // GUI/Text Shader is an editor-only transient asset; legacy uGUI text uses UI/Default.
            for (int i = included.arraySize - 1; i >= 0; i--)
            {
                var entry = included.GetArrayElementAtIndex(i);
                if (entry.objectReferenceValue != null && entry.objectReferenceValue.name == "GUI/Text Shader")
                {
                    entry.objectReferenceValue = null;
                    included.DeleteArrayElementAtIndex(i);
                }
            }
            foreach (string shaderName in new[] { "UI/Default", "Sprites/Default" })
            {
                var shader = Shader.Find(shaderName);
                if (shader == null) throw new InvalidOperationException("Missing required built-in shader: " + shaderName);
                bool present = false;
                for (int i = 0; i < included.arraySize; i++) if (included.GetArrayElementAtIndex(i).objectReferenceValue == shader) present = true;
                if (!present) { int index = included.arraySize++; included.GetArrayElementAtIndex(index).objectReferenceValue = shader; }
            }
            graphics.ApplyModifiedPropertiesWithoutUndo();
        }

        static void AssertTarget()
        {
            string expected = Path.GetFullPath("C:/Github/StageGame/Client/Assets").TrimEnd('/', '\\');
            if (!string.Equals(Path.GetFullPath(Application.dataPath).TrimEnd('/', '\\'), expected, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("This tool only runs in C:/Github/StageGame/Client.");
        }

        static GameObject Ensure(string path, Func<GameObject> create)
        {
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null) return existing;
            if (File.Exists(path)) throw new InvalidOperationException("Existing unrecognized asset is protected: " + path);
            var go = create();
            try { return PrefabUtility.SaveAsPrefabAsset(go, path); }
            finally { UnityEngine.Object.DestroyImmediate(go); }
        }
        static GameObject Instance(string path, Transform parent, string name)
        {
            var go = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(path), parent);
            go.name = name; return go;
        }
        static GameObject Screen(string name) => Rect(name, null, 720, 1280);
        static GameObject Modal(string name, string title, string body)
        {
            var go = Panel(name, null, new Color(0.015f, 0.03f, 0.07f, .93f), 720, 1280);
            var frame = Instance(Common + "ModalFrame.prefab", go.transform, "Frame"); Place(frame, 0, 50, 640, 790);
            Get<Text>(frame, "Title").text = title;
            Get<Text>(frame, "Body").text = body;
            if (name == "HelpModal") { Place(frame.transform.Find("Body").gameObject, 0, 0, 535, 420); Get<Text>(frame, "Body").fontSize = 23; }
            return go;
        }
        static void Banner(Transform parent, string name, float y)
        {
            var go = Panel(name, parent, Hex("152237"), 624, 64, 0, y);
            Label("Label", go.transform, "광고 준비 중 · 현재 광고 미연결", 18, Muted, 600, 56);
        }
        static Button Action(Transform parent, string name, string label, float x, float y, float width, float height, bool primary = true)
        {
            var go = Instance(Common + "PrimaryButton.prefab", parent, name); Place(go, x, y, width, height);
            var text = Get<Text>(go, "Label"); text.text = label; text.color = primary ? Ink : White; text.fontSize = width < 150 ? 22 : 26;
            Stretch(text.gameObject); text.rectTransform.offsetMin = new Vector2(8, 3); text.rectTransform.offsetMax = new Vector2(-8, -3);
            go.GetComponent<Image>().color = primary ? Mint : Hex("293D57");
            return go.GetComponent<Button>();
        }
        static GameObject Panel(string name, Transform parent, Color color, float width, float height, float x = 0, float y = 0)
        {
            var go = Rect(name, parent, width, height, x, y); go.AddComponent<Image>().color = color; return go;
        }
        static GameObject Rect(string name, Transform parent, float width, float height, float x = 0, float y = 0)
        {
            var go = new GameObject(name, typeof(RectTransform));
            if (parent != null) go.transform.SetParent(parent, false);
            Place(go, x, y, width, height); return go;
        }
        static void Place(GameObject go, float x, float y, float width, float height)
        {
            var rect = (RectTransform)go.transform; rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(.5f, .5f);
            rect.anchoredPosition = new Vector2(x, y); rect.sizeDelta = new Vector2(width, height); rect.localScale = Vector3.one;
        }
        static void Stretch(GameObject go)
        {
            var rect = (RectTransform)go.transform; rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = rect.offsetMax = Vector2.zero;
        }
        static Text Label(string name, Transform parent, string value, int size, Color color, float width, float height, float x = 0, float y = 0)
        {
            var go = Rect(name, parent, width, height, x, y); var text = go.AddComponent<Text>();
            text.font = font; text.fontSize = size; text.color = color; text.text = value; text.alignment = TextAnchor.MiddleCenter;
            text.horizontalOverflow = HorizontalWrapMode.Wrap; text.verticalOverflow = VerticalWrapMode.Truncate; text.raycastTarget = false;
            return text;
        }
        static T Get<T>(GameObject go, string path) where T : Component => go.transform.Find(path).GetComponent<T>();
        static Color Hex(string value) { ColorUtility.TryParseHtmlString("#" + value, out Color color); return color; }
        static Sprite CreateCircle()
        {
            string path = Root + "Art/Disc.png";
            if (!File.Exists(path))
            {
                var texture = new Texture2D(128, 128, TextureFormat.RGBA32, false);
                for (int y = 0; y < 128; y++) for (int x = 0; x < 128; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x + .5f, y + .5f), new Vector2(64, 64));
                    texture.SetPixel(x, y, new Color(1, 1, 1, Mathf.Clamp01(62.5f - distance)));
                }
                texture.Apply(); File.WriteAllBytes(path, texture.EncodeToPNG()); UnityEngine.Object.DestroyImmediate(texture);
                AssetDatabase.ImportAsset(path);
                var importer = (TextureImporter)AssetImporter.GetAtPath(path); importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single; importer.alphaIsTransparency = true; importer.mipmapEnabled = false; importer.SaveAndReimport();
            }
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        // Reference-only migration for the prototype's missing disc import and safe-area link.
        // Existing hierarchy, layout, colors and text are preserved.
        public static void RepairMissingReferences()
        {
            AssertTarget();
            var importer = (TextureImporter)AssetImporter.GetAtPath(Root + "Art/Disc.png");
            importer.spriteImportMode = SpriteImportMode.Single; importer.SaveAndReimport();
            var disc = AssetDatabase.LoadAssetAtPath<Sprite>(Root + "Art/Disc.png");
            if (disc == null) throw new InvalidOperationException("Disc sprite import failed.");
            foreach (string path in new[] { Root + "Prefabs/Board/Cell.prefab", Screens + "LobbyScreen.prefab", Root + "Prefabs/AppRoot.prefab" })
            {
                var go = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    if (go.name == "Cell") go.GetComponent<Image>().sprite = disc;
                    if (go.name == "LobbyScreen")
                        for (int i = 0; i < 4; i++) Get<Image>(go, "ConnectedStone" + i).sprite = disc;
                    if (go.name == "AppRoot")
                    {
                        var safe = go.transform.Find("SafeArea");
                        safe.GetComponent<SafeAreaFitter>().content = (RectTransform)safe.Find("Content");
                    }
                    PrefabUtility.SaveAsPrefabAsset(go, path);
                }
                finally { PrefabUtility.UnloadPrefabContents(go); }
            }
            AssetDatabase.SaveAssets();
            Validate();
        }
        static void CreateSound()
        {
            string path = Root + "Audio/Move.wav";
            if (File.Exists(path)) return;
            const int rate = 22050, samples = 2205;
            using (var writer = new BinaryWriter(File.Create(path)))
            {
                writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF")); writer.Write(36 + samples * 2); writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt "));
                writer.Write(16); writer.Write((short)1); writer.Write((short)1); writer.Write(rate); writer.Write(rate * 2); writer.Write((short)2); writer.Write((short)16);
                writer.Write(System.Text.Encoding.ASCII.GetBytes("data")); writer.Write(samples * 2);
                for (int i = 0; i < samples; i++) writer.Write((short)(Math.Sin(i * 2 * Math.PI * 660 / rate) * Math.Exp(-i / 380.0) * 6500));
            }
            AssetDatabase.ImportAsset(path);
        }
    }
}
