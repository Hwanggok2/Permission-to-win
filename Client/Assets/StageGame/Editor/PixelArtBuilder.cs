using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace StageGame.Editor
{
    // Edits saved prefabs in place while preserving their children, references and GUIDs.
    public static class PixelArtBuilder
    {
        const string Art = "Assets/StageGame/Art/PixelMilitary/";
        const string Prefabs = "Assets/StageGame/Prefabs/RankUI/";
        static float U(float n) => RankUiMetrics.U(n);
        [Serializable] sealed class ImportFile { public ImportPortrait[] portraits; public ImportPortrait finalRecruit; }
        [Serializable] sealed class ImportPortrait
        {
            public string characterId, file;
            public int x, y, width, height;
        }

        [MenuItem("StageGame/Apply Pixel Military Art")]
        public static void Apply()
        {
            EnsureCanApply();
            ApplyArt();
        }
        static void EnsureCanApply()
        {
            if (!string.Equals(Path.GetFullPath(Application.dataPath), Path.GetFullPath("C:/Github/StageGame/Client/Assets"), StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("StageGame Client only.");
            if (EditorApplication.isPlayingOrWillChangePlaymode || PrefabStageUtility.GetCurrentPrefabStage() != null)
                throw new InvalidOperationException("Leave Play/prefab mode first.");
            for (int i=0;i<UnityEngine.SceneManagement.SceneManager.sceneCount;i++)
                if (UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).isDirty)
                    throw new InvalidOperationException("Save open scenes before applying art.");
        }
        [MenuItem("StageGame/Apply Pixel Match Header")]
        public static void ApplyMatchHeader()
        {
            EnsureCanApply();
            var theme=AssetDatabase.LoadAssetAtPath<RankTheme>(RankUiBuilder.Art+"RankTheme.asset");
            foreach(string file in new[]{"Match.prefab","RankAppRoot.prefab"})
            {
                string path=Prefabs+file;var root=PrefabUtility.LoadPrefabContents(path);
                try
                {
                    ConfigureMatchHeader(file=="Match.prefab"?root:root.GetComponent<RankGameApp>().match,theme);
                    PrefabUtility.SaveAsPrefabAsset(root,path);
                }
                finally { PrefabUtility.UnloadPrefabContents(root); }
            }
            AssetDatabase.SaveAssets();RankUiBuilder.Validate();
            Debug.Log("[Pixel header] Saved text-only larger title, aligned pause height and compact opponent panel.");
        }
        static void ApplyArt()
        {
            AssetDatabase.Refresh();
            var theme = AssetDatabase.LoadAssetAtPath<RankTheme>(RankUiBuilder.Art+"RankTheme.asset");
            var oldPanels = new[] { theme.panel, theme.primary, theme.currentCard, theme.lockedCard, theme.roundPrimary };
            var input = JsonUtility.FromJson<ImportFile>(File.ReadAllText(Art+"portrait-import.json"));
            if (input.portraits.Length != 19) throw new InvalidDataException("Expected 19 portraits.");
            theme.portraits = input.portraits.Select(Import).ToArray();
            if (input.finalRecruit != null) theme.finalRecruit = Import(input.finalRecruit);
            theme.barracks = ImportSingle("Barracks");
            theme.woodenBoard = ImportSingle("WoodenBoard");
            theme.panel = Panel("Panel",new Color32(239,229,205,255),new Color32(53,47,34,255));
            theme.primary = Panel("Primary",new Color32(72,84,53,255),new Color32(31,38,24,255));
            theme.currentCard = Panel("CurrentCard",new Color32(249,236,197,255),new Color32(124,86,31,255));
            theme.lockedCard = Panel("LockedCard",new Color32(209,207,186,255),new Color32(92,91,74,255));
            theme.roundPrimary = Stone("Place",false,false,true);
            theme.blackStone = Stone("BlackStone",true,false,false);
            theme.whiteStone = Stone("WhiteStone",false,false,false);
            theme.blackWin = Stone("BlackWin",true,true,false);
            theme.whiteWin = Stone("WhiteWin",false,true,false);
            theme.preview = Stone("Preview",false,true,true);
            EditorUtility.SetDirty(theme);
            AssetDatabase.SaveAssets();
            var newPanels = new[] { theme.panel, theme.primary, theme.currentCard, theme.lockedCard, theme.roundPrimary };
            var emptyStar=EmptyStar();
            foreach (string path in Directory.GetFiles(Prefabs,"*.prefab").OrderBy(p=>p.EndsWith("RankAppRoot.prefab")?1:0))
            {
                var root = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    foreach (var image in root.GetComponentsInChildren<Image>(true))
                    {
                        for (int i=0;i<oldPanels.Length;i++)
                            if (image.sprite == oldPanels[i]) { image.sprite=newPanels[i]; break; }
                        if (image.name.StartsWith("Star_")) { image.sprite=emptyStar;image.color=Color.white; }
                        if (image.sprite!=null && AssetDatabase.GetAssetPath(image.sprite).StartsWith(RankUiBuilder.Art,StringComparison.Ordinal))
                        {
                            switch(image.sprite.name)
                            {
                                case "BlackStone":image.sprite=theme.blackStone;break;
                                case "WhiteStone":image.sprite=theme.whiteStone;break;
                                case "BlackWin":image.sprite=theme.blackWin;break;
                                case "WhiteWin":image.sprite=theme.whiteWin;break;
                                case "Preview":image.sprite=theme.preview;break;
                            }
                        }
                    }
                    string name = Path.GetFileNameWithoutExtension(path);
                    if (name == "Board" || name == "OpeningBoard") ConfigureBoard(root,theme);
                    if (name == "Match") ConfigureMatch(root,theme);
                    if (name == "Detail")
                    {
                        root.GetComponent<Image>().color = new Color32(233,225,204,255);
                        var label=root.transform.Find("Scroll/Viewport/Content/Rank").GetComponent<Text>();
                        label.resizeTextForBestFit=true;label.resizeTextMinSize=(int)U(24);label.resizeTextMaxSize=(int)U(40);
                    }
                    if (name == "StageCard")
                    {
                        var label=root.transform.Find("Rank").GetComponent<Text>();
                        label.resizeTextForBestFit=true;label.resizeTextMinSize=(int)U(22);label.resizeTextMaxSize=(int)U(30);
                    }
                    if (name == "RankAppRoot")
                    {
                        var app=root.GetComponent<RankGameApp>();app.theme=theme;
                        var backdrop=root.transform.Find("Backdrop").GetComponent<Image>();
                        backdrop.sprite=theme.barracks;backdrop.color=Color.white;backdrop.raycastTarget=false;
                        var fit=backdrop.GetComponent<AspectRatioFitter>()??backdrop.gameObject.AddComponent<AspectRatioFitter>();
                        fit.aspectMode=AspectRatioFitter.AspectMode.EnvelopeParent;
                        fit.aspectRatio=theme.barracks.rect.width/theme.barracks.rect.height;
                        // Existing nested prefab references remain serialized; no runtime tree construction.
                        ConfigureMatch(app.match,theme);
                        app.aiFace.sprite=theme.portraits[0].intro;app.aiFace.preserveAspect=true;
                        app.rankHat.gameObject.SetActive(false);
                        RankUiBuilder.ConfigureMoveGuide(app);
                    }
                    PrefabUtility.SaveAsPrefabAsset(root,path);
                }
                finally { PrefabUtility.UnloadPrefabContents(root); }
            }
            AssetDatabase.SaveAssets();
            RankUiBuilder.Validate();
            Debug.Log("[Pixel art] Saved 19 three-state portraits, barracks, wood board and pixel UI in existing prefabs.");
        }

        [MenuItem("StageGame/Apply Character Portraits")]
        public static void ApplyCharacterPortraits()
        {
            EnsureCanApply();
            AssetDatabase.Refresh();
            var theme = AssetDatabase.LoadAssetAtPath<RankTheme>(RankUiBuilder.Art+"RankTheme.asset");
            var input = JsonUtility.FromJson<ImportFile>(File.ReadAllText(Art+"portrait-import.json"));
            if (input.portraits.Length != 19 || input.finalRecruit == null)
                throw new InvalidDataException("Expected 19 portraits and the enlisted final recruit.");
            theme.portraits = input.portraits.Select(Import).ToArray();
            theme.finalRecruit = Import(input.finalRecruit);
            EditorUtility.SetDirty(theme);
            foreach (string name in new[]{"Match.prefab", "RankAppRoot.prefab"})
            {
                string path = Prefabs+name;
                var root = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    var face = name == "Match.prefab" ? root.transform.Find("Scroll/Viewport/Content/AiHeader/AiAvatar").GetComponent<Image>() : root.GetComponent<RankGameApp>().aiFace;
                    face.sprite = theme.Portrait("C01").intro;
                    PrefabUtility.SaveAsPrefabAsset(root,path);
                }
                finally { PrefabUtility.UnloadPrefabContents(root); }
            }
            AssetDatabase.SaveAssets();
            RankUiBuilder.Validate();
            Debug.Log("[Character art] Saved 19 characters and the final recruit uniform variant.");
        }

        static CharacterPortrait Import(ImportPortrait p)
        {
            string path=Art+p.file;
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);
            ConfigureTexture(importer);
            // Preserve the source pixel colors and small insignia without block compression.
            importer.textureCompression=TextureImporterCompression.Uncompressed;
            importer.spriteImportMode=SpriteImportMode.Multiple;
            string[] states={"intro","thinking","defeat"};
            var cells=new SpriteMetaData[3];
            for(int i=0;i<3;i++) cells[i]=new SpriteMetaData{name=p.characterId+"_"+states[i],rect=new Rect(i*512+p.x,p.y,p.width,p.height),alignment=(int)SpriteAlignment.BottomCenter,pivot=new Vector2(.5f,0)};
#pragma warning disable 618
            importer.spritesheet=cells;
#pragma warning restore 618
            importer.SaveAndReimport();
            var sprites=AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToArray();
            return new CharacterPortrait{characterId=p.characterId,intro=sprites.Single(s=>s.name==p.characterId+"_intro"),thinking=sprites.Single(s=>s.name==p.characterId+"_thinking"),defeat=sprites.Single(s=>s.name==p.characterId+"_defeat")};
        }
        static void ConfigureTexture(TextureImporter importer)
        {
            importer.textureType=TextureImporterType.Sprite;importer.filterMode=FilterMode.Point;
            importer.mipmapEnabled=false;importer.alphaIsTransparency=true;importer.wrapMode=TextureWrapMode.Clamp;
            importer.maxTextureSize=2048;importer.textureCompression=TextureImporterCompression.CompressedHQ;
            importer.spritePixelsPerUnit=100;
        }
        static Sprite ImportSingle(string name, int border=0)
        {
            string path=Art+name+".png";AssetDatabase.ImportAsset(path);
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);ConfigureTexture(importer);
            importer.spriteImportMode=SpriteImportMode.Single;importer.spriteBorder=Vector4.one*border;
            importer.textureCompression=TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
        static Sprite Panel(string name,Color fill,Color edge)
        {
            var texture=new Texture2D(16,16,TextureFormat.RGBA32,false);
            for(int y=0;y<16;y++) for(int x=0;x<16;x++)
            {
                int d=Math.Min(Math.Min(x,15-x),Math.Min(y,15-y));
                bool corner=(x<2||x>13)&&(y<2||y>13);
                texture.SetPixel(x,y,corner?Color.clear:d<2?edge:d==2?new Color32(184,163,117,255):fill);
            }
            texture.Apply();File.WriteAllBytes(Art+name+".png",texture.EncodeToPNG());UnityEngine.Object.DestroyImmediate(texture);
            return ImportSingle(name,4);
        }
        static Sprite Stone(string name,bool black,bool winning,bool hollow)
        {
            var texture=new Texture2D(32,32,TextureFormat.RGBA32,false);
            for(int y=0;y<32;y++) for(int x=0;x<32;x++)
            {
                float r=new Vector2(x-15.5f,y-15.5f).magnitude;
                Color color=Color.clear;
                if(r<15) color=r>12?winning?new Color32(244,190,66,255):new Color32(31,29,25,255):
                    hollow?(name=="Place"?new Color32(68,81,47,255):new Color(1,.85f,.35f,.28f)):
                    black?new Color32(48,49,44,255):new Color32(247,242,220,255);
                if(!hollow && r<10 && x<14 && y>18) color=black?new Color32(87,89,79,255):Color.white;
                texture.SetPixel(x,y,color);
            }
            texture.Apply();File.WriteAllBytes(Art+name+".png",texture.EncodeToPNG());UnityEngine.Object.DestroyImmediate(texture);
            return ImportSingle(name);
        }
        static Sprite EmptyStar()
        {
            // Match the existing procedural star silhouette, with an opaque light fill.
            var texture=new Texture2D(64,64,TextureFormat.RGBA32,false);
            for(int y=0;y<64;y++) for(int x=0;x<64;x++)
            {
                var p=new Vector2(x-32,y-32);
                float radius=16+6*Mathf.Cos(5*Mathf.Atan2(p.x,p.y));
                texture.SetPixel(x,y,p.magnitude<=radius?new Color32(216,208,184,255):Color.clear);
            }
            texture.Apply();File.WriteAllBytes(Art+"EmptyStar.png",texture.EncodeToPNG());UnityEngine.Object.DestroyImmediate(texture);
            return ImportSingle("EmptyStar");
        }
        static void ConfigureBoard(GameObject go,RankTheme theme)
        {
            var image=go.GetComponent<Image>()??go.AddComponent<Image>();
            image.sprite=theme.woodenBoard;image.color=Color.white;image.raycastTarget=false;
            foreach(Transform child in go.transform)
            {
                if(child.name.StartsWith("Frame")) child.gameObject.SetActive(false);
                if(child.name.StartsWith("Vertical_")||child.name.StartsWith("Horizontal_")) child.GetComponent<Image>().color=new Color32(42,32,20,255);
                if(child.name.StartsWith("Stone_")) child.GetComponent<Image>().sprite=theme.blackStone;
            }
            if(go.transform.Find("Preview")) go.transform.Find("Preview").GetComponent<Image>().sprite=theme.preview;
        }
        static void ConfigureMatch(GameObject go,RankTheme theme)
        {
            var content=go.transform.Find("Scroll/Viewport/Content");
            var header=content.Find("AiHeader");
            var portrait=header.Find("AiAvatar").GetComponent<Image>();
            Place(portrait.rectTransform,-220,-36,220,260);portrait.sprite=theme.portraits[0].intro;portrait.preserveAspect=true;portrait.raycastTarget=false;
            header.Find("RankHat").gameObject.SetActive(false);
            ConfigureMatchHeader(go,theme);
            var bubble=content.Find("SpeechBubble").GetComponent<Image>();
            bubble.color=new Color(1,1,1,.68f);Place(bubble.rectTransform,105,-302,420,88,true);
            var speech=bubble.transform.Find("Speech").GetComponent<Text>();
            speech.rectTransform.sizeDelta=RankUiMetrics.Point(394,80);speech.fontSize=(int)U(24);
            speech.resizeTextForBestFit=true;speech.resizeTextMinSize=(int)U(21);speech.resizeTextMaxSize=(int)U(26);
            ConfigureBoard(content.Find("Board").gameObject,theme);
            RankUiBuilder.ConfigureCompactMatchControls(go);
            var notice=content.Find("Notice").GetComponent<Text>();notice.color=new Color32(30,30,23,255);
        }
        static void ConfigureMatchHeader(GameObject go,RankTheme theme)
        {
            var content=go.transform.Find("Scroll/Viewport/Content");
            var title=content.Find("Title").GetComponent<Text>();
            Place(title.rectTransform,0,-52,540,104,true);title.fontSize=(int)U(52);
            title.resizeTextForBestFit=false;
            var titlePlate=content.Find("TitlePlate");if(titlePlate!=null)titlePlate.gameObject.SetActive(false);
            Place((RectTransform)content.Find("Pause"),290,-52,110,104,true);
            RankUiBuilder.ConfigureMatchPauseIcon(go);
            var header=content.Find("AiHeader");
            Place((RectTransform)header.Find("AiAvatar"),-220,-36,220,260);
            var name=header.Find("AiName").GetComponent<Text>();
            Place(name.rectTransform,105,24,374,68);name.resizeTextForBestFit=true;name.resizeTextMinSize=(int)U(22);name.resizeTextMaxSize=(int)U(28);
            Place((RectTransform)header.Find("AiStars"),105,-42,256,42);
            Plate(header,"NamePlate",105,-6,400,144,theme.panel);
            if(header.Find("Rating")!=null)RankUiBuilder.ConfigureMatchRating(go);
        }
        static void Plate(Transform parent,string name,float x,float y,float width,float height,Sprite sprite,bool top=false)
        {
            var t=parent.Find(name) as RectTransform;
            if(t==null) t=new GameObject(name,typeof(RectTransform),typeof(Image)).GetComponent<RectTransform>();
            t.SetParent(parent,false);Place(t,x,y,width,height,top);t.SetAsFirstSibling();
            var image=t.GetComponent<Image>();image.sprite=sprite;image.type=Image.Type.Sliced;image.color=new Color(1,1,1,.68f);image.raycastTarget=false;
        }
        static void Place(RectTransform rect,float x,float y,float width,float height,bool top=false)
        {
            rect.anchorMin=rect.anchorMax=new Vector2(.5f,top?1:.5f);rect.pivot=new Vector2(.5f,.5f);
            rect.anchoredPosition=RankUiMetrics.Point(x,y);rect.sizeDelta=RankUiMetrics.Point(width,height);
        }
    }
}
