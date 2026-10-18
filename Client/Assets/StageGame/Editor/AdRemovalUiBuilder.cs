using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace StageGame.Editor
{
    public static partial class RankUiBuilder
    {
        [MenuItem("StageGame/Apply Ad Removal Offer")]
        public static void ApplyAdRemovalOffer()
        {
            EnsureEditorReady();font=AssetDatabase.LoadAssetAtPath<Font>(ProjectBuilder.Root+"Fonts/Pretendard-Regular.ttf");
            theme=AssetDatabase.LoadAssetAtPath<RankTheme>(Art+"RankTheme.asset");table=StringTable.Load();
            Save("AdRemovalOffer",BuildAdRemovalOffer);Save("RankAppRoot",ConfigureAdRemovalRoot);
            AssetDatabase.SaveAssets();Validate();Debug.Log("[Ad removal] Saved offer prefab and references. Real ads and purchases remain unconfigured.");
        }
        static void BuildAdRemovalOffer(GameObject go)
        {
            Size(go.transform,720,1280);
            var outside=Rect(go.transform,"Outside",0,0,720,1280);Stretch(outside);
            var shade=Add<Image>(outside.gameObject);shade.color=new Color(0,0,0,.55f);shade.raycastTarget=true;
            var dismiss=Add<Button>(outside.gameObject);dismiss.targetGraphic=shade;dismiss.transition=Selectable.Transition.None;dismiss.navigation=new Navigation{mode=Navigation.Mode.None};
            var frame=Rect(go.transform,"Frame",0,0,660,900);Paint(Add<Image>(frame.gameObject),theme.panel);
            Label(frame,"Title",table.Text("ad_offer_title"),48,-60,340,470,90).fontStyle=FontStyle.Bold;
            Label(frame,"Subtitle",table.Text("ad_offer_subtitle"),28,0,260,600,60);
            foreach(string name in new[]{"Benefits","HintBenefit"})
            {
                var row=Rect(frame,name,0,0,600,110);var bg=Add<Image>(row.gameObject);bg.color=new Color32(218,221,188,255);bg.raycastTarget=false;
                Label(row,"Label",table.Text(name=="Benefits"?"ad_offer_ads":"ad_offer_hint"),28,0,0,560,100).fontStyle=FontStyle.Bold;
            }
            Label(frame,"Permanent",table.Text("ad_offer_permanent"),26,0,-100,600,80);
            var buy=Button(frame,"Buy",0,-200,600,104);Paint(buy.GetComponent<Image>(),theme.primary);buy.GetComponentInChildren<Text>().color=Color.white;
            var later=Button(frame,"Later",-90,-310,400,88);var restore=Button(frame,"Restore",220,-310,160,88);
            foreach(var b in new[]{later,restore}){var bg=b.GetComponent<Image>();bg.sprite=null;bg.color=Color.clear;b.targetGraphic=b.transform.Find("Label").GetComponent<Text>();}
            var close=Rect(frame,"Close",260,350,88,88);var hit=Add<Image>(close.gameObject);hit.sprite=null;hit.color=Color.clear;hit.raycastTarget=true;
            var closeButton=Add<Button>(close.gameObject);Picture(close,"Icon",theme.close,0,0,32,32);closeButton.targetGraphic=hit;hit.color=Color.white;
            var colors=closeButton.colors;colors.normalColor=Color.clear;colors.highlightedColor=new Color(.77f,.78f,.62f,.6f);colors.selectedColor=colors.highlightedColor;colors.pressedColor=new Color(.7f,.72f,.55f,.8f);closeButton.colors=colors;
            Label(frame,"Status","",24,0,-410,600,56);
            foreach(var b in new[]{buy,later,restore,closeButton})b.navigation=new Navigation{mode=Navigation.Mode.None};
            var fit=Add<AdRemovalOfferLayout>(go);fit.frame=frame;fit.close=close;
            fit.title=Get<RectTransform>(frame.gameObject,"Title");fit.subtitle=Get<RectTransform>(frame.gameObject,"Subtitle");
            fit.benefits=Get<RectTransform>(frame.gameObject,"Benefits");fit.hintBenefit=Get<RectTransform>(frame.gameObject,"HintBenefit");
            fit.permanent=Get<RectTransform>(frame.gameObject,"Permanent");fit.buy=(RectTransform)buy.transform;fit.later=(RectTransform)later.transform;fit.restore=(RectTransform)restore.transform;
            fit.status=Get<RectTransform>(frame.gameObject,"Status");
        }
        static void ConfigureAdRemovalRoot(GameObject go)
        {
            var app=go.GetComponent<RankGameApp>();var panel=Instance("AdRemovalOffer",app.lobby.transform.parent,"AdRemovalOffer");Stretch(panel);panel.SetAsLastSibling();
            var offer=Add<AdRemovalOffer>(go);app.adOffer=offer;offer.app=app;offer.panel=panel.gameObject;
            var frame=panel.Find("Frame").gameObject;
            offer.outside=Get<Button>(panel.gameObject,"Outside");offer.close=Get<Button>(frame,"Close");offer.buy=Get<Button>(frame,"Buy");offer.later=Get<Button>(frame,"Later");offer.restore=Get<Button>(frame,"Restore");
            offer.title=Get<Text>(frame,"Title");offer.subtitle=Get<Text>(frame,"Subtitle");offer.benefits=Get<Text>(frame,"Benefits/Label");offer.hintBenefit=Get<Text>(frame,"HintBenefit/Label");
            offer.permanent=Get<Text>(frame,"Permanent");offer.buyLabel=Get<Text>(frame,"Buy/Label");offer.laterLabel=Get<Text>(frame,"Later/Label");offer.restoreLabel=Get<Text>(frame,"Restore/Label");offer.status=Get<Text>(frame,"Status");
            app.lobbyInput=Add<CanvasGroup>(app.lobby);app.lobbyInput.interactable=true;
            app.soundButtons=go.GetComponentsInChildren<Button>(true);panel.gameObject.SetActive(false);
        }
    }
}
