using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace StageGame.Tests
{
    public sealed partial class RankFlowTests
    {
        [UnityTest] public IEnumerator LobbyRatingUsesOwnStateAndSurvivesExitAndLanguageChange()
        {
            app.ShowLobby();var online=app.online;online.Open();
            online.Receive("{\"type\":\"state\",\"status\":\"idle\",\"rating\":416,\"rankKey\":\"mp_rank_4\"}");
            Assert.AreEqual(app.UiFormat("ai_rating",416),online.lobbyRatingLabel.text);
            online.Receive("{\"type\":\"preview\",\"rating\":900,\"rated\":true}");
            Assert.AreEqual(app.UiFormat("ai_rating",416),online.lobbyRatingLabel.text);
            online.close.onClick.Invoke();yield return null;Assert.IsTrue(app.lobby.activeSelf);
            app.SetLanguage("en");Assert.AreEqual(app.UiFormat("ai_rating",416),online.lobbyRatingLabel.text);
            online.Receive("{\"type\":\"state\",\"status\":\"idle\",\"rating\":99999,\"rankKey\":\"mp_rank_18\"}");
            yield return Capture("en-high-rating",app,320,568,true,validate:()=>AssertLobbyOfferFits(1),directory:"Evidence/lobby-redesign-20261018/screens",scaledCanvas:true);
        }
        [UnityTest] public IEnumerator LobbyProfileQueryDoesNotOpenOrReplaceGameTransport()
        {
            string endpoint=System.Environment.GetEnvironmentVariable("STAGEGAME_TEST_MULTIPLAYER_URL");
            if(string.IsNullOrEmpty(endpoint)){Assert.Ignore("Requires explicit local profile server");yield break;}
            const string identityKey="stagegame.multiplayer.token.v1",ratingKey="stagegame.multiplayer.rating.v1",ownerKey=ratingKey+".owner";
            bool hadIdentity=PlayerPrefs.HasKey(identityKey),hadRating=PlayerPrefs.HasKey(ratingKey);string identity=PlayerPrefs.GetString(identityKey);int rating=PlayerPrefs.GetInt(ratingKey);
            bool hadOwner=PlayerPrefs.HasKey(ownerKey);string owner=PlayerPrefs.GetString(ownerKey);
            var online=app.online;PlayerPrefs.DeleteKey(identityKey);PlayerPrefs.DeleteKey(ratingKey);
            try
            {
                online.endpoint=endpoint;app.ShowLobby();
                yield return online.RefreshLobbyProfile();Assert.AreEqual(app.UiFormat("ai_rating",200),online.lobbyRatingLabel.text);
                Assert.IsFalse(online.panel.activeSelf);Assert.IsFalse(PlayerPrefs.HasKey(identityKey));
                online.Open();yield return WaitOnline(()=>PlayerPrefs.HasKey(identityKey),"Authenticated once");
                yield return WaitOnline(()=>online.description.text==app.UiText("mp_rules"),"Server state received");
                online.close.onClick.Invoke();yield return WaitOnline(()=>app.lobby.activeSelf,"Leave confirmed");
                var before=PlayerPrefs.GetString(identityKey);yield return online.RefreshLobbyProfile();
                Assert.AreEqual(before,PlayerPrefs.GetString(identityKey));Assert.AreEqual(app.UiFormat("ai_rating",200),online.lobbyRatingLabel.text);
                Assert.IsFalse((bool)typeof(OnlineGameController).GetField("connected",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(online));
                Assert.IsFalse((bool)typeof(OnlineGameController).GetField("profileLoading",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(online),"Previous query finished");
                PlayerPrefs.SetString(identityKey,new string('d',64));online.RefreshLobbyRating();
                Assert.AreEqual(app.UiFormat("ai_rating","—"),online.lobbyRatingLabel.text,"Changed identity must not inherit the previous player's cache");
                online.Receive("{\"type\":\"state\",\"status\":\"idle\",\"playerId\":\"fixture\",\"rating\":416,\"rankKey\":\"mp_rank_4\"}");
                Assert.AreEqual(app.UiFormat("ai_rating",416),online.lobbyRatingLabel.text);
                Assert.IsFalse((bool)typeof(OnlineGameController).GetField("opening",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(online),"Closed lobby query");
                yield return online.RefreshLobbyProfile();
                Assert.AreEqual(app.UiFormat("ai_rating","—"),online.lobbyRatingLabel.text,"401 clears an unverified cached identity");Assert.IsFalse(PlayerPrefs.HasKey(ratingKey));
            }
            finally
            {
                online.endpoint="";if(hadIdentity)PlayerPrefs.SetString(identityKey,identity);else PlayerPrefs.DeleteKey(identityKey);
                if(hadRating)PlayerPrefs.SetInt(ratingKey,rating);else PlayerPrefs.DeleteKey(ratingKey);PlayerPrefs.Save();
                if(hadOwner)PlayerPrefs.SetString(ownerKey,owner);else PlayerPrefs.DeleteKey(ownerKey);PlayerPrefs.Save();
            }
        }
    }
}
