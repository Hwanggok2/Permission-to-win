using System;
using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_WEBGL && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif

namespace StageGame
{
    [Serializable] public sealed class OnlineState
    {
        public string type, token, code, playerId, rankKey, status, roomId, invite, otherRankKey, result, reason;
        public int rating=200, otherRating=200, round, seq, me, turn, first, lastOpponentMove=-1, delta, searchRange;
        public int[] board;
        public bool rated=true, rematchRequested, opponentRequested, opponentLeft, otherDisconnected;
        public double deadline, now;
    }
    [Serializable] public sealed class OnlineCommand
    {
        public string type, token, roomId, invite;
        public int round, seq, column;
        public bool rated=true;
    }
    public sealed partial class OnlineGameController : MonoBehaviour
    {
        public RankGameApp app;
        public OnlineTransport transport;
        // Empty in the shipped prefab until a separately verified server is configured.
        public string endpoint="", publicGameUrl="https://hwanggok2.github.io/Permission-to-win/";
        public GameObject panel, menu, game, invitation, leaveConfirm;
        public Button randomMatch, friendly, ratedToggle, share, copy, join, close, place, rematch, resultLeave, leaveAccept, leaveCancel;
        public Text title, profile, description, randomLabel, friendlyLabel, ratedLabel, shareLabel, copyLabel, joinLabel;
        public Text myProfile, otherProfile, randomHint, friendlyHint, firstPlayer, turn, resultLabel, statusLabel, placeLabel, rematchLabel, resultLeaveLabel, confirmLabel, confirmAcceptLabel, confirmCancelLabel;
        public InputField inviteUrl;
        public Image[] stones;
        public Image preview;
        public Button[] columns;
        public LastMoveHighlight lastOpponentMove;
        OnlineState state, invitationPreview;
        string pendingInvite, queuedAction, messageKey="", token;
        bool connected, sentMove, leaving, opening;
        bool completedBeforeLeave, advertiseOnLeave;
        int selected=-1, reconnects;
        double stateAt;
        float reconnectAt=-1, heartbeatAt;
        const string TokenKey="stagegame.multiplayer.token.v1";
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")] static extern void PTWShare(string url, string title, string target, bool copyOnly);
#endif
        IEnumerator Start()
        {
            transport.Opened=()=>transport.Send(JsonUtility.ToJson(new OnlineCommand{type="auth",token=token}));
            transport.Received=Receive;transport.Closed=Disconnected;
            randomMatch.onClick.AddListener(()=> { if(state?.status=="queue")Send("cancel");else Begin("queue"); });
            friendly.onClick.AddListener(()=>Begin("create"));
            ratedToggle.onClick.AddListener(()=>Send("setRated",rated:!state.rated));
            share.onClick.AddListener(()=>Share(false));copy.onClick.AddListener(()=>Share(true));
            join.onClick.AddListener(()=> { if(invitationPreview!=null)Send("join",rated:invitationPreview.rated,invite:pendingInvite); });
            close.onClick.AddListener(RequestLeave);leaveAccept.onClick.AddListener(Leave);leaveCancel.onClick.AddListener(()=>{leaveConfirm.SetActive(false);RefreshBoard();});
            resultLeave.onClick.AddListener(Leave);
            place.onClick.AddListener(Place);rematch.onClick.AddListener(()=>Send("rematch"));
            for(int i=0;i<columns.Length;i++){int c=i;columns[i].onClick.AddListener(()=>Select(c));}
            Application.deepLinkActivated+=ReadInvite;
            yield return null;ReadInvite(Application.absoluteURL);RefreshLobbyRating();
#if !UNITY_EDITOR
            if(!opening)StartCoroutine(RefreshLobbyProfile());
#endif
        }
        void ReadInvite(string url)
        {
            var match=Regex.Match(url??"",@"(?:#|[?&])invite=([a-f0-9]{32})(?:&|$)");
            if(!match.Success)return;pendingInvite=match.Groups[1].Value;Open();Begin("preview");
        }
        public void Open()
        {
            app.ShowLobby();app.lobby.SetActive(false);panel.SetActive(true);leaveConfirm.SetActive(false);
            opening=true;leaving=false;messageKey="";Refresh();
            if(!connected && !string.IsNullOrEmpty(endpoint))Connect();
            app.EnsureFirstRules();
        }
        void Connect()
        {
            token=PlayerPrefs.GetString(TokenKey,"");messageKey="mp_connecting";Refresh();transport.Connect(endpoint);reconnectAt=-1;
        }
        void Begin(string action)
        {
            if(string.IsNullOrEmpty(endpoint)){messageKey="mp_unavailable";Refresh();return;}
            if(connected)Send(action,invite:pendingInvite);else { queuedAction=action;if(reconnectAt<0)Connect(); }
        }
        void Send(string action, int column=0, bool rated=true, string invite=null)
        {
            if(!connected)return;
            transport.Send(JsonUtility.ToJson(new OnlineCommand{type=action,roomId=state?.roomId,round=state?.round??0,seq=state?.seq??0,column=column,rated=rated,invite=invite}));
        }
        public void Receive(string json)
        {
            OnlineState incoming;
            try{incoming=JsonUtility.FromJson<OnlineState>(json);}catch{Disconnected(1006);return;}
            if(incoming==null)return;
            if(incoming.type=="auth")
            {
                connected=true;reconnects=0;messageKey="";token=incoming.token;PlayerPrefs.SetString(TokenKey,token);PlayerPrefs.Save();
                if(queuedAction!=null){string action=queuedAction;queuedAction=null;Send(action,invite:pendingInvite);}return;
            }
            if(incoming.type=="error")
            {
                sentMove=false;
                messageKey=incoming.code=="INVITE_EXPIRED"?"mp_invite_expired":incoming.code=="SETTING_CHANGED"?"mp_setting_changed":incoming.code=="AUTH"?"mp_auth_error":"mp_request_failed";
                if(incoming.code=="SETTING_CHANGED")Send("preview",invite:pendingInvite);
                Refresh();return;
            }
            if(incoming.type=="preview"){invitationPreview=incoming;messageKey="";Refresh();return;}
            if(incoming.type!="state")return;
            profileVersion++;ApplyLobbyProfile(incoming);
            if(state==null || incoming.roomId!=state.roomId || incoming.round!=state.round || incoming.seq!=state.seq){selected=-1;sentMove=false;}
            bool newGame=incoming.status=="playing" && (state?.status!="playing" || incoming.round!=state.round || incoming.roomId!=state.roomId);
            if(newGame || incoming.roomId!=state?.roomId || incoming.status=="idle")completedBeforeLeave=false;
            state=incoming;stateAt=Time.realtimeSinceStartupAsDouble;
            if(state.status=="finished" && (state.reason=="four" || state.reason=="draw") && app.RecordOnlineCompletion(state.roomId,state.round))completedBeforeLeave=true;
            if(state.status=="waiting" || state.status=="playing" || state.status=="finished")invitationPreview=null;
            if(leaving && state.status=="idle"){ReturnToLobby(true);return;}
            if(newGame){messageKey="";leaveConfirm.SetActive(false);}
            Refresh();
        }
        void Disconnected(int code)
        {
            connected=false;sentMove=false;
            if(leaving){ReturnToLobby();return;}
            if(!opening)return;
            messageKey=code==4001?"mp_other_session":"mp_reconnecting";
            reconnectAt=code==4001 || reconnects>=5 ? -1 : Time.unscaledTime+Mathf.Min(10,1<<reconnects++);
            if(reconnectAt<0 && code!=4001)messageKey="mp_connection_lost";
            Refresh();
        }
        void Update()
        {
            if(!opening)return;
            if(reconnectAt>=0 && Time.unscaledTime>=reconnectAt)Connect();
            if(connected && Time.unscaledTime>=heartbeatAt){heartbeatAt=Time.unscaledTime+20;transport.Send("ping");}
            if(state?.status=="playing")RefreshTurn();
            if(Input.GetKeyDown(KeyCode.Escape))RequestLeave();
        }
        bool CanPlace => connected && state?.status=="playing" && state.turn==state.me && !sentMove && !leaveConfirm.activeSelf;
        void Select(int column){if(!CanPlace || state.board[35+column]!=0)return;selected=column;RefreshBoard();}
        void Place(){if(!CanPlace || selected<0)return;sentMove=true;Send("move",selected);RefreshBoard();}
        void RequestLeave(){if(state?.status=="playing"){leaveConfirm.SetActive(true);RefreshBoard();}else Leave();}
        void Leave(){if(leaving)return;advertiseOnLeave=completedBeforeLeave && state?.status=="finished";completedBeforeLeave=false;leaveConfirm.SetActive(false);queuedAction=null;leaving=true;if(connected)Send("leave");else ReturnToLobby();}
        void ReturnToLobby(bool confirmed=false){bool showAd=confirmed && advertiseOnLeave;advertiseOnLeave=completedBeforeLeave=false;opening=false;leaving=false;reconnectAt=-1;connected=false;state=null;invitationPreview=null;pendingInvite=null;transport.Close();panel.SetActive(false);app.ReturnFromMultiplayer(showAd);}
        string T(string key)=>app.UiText(key);
        string F(string key,params object[] args)=>app.UiFormat(key,args);
        public void Refresh()
        {
            RefreshLobbyRating();
            if(!opening)return;
            title.text=T("multiplayer_title");profile.text=F("mp_profile",T(state?.rankKey??"mp_rank_2"),state?.rating??200);
            bool inGame=state?.status=="playing" || state?.status=="finished";
            bool waiting=state?.status=="waiting", searching=state?.status=="queue", invited=invitationPreview!=null;
            menu.SetActive(!inGame);game.SetActive(inGame);invitation.SetActive(waiting||invited);
            randomMatch.gameObject.SetActive(!inGame&&!waiting&&!invited);friendly.gameObject.SetActive(!inGame&&!waiting&&!invited&&!searching);
            randomLabel.text=T(searching?"mp_cancel_search":"mp_random");friendlyLabel.text=T("mp_friendly");
            randomHint.text=searching?"":T("mp_random_hint");friendlyHint.text=T("mp_friendly_hint");
            randomMatch.interactable=friendly.interactable=!leaving;
            ratedLabel.text=T((waiting?state.rated:invitationPreview?.rated??true)?"mp_rated_on":"mp_rated_off");
            ratedToggle.interactable=connected&&waiting;
            share.gameObject.SetActive(waiting);copy.gameObject.SetActive(waiting);inviteUrl.gameObject.SetActive(waiting);
            join.gameObject.SetActive(invited);join.interactable=connected;shareLabel.text=T("mp_share");copyLabel.text=T("mp_copy");joinLabel.text=T("mp_join");
            if(waiting)inviteUrl.text=publicGameUrl.TrimEnd('/')+"/#invite="+state.invite;
            description.text=messageKey!=""?T(messageKey):searching?F("mp_searching",state.searchRange):waiting?T("mp_wait_friend"):invited?F("mp_invited",invitationPreview.rating,T(invitationPreview.rated?"mp_rated_on":"mp_rated_off")):T("mp_rules");
            confirmLabel.text=T("mp_leave_warning");confirmAcceptLabel.text=T("mp_leave");confirmCancelLabel.text=T("ui_cancel");
            if(inGame)
            {
                myProfile.text=F("mp_you_profile",T(state.rankKey),state.rating);
                otherProfile.text=F("mp_opponent_profile",T(state.otherRankKey),state.otherRating);
                firstPlayer.text=T(state.first==state.me?"mp_you_first":"mp_other_first")+" · "+T(state.me==0?"mp_black":"mp_white");
                placeLabel.text=T("place");rematchLabel.text=T(state.rematchRequested&&!state.opponentLeft?"mp_rematch_wait":"mp_rematch");
                resultLeaveLabel.text=T("mp_leave");resultLeave.gameObject.SetActive(state.status=="finished");
                rematch.gameObject.SetActive(state.status=="finished");place.gameObject.SetActive(state.status=="playing");
                rematch.interactable=connected&&!state.rematchRequested&&!state.opponentLeft;
                bool leavePrimary=state.opponentLeft;
                resultLeave.GetComponent<Image>().sprite=leavePrimary?app.theme.primary:app.theme.panel;
                resultLeaveLabel.color=leavePrimary?Color.white:Color.black;
                resultLabel.text=state.status=="finished"?F("mp_result_"+state.result,state.delta.ToString("+0;-0;0"),state.rating):T(state.rated?"mp_rated_on":"mp_rated_off");
                statusLabel.text=messageKey!=""?T(messageKey):state.opponentLeft?T("mp_opponent_left"):state.rematchRequested?T("mp_rematch_wait"):state.opponentRequested?T("mp_rematch_request"):state.otherDisconnected?T("mp_other_disconnected"):"";
                RefreshBoard();RefreshTurn();
            }
        }
        void RefreshTurn()
        {
            if(state==null)return;
            int seconds=Mathf.Max(0,Mathf.CeilToInt((float)((state.deadline-state.now)/1000-(Time.realtimeSinceStartupAsDouble-stateAt))));
            turn.text=state.status=="playing"?F(state.turn==state.me?"mp_your_turn":"mp_other_turn",seconds):T("turn_finished");
        }
        void RefreshBoard()
        {
            if(state?.board==null || state.board.Length!=42)return;
            for(int i=0;i<42;i++){stones[i].enabled=state.board[i]!=0;stones[i].sprite=state.board[i]==1?app.theme.blackStone:app.theme.whiteStone;}
            int lastPlayer=state.seq%2==1?state.first:1-state.first;
            lastOpponentMove.Show(stones,!sentMove && state.seq>0 && lastPlayer!=state.me?state.lastOpponentMove:-1);
            for(int i=0;i<7;i++)columns[i].interactable=CanPlace&&state.board[35+i]==0;
            preview.gameObject.SetActive(CanPlace&&selected>=0);
            if(preview.gameObject.activeSelf){int row=0;while(row<6&&state.board[row*7+selected]!=0)row++;if(row<6)preview.rectTransform.anchoredPosition=stones[row*7+selected].rectTransform.anchoredPosition;}
            place.interactable=CanPlace&&selected>=0;
        }
        void Share(bool copyOnly)
        {
            if(state?.status!="waiting")return;
#if UNITY_WEBGL && !UNITY_EDITOR
            PTWShare(inviteUrl.text,T("multiplayer_title"),gameObject.name,copyOnly);
#else
#if UNITY_ANDROID && !UNITY_EDITOR
            if(!copyOnly && Application.platform==RuntimePlatform.Android)
            {
                using(var intent=new AndroidJavaObject("android.content.Intent","android.intent.action.SEND"))
                using(var unity=new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using(var activity=unity.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    intent.Call<AndroidJavaObject>("setType","text/plain");
                    intent.Call<AndroidJavaObject>("putExtra","android.intent.extra.TEXT",inviteUrl.text);
                    using(var chooser=intent.CallStatic<AndroidJavaObject>("createChooser",intent,T("mp_share")))activity.Call("startActivity",chooser);
                }
                return;
            }
#endif
            GUIUtility.systemCopyBuffer=inviteUrl.text;OnShareResult("copied");
#endif
        }
        public void OnShareResult(string result){if(state?.status!="waiting")return;messageKey=result=="copied"?"mp_copied":result=="failed"?"mp_copy_manual":"mp_shared";Refresh();}
        void OnDestroy(){Application.deepLinkActivated-=ReadInvite;transport.Close();}
    }
}
