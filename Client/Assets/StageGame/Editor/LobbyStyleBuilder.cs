using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace StageGame.Editor
{
    public static partial class RankUiBuilder
    {
        const string LobbyArt="Assets/StageGame/Art/LobbyUI/";
        [MenuItem("StageGame/Apply Lobby Redesign")]
        public static void ApplyLobbyRedesign()
        {
            EnsureEditorReady();font=AssetDatabase.LoadAssetAtPath<Font>(ProjectBuilder.Root+"Fonts/Pretendard-Regular.ttf");
            theme=AssetDatabase.LoadAssetAtPath<RankTheme>(Art+"RankTheme.asset");table=StringTable.Load();
            Save("StageCard",ConfigureLobbyCard);
            Save("Lobby",go=>{ConfigureLobbyLayout(go);ConfigureLobbyStyle(go);foreach(var card in go.GetComponentsInChildren<RankStageCard>(true))ConfigureLobbyCard(card.gameObject);});
            Save("RankAppRoot",go=>
            {
                var app=go.GetComponent<RankGameApp>();ConfigureLobbyLayout(app.lobby);ConfigureAdRemovalRoot(go);
                foreach(var card in app.cards)ConfigureLobbyCard(card.gameObject);
            });
            AssetDatabase.SaveAssets();Validate();Debug.Log("[Lobby redesign] Saved icon toolbar, mission cards and full-width multiplayer with own rating.");
        }
        static Sprite LobbySprite(string name,int size,int border,Func<int,int,Color> pixel)
        {
            Directory.CreateDirectory(LobbyArt);string path=LobbyArt+name+".png";
            if(!File.Exists(path))
            {
                var texture=new Texture2D(size,size,TextureFormat.RGBA32,false);
                for(int y=0;y<size;y++)for(int x=0;x<size;x++)texture.SetPixel(x,y,pixel(x,y));
                texture.Apply();File.WriteAllBytes(path,texture.EncodeToPNG());UnityEngine.Object.DestroyImmediate(texture);AssetDatabase.ImportAsset(path);
            }
            return ImportLobbySprite(path,border);
        }
        static Sprite ImportLobbySprite(string path,int border=0)
        {
            AssetDatabase.ImportAsset(path);var importer=(TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;
            importer.spriteBorder=new Vector4(border,border,border,border);importer.filterMode=FilterMode.Point;
            importer.mipmapEnabled=false;importer.alphaIsTransparency=true;importer.textureCompression=TextureImporterCompression.Uncompressed;
            importer.maxTextureSize=128;importer.wrapMode=TextureWrapMode.Clamp;importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
        static Sprite LobbyPanel(string name,Color fill,Color edge)=>LobbySprite(name,32,8,(x,y)=>
        {
            int d=Math.Min(Math.Min(x,31-x),Math.Min(y,31-y));
            if((x<2||x>29)&&(y<2||y>29))return Color.clear;
            if(d<2)return edge;
            if(y<4)return Color.Lerp(edge,fill,.28f);
            if(y>27||x<4)return Color.Lerp(fill,Color.white,.22f);
            return fill;
        });
        static void ConfigureLobbyCard(GameObject go)
        {
            var card=go.GetComponent<RankStageCard>();
            card.paper=LobbyPanel("MissionPaper",new Color32(230,222,192,255),new Color32(53,57,40,255));
            card.currentPaper=LobbyPanel("MissionCurrent",new Color32(249,236,197,255),new Color32(105,79,37,255));
            card.lockedPaper=LobbyPanel("MissionLocked",new Color32(207,209,183,255),new Color32(67,73,52,255));
            card.lockedMark=LobbySprite("CardLock",32,0,(x,y)=>((x>=7&&x<=24&&y>=5&&y<=18)||((x>=10&&x<=21&&y>=18&&y<=25)&&!(x>=13&&x<=18&&y<=22)))?Color.white:Color.clear);
            card.clearedMark=LobbySprite("CardCheck",32,0,(x,y)=>Mathf.Min(Line(x,y,6,15,13,8),Line(x,y,13,8,26,24))<2?Color.white:Color.clear);
            card.currentMark=LobbySprite("CardArrow",32,0,(x,y)=>Mathf.Min(Line(x,y,11,7,21,16),Line(x,y,21,16,11,25))<2?Color.white:Color.clear);
            var bg=go.GetComponent<Image>();Paint(bg,card.paper);bg.pixelsPerUnitMultiplier=.5f;
            card.header=Picture(go.transform,"Header",null,0,83,304,48);card.header.transform.SetAsFirstSibling();
            card.accent=Picture(go.transform,"Accent",null,-150,-21,5,152);card.accent.transform.SetAsFirstSibling();
            var rule=Picture(go.transform,"Rule",null,0,-64,280,2);rule.color=new Color32(160,159,126,255);rule.transform.SetAsFirstSibling();
            Layout(card.number.transform,-24,83,232,38);card.number.alignment=TextAnchor.MiddleLeft;card.number.fontSize=Mathf.RoundToInt(U(22));card.number.color=new Color32(250,243,218,255);
            Layout(card.rank.transform,0,18,280,72);card.rank.fontSize=Mathf.RoundToInt(U(29));card.rank.fontStyle=FontStyle.Bold;
            Layout(card.stars.transform,0,-35,280,36);
            Layout(card.rating.transform,0,-87,270,34);card.rating.fontSize=Mathf.RoundToInt(U(22));
            Layout(card.state.transform,126,83,26,26);card.state.color=new Color32(250,243,218,255);
            card.SetAppearance(false,false);
        }
        static void ConfigureLobbyStyle(GameObject go)
        {
            var help=LobbySprite("HelpMark",32,0,(x,y)=>
            {
                string[] glyph={"0111110","1100011","0000011","0000110","0001100","0011000","0011000","0000000","0011000","0011000"};
                int col=(x-9)/2,row=(25-y)/2;bool ink=x>=9&&y<=25&&x<23&&y>5&&row>=0&&row<10&&col>=0&&col<7&&glyph[row][col]=='1';
                return ink?Color.white:Color.clear;
            });
            var gear=LobbySprite("GearMark",32,0,(x,y)=>
            {
                float dx=x-15.5f,dy=y-15.5f,r=Mathf.Sqrt(dx*dx+dy*dy),angle=Mathf.Atan2(dy,dx);
                bool tooth=Mathf.Cos(angle*8)>.3f;return r>=5&&r<(tooth?14:11)?Color.white:Color.clear;
            });
            ConfigureLobbyIcon(go,"Help",help,-290,90);
            ConfigureLobbyIcon(go,"AdRemoval",ImportLobbySprite(LobbyArt+"NoAds.png"),-174,104);
            Get<Text>(go,"AdRemoval/Label").text=table.Text("ad_offer_title");
            ConfigureLobbyIcon(go,"Settings",gear,290,72);
            var multi=Button(go.transform,"Multiplayer",0,55+HeaderOffset,680,110);Bottom((RectTransform)multi.transform);
            Paint(multi.GetComponent<Image>(),LobbyPanel("BattlePanel",new Color32(62,79,47,255),new Color32(30,39,25,255)));multi.GetComponent<Image>().pixelsPerUnitMultiplier=.5f;
            var label=Get<Text>(go,"Multiplayer/Label");Layout(label.transform,-65,0,310,84);label.fontSize=Mathf.RoundToInt(U(34));label.fontStyle=FontStyle.Bold;label.color=new Color32(255,245,215,255);label.alignment=TextAnchor.MiddleLeft;
            Picture(multi.transform,"BlackDisc",theme.blackStone,-281,8,52,52);Picture(multi.transform,"WhiteDisc",theme.whiteStone,-254,-10,48,48);
            var divider=Picture(multi.transform,"Divider",null,123,0,2,66);divider.color=new Color32(148,159,111,255);
            var rating=Label(multi.transform,"Rating",table.Format("ai_rating",200),24,229,0,180,80);rating.color=new Color32(244,222,157,255);rating.fontStyle=FontStyle.Bold;
            var fit=go.GetComponent<RankLobbyFitter>();fit.multiplayer=(RectTransform)multi.transform;fit.adRemoval=Get<RectTransform>(go,"AdRemoval");fit.Fit();
        }
        static void ConfigureLobbyIcon(GameObject go,string name,Sprite sprite,float x,float iconSize)
        {
            var button=Button(go.transform,name,x,0,110,110);Top((RectTransform)button.transform);
            var background=button.GetComponent<Image>();background.sprite=null;background.color=Color.clear;background.raycastTarget=true;
            button.transform.Find("Label").gameObject.SetActive(false);
            var icon=Picture(button.transform,"Icon",sprite,0,0,iconSize,iconSize);icon.preserveAspect=true;button.targetGraphic=icon;
            var shadow=icon.GetComponent<Shadow>();if(shadow!=null)shadow.enabled=false;
            if(name!="AdRemoval")
            {
                icon.color=new Color32(33,42,30,255);
                var outline=Add<Outline>(icon.gameObject);outline.enabled=true;outline.effectColor=new Color32(255,248,220,255);outline.effectDistance=RankUiMetrics.Point(3,-3);
            }
            var colors=button.colors;colors.normalColor=Color.white;colors.highlightedColor=colors.selectedColor=new Color32(214,190,121,255);colors.pressedColor=new Color32(171,157,115,255);button.colors=colors;
        }
    }
}
