using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace StageGame.Tests
{
    public sealed partial class RankFlowTests
    {
        sealed class TestPurchases:IAdRemovalPurchases
        {
            public bool Ready=true,Known=true,Owned;public string Price="₩3,900";public int Buys,Restores;public Action<PurchaseResult> Callback;
            public bool ProductReady=>Ready;public bool EntitlementChecked=>Known;public bool OwnsAdRemoval=>Owned;public string DisplayPrice=>Price;
            public void Purchase(Action<PurchaseResult> callback){Buys++;Callback=callback;}
            public void Restore(Action<PurchaseResult> callback){Restores++;Callback=callback;}
        }
        sealed class CountingAds:IAdService
        {
            public int Interstitials,Rewards;public AdResult Result=AdResult.Completed;
            public string Status=>"test";
            public void ShowInterstitial(Action<AdResult> callback){Interstitials++;callback(Result);callback(Result);}
            public void ShowRewarded(Action<AdResult> callback){Rewards++;callback(Result);}
            public void CancelPending(){}
        }
        void OfferLaunch(bool returning)
        {
            typeof(AdRemovalOffer).GetField("launchChecked",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(app.adOffer,false);
            app.adOffer.CheckLaunch(returning);
        }
        void OnlineConnected()=>typeof(OnlineGameController).GetField("connected",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(app.online,true);
        static OnlineState AdRoom(string room,int round=1,string status="finished",string reason="four")=>new OnlineState
        {
            type="state",roomId=room,round=round,status=status,reason=reason,result=reason=="draw"?"draw":"win",board=new int[42],rankKey="mp_rank_2",otherRankKey="mp_rank_2",seq=7
        };
        void ReceiveRoom(OnlineState state)=>app.online.Receive(JsonUtility.ToJson(state));
        void ConfirmOnlineLeave(){app.online.resultLeave.onClick.Invoke();ReceiveRoom(new OnlineState{type="state",status="idle",rankKey="mp_rank_2"});}

        [UnityTest] public IEnumerator OfferAutoLaunchRequiresPreviousCompletedGame()
        {
            app.ShowLobby();var store=new TestPurchases();app.SetAdsForTests(new CountingAds());app.adOffer.SetPurchases(store);
            OfferLaunch(false);Assert.IsFalse(app.adOffer.Visible);
            PlayerPrefs.SetString(Key,"{\"saveVersion\":1,\"unlockedStage\":1,\"completedMatches\":1,\"rulesAcknowledged\":true}");PlayerPrefs.Save();
            SceneManager.LoadScene("Main");yield return null;
            app=UnityEngine.Object.FindFirstObjectByType<RankGameApp>();app.online.endpoint="";app.SetAdsForTests(new CountingAds());app.adOffer.SetPurchases(store);
            yield return null;yield return null;yield return null;Assert.IsTrue(app.adOffer.Visible);
            app.adOffer.close.onClick.Invoke();app.ShowLobby();yield return null;Assert.IsFalse(app.adOffer.Visible);
            app.adOffer.CheckLaunch(true);Assert.IsFalse(app.adOffer.Visible,"Dismissed launch must not be promoted again.");
        }
        [UnityTest] public IEnumerator OfferGatesAndDismissalDoNotStartPurchases()
        {
            app.ShowLobby();var store=new TestPurchases();app.adOffer.SetPurchases(store);
            app.SetAdsForTests(new NoFillAds());OfferLaunch(true);Assert.IsFalse(app.adOffer.Visible);
            app.SetAdsForTests(new CountingAds());store.Ready=false;OfferLaunch(true);Assert.IsFalse(app.adOffer.Visible);
            store.Ready=true;store.Known=false;OfferLaunch(true);Assert.IsFalse(app.adOffer.Visible);store.Known=true;
            store.Ready=true;store.Price="";OfferLaunch(true);Assert.IsFalse(app.adOffer.Visible);
            store.Price="₩3,900";store.Owned=true;OfferLaunch(true);Assert.IsFalse(app.adOffer.Visible);
            store.Owned=false;
            foreach(var button in new[]{app.adOffer.close,app.adOffer.outside,app.adOffer.later})
            {
                OfferLaunch(true);Assert.IsTrue(app.adOffer.Visible);button.onClick.Invoke();Assert.IsFalse(app.adOffer.Visible);
                app.adOffer.CheckLaunch(true);Assert.IsFalse(app.adOffer.Visible);
            }
            Assert.AreEqual(0,store.Buys);Assert.AreEqual(0,store.Restores);
            Assert.IsNull(app.adOffer.close.GetComponent<Image>().sprite,"X must not use a bordered panel sprite.");
            Assert.AreEqual(0,app.adOffer.close.GetComponents<Outline>().Length);
            yield return null;
        }
        [UnityTest] public IEnumerator OfferNavigationAndInvitesConsumeStartupOpportunity()
        {
            app.ShowLobby();PlayerPrefs.SetString(Key,"{\"saveVersion\":1,\"unlockedStage\":1,\"completedMatches\":1,\"rulesAcknowledged\":true}");PlayerPrefs.Save();
            SceneManager.LoadScene("Main");yield return null;app=UnityEngine.Object.FindFirstObjectByType<RankGameApp>();app.online.endpoint="";
            app.SetAdsForTests(new CountingAds());app.adOffer.SetPurchases(new TestPurchases());app.online.Open();
            yield return null;yield return null;yield return null;Assert.IsFalse(app.adOffer.Visible);
            app.online.close.onClick.Invoke();yield return null;Assert.IsTrue(app.lobby.activeSelf);Assert.IsFalse(app.adOffer.Visible);
        }
        [UnityTest] public IEnumerator PurchaseNeedsVerifiedEntitlementAndPendingCanBeClosed()
        {
            app.ShowLobby();var ads=new CountingAds();app.SetAdsForTests(ads);var store=new TestPurchases();app.adOffer.SetPurchases(store);OfferLaunch(true);
            app.adOffer.buy.onClick.Invoke();app.adOffer.buy.onClick.Invoke();Assert.AreEqual(1,store.Buys);
            store.Callback(PurchaseResult.Completed);Assert.IsFalse(app.AdRemovalOwned);Assert.IsTrue(app.adOffer.Visible);Assert.IsFalse(app.adOffer.buy.interactable);
            app.adOffer.close.onClick.Invoke();Assert.IsFalse(app.adOffer.Visible);
            store.Owned=true;store.Callback(PurchaseResult.Completed);yield return null;Assert.IsTrue(app.AdRemovalOwned);
            app.StartStage(0);app.hint.onClick.Invoke();yield return AiReady();Assert.AreEqual(0,ads.Rewards);Assert.IsTrue(Field<RewardGate>("rewardGate").Granted);
            app.hint.onClick.Invoke();Assert.AreEqual(0,ads.Rewards);
            Inject(Replay(1,0,6,1,6,2,5,3));Call("Finish");yield return ResultReady();Assert.AreEqual(0,ads.Interstitials);
            app.ReturnFromMultiplayer(true);yield return null;Assert.AreEqual(0,ads.Interstitials);
        }
        [UnityTest] public IEnumerator PurchaseCancelFailureAndRestoreLeavePlayAvailable()
        {
            app.ShowLobby();app.SetAdsForTests(new CountingAds());var store=new TestPurchases();app.adOffer.SetPurchases(store);OfferLaunch(true);
            app.adOffer.buy.onClick.Invoke();store.Callback(PurchaseResult.Cancelled);Assert.AreEqual("",app.adOffer.status.text);Assert.IsTrue(app.adOffer.buy.interactable);
            app.adOffer.buy.onClick.Invoke();store.Callback(PurchaseResult.Failed);Assert.AreEqual(app.UiText("ad_offer_failed"),app.adOffer.status.text);
            app.adOffer.restore.onClick.Invoke();store.Callback(PurchaseResult.NothingToRestore);Assert.AreEqual(app.UiText("ad_offer_none"),app.adOffer.status.text);
            app.adOffer.restore.onClick.Invoke();store.Owned=true;store.Callback(PurchaseResult.Completed);yield return null;
            Assert.IsFalse(app.adOffer.Visible,"Verified purchase closes the offer.");Assert.IsTrue(app.AdRemovalOwned,"The verified store entitlement remains owned.");Assert.IsTrue(app.lobby.activeSelf,"Purchase/restore must stay in the lobby.");Assert.AreEqual(2,store.Restores);
        }
        [UnityTest] public IEnumerator StoryAdsRunOnceFromFirstCompletedMatchWithoutCooldown()
        {
            var ads=new CountingAds();app.SetAdsForTests(ads);
            for(int i=0;i<3;i++)
            {
                app.StartStage(0);Progress.Data.lastInterstitialUtc=long.MaxValue;
                Inject(i==2?Replay(1,0,2,2,2,2,2,2,0,0,0,0,0,1,3,3,3,3,3,3,1,1,1,1,1,4,6,6,6,6,6,6,4,4,4,4,4,5,5,5,5,5,5):Replay(i==1?-1:1,0,6,1,6,2,5,3));Call("Finish");Call("Finish");yield return ResultReady();
                Assert.AreEqual(i+1,ads.Interstitials);Assert.AreEqual(i+1,Progress.Data.completedMatches);
            }
            ads.Result=AdResult.Unavailable;app.StartStage(0);Inject(Replay(1,0,6,1,6,2,5,3));Call("Finish");yield return ResultReady();Call("Finish");
            Assert.AreEqual(4,ads.Interstitials);Assert.IsTrue(app.resultLobby.interactable);
            app.StartStage(0);app.ShowLobby();Assert.AreEqual(4,ads.Interstitials,"Abandoned stage is not a completion.");
        }
        [UnityTest] public IEnumerator MultiplayerAdsWaitForConfirmedLobbyReturnAndDeduplicateRounds()
        {
            var ads=new CountingAds();app.SetAdsForTests(ads);app.online.Open();OnlineConnected();
            var state=AdRoom("complete-room");ReceiveRoom(state);ReceiveRoom(state);Assert.AreEqual(1,Progress.Data.completedOnlineMatches);Assert.AreEqual(0,ads.Interstitials);
            app.online.rematch.onClick.Invoke();state.status="playing";state.round=2;ReceiveRoom(state);Assert.AreEqual(0,ads.Interstitials);
            state.status="finished";state.reason="draw";state.result="draw";ReceiveRoom(state);Assert.AreEqual(2,Progress.Data.completedOnlineMatches);
            app.online.resultLeave.onClick.Invoke();Assert.AreEqual(0,ads.Interstitials,"Leave must be acknowledged first.");
            ReceiveRoom(new OnlineState{type="state",status="idle",rankKey="mp_rank_2"});yield return null;Assert.AreEqual(1,ads.Interstitials);Assert.IsTrue(app.lobby.activeSelf);
            ReceiveRoom(new OnlineState{type="state",status="idle",rankKey="mp_rank_2"});Assert.AreEqual(1,ads.Interstitials);
            app.online.Open();OnlineConnected();ReceiveRoom(state);ConfirmOnlineLeave();yield return null;Assert.AreEqual(1,ads.Interstitials,"Reloaded finished round cannot grant another opportunity.");
            Assert.AreEqual(0,Progress.Data.completedMatches,"Online results must not unlock story stages.");
        }
        [UnityTest] public IEnumerator MultiplayerAbortAndUnconfirmedLeaveDoNotShowAds()
        {
            var ads=new CountingAds();app.SetAdsForTests(ads);
            foreach(string reason in new[]{"left","timeout","server_pause","both_disconnected"})
            {
                app.online.Open();OnlineConnected();ReceiveRoom(AdRoom(reason,reason:reason));ConfirmOnlineLeave();yield return null;Assert.AreEqual(0,ads.Interstitials);
            }
            foreach(string status in new[]{"idle","queue","waiting","playing"})
            {
                app.online.Open();OnlineConnected();ReceiveRoom(AdRoom(status,status:status,reason:""));
                app.online.close.onClick.Invoke();if(status=="playing")app.online.leaveAccept.onClick.Invoke();ReceiveRoom(new OnlineState{type="state",status="idle",rankKey="mp_rank_2"});yield return null;Assert.AreEqual(0,ads.Interstitials);
            }
            app.online.Open();OnlineConnected();ReceiveRoom(AdRoom("unconfirmed"));app.online.resultLeave.onClick.Invoke();
            typeof(OnlineGameController).GetMethod("Disconnected",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(app.online,new object[]{1006});yield return null;
            Assert.AreEqual(0,ads.Interstitials);Assert.IsTrue(app.lobby.activeSelf);
        }
        [UnityTest] public IEnumerator LobbyAdPauseAndNavigationRejectLateCallbacks()
        {
            var ads=new PendingAds();app.SetAdsForTests(ads);app.ReturnFromMultiplayer(true);Assert.IsFalse(app.lobbyInput.interactable);var callback=ads.Interstitial;
            Call("OnApplicationPause",true);Call("OnApplicationPause",false);long before=Progress.Data.lastInterstitialUtc;callback(AdResult.Completed);yield return null;
            Assert.AreEqual(before,Progress.Data.lastInterstitialUtc);Assert.IsTrue(app.lobbyInput.interactable);
            app.ReturnFromMultiplayer(true);callback=ads.Interstitial;app.StartStage(0);callback(AdResult.Completed);yield return null;Assert.AreEqual(before,Progress.Data.lastInterstitialUtc);
            app.ReturnFromMultiplayer(true);float until=Time.realtimeSinceStartup+5;while(!app.lobbyInput.interactable && Time.realtimeSinceStartup<until)yield return null;
            Assert.IsTrue(app.lobbyInput.interactable);ads.Interstitial(AdResult.Completed);Assert.AreEqual(before,Progress.Data.lastInterstitialUtc);
        }
        sealed class ThrowingAds:IAdService
        {
            public string Status=>"test";
            public void ShowRewarded(Action<AdResult> callback)=>throw new InvalidOperationException();
            public void ShowInterstitial(Action<AdResult> callback)=>throw new InvalidOperationException();
            public void CancelPending(){}
        }
        [UnityTest] public IEnumerator AdExceptionsAndPreviousRoomsCannotBlockOrChargeNewSessions()
        {
            app.SetAdsForTests(new ThrowingAds());app.StartStage(0);Inject(Replay(1,0,6,1,6,2,5,3));Call("Finish");yield return ResultReady();
            app.ReturnFromMultiplayer(true);yield return null;Assert.IsTrue(app.lobbyInput.interactable);
            var ads=new CountingAds();app.SetAdsForTests(ads);app.online.Open();OnlineConnected();ReceiveRoom(AdRoom("expired-room"));
            ReceiveRoom(new OnlineState{type="state",status="idle",rankKey="mp_rank_2"});ReceiveRoom(AdRoom("new-room",reason:"timeout"));ConfirmOnlineLeave();yield return null;Assert.AreEqual(0,ads.Interstitials);
            app.online.Open();OnlineConnected();ReceiveRoom(AdRoom("rematch-room"));ReceiveRoom(AdRoom("rematch-room",2,"playing",""));
            ReceiveRoom(AdRoom("rematch-room",2,reason:"left"));ConfirmOnlineLeave();yield return null;Assert.AreEqual(0,ads.Interstitials);
        }
        void AssertOfferFits(float ratio)
        {
            var offer=app.adOffer;var camera=app.GetComponent<Canvas>().worldCamera;var area=ScreenRect((RectTransform)offer.panel.transform,camera,ratio);
            Assert.AreEqual(0,offer.panel.GetComponentsInChildren<ScrollRect>().Length);
            foreach(var graphic in offer.panel.GetComponentsInChildren<Graphic>())
            {
                if(!graphic.enabled)continue;var r=ScreenRect(graphic.rectTransform,camera,ratio);
                Assert.GreaterOrEqual(r.yMin,area.yMin-.1f,"Bottom: "+graphic.name);Assert.LessOrEqual(r.yMax,area.yMax+.1f,"Top: "+graphic.name);
                Assert.GreaterOrEqual(r.xMin,area.xMin-.1f,"Left: "+graphic.name);Assert.LessOrEqual(r.xMax,area.xMax+.1f,"Right: "+graphic.name);
                if(graphic is Text t)Assert.LessOrEqual(t.preferredHeight,t.rectTransform.rect.height+1,"Text clipped: "+t.name+" "+t.text);
            }
            foreach(var b in new[]{offer.close,offer.buy,offer.later,offer.restore})
            {
                var r=ScreenRect((RectTransform)b.transform,camera,ratio);Assert.GreaterOrEqual(r.width,43.99f);Assert.GreaterOrEqual(r.height,b==offer.buy?47.99f:43.99f);
            }
            // The frame consumes clicks without activating the underlying outside close button.
            var frame=offer.panel.transform.Find("Frame");
            var pointer=new PointerEventData(EventSystem.current){position=RectTransformUtility.WorldToScreenPoint(camera,frame.position),button=PointerEventData.InputButton.Left};
            var hits=new System.Collections.Generic.List<RaycastResult>();EventSystem.current.RaycastAll(pointer,hits);Assert.IsNotEmpty(hits);var hit=hits[0].gameObject;
            Assert.IsTrue(hit.transform.IsChildOf(frame));Assert.AreNotEqual(offer.outside.gameObject,ExecuteEvents.GetEventHandler<IPointerClickHandler>(hit));
        }
        [UnityTest] public IEnumerator OfferSavedLayoutFitsBothLanguagesAndSmallHostAreas()
        {
            app.ShowLobby();app.SetAdsForTests(new CountingAds());app.adOffer.SetPurchases(new TestPurchases());OfferLaunch(true);
            foreach(string language in new[]{"ko","en"})
            {
                app.SetLanguage(language);
                foreach(int viewport in new[]{0,1,2,3,4})
                {
                    int width=viewport==0||viewport==4?1080:viewport==1?390:320,height=viewport==0||viewport==4?1920:viewport==1?844:568;
                    bool host=viewport==3||viewport==4;float ratio=viewport==0||viewport==4?3:1;
                    yield return Capture(language+"-"+viewport+"-offer",app,width,height,host,validate:()=>AssertOfferFits(ratio),directory:"Evidence/ad-offer-implementation-20261018/screens",scaledCanvas:true,pixelRatio:ratio);
                }
            }
            app.adOffer.buy.onClick.Invoke();app.SetLanguage("ko");
            yield return Capture("ko-pending-small",app,320,568,true,validate:()=>AssertOfferFits(1),directory:"Evidence/ad-offer-implementation-20261018/screens",scaledCanvas:true);
        }
    }
}

