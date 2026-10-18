using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace StageGame.Tests
{
    public sealed partial class RankFlowTests
    {
        [UnityTest] public IEnumerator LobbyOfferReopensAfterDismissalWithoutRepeatingLaunchPromotion()
        {
            app.ShowLobby();app.SetAdsForTests(new CountingAds());var store=new TestPurchases();app.adOffer.SetPurchases(store);
            OfferLaunch(true);Assert.IsTrue(app.adOffer.Visible);app.adOffer.close.onClick.Invoke();
            foreach(var dismiss in new[]{app.adOffer.close,app.adOffer.outside,app.adOffer.later})
            {
                EventSystem.current.SetSelectedGameObject(app.adOffer.lobbyOpen.gameObject);
                app.adOffer.lobbyOpen.onClick.Invoke();yield return null;Assert.IsTrue(app.adOffer.Visible);
                dismiss.onClick.Invoke();Assert.IsFalse(app.adOffer.Visible);
                Assert.AreSame(app.adOffer.lobbyOpen.gameObject,EventSystem.current.currentSelectedGameObject);
                app.adOffer.CheckLaunch(true);Assert.IsFalse(app.adOffer.Visible);
            }
            Assert.AreEqual(0,store.Buys);Assert.AreEqual(0,store.Restores);
            app.lobbyHelp.onClick.Invoke();app.adOffer.OpenFromLobby();Assert.IsFalse(app.adOffer.Visible);app.helpClose.onClick.Invoke();
            app.lobbySettings.onClick.Invoke();app.adOffer.OpenFromLobby();Assert.IsFalse(app.adOffer.Visible);app.settingsClose.onClick.Invoke();
            app.online.Open();app.adOffer.OpenFromLobby();Assert.IsFalse(app.adOffer.Visible);app.online.close.onClick.Invoke();
            app.adOffer.lobbyOpen.onClick.Invoke();Assert.IsTrue(app.adOffer.Visible);app.StartStage(0);Assert.IsFalse(app.adOffer.Visible);
        }
        [UnityTest] public IEnumerator LobbyOfferUnavailableRemainsInformationalAndUpdatesWhenReady()
        {
            app.ShowLobby();var store=new TestPurchases();app.adOffer.SetPurchases(store);app.SetAdsForTests(new NoFillAds());
            OfferLaunch(true);Assert.IsFalse(app.adOffer.Visible);
            app.adOffer.lobbyOpen.onClick.Invoke();yield return null;yield return null;
            Assert.IsTrue(app.adOffer.Visible);Assert.AreEqual(app.UiText("coming_soon"),app.adOffer.buyLabel.text);
            Assert.IsFalse(app.adOffer.buy.interactable);Assert.IsFalse(app.adOffer.restore.interactable);
            app.adOffer.buy.onClick.Invoke();app.adOffer.restore.onClick.Invoke();Assert.AreEqual(0,store.Buys);Assert.AreEqual(0,store.Restores);
            app.SetAdsForTests(new CountingAds());store.Known=false;yield return null;Assert.IsFalse(app.adOffer.buy.interactable);
            store.Known=true;store.Ready=false;yield return null;Assert.IsFalse(app.adOffer.buy.interactable);
            store.Ready=true;store.Price="";yield return null;Assert.IsFalse(app.adOffer.buy.interactable);
            store.Price="₩3,900";yield return null;Assert.IsTrue(app.adOffer.buy.interactable);StringAssert.Contains(store.Price,app.adOffer.buyLabel.text);
            app.SetLanguage("en");Assert.AreEqual("Remove ads",app.adOffer.lobbyLabel.text);
            store.Ready=false;yield return null;Assert.IsTrue(app.adOffer.Visible);Assert.AreEqual(app.UiText("coming_soon"),app.adOffer.buyLabel.text);
        }
        [UnityTest] public IEnumerator LobbyOfferReopeningPreservesPendingPurchaseAndOwnershipFocus()
        {
            app.ShowLobby();app.SetAdsForTests(new CountingAds());var store=new TestPurchases();app.adOffer.SetPurchases(store);
            EventSystem.current.SetSelectedGameObject(app.adOffer.lobbyOpen.gameObject);app.adOffer.lobbyOpen.onClick.Invoke();
            app.adOffer.buy.onClick.Invoke();store.Callback(PurchaseResult.Pending);app.adOffer.close.onClick.Invoke();
            app.adOffer.lobbyOpen.onClick.Invoke();Assert.IsTrue(app.adOffer.Visible);Assert.IsFalse(app.adOffer.buy.interactable);
            Assert.AreEqual(app.UiText("ad_offer_pending"),app.adOffer.status.text);app.adOffer.buy.onClick.Invoke();Assert.AreEqual(1,store.Buys);
            store.Callback(PurchaseResult.Completed);app.adOffer.outside.onClick.Invoke();app.adOffer.lobbyOpen.onClick.Invoke();
            Assert.IsFalse(app.adOffer.buy.interactable);Assert.AreEqual(1,store.Buys);
            store.Owned=true;store.Callback(PurchaseResult.Completed);yield return null;Canvas.ForceUpdateCanvases();
            Assert.IsFalse(app.adOffer.Visible);Assert.IsFalse(app.adOffer.lobbyOpen.gameObject.activeSelf);
            Assert.AreSame(app.multiplayer.gameObject,EventSystem.current.currentSelectedGameObject);
            var rect=(RectTransform)app.multiplayer.transform;Assert.AreEqual(RankUiMetrics.U(680),rect.rect.width,.1f);Assert.AreEqual(0,rect.anchoredPosition.x,.1f);
            app.adOffer.OpenFromLobby();Assert.IsFalse(app.adOffer.Visible);
        }
        void AssertLobbyOfferFits(float ratio)
        {
            var camera=app.GetComponent<Canvas>().worldCamera;
            var area=ScreenRect((RectTransform)app.lobby.transform,camera,ratio);
            var multi=ScreenRect((RectTransform)app.multiplayer.transform,camera,ratio);
            var entry=ScreenRect((RectTransform)app.adOffer.lobbyOpen.transform,camera,ratio);
            var list=ScreenRect(app.stageScroll.viewport,camera,ratio);
            Assert.Greater(entry.xMin,multi.xMax);Assert.AreEqual(multi.yMin,entry.yMin,.1f);
            Assert.Greater(list.yMin,multi.yMax,"Footer must stay below the stage list.");
            foreach(var button in new[]{app.multiplayer,app.adOffer.lobbyOpen})
            {
                var r=ScreenRect((RectTransform)button.transform,camera,ratio);var label=button.GetComponentInChildren<Text>();
                Assert.GreaterOrEqual(r.height,47.99f);Assert.GreaterOrEqual(r.width,43.99f);
                Assert.GreaterOrEqual(r.yMin,area.yMin-.1f);Assert.LessOrEqual(r.yMax,area.yMax+.1f);
                Assert.GreaterOrEqual(r.xMin,area.xMin-.1f);Assert.LessOrEqual(r.xMax,area.xMax+.1f);
                Assert.LessOrEqual(label.preferredWidth,label.rectTransform.rect.width+1);Assert.LessOrEqual(label.preferredHeight,label.rectTransform.rect.height+1);
            }
        }
        [UnityTest] public IEnumerator LobbyOfferLayoutsFitBothLanguagesAndHostAreas()
        {
            app.ShowLobby();
            foreach(string language in new[]{"ko","en"})
            {
                app.SetLanguage(language);
                foreach(int viewport in new[]{0,1,2,3,4})
                {
                    int width=viewport==0||viewport==4?1080:viewport==1?390:320,height=viewport==0||viewport==4?1920:viewport==1?844:568;
                    bool host=viewport==3||viewport==4;float ratio=viewport==0||viewport==4?3:1;
                    yield return Capture(language+"-"+viewport+"-lobby",app,width,height,host,validate:()=>AssertLobbyOfferFits(ratio),directory:"Evidence/lobby-ad-entry-20261018/screens",scaledCanvas:true,pixelRatio:ratio);
                    if(viewport==1||viewport==3)
                    {
                        app.adOffer.lobbyOpen.onClick.Invoke();
                        yield return Capture(language+"-"+viewport+"-offer",app,width,height,host,validate:()=>{AssertOfferFits(ratio);Assert.IsFalse(app.adOffer.buy.interactable);},directory:"Evidence/lobby-ad-entry-20261018/screens",scaledCanvas:true,pixelRatio:ratio);
                        app.adOffer.close.onClick.Invoke();
                    }
                }
            }
            app.SetAdsForTests(new CountingAds());app.adOffer.SetPurchases(new TestPurchases{Owned=true});yield return null;
            yield return Capture("en-owned-lobby",app,390,844,false,directory:"Evidence/lobby-ad-entry-20261018/screens",scaledCanvas:true);
        }
    }
}
