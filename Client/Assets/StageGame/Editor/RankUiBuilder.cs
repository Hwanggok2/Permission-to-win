using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace StageGame.Editor
{
    // Saves a separate set of prefabs/art. Older Tabletop assets remain intact.
    public static class RankUiBuilder
    {
        public const string Art = ProjectBuilder.Root + "Art/RankUI/";
        public const string Prefabs = ProjectBuilder.Root + "Prefabs/RankUI/";
        public const string AppRootPath = Prefabs + "RankAppRoot.prefab";
        static readonly Color Black = new Color32(29, 30, 32, 255);
        const float HeaderOffset=144; // Minimum top spacing in 720-wide layout units.
        static float U(float value)=>RankUiMetrics.U(value);
        static Font font;
        static RankTheme theme;
        static StringTable table;

        [MenuItem("StageGame/Apply Rank by Rank UI")]
        public static void Apply()
        {
            EnsureEditorReady();
            Directory.CreateDirectory(Art); Directory.CreateDirectory(Prefabs);
            font = AssetDatabase.LoadAssetAtPath<Font>(ProjectBuilder.Root + "Fonts/Pretendard-Regular.ttf");
            table = StringTable.Load(); GenerateArt();
            Save("Button", go => { Size(go.transform, 328, 110); Paint(Add<Image>(go), theme.panel); Add<Button>(go).targetGraphic = go.GetComponent<Image>(); Label(go.transform, "Label", "", 28, 0, 0, 304, 90); });
            Save("CloseButton", go => { Size(go.transform, 110, 110); Paint(Add<Image>(go), theme.panel); Add<Button>(go).targetGraphic = go.GetComponent<Image>(); Picture(go.transform, "Icon", theme.close, 0, 0, 40, 40); });
            Save("StageCard", go =>
            {
                Size(go.transform, 328, 236); Paint(Add<Image>(go), theme.panel); var card=Add<RankStageCard>(go);
                card.button=Add<Button>(go); card.button.targetGraphic=go.GetComponent<Image>();
                card.number=Label(go.transform,"Number","",22,0,81,290,36);
                card.rank=Label(go.transform,"Rank","",30,0,28,294,88); card.rank.fontStyle=FontStyle.Bold;
                card.stars=StarsLabel(go.transform,"Stars",0,-38,280,44);
                card.state=Picture(go.transform,"State",theme.locked,130,-85,24,24);
                ConfigureCardRating(go);
            });
            Save("Board", go => Board(go.transform, true, 704, 704, 720f/7, 720f/7, 68));
            Save("OpeningBoard", go => Board(go.transform, false, 550, 550, 76, 76, 52));
            Save("Lobby", BuildLobby); Save("Detail", BuildDetail); Save("Match", BuildMatch); Save("Result", BuildResult);
            foreach(string screen in new[]{"Settings","Help","Confirm","Notices"}) Save(screen, go=>BuildModal(go,screen));
            Save("ActionConfirm", BuildActionConfirm); Save("TurnToast", BuildTurnToast);
            Save("RankAppRoot", BuildRoot);
            var scene=EditorSceneManager.OpenScene(ProjectBuilder.ScenePath);
            foreach(var legacy in UnityEngine.Object.FindObjectsByType<GameApp>(FindObjectsInactive.Include, FindObjectsSortMode.None)) legacy.gameObject.SetActive(false);
            if(UnityEngine.Object.FindFirstObjectByType<RankGameApp>(FindObjectsInactive.Include)==null) PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(AppRootPath));
            foreach(var camera in UnityEngine.Object.FindObjectsByType<Camera>(FindObjectsSortMode.None)) camera.backgroundColor=Color.white;
            EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets(); LocalizationBuilder.ExportWebStrings(); Validate();
            Debug.Log("[Rank UI] Saved 30 cards, sliding detail, actual opening board, monochrome game and common controls.");
        }

        static void EnsureEditorReady()
        {
            if (!string.Equals(Path.GetFullPath(Application.dataPath), Path.GetFullPath("C:/Github/StageGame/Client/Assets"), StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("StageGame Client only.");
            if (EditorApplication.isPlayingOrWillChangePlaymode || PrefabStageUtility.GetCurrentPrefabStage() != null) throw new InvalidOperationException("Leave Play/prefab mode before applying saved UI.");
            for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
                if (UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).isDirty) throw new InvalidOperationException("Save scene changes first. No assets modified.");
        }
        [MenuItem("StageGame/Apply Match Top Gap")]
        public static void ApplyMatchTopGap()
        {
            EnsureEditorReady();
            Save("Match",go=>
            {
                var scroll=Get<RectTransform>(go,"Scroll");
                scroll.offsetMax=new Vector2(scroll.offsetMax.x,-U(HeaderOffset));
            });
            AssetDatabase.SaveAssets();Validate();
            Debug.Log("[Match top gap] Saved216design units above the match viewport; other prefab geometry preserved.");
        }

        [MenuItem("StageGame/Apply Match Screen Fit")]
        public static void ApplyMatchScreenFit()
        {
            EnsureEditorReady();Save("Match",ConfigureMatchFit);AssetDatabase.SaveAssets();Validate();
            Debug.Log("[Match fit] Saved height fitting below216design-unit top gap;44px minimum column input preserved.");
        }
        static void ConfigureMatchFit(GameObject go)
        {
            var fit=Add<RankMatchFitter>(go);fit.scroll=Get<ScrollRect>(go,"Scroll");
            var scrollRect=(RectTransform)fit.scroll.transform;
            scrollRect.offsetMin=new Vector2(scrollRect.offsetMin.x,U(32));
            fit.minimumTapTarget=Get<RectTransform>(go,"Scroll/Viewport/Content/Board/Column_0");
            fit.minimumTapPixels=44;
        }

        [MenuItem("StageGame/Apply Match Pause Icon")]
        public static void ApplyMatchPauseIcon()
        {
            EnsureEditorReady();
            Save("Match",ConfigureMatchPauseIcon);
            Save("RankAppRoot",go=>ConfigureMatchPauseIcon(go.GetComponent<RankGameApp>().match));
            AssetDatabase.SaveAssets();Validate();
            Debug.Log("[Pause icon] Saved compact square backing behind the large icon; full-area input and icon tint feedback preserved.");
        }
        public static void ConfigureMatchPauseIcon(GameObject go)
        {
            var pause=Get<Button>(go,"Scroll/Viewport/Content/Pause");
            var background=pause.GetComponent<Image>();
            background.sprite=null;background.type=Image.Type.Simple;background.color=Color.clear;background.raycastTarget=true;
            var backing=Rect(pause.transform,"Backdrop",0,0,64,64);
            var backingImage=Add<Image>(backing.gameObject);
            Paint(backingImage,AssetDatabase.LoadAssetAtPath<RankTheme>(Art+"RankTheme.asset").panel);
            backingImage.raycastTarget=false;backing.SetAsFirstSibling();
            var icon=Get<Image>(pause.gameObject,"Icon");
            Size(icon.transform,80,80);icon.preserveAspect=true;icon.raycastTarget=false;
            pause.targetGraphic=icon;
            ConfigureMatchHelp(go);
        }

        [MenuItem("StageGame/Apply Match Help")]
        public static void ApplyMatchHelp()
        {
            EnsureEditorReady();
            Save("Match",ConfigureMatchHelp);
            Save("RankAppRoot",go=>
            {
                var app=go.GetComponent<RankGameApp>();ConfigureMatchHelp(app.match);
                app.matchHelp=Get<Button>(app.match,"Scroll/Viewport/Content/Help");
                app.soundButtons=go.GetComponentsInChildren<Button>(true);
            });
            AssetDatabase.SaveAssets();Validate();
            Debug.Log("[Match help] Saved a left help button aligned with pause, plus its app and sound references.");
        }
        public static void ConfigureMatchHelp(GameObject go)
        {
            var pause=Get<Button>(go,"Scroll/Viewport/Content/Pause");
            var pauseRect=(RectTransform)pause.transform;
            var rect=Rect(pauseRect.parent,"Help",0,0,110,104);
            rect.anchorMin=pauseRect.anchorMin;rect.anchorMax=pauseRect.anchorMax;rect.pivot=pauseRect.pivot;
            rect.sizeDelta=pauseRect.sizeDelta;rect.anchoredPosition=new Vector2(-pauseRect.anchoredPosition.x,pauseRect.anchoredPosition.y);
            var hit=Add<Image>(rect.gameObject);hit.color=Color.clear;hit.raycastTarget=true;
            var theme=AssetDatabase.LoadAssetAtPath<RankTheme>(Art+"RankTheme.asset");
            var backing=Rect(rect,"Backdrop",0,0,64,64);var image=Add<Image>(backing.gameObject);
            Paint(image,theme.panel);image.raycastTarget=false;backing.SetAsFirstSibling();
            var icon=Picture(rect,"Icon",theme.help,0,0,56,56);icon.preserveAspect=true;icon.raycastTarget=false;
            var button=Add<Button>(rect.gameObject);button.targetGraphic=icon;button.colors=pause.colors;
        }

        public const string MusicPath = ProjectBuilder.Root + "Audio/EasyLemon.mp3";
        [MenuItem("StageGame/Apply First Rules And Compact Controls")]
        public static void ApplyFirstRulesAndCompactControls()
        {
            EnsureEditorReady();
            font=AssetDatabase.LoadAssetAtPath<Font>(ProjectBuilder.Root+"Fonts/Pretendard-Regular.ttf");
            Save("Match",ConfigureCompactMatchControls);
            Save("TurnToast",BuildTurnToast);
            Save("RankAppRoot",go=>
            {
                var app=go.GetComponent<RankGameApp>();
                ConfigureCompactMatchControls(app.match);BuildTurnToast(app.turnToast.gameObject);ConfigureMoveGuide(app);
            });
            AssetDatabase.SaveAssets();Validate();
            Debug.Log("[First rules UI] Saved compact backdrops with unchanged hit areas and a centered white placement guide. Existing localized help is reused on first launch.");
        }
        public static void ConfigureCompactMatchControls(GameObject go)
        {
            var theme=AssetDatabase.LoadAssetAtPath<RankTheme>(Art+"RankTheme.asset");
            foreach(string name in new[]{"Undo","Retry"})
            {
                var button=Get<Button>(go,"Scroll/Viewport/Content/"+name);
                var hit=button.GetComponent<Image>();hit.color=Color.clear;hit.raycastTarget=true;
                var backing=Rect(button.transform,"Backdrop",0,0,170,76);backing.SetAsFirstSibling();
                var image=Add<Image>(backing.gameObject);Paint(image,theme.panel);image.color=new Color(1,1,1,.95f);image.raycastTarget=false;
                button.targetGraphic=image;
                Layout(button.transform.Find("Label"),0,0,162,68);
            }
        }
        public static void ConfigureMoveGuide(RankGameApp app)
        {
            var toast=(RectTransform)app.turnToast.transform;toast.SetParent(app.match.transform,false);Layout(toast,0,0,660,170);
            Layout(app.turnToastLabel.transform,0,0,630,145);
            app.turnToastLabel.fontSize=Mathf.RoundToInt(U(34));app.turnToastLabel.color=Color.white;app.turnToastLabel.fontStyle=FontStyle.Bold;
            var outline=Add<Outline>(app.turnToastLabel.gameObject);outline.effectColor=new Color(0,0,0,.9f);outline.effectDistance=new Vector2(2,-2);outline.useGraphicAlpha=true;
            app.turnToast.GetComponent<Image>().enabled=false;app.turnToast.blocksRaycasts=false;app.turnToast.interactable=false;
        }
        [MenuItem("StageGame/Apply Turn Timer")]
        public static void ApplyTurnTimer()
        {
            EnsureEditorReady();
            font=AssetDatabase.LoadAssetAtPath<Font>(ProjectBuilder.Root+"Fonts/Pretendard-Regular.ttf");
            theme=AssetDatabase.LoadAssetAtPath<GameObject>(AppRootPath).GetComponent<RankGameApp>().theme;
            table=StringTable.Load();table.Text("turn_timer_seconds");table.Text("turn_timeout_placed");
            Save("Match",ConfigureTurnTimer);
            Save("RankAppRoot",go=>
            {
                var app=go.GetComponent<RankGameApp>();
                app.turnTimerLabel=Get<Text>(app.match,"Scroll/Viewport/Content/Board/TurnTimer/Label");
                app.turnTimerSlider=Get<Slider>(app.match,"Scroll/Viewport/Content/Board/TurnTimer/Bar");
            });
            AssetDatabase.SaveAssets();Validate();
            Debug.Log("[Turn timer] Saved countdown text and slider in the board's empty upper strip; grid/action geometry preserved.");
        }
        static void ConfigureTurnTimer(GameObject go)
        {
            var content=Get<RectTransform>(go,"Scroll/Viewport/Content");
            var board=content.Find("Board");
            var old=content.Find("TurnTimer");if(old!=null)old.SetParent(board,false);
            var timer=Rect(board,"TurnTimer",0,290,580,60);
            var panel=Add<Image>(timer.gameObject);panel.enabled=false;panel.raycastTarget=false;
            var label=Label(timer,"Label",table.Format("turn_timer_seconds",30),20,0,13,200,30);
            label.raycastTarget=false;label.resizeTextForBestFit=false;
            var bar=Rect(timer,"Bar",0,-13,540,12);
            var background=Add<Image>(bar.gameObject);background.color=new Color32(63,46,28,255);background.raycastTarget=false;
            var inset=Rect(bar,"FillArea",0,0,536,8);
            var fill=Rect(inset,"Fill",0,0,536,8);Stretch(fill);
            var fillImage=Add<Image>(fill.gameObject);fillImage.color=new Color32(76,91,47,255);fillImage.raycastTarget=false;
            var slider=Add<Slider>(bar.gameObject);slider.fillRect=fill;slider.direction=Slider.Direction.LeftToRight;
            slider.minValue=0;slider.maxValue=1;slider.wholeNumbers=false;slider.value=1;slider.interactable=false;
            slider.transition=Selectable.Transition.None;slider.navigation=new Navigation{mode=Navigation.Mode.None};
            timer.SetAsLastSibling();
        }
        public const string StoneSoundPath = ProjectBuilder.Root + "Audio/GoStonePlaceHeavy.wav";
        // Windows AAC priming needs leading silence; originals stay intact beside these PCM-identical copies.
        public const string ButtonSoundPath = ProjectBuilder.Root + "Audio/ButtonClickAacSafe.wav";
        public const string TypingSoundPath = ProjectBuilder.Root + "Audio/TypingKeyAacSafe.wav";
        [MenuItem("StageGame/Apply Ratings and Typing Sound")]
        public static void ApplyRatingsAndTypingSound()
        {
            EnsureEditorReady();
            font=AssetDatabase.LoadAssetAtPath<Font>(ProjectBuilder.Root+"Fonts/Pretendard-Regular.ttf");
            theme=AssetDatabase.LoadAssetAtPath<GameObject>(AppRootPath).GetComponent<RankGameApp>().theme;
            table=StringTable.Load();table.Text("ai_rating");
            var importer=(AudioImporter)AssetImporter.GetAtPath(TypingSoundPath);
            var audio=importer.defaultSampleSettings;audio.loadType=AudioClipLoadType.DecompressOnLoad;audio.compressionFormat=AudioCompressionFormat.PCM;
            importer.defaultSampleSettings=audio;importer.SaveAndReimport();
            Save("StageCard",ConfigureCardRating);
            Save("Match",go=>{ConfigureMatchRating(go);ConfigureTurnTimer(go);});
            Save("RankAppRoot",go=>
            {
                var app=go.GetComponent<RankGameApp>();
                app.aiRating=Get<Text>(app.match,"Scroll/Viewport/Content/AiHeader/Rating");
                app.turnTimerLabel=Get<Text>(app.match,"Scroll/Viewport/Content/Board/TurnTimer/Label");
                app.turnTimerSlider=Get<Slider>(app.match,"Scroll/Viewport/Content/Board/TurnTimer/Bar");
                app.typingSound=AssetDatabase.LoadAssetAtPath<AudioClip>(TypingSoundPath);
            });
            AssetDatabase.SaveAssets();Validate();
            Debug.Log("[Rating/typing] Saved rating labels, dedicated typing clip and board countdown slider.");
        }
        static void ConfigureCardRating(GameObject go)
        {
            var card=go.GetComponent<RankStageCard>();
            card.rating=Label(go.transform,"Rating","",22,-14,-84,240,34);
            card.rating.font=card.number.font;
        }
        internal static void ConfigureMatchRating(GameObject go)
        {
            var header=go.transform.Find("Scroll/Viewport/Content/AiHeader");
            var name=header.Find("AiName").GetComponent<Text>();
            Layout(name.transform,105,30,374,68);
            Layout(header.Find("AiStars"),105,-59,256,42);
            var rating=Label(header,"Rating","",20,105,-22,374,34);rating.font=name.font;
        }
        [MenuItem("StageGame/Apply Stone and Button Sounds")]
        public static void ApplySoundEffects()
        {
            EnsureEditorReady();
            foreach(string path in new[]{StoneSoundPath,ButtonSoundPath,TypingSoundPath})
            {
                var importer=(AudioImporter)AssetImporter.GetAtPath(path);var audio=importer.defaultSampleSettings;
                audio.loadType=AudioClipLoadType.DecompressOnLoad;audio.compressionFormat=AudioCompressionFormat.PCM;
                importer.defaultSampleSettings=audio;importer.SaveAndReimport();
            }
            Save("RankAppRoot",ConfigureSoundEffects);LocalizationBuilder.ExportLegalNotices();AssetDatabase.SaveAssets();Validate();
            Debug.Log("[Sounds] Saved stone impact and UI click clips, with serialized references for every button.");
        }
        static void ConfigureSoundEffects(GameObject go)
        {
            var app=go.GetComponent<RankGameApp>();
            app.moveSound=AssetDatabase.LoadAssetAtPath<AudioClip>(StoneSoundPath);
            app.buttonSound=AssetDatabase.LoadAssetAtPath<AudioClip>(ButtonSoundPath);
            app.typingSound=AssetDatabase.LoadAssetAtPath<AudioClip>(TypingSoundPath);
            app.soundButtons=go.GetComponentsInChildren<Button>(true);
        }
        [MenuItem("StageGame/Apply Background Music")]
        public static void ApplyBackgroundMusic()
        {
            EnsureEditorReady(); table=StringTable.Load();
            // Validate the separately exported Excel keys before changing saved prefabs.
            table.Text("settings_music_on"); table.Text("settings_music_off");
            var importer=(AudioImporter)AssetImporter.GetAtPath(MusicPath);
            var audio=importer.defaultSampleSettings;
            audio.loadType=AudioClipLoadType.CompressedInMemory;audio.compressionFormat=AudioCompressionFormat.Vorbis;audio.quality=.65f;
            importer.defaultSampleSettings=audio;importer.SaveAndReimport();
            Save("Settings",ConfigureMusicSettings);
            Save("RankAppRoot",ConfigureMusicRoot);
            LocalizationBuilder.ExportLegalNotices(); AssetDatabase.SaveAssets(); Validate();
            Debug.Log("[Music] Saved Easy Lemon source at0.30 volume, loop, independent settings and license notices. No player build.");
        }
        static void ConfigureMusicSettings(GameObject go)
        {
            var frame=go.transform.Find("Frame");
            var music=frame.Find("Music");
            if(music==null)
            {
                var copy=UnityEngine.Object.Instantiate(frame.Find("Sound").gameObject,frame);
                copy.name="Music";music=copy.transform;
            }
            Layout(music,0,-250,620,110);
            Get<Text>(go,"Frame/Music/Label").text=table.Text("settings_music_on");
            ((RectTransform)frame.Find("Notices")).anchoredPosition=RankUiMetrics.Point(0,-380);
        }
        static void ConfigureMusicRoot(GameObject go)
        {
            var app=go.GetComponent<RankGameApp>(); ConfigureMusicSettings(app.settings);
            app.musicToggle=Get<Button>(app.settings,"Frame/Music");app.musicLabel=Get<Text>(app.settings,"Frame/Music/Label");
            var music=go.transform.Find("BackgroundMusic");
            if(music==null) { var child=new GameObject("BackgroundMusic");child.transform.SetParent(go.transform,false);music=child.transform; }
            app.musicSource=Add<AudioSource>(music.gameObject);app.musicSource.clip=AssetDatabase.LoadAssetAtPath<AudioClip>(MusicPath);
            app.musicSource.playOnAwake=false;app.musicSource.loop=true;app.musicSource.volume=.30f;app.musicSource.spatialBlend=0;app.musicSource.dopplerLevel=0;
        }

        [Serializable] sealed class AudioLevelContext
        {
            public bool saveExists, soundEnabled, musicMuted;
            public string editorMasterMute;
            public float previousMusicVolume, listenerVolume;
        }
        [MenuItem("StageGame/Apply Audible Audio Levels")]
        public static void ApplyAudioLevels()
        {
            EnsureEditorReady();
            var saved=PlayerPrefs.HasKey("stagegame.progress.v1")?JsonUtility.FromJson<ProgressData>(PlayerPrefs.GetString("stagegame.progress.v1")):new ProgressData();
            var mute=typeof(EditorUtility).GetProperty("audioMasterMute",System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Static);
            var app=AssetDatabase.LoadAssetAtPath<GameObject>(AppRootPath).GetComponent<RankGameApp>();
            var context=new AudioLevelContext {saveExists=PlayerPrefs.HasKey("stagegame.progress.v1"),soundEnabled=saved.soundEnabled,musicMuted=saved.musicMuted,
                editorMasterMute=mute==null?"unavailable":mute.GetValue(null).ToString(),previousMusicVolume=app.musicSource.volume,listenerVolume=AudioListener.volume};
            Directory.CreateDirectory("Evidence/audio-levels-20261015");
            File.WriteAllText("Evidence/audio-levels-20261015/editor-context.json",JsonUtility.ToJson(context,true));
            Save("RankAppRoot",go=>
            {
                var game=go.GetComponent<RankGameApp>();game.musicSource.volume=.30f;
                game.audioSource.volume=1;game.audioSource.spatialBlend=0;
            });
            AssetDatabase.SaveAssets();Validate();
            Debug.Log("[Audio levels] Saved music0.30 and full source gain for stone placement; user ON/OFF settings unchanged.");
        }

        static void BuildLobby(GameObject go)
        {
            const float headerOffset=HeaderOffset;
            Size(go.transform,720,1280);
            Top(Label(go.transform,"Title",table.Text("game_title"),40,0,-55-headerOffset,540,90).rectTransform);Get<Text>(go,"Title").fontStyle=FontStyle.Bold;
            var multi=Button(go.transform,"Multiplayer",0,55+headerOffset,680,110);Bottom((RectTransform)multi.transform);
            var scroll=Rect(go.transform,"Stages",0,0,696,700);Stretch(scroll);scroll.offsetMin=RankUiMetrics.Point(12,125+headerOffset);scroll.offsetMax=RankUiMetrics.Point(-12,-145-headerOffset);
            var viewport=Rect(scroll,"Viewport",0,0,696,700);Stretch(viewport);Add<RectMask2D>(viewport.gameObject);
            var hit=Add<Image>(viewport.gameObject);hit.color=new Color(1,1,1,.005f);hit.raycastTarget=true;
            var content=Rect(viewport,"Content",0,0,696,15*252);Top(content);content.pivot=new Vector2(.5f,1);content.anchoredPosition=Vector2.zero;
            for(int i=0;i<30;i++){var card=Instance("StageCard",content,"Stage_"+i);Layout(card,(i%2==0?-174:174),-118-(i/2)*252,328,236);Top(card);}
            var list=Add<ScrollRect>(scroll.gameObject);list.viewport=viewport;list.content=content;list.horizontal=false;list.vertical=true;list.movementType=ScrollRect.MovementType.Clamped;list.scrollSensitivity=U(44);list.inertia=true;
            IconButton(go.transform,"Help",theme.help,-290,-55-headerOffset);Top(Get<RectTransform>(go,"Help"));IconButton(go.transform,"Settings",theme.settings,290,-55-headerOffset);Top(Get<RectTransform>(go,"Settings"));
            Top(Label(go.transform,"Notice","",20,0,-128-headerOffset,680,28).rectTransform);
            ConfigureLobbyLayout(go);
        }
        [MenuItem("StageGame/Apply Compact Lobby")]
        public static void ApplyCompactLobby()
        {
            EnsureEditorReady();
            Save("Lobby",ConfigureLobbyLayout);
            Save("RankAppRoot",go=>ConfigureLobbyLayout(go.GetComponent<RankGameApp>().lobby));
            AssetDatabase.SaveAssets();Validate();
            Debug.Log("[Lobby layout] Saved two-and-a-half-row list with help/settings above it; short viewports retain scrolling.");
        }
        static void ConfigureLobbyLayout(GameObject go)
        {
            var fit=Add<RankLobbyFitter>(go);
            fit.stages=Get<RectTransform>(go,"Stages");fit.help=Get<RectTransform>(go,"Help");
            fit.settings=Get<RectTransform>(go,"Settings");fit.notice=Get<RectTransform>(go,"Notice");
            fit.maximumListHeight=U(2*252+236*.5f);fit.minimumTopInset=U(145+HeaderOffset);
            fit.buttonAboveList=U(90);fit.noticeAboveList=U(17);
            fit.Fit();
        }
        [MenuItem("StageGame/Apply Centered Detail")]
        public static void ApplyCenteredDetail()
        {
            EnsureEditorReady();
            Save("Detail",ConfigureDetailLayout);
            Save("RankAppRoot",go=>ConfigureDetailLayout(go.GetComponent<RankGameApp>().detail.gameObject));
            AssetDatabase.SaveAssets();Validate();
            Debug.Log("[Detail layout] Saved centered header/body and a two-line description area; slide root and other screens preserved.");
        }
        static void ConfigureDetailLayout(GameObject go)
        {
            var fit=Add<RankDetailFitter>(go);fit.scroll=Get<ScrollRect>(go,"Scroll");
            fit.number=Get<RectTransform>(go,"Number");fit.close=Get<RectTransform>(go,"CloseButton");
            fit.headerHeight=U(120);fit.headerCenter=U(55);fit.minimumVerticalGap=U(32);
            fit.scroll.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,U(1005));
            var description=Get<Text>(go,"Scroll/Viewport/Content/Description");
            Layout(description.transform,0,-745,680,100);Top(description.rectTransform);
            description.horizontalOverflow=HorizontalWrapMode.Wrap;description.resizeTextForBestFit=false;
            var state=Get<RectTransform>(go,"Scroll/Viewport/Content/State");state.anchoredPosition=RankUiMetrics.Point(0,-830);
            var play=Get<RectTransform>(go,"Scroll/Viewport/Content/Play");play.anchoredPosition=RankUiMetrics.Point(0,-930);
            fit.Fit();
        }
        static void BuildDetail(GameObject go)
        {
            Size(go.transform,720,1280);var background=Add<Image>(go);background.color=Color.white;background.raycastTarget=true;
            var content=ScrollContent(go.transform,985,120);
            MoveKnown(go.transform,content,new[]{"Rank","Stars","OpeningBoard","Previous","Following","Description","State","Play"});
            Close(go.transform,290,-55);Top(Get<RectTransform>(go,"CloseButton"));
            Top(Label(go.transform,"Number","",24,0,-55,490,44).rectTransform);
            Top(Label(content,"Rank","",44,0,-54,660,90).rectTransform);
            Top(StarsLabel(content,"Stars",0,-112,300,48).rectTransform);
            var board=Instance("OpeningBoard",content,"OpeningBoard");Layout(board,0,-410,550,550);Top(board);
            IconButton(content,"Previous",theme.left,-300,-410);Top((RectTransform)content.Find("Previous"));IconButton(content,"Following",theme.right,300,-410);Top((RectTransform)content.Find("Following"));
            Top(Label(content,"Description","",26,0,-735,680,70).rectTransform);
            Top(Label(content,"State","",24,0,-810,650,50).rectTransform);
            Top((RectTransform)Button(content,"Play",0,-910,680,110,true).transform);
            ConfigureDetailLayout(go);
        }
        static void BuildMatch(GameObject go)
        {
            Size(go.transform,720,1280);var content=ScrollContent(go.transform,1170,HeaderOffset);
            MoveKnown(go.transform,content,new[]{"Title","Pause","AiAvatar","AiName","AiStars","SpeechBubble","Turn","Board","NormalBoardSlot","ResultBoardSlot","PlayerAvatar","MoveCount","Notice","Undo","Place","Retry"});
            Top(Label(content,"Title",table.Text("game_title"),40,0,-55,540,90).rectTransform);content.Find("Title").GetComponent<Text>().fontStyle=FontStyle.Bold;
            IconButton(content,"Pause",theme.pause,290,-55);Top((RectTransform)content.Find("Pause"));
            ConfigureMatchPauseIcon(go);
            var header=Rect(content,"AiHeader",0,-175,680,110);Top(header);
            MoveKnown(content,header,new[]{"AiAvatar","AiName","AiStars"});
            Picture(header,"AiAvatar",theme.aiAvatar,-225,0,92,92);
            Picture(header,"RankHat",theme.rankHats[0],-225,40,160,96);
            Label(header,"AiName","",28,0,0,360,100);
            StarsLabel(header,"AiStars",230,0,256,50);
            var bubble=Rect(content,"SpeechBubble",0,-281,680,104);Top(bubble);Paint(Add<Image>(bubble.gameObject),theme.panel);bubble.GetComponent<Image>().raycastTarget=false;
            Label(bubble,"Speech","",26,0,0,642,96);
            Label(content,"Turn","",26,0,0,680,46).gameObject.SetActive(false);
            var board=Instance("Board",content,"Board");Layout(board,0,-703,704,704);Top(board);
            Top(Rect(content,"NormalBoardSlot",0,-703,704,704));var resultSlot=Rect(content,"ResultBoardSlot",0,-703,704,704);Top(resultSlot);resultSlot.localScale=Vector3.one;
            Picture(content,"PlayerAvatar",theme.playerAvatar,0,0,56,56).gameObject.SetActive(false);
            Label(content,"MoveCount","",20,0,0,154,35).gameObject.SetActive(false);
            Top(Label(content,"Notice","",24,0,-1025,680,44).rectTransform);
            var undo=Button(content,"Undo",-243,-1110,224,110);Top((RectTransform)undo.transform);undo.GetComponent<Image>().color=Color.clear;
            var place=Button(content,"Place",0,-1110,110,110,true);Top((RectTransform)place.transform);Paint(place.GetComponent<Image>(),theme.roundPrimary,false);place.GetComponentInChildren<Text>().fontSize=Mathf.RoundToInt(U(26));
            var retry=Button(content,"Retry",243,-1110,224,110);Top((RectTransform)retry.transform);retry.GetComponent<Image>().color=Color.clear;
            ConfigureMatchFit(go);
            ConfigureTurnTimer(go);
            ConfigureMatchRating(go);
            ConfigureCompactMatchControls(go);
        }
        static void BuildResult(GameObject go)
        {
            Size(go.transform,720,1385);
            Top(Label(go.transform,"Title","",30,0,-1100,680,46).rectTransform);
            Top((RectTransform)Button(go.transform,"Next",0,-1185,680,110,true).transform);
            Top((RectTransform)Button(go.transform,"Retry",-174,-1310,328,110).transform);
            Top((RectTransform)Button(go.transform,"Lobby",174,-1310,328,110).transform);
            Top(Label(go.transform,"AdStatus","",20,0,-1080,680,34).rectTransform);
        }
        static void BuildActionConfirm(GameObject go)
        {
            Size(go.transform,720,1280);var overlay=Add<Image>(go);overlay.color=new Color(0,0,0,.2f);overlay.raycastTarget=true;
            var frame=Rect(go.transform,"Frame",0,0,680,430);Paint(Add<Image>(frame.gameObject),theme.panel);
            Label(frame,"Question","",36,0,100,610,100);Label(frame,"Body","",26,0,0,610,76);
            Button(frame,"Cancel",-160,-110,300,110);Button(frame,"Accept",160,-110,300,110,true);
        }
        static void BuildTurnToast(GameObject go)
        {
            Size(go.transform,660,170);var panel=Add<Image>(go);panel.enabled=false;panel.raycastTarget=false;
            var group=Add<CanvasGroup>(go);group.alpha=0;group.blocksRaycasts=false;group.interactable=false;
            var label=Label(go.transform,"Label","",34,0,0,630,145);label.color=Color.white;label.fontStyle=FontStyle.Bold;
            var outline=Add<Outline>(label.gameObject);outline.effectColor=new Color(0,0,0,.9f);outline.effectDistance=new Vector2(2,-2);outline.useGraphicAlpha=true;
        }
        static RectTransform ScrollContent(Transform root,float height,float top)
        {
            var scroll=Rect(root,"Scroll",0,0,720,1280);Stretch(scroll);scroll.offsetMax=RankUiMetrics.Point(0,-top);
            var viewport=Rect(scroll,"Viewport",0,0,720,1280);Stretch(viewport);Add<RectMask2D>(viewport.gameObject);
            var hit=Add<Image>(viewport.gameObject);hit.color=new Color(1,1,1,.005f);hit.raycastTarget=true;
            var content=Rect(viewport,"Content",0,0,720,height);Top(content);content.pivot=new Vector2(.5f,1);content.anchoredPosition=Vector2.zero;
            var view=Add<ScrollRect>(scroll.gameObject);view.viewport=viewport;view.content=content;view.horizontal=false;view.vertical=true;view.movementType=ScrollRect.MovementType.Clamped;view.scrollSensitivity=U(44);
            return content;
        }
        static void MoveKnown(Transform oldParent,Transform parent,string[] names)
        {foreach(string name in names){var existing=oldParent.Find(name);if(existing!=null&&parent.Find(name)==null)existing.SetParent(parent,false);}}
        static void Top(RectTransform rect){rect.anchorMin=rect.anchorMax=new Vector2(.5f,1);}
        static void Bottom(RectTransform rect){rect.anchorMin=rect.anchorMax=new Vector2(.5f,0);}
        static void BuildModal(GameObject go,string kind)
        {
            Size(go.transform,720,1280); var overlay=Add<Image>(go); overlay.color=new Color(0,0,0,.12f); overlay.raycastTarget=true;
            var frame=Rect(go.transform,"Frame",0,0,680,900); Paint(Add<Image>(frame.gameObject),theme.panel);
            Close(frame,280,380); Label(frame,"Title","",36,0,378,450,78);
            if(kind=="Settings")
            {
                Label(frame,"Body","",26,0,240,600,78); Label(frame,"Language","",28,0,125,600,50);
                Button(frame,"Korean",-160,20,300,110); Button(frame,"English",160,20,300,110);
                Button(frame,"Sound",0,-120,620,110);
                var legal=Button(frame,"Notices",0,-300,620,110); legal.GetComponent<Image>().color=Color.clear;
                ConfigureMusicSettings(go);
            }
            else if(kind=="Confirm")
            {
                Label(frame,"Body","",28,0,205,590,110);
                Button(frame,"Hint",0,30,620,110); Button(frame,"Resume",0,-120,620,110,true); Button(frame,"Leave",0,-270,620,110);
            }
            else if(kind=="Help")
            {
                Label(frame,"Body","",27,0,35,600,570).gameObject.SetActive(false);
                Label(frame,"DropTitle","",28,0,268,610,55);
                var example=Rect(frame,"Gravity",-192,123,180,190);
                for(int c=0;c<3;c++) {var line=Rect(example,"V"+c,(c-1)*60,0,3,150);var im=Add<Image>(line.gameObject);im.color=Black;im.raycastTarget=false;}
                for(int r=0;r<4;r++) {var line=Rect(example,"H"+r,0,(r-1.5f)*50,120,3);var im=Add<Image>(line.gameObject);im.color=Black;im.raycastTarget=false;}
                Picture(example,"Support",theme.whiteStone,0,-75,42,42);
                Picture(example,"Preview",theme.preview,0,-25,42,42);
                Picture(example,"Arrow",theme.right,0,75,26,26).rectTransform.localEulerAngles=new Vector3(0,0,-90);
                Label(frame,"DropCaption","",25,110,130,360,190);
                var place=Rect(frame,"PlaceExample",-192,-5,170,64);Paint(Add<Image>(place.gameObject),theme.primary);
                Label(place,"Label","",26,0,0,158,50).color=Color.white;
                Label(frame,"ConnectTitle","",28,0,-110,610,55);
                for(int exampleIndex=0;exampleIndex<3;exampleIndex++)
                {
                    var diagram=Rect(frame,"Connection_"+exampleIndex,(exampleIndex-1)*206,-238,190,170);
                    for(int i=0;i<4;i++) {float offset=(i-1.5f)*42;Picture(diagram,"Stone_"+i,theme.blackStone,exampleIndex==1?0:offset,exampleIndex==0?0:offset,34,34);}
                }
                Label(frame,"ConnectCaption","",28,0,-365,610,55);
            }
            else if(kind=="Notices")
            {
                var scroll=Rect(frame,"Scroll",0,-40,604,690); var viewport=Rect(scroll,"Viewport",0,0,604,690); Add<RectMask2D>(viewport.gameObject);
                var hit=Add<Image>(viewport.gameObject); hit.color=new Color(1,1,1,.005f); hit.raycastTarget=true;
                var content=Label(viewport,"Content","",30,0,0,580,0); content.alignment=TextAnchor.UpperLeft;
                var rect=content.rectTransform; rect.anchorMin=rect.anchorMax=new Vector2(.5f,1); rect.pivot=new Vector2(.5f,1); rect.anchoredPosition=Vector2.zero;
                Add<ContentSizeFitter>(content.gameObject).verticalFit=ContentSizeFitter.FitMode.PreferredSize;
                var view=Add<ScrollRect>(scroll.gameObject); view.viewport=viewport; view.content=rect; view.horizontal=false; view.movementType=ScrollRect.MovementType.Clamped; view.scrollSensitivity=U(42);
            }
        }
        static void Board(Transform root,bool interactive,float width,float height,float dx,float dy,float stoneSize)
        {
            Size(root,width,height);
            for(int c=0;c<7;c++) { var line=Rect(root,"Vertical_"+c,(c-3)*dx,0,3,5*dy); var im=Add<Image>(line.gameObject); im.color=Black; im.raycastTarget=false; }
            for(int r=0;r<6;r++) { var line=Rect(root,"Horizontal_"+r,0,(r-2.5f)*dy,6*dx,3); var im=Add<Image>(line.gameObject); im.color=Black; im.raycastTarget=false; }
            var vertical=new List<RectTransform>();var horizontal=new List<RectTransform>();
            for(int c=0;c<7;c++)vertical.Add((RectTransform)root.Find("Vertical_"+c));
            for(int r=0;r<6;r++)horizontal.Add((RectTransform)root.Find("Horizontal_"+r));
            foreach(int sign in new[]{-1,1})
            {
                var v=Rect(root,"FrameV"+sign,sign*width/2,0,3,height);var vi=Add<Image>(v.gameObject);vi.color=Black;vi.raycastTarget=false;vertical.Add(v);
                var h=Rect(root,"FrameH"+sign,0,sign*height/2,width,3);var hi=Add<Image>(h.gameObject);hi.color=Black;hi.raycastTarget=false;horizontal.Add(h);
            }
            var grid=Add<UniformBoardLines>(root.gameObject);grid.referenceThickness=U(3);grid.vertical=vertical.ToArray();grid.horizontal=horizontal.ToArray();
            if(interactive) { var line=Rect(root,"WinLine",0,0,1,5); var im=Add<Image>(line.gameObject); im.color=Black; im.raycastTarget=false; line.gameObject.SetActive(false); }
            for(int r=0;r<6;r++) for(int c=0;c<7;c++) Picture(root,"Stone_"+(r*7+c),theme.blackStone,(c-3)*dx,(r-2.5f)*dy,stoneSize,stoneSize).enabled=false;
            if(!interactive) return;
            for(int c=0;c<7;c++)
            {
                var rect=Rect(root,"Column_"+c,(c-3)*dx,0,dx,height); var image=Add<Image>(rect.gameObject); image.color=Color.clear;
                var button=Add<Button>(rect.gameObject); button.targetGraphic=image; button.transition=Selectable.Transition.None;
            }
            Picture(root,"Preview",theme.preview,0,0,stoneSize,stoneSize).gameObject.SetActive(false);
        }
        static void BuildRoot(GameObject go)
        {
            var canvas=Add<Canvas>(go); canvas.renderMode=RenderMode.ScreenSpaceOverlay;
            var scaler=Add<CanvasScaler>(go); scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution=RankUiMetrics.Resolution; scaler.screenMatchMode=CanvasScaler.ScreenMatchMode.Expand;
            Add<GraphicRaycaster>(go); var backdrop=Rect(go.transform,"Backdrop",0,0,720,1280); Stretch(backdrop); Add<Image>(backdrop.gameObject).color=Color.white;
            var safe=Rect(go.transform,"SafeArea",0,0,720,1280); Stretch(safe); var content=Rect(safe,"Content",0,0,720,1280);
            var fitter=Add<SafeAreaFitter>(safe.gameObject); fitter.content=content; fitter.referenceSize=RankUiMetrics.Resolution; fitter.widthFitTopAligned=true; fitter.topPaddingCss=8;
            var app=Add<RankGameApp>(go); app.theme=theme;
            app.lobby=Instance("Lobby",content,"Lobby").gameObject; app.match=Instance("Match",content,"Match").gameObject;
            Stretch((RectTransform)app.lobby.transform);Stretch((RectTransform)app.match.transform);
            var matchContent=Get<RectTransform>(app.match,"Scroll/Viewport/Content").gameObject;app.matchScroll=Get<ScrollRect>(app.match,"Scroll");
            // Detail's stationary blocker covers the lobby while its saved content slides.
            var detailBlocker=Rect(content,"DetailBlocker",0,0,720,1280);Stretch(detailBlocker); var blocker=Add<Image>(detailBlocker.gameObject); blocker.color=new Color(1,1,1,.005f); blocker.raycastTarget=true;
            app.detail=Instance("Detail",detailBlocker,"Detail");Stretch(app.detail);
            ConfigureDetailLayout(app.detail.gameObject);
            var detailContent=Get<RectTransform>(app.detail.gameObject,"Scroll/Viewport/Content").gameObject;app.detailScroll=Get<ScrollRect>(app.detail.gameObject,"Scroll");
            // Blocker is parented to the sliding content's activation with a tiny saved relay.
            var guard=Add<RankDetailBlocker>(app.detail.gameObject); guard.blocker=blocker;
            MoveKnown(content,matchContent.transform,new[]{"Result"});app.result=Instance("Result",matchContent.transform,"Result").gameObject;var resultRect=(RectTransform)app.result.transform;Top(resultRect);resultRect.pivot=new Vector2(.5f,1);resultRect.anchoredPosition=Vector2.zero;
            app.actionConfirm=Instance("ActionConfirm",content,"ActionConfirm").gameObject;Stretch((RectTransform)app.actionConfirm.transform);
            app.turnToast=Instance("TurnToast",content,"TurnToast").GetComponent<CanvasGroup>();app.turnToastLabel=app.turnToast.transform.Find("Label").GetComponent<Text>();
            ConfigureMoveGuide(app);
            app.settings=Instance("Settings",content,"Settings").gameObject; app.help=Instance("Help",content,"Help").gameObject;
            app.confirm=Instance("Confirm",content,"Confirm").gameObject; app.notices=Instance("Notices",content,"Notices").gameObject;
            foreach(var modal in new[]{app.settings,app.help,app.confirm,app.notices})Stretch((RectTransform)modal.transform);app.actionConfirm.transform.SetAsLastSibling();
            app.stageScroll=Get<ScrollRect>(app.lobby,"Stages"); app.cards=new RankStageCard[30];
            for(int i=0;i<30;i++)app.cards[i]=Get<RankStageCard>(app.lobby,"Stages/Viewport/Content/Stage_"+i);
            app.multiplayer=Get<Button>(app.lobby,"Multiplayer"); app.multiplayerLabel=Get<Text>(app.lobby,"Multiplayer/Label");
            app.lobbySettings=Get<Button>(app.lobby,"Settings"); app.lobbyHelp=Get<Button>(app.lobby,"Help"); app.lobbyTitle=Get<Text>(app.lobby,"Title"); app.lobbyNotice=Get<Text>(app.lobby,"Notice");
            app.detailClose=Get<Button>(app.detail.gameObject,"CloseButton"); app.previous=Get<Button>(detailContent,"Previous"); app.following=Get<Button>(detailContent,"Following"); app.play=Get<Button>(detailContent,"Play");
            app.detailRank=Get<Text>(detailContent,"Rank"); app.detailNumber=Get<Text>(app.detail.gameObject,"Number"); app.detailStars=Get<Text>(detailContent,"Stars"); app.detailDescription=Get<Text>(detailContent,"Description"); app.detailState=Get<Text>(detailContent,"State"); app.playLabel=Get<Text>(detailContent,"Play/Label");
            app.stones=new Image[42]; app.detailStones=new Image[42]; app.columns=new Button[7];
            for(int i=0;i<42;i++) { app.stones[i]=Get<Image>(matchContent,"Board/Stone_"+i); app.detailStones[i]=Get<Image>(detailContent,"OpeningBoard/Stone_"+i); }
            for(int i=0;i<7;i++)app.columns[i]=Get<Button>(matchContent,"Board/Column_"+i);
            app.boardView=Get<RectTransform>(matchContent,"Board"); app.normalBoardSlot=Get<RectTransform>(matchContent,"NormalBoardSlot"); app.resultBoardSlot=Get<RectTransform>(matchContent,"ResultBoardSlot");
            app.movePreview=Get<Image>(matchContent,"Board/Preview"); app.winLine=Get<Image>(matchContent,"Board/WinLine"); app.playerAvatar=Get<Image>(matchContent,"PlayerAvatar");
            app.matchTitle=Get<Text>(matchContent,"Title"); app.aiName=Get<Text>(matchContent,"AiHeader/AiName"); app.aiStars=Get<Text>(matchContent,"AiHeader/AiStars"); app.aiSpeech=Get<Text>(matchContent,"SpeechBubble/Speech"); app.turnLabel=Get<Text>(matchContent,"Turn"); app.moveLabel=Get<Text>(matchContent,"MoveCount"); app.matchNotice=Get<Text>(matchContent,"Notice");
            app.turnTimerLabel=Get<Text>(matchContent,"Board/TurnTimer/Label");
            app.turnTimerSlider=Get<Slider>(matchContent,"Board/TurnTimer/Bar");
            app.aiRating=Get<Text>(matchContent,"AiHeader/Rating");
            app.pause=Get<Button>(matchContent,"Pause"); app.matchHelp=Get<Button>(matchContent,"Help"); app.undo=Get<Button>(matchContent,"Undo"); app.place=Get<Button>(matchContent,"Place"); app.matchRetry=Get<Button>(matchContent,"Retry");
            app.retry=Get<Button>(app.result,"Retry"); app.nextStage=Get<Button>(app.result,"Next"); app.resultLobby=Get<Button>(app.result,"Lobby"); app.resultTitle=Get<Text>(app.result,"Title"); app.resultAdStatus=Get<Text>(app.result,"AdStatus");
            app.settingsClose=Get<Button>(app.settings,"Frame/CloseButton"); app.helpClose=Get<Button>(app.help,"Frame/CloseButton"); app.confirmClose=Get<Button>(app.confirm,"Frame/CloseButton"); app.noticesClose=Get<Button>(app.notices,"Frame/CloseButton");
            app.confirmLeave=Get<Button>(app.confirm,"Frame/Leave"); app.confirmResume=Get<Button>(app.confirm,"Frame/Resume"); app.hint=Get<Button>(app.confirm,"Frame/Hint"); app.hintLabel=Get<Text>(app.confirm,"Frame/Hint/Label");
            app.soundToggle=Get<Button>(app.settings,"Frame/Sound"); app.korean=Get<Button>(app.settings,"Frame/Korean"); app.english=Get<Button>(app.settings,"Frame/English"); app.noticesOpen=Get<Button>(app.settings,"Frame/Notices");
            app.soundLabel=Get<Text>(app.settings,"Frame/Sound/Label"); app.koreanLabel=Get<Text>(app.settings,"Frame/Korean/Label"); app.englishLabel=Get<Text>(app.settings,"Frame/English/Label"); app.settingsDetail=Get<Text>(app.settings,"Frame/Body"); app.noticesBody=Get<Text>(app.notices,"Frame/Scroll/Viewport/Content");
            app.aiFace=Get<Image>(matchContent,"AiHeader/AiAvatar");app.rankHat=Get<Image>(matchContent,"AiHeader/RankHat");
            app.actionCancel=Get<Button>(app.actionConfirm,"Frame/Cancel");app.actionAccept=Get<Button>(app.actionConfirm,"Frame/Accept");app.actionQuestion=Get<Text>(app.actionConfirm,"Frame/Question");
            app.actionBody=Get<Text>(app.actionConfirm,"Frame/Body");app.actionAcceptLabel=Get<Text>(app.actionConfirm,"Frame/Accept/Label");
            app.audioSource=Add<AudioSource>(go); app.audioSource.playOnAwake=false;
            ConfigureMusicRoot(go);
            ConfigureSoundEffects(go);
            var labels=new List<LocalizedLabel>();
            void Bind(GameObject screen,string path,string key) { var label=Get<Text>(screen,path); label.text=table.Text(key); labels.Add(new LocalizedLabel{target=label,key=key}); }
            Bind(app.lobby,"Help/Label","client_ui_help"); Bind(app.lobby,"Settings/Label","ui_settings");
            Bind(matchContent,"Undo/Label","undo"); Bind(matchContent,"Place/Label","place"); Bind(matchContent,"Retry/Label","retry");
            Bind(app.result,"Next/Label","nextstage"); Bind(app.result,"Retry/Label","retry"); Bind(app.result,"Lobby/Label","ui_lobby");
            Bind(app.settings,"Frame/Title","ui_settings"); Bind(app.settings,"Frame/Language","settings_language"); Bind(app.settings,"Frame/Notices/Label","legal_title");
            Bind(app.help,"Frame/Title","client_ui_help"); Bind(app.help,"Frame/Body","client_help_rules");
            Bind(app.help,"Frame/DropTitle","help_drop_title"); Bind(app.help,"Frame/DropCaption","help_drop_caption"); Bind(app.help,"Frame/PlaceExample/Label","place"); Bind(app.help,"Frame/ConnectTitle","help_connect_title"); Bind(app.help,"Frame/ConnectCaption","help_connect_caption");
            Bind(app.confirm,"Frame/Title","confirm_title"); Bind(app.confirm,"Frame/Body","confirm_body"); Bind(app.confirm,"Frame/Resume/Label","client_ui_continue"); Bind(app.confirm,"Frame/Leave/Label","confirm_leave"); Bind(app.notices,"Frame/Title","legal_title");
            Bind(app.actionConfirm,"Frame/Cancel/Label","ui_cancel");
            app.localizedLabels=labels.ToArray();
            foreach(var screen in new[]{app.match,app.result,app.settings,app.help,app.confirm,app.notices,app.detail.gameObject,app.actionConfirm,app.turnToast.gameObject})screen.SetActive(false);
            blocker.enabled=false; app.lobby.SetActive(true);
        }
        public static void Validate()
        {
            var app=AssetDatabase.LoadAssetAtPath<GameObject>(AppRootPath)?.GetComponent<RankGameApp>(); if(app==null)throw new InvalidOperationException("Missing Rank UI root.");
            var data=GameData.Load(); data.ValidateRankData();
            foreach(var target in new UnityEngine.Object[]{app,app.theme})
            {
                var iterator=new SerializedObject(target).GetIterator();
                while(iterator.NextVisible(true)) if(iterator.propertyType==SerializedPropertyType.ObjectReference && iterator.objectReferenceValue==null)throw new InvalidOperationException("Missing Rank reference: "+iterator.propertyPath);
            }
            if(app.cards.Length!=30||app.stones.Length!=42||app.detailStones.Length!=42||app.columns.Length!=7)throw new InvalidOperationException("Rank UI dimensions invalid.");
            foreach(var card in app.cards) if(card.rank==null||card.stars==null||card.number==null||card.button==null||card.state==null||card.rating==null)throw new InvalidOperationException("Incomplete stage card.");
            foreach(var stars in app.GetComponentsInChildren<RankStarRating>(true))
                if(stars.filledStars.Length!=8||Array.Exists(stars.filledStars,image=>image==null))throw new InvalidOperationException("Incomplete saved star rating.");
            foreach(var binding in app.localizedLabels)if(!data.Strings.Contains(binding.key))throw new InvalidOperationException("Missing UI string: "+binding.key);
            Debug.Log("[Rank UI] Saved references,30 cards and42 board/preview intersections validated.");
        }

        static void Save(string name,Action<GameObject> build)
        {
            string path=Prefabs+name+".prefab"; bool exists=File.Exists(path);
            var go=exists?PrefabUtility.LoadPrefabContents(path):new GameObject(name,typeof(RectTransform));
            try { build(go); PrefabUtility.SaveAsPrefabAsset(go,path); }
            finally { if(exists)PrefabUtility.UnloadPrefabContents(go);else UnityEngine.Object.DestroyImmediate(go); }
        }
        static RectTransform Instance(string prefab,Transform parent,string name)
        {
            var old=parent.Find(name);if(old!=null)return (RectTransform)old;
            var go=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Prefabs+prefab+".prefab"),parent);go.name=name;return (RectTransform)go.transform;
        }
        static Button Button(Transform parent,string name,float x,float y,float w,float h,bool primary=false)
        {
            var rect=Instance("Button",parent,name);Layout(rect,x,y,w,h);Paint(rect.GetComponent<Image>(),primary?theme.primary:theme.panel);
            var text=rect.Find("Label").GetComponent<Text>();Layout(text.transform,0,0,w-24,h-20);text.color=primary?Color.white:Black;
            var button=rect.GetComponent<Button>();var colors=button.colors;colors.normalColor=colors.highlightedColor=colors.selectedColor=Color.white;colors.pressedColor=new Color(.85f,.85f,.85f);colors.disabledColor=new Color(.75f,.75f,.75f,.65f);button.colors=colors;return button;
        }
        static void Close(Transform parent,float x,float y) { var close=Instance("CloseButton",parent,"CloseButton");Layout(close,x,y,110,110); }
        static void IconButton(Transform parent,string name,Sprite icon,float x,float y) { var rect=Button(parent,name,x,y,110,110).transform;rect.Find("Label").gameObject.SetActive(false);Picture(rect,"Icon",icon,0,0,40,40); }
        static T Add<T>(GameObject go) where T:Component { var component=go.GetComponent<T>(); return component!=null?component:go.AddComponent<T>(); }
        static T Get<T>(GameObject go,string path) where T:Component => go.transform.Find(path).GetComponent<T>();
        static RectTransform Rect(Transform parent,string name,float x,float y,float w,float h) { var old=parent.Find(name);var rect=old!=null?(RectTransform)old:new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();if(old==null)rect.SetParent(parent,false);Layout(rect,x,y,w,h);return rect; }
        static void Layout(Transform target,float x,float y,float w,float h) { var rect=(RectTransform)target;rect.anchorMin=rect.anchorMax=rect.pivot=Vector2.one*.5f;rect.anchoredPosition=RankUiMetrics.Point(x,y);rect.sizeDelta=RankUiMetrics.Point(w,h); }
        static void Size(Transform target,float w,float h)=>((RectTransform)target).sizeDelta=RankUiMetrics.Point(w,h);
        static void Stretch(RectTransform rect) { rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=rect.offsetMax=Vector2.zero; }
        static Text Label(Transform parent,string name,string value,int size,float x,float y,float w,float h) { var rect=Rect(parent,name,x,y,w,h);var label=Add<Text>(rect.gameObject);label.font=font;label.fontSize=Mathf.RoundToInt(U(size));label.text=value;label.color=Black;label.alignment=TextAnchor.MiddleCenter;label.raycastTarget=false;label.horizontalOverflow=HorizontalWrapMode.Wrap;label.verticalOverflow=VerticalWrapMode.Truncate;return label; }
        static Text StarsLabel(Transform parent,string name,float x,float y,float w,float h)
        {
            var label=Label(parent,name,"",28,x,y,w,h);label.enabled=false;
            var rating=Add<RankStarRating>(label.gameObject);rating.filledStars=new Image[8];
            float step=w/8,size=Mathf.Min(h-8,step-3);
            for(int i=0;i<8;i++)
            {
                var empty=Picture(label.transform,"Star_"+i,theme.star,(i-3.5f)*step,0,size,size);empty.color=new Color(1,1,1,.2f);
                var filled=Picture(empty.transform,"Filled",theme.star,0,0,size,size);filled.type=Image.Type.Filled;filled.fillMethod=Image.FillMethod.Horizontal;filled.fillOrigin=0;filled.fillAmount=0;
                rating.filledStars[i]=filled;
            }
            return label;
        }
        static Image Picture(Transform parent,string name,Sprite sprite,float x,float y,float w,float h) { var rect=Rect(parent,name,x,y,w,h);var image=Add<Image>(rect.gameObject);Paint(image,sprite,false);image.raycastTarget=false;return image; }
        static void Paint(Image image,Sprite sprite,bool sliced=true) { image.sprite=sprite;image.type=sliced?Image.Type.Sliced:Image.Type.Simple;image.color=Color.white; image.pixelsPerUnitMultiplier=sliced?1/RankUiMetrics.Unit:1; }

        static void GenerateArt()
        {
            string path=Art+"RankTheme.asset";theme=AssetDatabase.LoadAssetAtPath<RankTheme>(path);
            if(theme==null) { if(File.Exists(path))throw new InvalidOperationException("Existing theme protected.");theme=ScriptableObject.CreateInstance<RankTheme>();AssetDatabase.CreateAsset(theme,path); }
            theme.panel=Panel("WhitePanel",Color.white,1.7f);theme.primary=Panel("BlackButton",Black,1.7f);
            theme.currentCard=Panel("CurrentCard",Color.white,4);theme.lockedCard=Panel("LockedCard",new Color(.97f,.97f,.97f),1.2f);
            theme.blackStone=Coin("BlackStone",true,false);theme.whiteStone=Coin("WhiteStone",false,false);theme.blackWin=Coin("BlackWin",true,true);theme.whiteWin=Coin("WhiteWin",false,true);
            theme.preview=Sprite("Preview",128,0,(x,y)=> {float r=new Vector2(x-64,y-64).magnitude;var c=Shape(Color.clear,new Color(0,0,0,.65f),r-55);return Shape(c,new Color(.1f,.1f,.1f,.3f),r-51);});
            theme.aiAvatar=Face("AiFace",false);theme.playerAvatar=Face("PlayerFace",true);
            theme.close=Icon("Close",(x,y)=>Mathf.Min(Line(x,y,-14,-14,14,14),Line(x,y,-14,14,14,-14))-2.6f);
            theme.pause=Icon("Pause",(x,y)=>Mathf.Min(Box(x+10,y,3,17,1),Box(x-10,y,3,17,1)));
            theme.left=Icon("Left",(x,y)=>Mathf.Max(x-12,Mathf.Abs(y)*.8f-x-16));theme.right=Icon("Right",(x,y)=>Mathf.Max(-x-12,Mathf.Abs(y)*.8f+x-16));
            theme.check=Icon("Check",(x,y)=>Mathf.Min(Line(x,y,-17,0,-5,-12),Line(x,y,-5,-12,19,15))-2.6f);
            theme.locked=Icon("Lock",(x,y)=>Mathf.Min(Box(x,y+8,16,12,4),Mathf.Max(Mathf.Abs(new Vector2(x,y-6).magnitude-10)-2.5f,3-y)));
            theme.help=Icon("Help",(x,y)=>Mathf.Min(Mathf.Abs(new Vector2(x,y).magnitude-22)-2,Mathf.Min(y>0?Mathf.Abs(new Vector2(x,y-8).magnitude-7)-2:100,Mathf.Min(Box(x,y+2,2,5,0),new Vector2(x,y+13).magnitude-2.8f))));
            theme.settings=Icon("Settings",(x,y)=>{float r=new Vector2(x,y).magnitude;return Mathf.Min(Mathf.Max(r-(20+3*Mathf.Cos(Mathf.Atan2(y,x)*8)),11-r),Mathf.Abs(r-5)-2);});
            theme.star=Icon("Star",(x,y)=>new Vector2(x,y).magnitude-(16+6*Mathf.Cos(5*Mathf.Atan2(x,y))));
            theme.undo=Icon("Undo",(x,y)=>Mathf.Min(Mathf.Abs(new Vector2(x,y).magnitude-17)-2,Mathf.Min(Line(x,y,-20,15,-6,15),Line(x,y,-20,15,-20,1))-2));
            theme.retry=theme.undo;
            theme.roundPrimary=Sprite("RoundPrimary",128,0,(x,y)=>Shape(Color.clear,Black,new Vector2(x-64,y-64).magnitude-61));
            theme.rankHats=new Sprite[10];for(int i=0;i<10;i++){int rank=i;theme.rankHats[i]=Sprite("RankHat"+(i+1).ToString("00"),128,0,(x,y)=>Hat(rank,x-64,y-64));}
            theme.rankHats[0]=Sprite("RecruitCapV10",192,0,(x,y)=>RecruitCap(x-96,y-96));
            EditorUtility.SetDirty(theme);AssetDatabase.SaveAssets();
        }
        static Color RecruitCap(float x,float y)
        {
            // Original field-cap silhouette: raised crown, dark band and projecting front brim.
            float crown=Box(x,y-10,50,28,10);
            var color=Shape(Shape(Color.clear,Black,crown),Color.white,crown+3);
            color=Shape(color,Black,Box(x,y+10,50,4,1));
            return Shape(color,Black,Box(x-12,y+25,64,6,5));
        }
        static Color Hat(int rank,float x,float y)
        {
            float width=27+(rank%3)*7,height=13+(rank/3)*6;
            float crown=rank%2==0?Box(x,y-4,width,height,rank%3==0?12:2):Mathf.Max(Mathf.Abs(x)*(1+(y+12)/100)-width,Mathf.Abs(y-4)-height);
            var color=Shape(Shape(Color.clear,Black,crown),Color.white,crown+3);
            color=Shape(color,Black,Box(x,y+height-2,width+10,3,2));
            for(int mark=0;mark<=rank%5;mark++)color=Shape(color,Black,new Vector2(x-(mark-rank%5*.5f)*8,y-4).magnitude-2);
            return color;
        }
        static Sprite Panel(string name,Color fill,float stroke)=>Sprite(name,128,24,(x,y)=>{float d=Box(x-64,y-64,61,61,13);return Shape(Shape(Color.clear,Black,d),fill,d+stroke);});
        static Sprite Coin(string name,bool black,bool win)=>Sprite(name,128,0,(x,y)=>{float r=new Vector2(x-64,y-64).magnitude;var c=Shape(Color.clear,Black,r-57);c=Shape(c,black?Black:Color.white,r-54);if(win)c=Shape(c,black?Color.white:Black,Mathf.Abs(r-46)-2);return c;});
        static Sprite Face(string name,bool dark)=>Sprite(name,96,0,(x,y)=>{x-=48;y-=48;var c=Shape(Shape(Color.clear,Black,new Vector2(x,y).magnitude-44),dark?Black:Color.white,new Vector2(x,y).magnitude-41);var ink=dark?Color.white:Black;c=Shape(c,ink,Mathf.Min(new Vector2(x-12,y-8).magnitude,new Vector2(x+12,y-8).magnitude)-3);return Shape(c,ink,Mathf.Max(Mathf.Abs(new Vector2(x,y-1).magnitude-20)-1.7f,y+5));});
        static Sprite Icon(string name,Func<float,float,float> distance)=>Sprite("Icon"+name,64,0,(x,y)=>Shape(Color.clear,Black,distance(x-32,y-32)));
        static float Box(float x,float y,float hx,float hy,float r) {var q=new Vector2(Mathf.Abs(x)-hx+r,Mathf.Abs(y)-hy+r);return new Vector2(Mathf.Max(q.x,0),Mathf.Max(q.y,0)).magnitude+Mathf.Min(Mathf.Max(q.x,q.y),0)-r;}
        static float Line(float x,float y,float ax,float ay,float bx,float by) {var p=new Vector2(x-ax,y-ay);var b=new Vector2(bx-ax,by-ay);return(p-b*Mathf.Clamp01(Vector2.Dot(p,b)/b.sqrMagnitude)).magnitude;}
        static Color Shape(Color under,Color top,float distance) {float a=top.a*Mathf.Clamp01(.5f-distance),outA=a+under.a*(1-a);if(outA<=0)return Color.clear;return new Color((top.r*a+under.r*under.a*(1-a))/outA,(top.g*a+under.g*under.a*(1-a))/outA,(top.b*a+under.b*under.a*(1-a))/outA,outA);}
        static Sprite Sprite(string name,int size,int border,Func<float,float,Color> pixel)
        {
            string path=Art+name+".png";if(File.Exists(path)) {var old=AssetDatabase.LoadAssetAtPath<Sprite>(path);if(old==null)throw new InvalidOperationException("Existing art protected: "+path);return old;}
            var texture=new Texture2D(size,size,TextureFormat.RGBA32,false);for(int y=0;y<size;y++)for(int x=0;x<size;x++)texture.SetPixel(x,y,pixel(x+.5f,y+.5f));texture.Apply();File.WriteAllBytes(path,texture.EncodeToPNG());UnityEngine.Object.DestroyImmediate(texture);AssetDatabase.ImportAsset(path);
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;importer.spriteBorder=new Vector4(border,border,border,border);importer.alphaIsTransparency=true;importer.mipmapEnabled=false;importer.filterMode=FilterMode.Bilinear;importer.wrapMode=TextureWrapMode.Clamp;importer.textureCompression=TextureImporterCompression.Uncompressed;
            var settings=new TextureImporterSettings();importer.ReadTextureSettings(settings);settings.spriteMeshType=SpriteMeshType.FullRect;importer.SetTextureSettings(settings);importer.SaveAndReimport();return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
    }
}
