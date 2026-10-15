using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace StageGame.Editor
{
    // Original, antialiased geometric artwork. Missing files are created; existing art is protected.
    // Run after the earlier layout migrations. Every visual is saved in a prefab or scene.
    public static class TabletopUiBuilder
    {
        public const string Art = ProjectBuilder.Root + "Art/TabletopUI/";
        const string Common = ProjectBuilder.Root + "Prefabs/UI/Common/";
        const string Screens = ProjectBuilder.Root + "Prefabs/UI/Screens/";
        static readonly Color Cream = Hex(245, 242, 233), Ink = Hex(23, 62, 67);
        static readonly Color Mint = Hex(103, 214, 176), Coral = Hex(241, 140, 120), Gold = Hex(228, 182, 92);
        static TabletopTheme theme;

        [MenuItem("StageGame/Apply tabletop artwork and board results")]
        public static void Apply()
        {
            if (!string.Equals(Path.GetFullPath(Application.dataPath), Path.GetFullPath("C:/Github/StageGame/Client/Assets"), StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Only the StageGame Client project is allowed.");
            if (EditorApplication.isPlayingOrWillChangePlaymode || PrefabStageUtility.GetCurrentPrefabStage() != null)
                throw new InvalidOperationException("Leave Play mode and prefab editing before applying saved artwork.");
            for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
                if (UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).isDirty)
                    throw new InvalidOperationException("Save scene changes before applying saved artwork; no assets were modified.");
            Directory.CreateDirectory(Art);
            string themePath = Art + "TabletopTheme.asset";
            theme = AssetDatabase.LoadAssetAtPath<TabletopTheme>(themePath);
            if (theme == null)
            {
                if (File.Exists(themePath)) throw new InvalidOperationException("Unrecognized existing theme is protected.");
                theme = ScriptableObject.CreateInstance<TabletopTheme>();
                AssetDatabase.CreateAsset(theme, themePath);
            }
            theme.panel = Panel("PanelCream", Hex(255, 253, 246));
            theme.primary = Panel("ButtonMint", Mint);
            theme.secondary = Panel("ButtonCream", Cream);
            theme.board = Panel("BoardTeal", Hex(38, 86, 91));
            theme.nodeCurrent = Panel("NodeCurrent", Mint);
            theme.nodeCleared = Panel("NodeCleared", Hex(211, 232, 216));
            theme.nodeLocked = Panel("NodeLocked", Hex(228, 228, 216));
            theme.nodeSelected = Panel("NodeSelected", Gold);
            theme.hole = Sprite("RecessedHole", 128, 0, Hole);
            theme.player = Coin("PlayerRing", Mint, false, false);
            theme.opponent = Coin("AiDiamond", Coral, true, false);
            theme.playerWin = Coin("PlayerRingWin", Mint, false, true);
            theme.opponentWin = Coin("AiDiamondWin", Coral, true, true);
            theme.pause = Icon("Pause", (x,y) => Mathf.Min(Box(x+10,y,3,17),Box(x-10,y,3,17)));
            theme.help = Icon("Help", (x,y) => Mathf.Min(Mathf.Abs(new Vector2(x,y).magnitude-22)-2,
                Mathf.Min(y > 0 ? Mathf.Abs(new Vector2(x,y-8).magnitude-7)-2 : 100, Mathf.Min(Box(x,y+2,2,5),new Vector2(x,y+13).magnitude-2.8f))));
            theme.settings = Icon("Settings", (x,y) => { float a=Mathf.Atan2(y,x); float r=new Vector2(x,y).magnitude; return Mathf.Min(Mathf.Max(r-(20+3*Mathf.Cos(a*8)),11-r),Mathf.Abs(r-5)-2); });
            theme.chevron = Icon("Chevron", (x,y) => Mathf.Min(Line(x,y,-6,16,9,0),Line(x,y,9,0,-6,-16))-2.4f);
            theme.locked = Icon("Lock", (x,y) => Mathf.Min(RoundBox(x,y+8,16,12,4), Mathf.Max(Mathf.Abs(new Vector2(x,y-6).magnitude-10)-2.5f,3-y)));
            theme.check = Icon("Check", (x,y) => Mathf.Min(Line(x,y,-17,0,-5,-12),Line(x,y,-5,-12,19,15))-2.7f);
            theme.play = Icon("Play", (x,y) => Mathf.Max(-x-12, Mathf.Abs(y)*.82f+x-17));
            theme.close = Icon("Close", (x,y) => Mathf.Min(Line(x,y,-14,-14,14,14),Line(x,y,-14,14,14,-14))-2.5f);
            EditorUtility.SetDirty(theme); AssetDatabase.SaveAssets();

            foreach (string name in new[] { "PrimaryButton", "CloseButton", "ModalFrame", "StageCard", "StageNode" })
                Edit(Common + name + ".prefab", go =>
                {
                    BaseStyle(go);
                    if (name == "ModalFrame" || name == "StageCard") Paint(go.GetComponent<Image>(), theme.panel);
                    if (name == "CloseButton") IconButton(go.transform, theme.close, true);
                    if (name == "StageNode") Node(go.transform);
                });
            Edit(ProjectBuilder.Root + "Prefabs/Board/Cell.prefab", go =>
            {
                Paint(go.GetComponent<Image>(), theme.hole, false);
                foreach(var text in go.GetComponentsInChildren<Text>(true)) text.gameObject.SetActive(false);
            });
            Edit(ProjectBuilder.Root + "Prefabs/Board/Board.prefab", Board);
            foreach (string name in new[] { "LobbyScreen", "GameScreen", "ResultModal", "SettingsModal", "HelpModal", "ConfirmModal", "NoticesModal" })
                Edit(Screens + name + ".prefab", go => Screen(go, name));
            Edit(ProjectBuilder.Root + "Prefabs/AppRoot.prefab", go =>
            {
                var app = go.GetComponent<GameApp>(); app.theme = theme;
                go.transform.Find("Backdrop").GetComponent<Image>().color = Cream;
                Screen(app.lobby,"LobbyScreen"); Screen(app.match,"GameScreen"); Screen(app.result,"ResultModal");
                Screen(app.settings,"SettingsModal"); Screen(app.help,"HelpModal"); Screen(app.confirm,"ConfirmModal"); Screen(app.notices,"NoticesModal");
                app.stageNodeIcons = new Image[10];
                for(int i=0;i<10;i++) app.stageNodeIcons[i] = app.stageNodes[i].transform.Find("TabletopIcon").GetComponent<Image>();
                foreach(var stone in app.stones) Paint(stone,theme.hole,false);
            });
            var scene = EditorSceneManager.OpenScene(ProjectBuilder.ScenePath);
            foreach(var camera in UnityEngine.Object.FindObjectsByType<Camera>(FindObjectsSortMode.None)) camera.backgroundColor=Cream;
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            ProjectBuilder.Validate();
            Debug.Log("[Tabletop] Saved original 21 sprites, palette theme, all screens and transparent two-action result.");
        }

        static void BaseStyle(GameObject go)
        {
            foreach(var text in go.GetComponentsInChildren<Text>(true)) text.color=Ink;
            foreach(var shadow in go.GetComponentsInChildren<Shadow>(true)) shadow.enabled=false;
            foreach(var button in go.GetComponentsInChildren<Button>(true))
            {
                if(button.name.StartsWith("Column_")) continue;
                var image=button.GetComponent<Image>();
                if(image!=null) { Paint(image,theme.secondary); button.targetGraphic=image; }
                var colors=button.colors;
                colors.normalColor=Color.white; colors.highlightedColor=Color.white;
                colors.pressedColor=new Color(.88f,.94f,.92f,1); colors.selectedColor=Color.white;
                colors.disabledColor=new Color(.82f,.85f,.83f,.72f); button.colors=colors;
                if(button.name=="CloseButton") IconButton(button.transform,theme.close,true);
            }
            var frame=go.transform.Find("Frame");
            if(frame!=null && frame.GetComponent<Image>()!=null) Paint(frame.GetComponent<Image>(),theme.panel);
        }

        static void Screen(GameObject go,string name)
        {
            BaseStyle(go);
            var overlay=go.GetComponent<Image>();
            if(overlay!=null) { overlay.sprite=null; overlay.color=new Color(23/255f,62/255f,67/255f,.25f); }
            var frame=go.transform.Find("Frame");
            if(name=="LobbyScreen")
            {
                Layout(go.transform.Find("Title"),66,428,480,90);
                go.transform.Find("Title").GetComponent<Text>().fontSize=58;
                var emblem=Rect(go.transform,"TabletopEmblem",-252,428,116,100);
                for(int i=0;i<4;i++) Picture(emblem,"Coin"+i,i%2==0?theme.player:theme.opponent,(i%2-.5f)*48,(i/2-.5f)*48,48,48);
                Layout(go.transform.Find("Previous"),-260,319,120,110);
                Layout(go.transform.Find("Following"),260,319,120,110);
                IconButton(go.transform.Find("Previous"),theme.chevron,true).localEulerAngles=new Vector3(0,0,180);
                IconButton(go.transform.Find("Following"),theme.chevron,true).localEulerAngles=Vector3.zero;
                Layout(go.transform.Find("Chapter"),0,345,360,42);
                Layout(go.transform.Find("Tier"),0,300,360,50);
                for(int i=0;i<10;i++)
                {
                    var node=go.transform.Find("StageMap/StageNode_"+i);
                    Layout(node,(i%5-2)*132,i<5?196:56,120,120); Node(node);
                }
                var card=go.transform.Find("StageCard"); Paint(card.GetComponent<Image>(),theme.panel);
                Layout(card,0,-106,648,160);
                Layout(card.Find("Number"),0,48,600,30);
                Layout(card.Find("Title"),0,7,610,55);
                Layout(card.Find("Detail"),0,-43,610,46);
                Layout(go.transform.Find("Progress"),0,-215,640,42);
                Layout(go.transform.Find("Notice"),0,-215,640,42);
                Layout(go.transform.Find("Play"),0,-303,648,110);
                Paint(go.transform.Find("Play").GetComponent<Image>(),theme.primary);
                IconButton(go.transform.Find("Play"),theme.play,false,-241);
                Layout(go.transform.Find("Help"),-170,-425,308,110);
                Layout(go.transform.Find("Settings"),170,-425,308,110);
                IconButton(go.transform.Find("Help"),theme.help,false,-109);
                IconButton(go.transform.Find("Settings"),theme.settings,false,-109);
            }
            else if(name=="GameScreen")
            {
                IconButton(go.transform.Find("Pause"),theme.pause,true);
                Board(go.transform.Find("Board").gameObject);
                Layout(go.transform.Find("Board"),0,-20,720,514);
                Paint(go.transform.Find("Hint").GetComponent<Image>(),theme.secondary);
                // One legend, matching the two embossed coin shapes on the board.
                Layout(go.transform.Find("Legend"),128,345,390,35);
                Picture(go.transform,"LegendRing",theme.player,-77,345,32,32);
                Picture(go.transform,"LegendDiamond",theme.opponent,333,345,32,32);
                for(int i=0;i<7;i++)
                {
                    var marker=go.transform.Find("Marker_"+i).GetComponent<Image>();
                    marker.sprite=theme.panel; marker.color=new Color(23/255f,62/255f,67/255f,.09f);
                }
            }
            else if(name=="ResultModal")
            {
                if(overlay!=null) { overlay.color=Color.clear; overlay.raycastTarget=false; }
                Layout(frame,0,0,720,960);
                var backing=frame.GetComponent<Image>(); if(backing!=null) { backing.color=Color.clear; backing.raycastTarget=false; }
                frame.Find("Eyebrow").gameObject.SetActive(false);
                frame.Find("Next").gameObject.SetActive(false); frame.Find("CloseButton").gameObject.SetActive(false);
                Layout(frame.Find("Title"),0,305,690,50); frame.Find("Title").GetComponent<Text>().fontSize=34;
                Layout(frame.Find("Body"),0,-330,690,72); frame.Find("Body").GetComponent<Text>().fontSize=24;
                Layout(frame.Find("Retry"),-170,-425,308,110); Layout(frame.Find("Lobby"),170,-425,308,110);
                Paint(frame.Find("Retry").GetComponent<Image>(),theme.primary);
                Layout(go.transform.Find("AdStatus"),0,255,610,36);
                go.transform.Find("AdStatus").GetComponent<Text>().fontSize=20;
            }
            else if(name=="SettingsModal")
            {
                var link=frame.Find("Notices"); var image=link.GetComponent<Image>(); image.color=Color.clear;
                link.GetComponent<Button>().targetGraphic=link.GetComponentInChildren<Text>();
            }
            else if(name=="HelpModal")
            {
                var gravity=frame.Find("GravityExample"); Paint(gravity.GetComponent<Image>(),theme.board);
                Paint(gravity.Find("ChosenColumn").GetComponent<Image>(),theme.secondary);
                gravity.Find("ChosenColumn").GetComponent<Image>().color=new Color(1,1,1,.15f);
                for(int row=0;row<4;row++) for(int col=0;col<3;col++)
                    Paint(gravity.Find($"Cell_{row}_{col}").GetComponent<Image>(),col==1&&row==0?theme.opponent:col==1&&row==1?theme.player:theme.hole,false);
                foreach(var bar in gravity.Find("Arrow").GetComponentsInChildren<Image>()) { bar.sprite=null; bar.color=Mint; }
                for(int i=0;i<3;i++)
                {
                    var diagram=frame.Find("ConnectExample_"+i); Paint(diagram.GetComponent<Image>(),theme.board);
                    for(int j=0;j<4;j++) Paint(diagram.Find("Stone_"+j).GetComponent<Image>(),theme.player,false);
                }
            }
        }

        static void Node(Transform node)
        {
            Paint(node.GetComponent<Image>(),theme.nodeLocked);
            Layout(node.Find("Number"),0,18,110,50);
            node.Find("State").gameObject.SetActive(false);
            Picture(node,"TabletopIcon",theme.locked,0,-28,28,28).GetComponent<Image>().color=Ink;
        }
        static void Board(GameObject board)
        {
            Paint(board.GetComponent<Image>(),theme.board);
            ((RectTransform)board.transform).sizeDelta=new Vector2(720,514);
            for(int row=0;row<6;row++) for(int col=0;col<7;col++)
                Layout(board.transform.Find($"Cell_{row*7+col:00}"),(col-3)*720f/7,(row-2.5f)*82,72,72);
            for(int col=0;col<7;col++) Layout(board.transform.Find("Column_"+col),(col-3)*720f/7,0,720f/7,514);
            foreach(var image in board.GetComponentsInChildren<Image>(true))
                if(image.name.StartsWith("Cell_"))
                {
                    Paint(image,theme.hole,false);
                    foreach(var text in image.GetComponentsInChildren<Text>(true)) text.gameObject.SetActive(false);
                }
        }
        static RectTransform IconButton(Transform button,Sprite sprite,bool iconOnly,float x=0)
        {
            foreach(var text in button.GetComponentsInChildren<Text>(true))
            {
                if(iconOnly) text.gameObject.SetActive(false);
                else { var rect=text.rectTransform; rect.offsetMin=new Vector2(44,0); rect.offsetMax=new Vector2(-4,0); }
            }
            var icon=Picture(button,"TabletopIcon",sprite,x,0,44,44); icon.GetComponent<Image>().color=Ink;
            return icon;
        }
        static void Paint(Image image,Sprite sprite,bool sliced=true) { image.sprite=sprite; image.type=sliced?Image.Type.Sliced:Image.Type.Simple; image.color=Color.white; }
        static RectTransform Picture(Transform parent,string name,Sprite sprite,float x,float y,float w,float h)
        {
            var rect=Rect(parent,name,x,y,w,h); var image=rect.GetComponent<Image>()??rect.gameObject.AddComponent<Image>();
            Paint(image,sprite,false); image.raycastTarget=false; return rect;
        }
        static RectTransform Rect(Transform parent,string name,float x,float y,float w,float h)
        {
            var found=parent.Find(name); var rect=found!=null?(RectTransform)found:new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();
            if(found==null) rect.SetParent(parent,false); Layout(rect,x,y,w,h); return rect;
        }
        static void Layout(Transform target,float x,float y,float w,float h)
        {
            var rect=(RectTransform)target; rect.anchorMin=rect.anchorMax=rect.pivot=Vector2.one*.5f;
            rect.anchoredPosition=new Vector2(x,y); rect.sizeDelta=new Vector2(w,h);
        }
        static void Edit(string path,Action<GameObject> action)
        {
            var go=PrefabUtility.LoadPrefabContents(path);
            try { action(go); PrefabUtility.SaveAsPrefabAsset(go,path); }
            finally { PrefabUtility.UnloadPrefabContents(go); }
        }

        static Color Hex(byte r,byte g,byte b) => new Color32(r,g,b,255);
        static float Box(float x,float y,float hx,float hy) => RoundBox(x,y,hx,hy,0);
        static float RoundBox(float x,float y,float hx,float hy,float radius)
        {
            var q=new Vector2(Mathf.Abs(x)-hx+radius,Mathf.Abs(y)-hy+radius);
            return new Vector2(Mathf.Max(q.x,0),Mathf.Max(q.y,0)).magnitude+Mathf.Min(Mathf.Max(q.x,q.y),0)-radius;
        }
        static float Line(float x,float y,float ax,float ay,float bx,float by)
        {
            var p=new Vector2(x-ax,y-ay); var b=new Vector2(bx-ax,by-ay);
            return (p-b*Mathf.Clamp01(Vector2.Dot(p,b)/b.sqrMagnitude)).magnitude;
        }
        static Color Over(Color under,Color over,float coverage)
        {
            float a=over.a*Mathf.Clamp01(coverage), outA=a+under.a*(1-a);
            if(outA<=0) return Color.clear;
            return new Color((over.r*a+under.r*under.a*(1-a))/outA,(over.g*a+under.g*under.a*(1-a))/outA,(over.b*a+under.b*under.a*(1-a))/outA,outA);
        }
        static Color Shape(Color under,Color color,float distance) => Over(under,color,.5f-distance);
        static Sprite Panel(string name,Color fill) => Sprite(name,128,32,(x,y)=>
        {
            x-=64; y-=64;
            float d=RoundBox(x,y-3,57,54,20);
            var color=Over(Color.clear,new Color(Ink.r,Ink.g,Ink.b,.17f),Mathf.Clamp01((3-RoundBox(x,y+4,57,54,20))/4));
            color=Shape(color,Ink,d);
            color=Shape(color,fill,d+1.6f);
            // Small top edge highlight and a short baked lower edge give a physical paper/plastic rim.
            color=Over(color,new Color(1,1,1,.42f),Mathf.Clamp01(.5f-(d+2.8f))*Mathf.Clamp01((y-39)/4));
            return color;
        });
        static Color Hole(float x,float y)
        {
            x-=64; y-=64; float r=new Vector2(x,y).magnitude;
            var color=Shape(Color.clear,Hex(75,115,117),r-58);
            color=Shape(color,Hex(16,46,50),r-54);
            color=Shape(color,Hex(27,65,69),new Vector2(x,y+5).magnitude-49);
            return color;
        }
        static Sprite Coin(string name,Color fill,bool diamond,bool win) => Sprite(name,128,0,(x,y)=>
        {
            x-=64; y-=64; float r=new Vector2(x,y-3).magnitude;
            var color=Over(Color.clear,new Color(0,0,0,.22f),Mathf.Clamp01((59-new Vector2(x,y+3).magnitude)/3));
            color=Shape(color,win?Gold:Ink,r-56);
            color=Shape(color,fill,r-(win?50:53.5f));
            color=Over(color,new Color(1,1,1,.3f),Mathf.Clamp01(.5f-(Mathf.Abs(r-49)-1.2f))*Mathf.Clamp01(y/12));
            float mark=diamond?Mathf.Abs((Mathf.Abs(x)+Mathf.Abs(y-3))*.7071f-23)-2.8f:Mathf.Abs(new Vector2(x,y-3).magnitude-25)-2.8f;
            return Shape(color,Ink,mark);
        });
        static Sprite Icon(string name,Func<float,float,float> distance) => Sprite("Icon"+name,64,0,(x,y)=>Shape(Color.clear,Color.white,distance(x-32,y-32)));
        static Sprite Sprite(string name,int size,int border,Func<float,float,Color> pixel)
        {
            string path=Art+name+".png";
            if(File.Exists(path))
            {
                var existing=AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if(existing==null) throw new InvalidOperationException("Unrecognized existing art protected: "+path);
                return existing;
            }
            var texture=new Texture2D(size,size,TextureFormat.RGBA32,false);
            for(int y=0;y<size;y++) for(int x=0;x<size;x++) texture.SetPixel(x,y,pixel(x+.5f,y+.5f));
            texture.Apply(); File.WriteAllBytes(path,texture.EncodeToPNG()); UnityEngine.Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(path);
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType=TextureImporterType.Sprite; importer.spriteImportMode=SpriteImportMode.Single;
            importer.spriteBorder=new Vector4(border,border,border,border); importer.alphaIsTransparency=true;
            importer.mipmapEnabled=false; importer.filterMode=FilterMode.Bilinear; importer.wrapMode=TextureWrapMode.Clamp;
            importer.textureCompression=TextureImporterCompression.Uncompressed;
            var settings=new TextureImporterSettings(); importer.ReadTextureSettings(settings); settings.spriteMeshType=SpriteMeshType.FullRect; importer.SetTextureSettings(settings);
            importer.SaveAndReimport(); return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
    }
}
