using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace StageGame.Tests
{
    public sealed class StageMapTests
    {
        const string Key = "stagegame.progress.v1";
        [Test]
        public void RankMigrationRejectsUnsavedScenesBeforeWritingAssets()
        {
            if (!Application.isBatchMode) Assert.Ignore("This scene-safety regression runs in the owned batch editor only.");
            var scene = UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.EmptyScene, UnityEditor.SceneManagement.NewSceneMode.Single);
            string path = Editor.RankUiBuilder.AppRootPath;
            var before = System.IO.File.ReadAllBytes(path);
            try
            {
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
                var error = Assert.Throws<System.InvalidOperationException>(() => Editor.RankUiBuilder.Apply());
                StringAssert.Contains("Save scene changes", error.Message);
                CollectionAssert.AreEqual(before, System.IO.File.ReadAllBytes(path));
                Assert.IsTrue(scene.isDirty, "The migration must not save or discard the user's scene.");
            }
            finally { UnityEditor.SceneManagement.EditorSceneManager.CloseScene(scene, true); }
        }

        [TestCase(1)]
        [TestCase(12)]
        [TestCase(30)]
        public void LegacyProgressInfersOnlyStagesBeforeTheUnlock(int unlocked)
        {
            bool had = PlayerPrefs.HasKey(Key); string old = PlayerPrefs.GetString(Key);
            try
            {
                PlayerPrefs.SetString(Key, "{\"saveVersion\":1,\"unlockedStage\":" + unlocked + ",\"completedMatches\":9,\"lastInterstitialMatch\":4,\"lastInterstitialUtc\":123,\"soundEnabled\":false,\"language\":\"en\",\"processedMatchIds\":[\"old\"]}");
                var store = new ProgressStore(30);
                Assert.AreEqual(unlocked - 1, store.Data.highestClearedStage);
                Assert.AreEqual(unlocked, store.Data.unlockedStage);
                Assert.AreEqual(9, store.Data.completedMatches); Assert.AreEqual(4, store.Data.lastInterstitialMatch);
                Assert.AreEqual(123, store.Data.lastInterstitialUtc); Assert.IsFalse(store.Data.soundEnabled);
                Assert.AreEqual("en", store.Data.language); Assert.IsFalse(store.RecordResult("old", 1, true, 30));
                Assert.IsTrue(store.Save());
                Assert.AreEqual(unlocked - 1, new ProgressStore(30).Data.highestClearedStage);
            }
            finally { if (had) PlayerPrefs.SetString(Key, old); else PlayerPrefs.DeleteKey(Key); PlayerPrefs.Save(); }
        }

        [Test]
        public void FinalClearSurvivesReloadAndReplayWithoutDuplicateCounters()
        {
            bool had = PlayerPrefs.HasKey(Key); string old = PlayerPrefs.GetString(Key);
            try
            {
                PlayerPrefs.SetString(Key, "{\"saveVersion\":1,\"unlockedStage\":30,\"completedMatches\":9,\"lastInterstitialMatch\":4,\"lastInterstitialUtc\":123,\"language\":\"en\"}");
                var store = new ProgressStore(30);
                Assert.AreEqual(29, store.Data.highestClearedStage);
                Assert.IsTrue(store.RecordResult("loss30", 30, false, 30));
                Assert.AreEqual(29, store.Data.highestClearedStage);
                Assert.IsTrue(store.RecordResult("win30", 30, true, 30));
                Assert.AreEqual(30, new ProgressStore(30).Data.highestClearedStage);
                Assert.IsFalse(new ProgressStore(30).RecordResult("win30", 30, true, 30));
                Assert.IsTrue(store.RecordResult("replay1", 1, true, 30));
                var reloaded = new ProgressStore(30);
                Assert.AreEqual(30, reloaded.Data.highestClearedStage); Assert.AreEqual(30, reloaded.Data.unlockedStage);
                Assert.AreEqual(12, reloaded.Data.completedMatches); Assert.AreEqual(4, reloaded.Data.lastInterstitialMatch);
                Assert.AreEqual(123, reloaded.Data.lastInterstitialUtc); Assert.AreEqual("en", reloaded.Data.language);
            }
            finally { if (had) PlayerPrefs.SetString(Key, old); else PlayerPrefs.DeleteKey(Key); PlayerPrefs.Save(); }
        }

        [Test]
        public void SavedNodesAndHelpAreNestedAndShowExactlyFourConnectedStones()
        {
            var root = AssetDatabase.LoadAssetAtPath<GameObject>(Editor.RankUiBuilder.AppRootPath);
            var app = root.GetComponent<RankGameApp>();
            Assert.AreEqual(30, app.cards.Length);
            foreach (var card in app.cards)
            {
                Assert.AreEqual(Editor.RankUiBuilder.Prefabs + "StageCard.prefab", PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(card));
                Assert.AreEqual(new Vector2(.5f,1), ((RectTransform)card.transform).anchorMin);
            }
            for (int example = 0; example < 3; example++)
            {
                var pattern = app.help.transform.Find("Frame/Connection_" + example);
                Assert.AreEqual(4, pattern.childCount, "Four-stone direction pattern must not look like a floating grid.");
                for (int i = 0; i < 4; i++)
                {
                    var stone = (RectTransform)pattern.Find("Stone_" + i);
                    Assert.NotNull(stone.GetComponent<Image>().sprite);
                    var expected = new Vector2(example == 1 ? 0 : (i - 1.5f) * 42, example == 0 ? 0 : (i - 1.5f) * 42);
                    Assert.AreEqual(expected*RankUiMetrics.Unit, stone.anchoredPosition);
                }
            }
            var gravity = app.help.transform.Find("Frame/Gravity");
            Assert.AreSame(app.theme.whiteStone, gravity.Find("Support").GetComponent<Image>().sprite);
            Assert.AreSame(app.theme.preview, gravity.Find("Preview").GetComponent<Image>().sprite);
            Assert.IsFalse(app.help.transform.Find("Frame/Body").gameObject.activeSelf);
        }
    }
}
