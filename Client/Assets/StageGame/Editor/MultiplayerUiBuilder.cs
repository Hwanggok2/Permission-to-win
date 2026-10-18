using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace StageGame.Editor
{
    public static partial class RankUiBuilder
    {
        public static void BuildPublicMultiplayerWebGL()
        {
            EnsureEditorReady();
            string endpoint=System.Environment.GetEnvironmentVariable("STAGEGAME_MULTIPLAYER_ENDPOINT");
            if(!System.Uri.TryCreate(endpoint,System.UriKind.Absolute,out var uri) || uri.Scheme!="wss" || uri.AbsolutePath!="/v1/play" || uri.Query!="" || uri.UserInfo!="")
                throw new System.InvalidOperationException("A public wss endpoint ending in /v1/play is required.");
            Save("RankAppRoot",go=>
            {
                var online=go.GetComponent<OnlineGameController>();
                if(online==null)throw new System.InvalidOperationException("Apply the saved multiplayer UI first.");
                online.endpoint=endpoint;
            });
            AssetDatabase.SaveAssets();Validate();
            ProjectBuilder.BuildWebGL();
        }

        [MenuItem("StageGame/Apply Multiplayer and Last Move")]
        public static void ApplyMultiplayer()
        {
            EnsureEditorReady();font=AssetDatabase.LoadAssetAtPath<Font>(ProjectBuilder.Root+"Fonts/Pretendard-Regular.ttf");
            theme=AssetDatabase.LoadAssetAtPath<RankTheme>(Art+"RankTheme.asset");table=StringTable.Load();
            Save("Board",ConfigureLastMoveHighlight);Save("OnlinePanel",BuildOnlinePanel);Save("RankAppRoot",ConfigureOnlineRoot);
            AssetDatabase.SaveAssets();Validate();Debug.Log("[Multiplayer UI] Saved online panel, controls, board and opponent move marker. No server deployment.");
        }
        static void ConfigureLastMoveHighlight(GameObject go)
        {
            var rect=Rect(go.transform,"LastOpponentMove",0,0,90,90);Add<CanvasRenderer>(rect.gameObject);var ring=Add<LastMoveHighlight>(rect.gameObject);
            ring.color=Color.white;ring.raycastTarget=false;rect.gameObject.SetActive(false);
        }
        static void BuildOnlinePanel(GameObject go)
        {
            Size(go.transform,720,1280);var background=Add<Image>(go);background.color=new Color32(224,216,189,246);background.raycastTarget=true;
            Top(Label(go.transform,"Title",table.Text("multiplayer_title"),34,-40,-55,540,90).rectTransform);
            Close(go.transform,290,-55);Top(Get<RectTransform>(go,"CloseButton"));
            // Migrate the generated tree without replacing its saved board or button instances.
            var oldContent=go.transform.Find("Scroll/Viewport/Content");
            if(oldContent!=null)foreach(string name in new[]{"Menu","Game"}){var child=oldContent.Find(name);if(child!=null)child.SetParent(go.transform,false);}
            var oldScroll=go.transform.Find("Scroll");if(oldScroll!=null)oldScroll.gameObject.SetActive(false);
            var oldFit=go.GetComponent<RankDetailFitter>();if(oldFit!=null)oldFit.enabled=false;
            var menu=Rect(go.transform,"Menu",0,0,696,850);
            Top(Label(menu,"Profile","",30,0,-55,660,70).rectTransform);
            Top(Label(menu,"Description","",25,0,-150,650,120).rectTransform);
            Top((RectTransform)Button(menu,"Random",0,-280,620,110).transform);
            Top((RectTransform)Button(menu,"Friendly",0,-410,620,110).transform);
            var randomHint=Label(menu.Find("Random"),"Hint",table.Text("mp_random_hint"),28,0,-25,570,65);
            var friendlyHint=Label(menu.Find("Friendly"),"Hint",table.Text("mp_friendly_hint"),28,0,-25,570,65);
            var invite=Rect(menu,"Invitation",0,0,680,850);Top(invite);invite.pivot=new Vector2(.5f,1);
            Top((RectTransform)Button(invite,"Rated",0,-280,620,110).transform);
            Top((RectTransform)Button(invite,"Share",0,-410,620,110).transform);
            Top((RectTransform)Button(invite,"Copy",0,-540,620,110).transform);
            Top((RectTransform)Button(invite,"Join",0,-410,620,110).transform);
            var link=Rect(invite,"Url",0,-660,620,90);Top(link);Paint(Add<Image>(link.gameObject),theme.panel);
            var linkText=Label(link,"Text","",18,0,0,590,80);linkText.horizontalOverflow=HorizontalWrapMode.Wrap;
            var input=Add<InputField>(link.gameObject);input.textComponent=linkText;input.readOnly=true;input.lineType=InputField.LineType.MultiLineNewline;
            var game=Rect(go.transform,"Game",0,0,696,1220);
            var legacyPlayers=game.Find("Players");if(legacyPlayers!=null)legacyPlayers.gameObject.SetActive(false);
            var mine=Rect(game,"Mine",-170,-60,328,112);Paint(Add<Image>(mine.gameObject),theme.panel);
            var opponent=Rect(game,"Opponent",170,-60,328,112);Paint(Add<Image>(opponent.gameObject),theme.panel);
            Label(mine,"Label","",28,0,0,300,100);Label(opponent,"Label","",28,0,0,300,100);
            Top(Label(game,"First","",25,0,-120,660,50).rectTransform);
            Top(Label(game,"Turn","",28,0,-180,660,50).rectTransform);
            var board=Instance("Board",game,"Board");Layout(board,0,-590,704,704);Top(board);
            var oldFooter=game.Find("Footer");
            if(oldFooter!=null)foreach(string name in new[]{"Place","Rematch","Leave","Result"})oldFooter.Find(name)?.SetParent(game,false);
            Top((RectTransform)Button(game,"Place",0,-1010,620,110).transform);
            Top((RectTransform)Button(game,"Rematch",-160,-1010,300,110).transform);
            Top((RectTransform)Button(game,"Leave",160,-1010,300,110).transform);
            Top(Label(game,"Result","",25,0,-1135,650,120).rectTransform);
            var footer=Rect(game,"Footer",0,0,720,220);
            foreach(string name in new[]{"Place","Rematch","Leave","Result"})game.Find(name)?.SetParent(footer,false);
            Label(footer,"Notice","",25,0,0,650,50);
            var confirm=Rect(go.transform,"LeaveConfirm",0,0,680,520);Paint(Add<Image>(confirm.gameObject),theme.panel);
            var shade=confirm.GetComponent<Image>();shade.sprite=null;shade.color=new Color(0,0,0,.55f);shade.raycastTarget=true;
            var frame=Rect(confirm,"Frame",0,0,660,440);Paint(Add<Image>(frame.gameObject),theme.panel);
            foreach(string name in new[]{"Body","Cancel","Accept"})confirm.Find(name)?.SetParent(frame,false);
            Label(frame,"Body","",28,0,100,620,190);
            Button(frame,"Cancel",-160,-100,300,110,true);Button(frame,"Accept",160,-100,300,110);
            // Reapplying must reuse footer children instead of leaving duplicate buttons in Game.
            var layout=Add<OnlinePanelLayout>(go);
            layout.menu=menu;layout.game=game;layout.title=Get<RectTransform>(go,"Title");layout.close=Get<RectTransform>(go,"CloseButton");
            layout.profile=Get<RectTransform>(menu.gameObject,"Profile");layout.description=Get<RectTransform>(menu.gameObject,"Description");
            layout.random=Get<RectTransform>(menu.gameObject,"Random");layout.friendly=Get<RectTransform>(menu.gameObject,"Friendly");layout.invitation=invite;
            layout.rated=Get<RectTransform>(invite.gameObject,"Rated");layout.share=Get<RectTransform>(invite.gameObject,"Share");layout.copy=Get<RectTransform>(invite.gameObject,"Copy");layout.join=Get<RectTransform>(invite.gameObject,"Join");layout.url=link;
            layout.mine=mine;layout.opponent=opponent;layout.first=Get<RectTransform>(game.gameObject,"First");layout.turn=Get<RectTransform>(game.gameObject,"Turn");layout.board=board;layout.footer=footer;
            layout.result=Get<RectTransform>(footer.gameObject,"Result");layout.notice=Get<RectTransform>(footer.gameObject,"Notice");layout.place=Get<RectTransform>(footer.gameObject,"Place");layout.rematch=Get<RectTransform>(footer.gameObject,"Rematch");layout.leave=Get<RectTransform>(footer.gameObject,"Leave");
            layout.confirm=confirm;layout.confirmFrame=frame;layout.confirmBody=Get<RectTransform>(frame.gameObject,"Body");layout.confirmCancel=Get<RectTransform>(frame.gameObject,"Cancel");layout.confirmAccept=Get<RectTransform>(frame.gameObject,"Accept");layout.randomHint=randomHint;layout.friendlyHint=friendlyHint;
            layout.title.GetComponent<Text>().alignment=TextAnchor.MiddleLeft;
            foreach(var r in new[]{layout.random,layout.share,layout.join,layout.place,layout.rematch}){Paint(r.GetComponent<Image>(),theme.primary);r.Find("Label").GetComponent<Text>().color=Color.white;}
            randomHint.color=Color.white;
            go.transform.Find("Title").SetAsLastSibling();go.transform.Find("CloseButton").SetAsLastSibling();confirm.SetAsLastSibling();
            confirm.gameObject.SetActive(false);game.gameObject.SetActive(false);invite.gameObject.SetActive(false);
        }
        static void ConfigureOnlineRoot(GameObject go)
        {
            var app=go.GetComponent<RankGameApp>();
            app.lastOpponentMove=app.boardView.Find("LastOpponentMove").GetComponent<LastMoveHighlight>();
            var panel=Instance("OnlinePanel",app.lobby.transform.parent,"OnlinePanel");Stretch(panel);panel.SetAsLastSibling();
            var online=Add<OnlineGameController>(go);app.online=online;online.app=app;online.transport=Add<OnlineTransport>(go);
            online.panel=panel.gameObject;online.menu=panel.Find("Menu").gameObject;online.game=panel.Find("Game").gameObject;
            online.invitation=online.menu.transform.Find("Invitation").gameObject;
            online.title=Get<Text>(panel.gameObject,"Title");online.profile=Get<Text>(online.menu,"Profile");online.description=Get<Text>(online.menu,"Description");
            online.randomMatch=Get<Button>(online.menu,"Random");online.friendly=Get<Button>(online.menu,"Friendly");
            online.ratedToggle=Get<Button>(online.invitation,"Rated");online.share=Get<Button>(online.invitation,"Share");online.copy=Get<Button>(online.invitation,"Copy");online.join=Get<Button>(online.invitation,"Join");
            online.randomLabel=Get<Text>(online.menu,"Random/Label");online.friendlyLabel=Get<Text>(online.menu,"Friendly/Label");
            online.randomHint=Get<Text>(online.menu,"Random/Hint");online.friendlyHint=Get<Text>(online.menu,"Friendly/Hint");
            online.ratedLabel=Get<Text>(online.invitation,"Rated/Label");online.shareLabel=Get<Text>(online.invitation,"Share/Label");online.copyLabel=Get<Text>(online.invitation,"Copy/Label");online.joinLabel=Get<Text>(online.invitation,"Join/Label");
            online.inviteUrl=Get<InputField>(online.invitation,"Url");online.close=Get<Button>(panel.gameObject,"CloseButton");
            online.myProfile=Get<Text>(online.game,"Mine/Label");online.otherProfile=Get<Text>(online.game,"Opponent/Label");online.firstPlayer=Get<Text>(online.game,"First");online.turn=Get<Text>(online.game,"Turn");online.resultLabel=Get<Text>(online.game,"Footer/Result");online.statusLabel=Get<Text>(online.game,"Footer/Notice");
            online.place=Get<Button>(online.game,"Footer/Place");online.rematch=Get<Button>(online.game,"Footer/Rematch");online.placeLabel=Get<Text>(online.game,"Footer/Place/Label");online.rematchLabel=Get<Text>(online.game,"Footer/Rematch/Label");
            online.resultLeave=Get<Button>(online.game,"Footer/Leave");online.resultLeaveLabel=Get<Text>(online.game,"Footer/Leave/Label");
            online.stones=new Image[42];online.columns=new Button[7];
            for(int i=0;i<42;i++)online.stones[i]=Get<Image>(online.game,"Board/Stone_"+i);
            for(int i=0;i<7;i++)online.columns[i]=Get<Button>(online.game,"Board/Column_"+i);
            online.preview=Get<Image>(online.game,"Board/Preview");online.lastOpponentMove=Get<LastMoveHighlight>(online.game,"Board/LastOpponentMove");
            online.leaveConfirm=panel.Find("LeaveConfirm").gameObject;online.leaveAccept=Get<Button>(online.leaveConfirm,"Frame/Accept");online.leaveCancel=Get<Button>(online.leaveConfirm,"Frame/Cancel");
            online.confirmLabel=Get<Text>(online.leaveConfirm,"Frame/Body");online.confirmAcceptLabel=Get<Text>(online.leaveConfirm,"Frame/Accept/Label");online.confirmCancelLabel=Get<Text>(online.leaveConfirm,"Frame/Cancel/Label");
            panel.gameObject.SetActive(false);
        }
    }
}
