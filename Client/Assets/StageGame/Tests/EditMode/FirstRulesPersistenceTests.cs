using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace StageGame.Tests
{
    public sealed partial class RankFlowTests
    {
        [UnityTest] public IEnumerator UnplayedSaveKeepsRulesUntilPlayerClosesThem()
        {
            // A visit/settings save is not evidence that the player saw the rules.
            app.ShowLobby();
            PlayerPrefs.SetString(Key,"{\"saveVersion\":1,\"unlockedStage\":1,\"completedMatches\":0,\"language\":\"ko\"}");
            PlayerPrefs.Save();SceneManager.LoadScene("Main");
            yield return null;yield return null;
            app=Object.FindFirstObjectByType<RankGameApp>();app.online.endpoint="";
            Assert.IsTrue(app.lobby.activeSelf);
            Assert.IsTrue(app.help.activeSelf,"A saved visit without a completed game must not consume the first rules.");
            yield return Capture("saved-visit-rules-1080",app,1080,1920,true,directory:"Evidence/first-rules-persistence-20261018/screens");
            app.SetLanguage("en");Call("OnApplicationPause",true);Call("OnApplicationPause",false);
            SceneManager.LoadScene("Main");yield return null;yield return null;
            app=Object.FindFirstObjectByType<RankGameApp>();app.online.endpoint="";
            Assert.IsTrue(app.help.activeSelf,"Leaving without acknowledging rules must not suppress them next time.");
            // An incoming invitation opens this same path. The rules must stay above its panel.
            app.online.Open();
            Assert.IsTrue(app.help.activeSelf);
            Assert.Greater(app.help.transform.GetSiblingIndex(),app.online.panel.transform.GetSiblingIndex());
            var caption=app.help.transform.Find("Frame/DropCaption").GetComponent<UnityEngine.UI.Text>();
            Assert.AreEqual(Data.Text("mp_help_drop_caption"),caption.text,"Online rules must explain the server's timeout, not single-player auto-placement.");
            app.SetLanguage("ko");Assert.AreEqual(Data.Text("mp_help_drop_caption"),caption.text);
            yield return Capture("online-first-rules-ko-390",app,390,844,true,directory:"Evidence/first-rules-persistence-20261018/screens");
            app.SetLanguage("en");Assert.AreEqual(Data.Text("mp_help_drop_caption"),caption.text);
            yield return Capture("online-first-rules-390",app,390,844,true,directory:"Evidence/first-rules-persistence-20261018/screens");
            app.helpClose.onClick.Invoke();Assert.IsFalse(app.help.activeSelf);
            SceneManager.LoadScene("Main");yield return null;yield return null;
            app=Object.FindFirstObjectByType<RankGameApp>();app.online.endpoint="";
            Assert.IsTrue(app.lobby.activeSelf);Assert.IsFalse(app.help.activeSelf,"An explicit acknowledgement persists.");
        }
    }
}
