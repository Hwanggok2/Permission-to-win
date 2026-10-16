using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace StageGame.Tests
{
    public sealed class LocalizationTests
    {
        [Test]
        public void LanguageLookupFormattingAndUnknownLocaleFollowContract()
        {
            var table = StringTable.Load();
            Assert.AreEqual("이겨도 됩니까?", table.Text("game_title"));
            table.SetLanguage("en");
            Assert.AreEqual("Permission to Win", table.Text("game_title"));
            Assert.AreEqual("CHAPTER 01 · STAGE 02", table.Format("stage_number", "01", "02"));
            Assert.AreEqual("한국어", table.Text("language_ko"));
            table.SetLanguage("unknown");
            Assert.AreEqual("ko", table.Language);
            Assert.Throws<KeyNotFoundException>(() => table.Text("not_a_valid_key"));
            var escaped = new StringTable("{\"schemaVersion\":1,\"strings\":[{\"key\":\"k\",\"ko\":\"{{{0}}}\",\"en\":\"{{{0}}}\"}]}");
            Assert.AreEqual("{7}", escaped.Format("k", 7));
        }

        [TestCase("", "Text")]
        [TestCase("문구", "")]
        [TestCase("{0}", "{1}")]
        [TestCase("{0:00}", "{0:00}")]
        [TestCase("{broken}", "{broken}")]
        [TestCase("{1}", "{1}")]
        public void MissingTranslationsAndInvalidArgumentsAreRejected(string ko, string en)
        {
            var file = new StringFile { schemaVersion = 1, strings = new[] { new StringEntry { key = "k", ko = ko, en = en } } };
            Assert.Throws<FormatException>(() => new StringTable(JsonUtility.ToJson(file)));
        }

        [Test]
        public void LegacyAndInvalidSavedLocalesPreserveProgressAndDefaultToKorean()
        {
            const string key = "stagegame.progress.v1";
            bool existed = PlayerPrefs.HasKey(key); string old = PlayerPrefs.GetString(key, "");
            try
            {
                PlayerPrefs.SetString(key, "{\"saveVersion\":1,\"unlockedStage\":7,\"completedMatches\":9,\"soundEnabled\":false}");
                var progress = new ProgressStore(30);
                Assert.AreEqual("ko", progress.Data.language);
                Assert.AreEqual(7, progress.Data.unlockedStage); Assert.IsFalse(progress.Data.soundEnabled);
                progress.Data.language = "en"; Assert.IsTrue(progress.Save());
                Assert.AreEqual("en", new ProgressStore(30).Data.language);
                progress.Data.language = "bad-locale"; Assert.IsTrue(progress.Save());
                var restored = new ProgressStore(30);
                Assert.AreEqual("ko", restored.Data.language); Assert.AreEqual(7, restored.Data.unlockedStage);
            }
            finally { if (existed) PlayerPrefs.SetString(key, old); else PlayerPrefs.DeleteKey(key); PlayerPrefs.Save(); }
        }

        [Test]
        public void EverySavedUiTextHasAKeyOrExplicitDynamicOwnerAndButtonsHaveTouchArea()
        {
            var root = AssetDatabase.LoadAssetAtPath<GameObject>(Editor.RankUiBuilder.AppRootPath);
            var app = root.GetComponent<RankGameApp>();
            var owned = new HashSet<Text>(app.localizedLabels.Select(item => item.target));
            foreach (var field in typeof(RankGameApp).GetFields())
            {
                if (field.FieldType == typeof(Text)) owned.Add((Text)field.GetValue(app));
                if (field.FieldType == typeof(Text[])) owned.UnionWith((Text[])field.GetValue(app));
            }
            foreach(var card in app.cards) { owned.Add(card.rank); owned.Add(card.number); owned.Add(card.stars); owned.Add(card.rating); }
            foreach (var text in root.GetComponentsInChildren<Text>(true))
            {
                if (!text.gameObject.activeSelf || text.transform.parent.name == "CloseButton") continue;
                Assert.IsTrue(owned.Contains(text), "Unbound UI text: " + text.name + " under " + text.transform.parent.name);
            }
            foreach (var button in root.GetComponentsInChildren<Button>(true))
            {
                if (button.name.StartsWith("Column_")) continue; // Final CSS hitboxes are covered by HostProfilesKeepAllScreensBelowControlsAndPreserveLicenseScroll.
                var rect = (RectTransform)button.transform;
                Assert.GreaterOrEqual(rect.sizeDelta.y, 100, button.name + " height at 720 reference");
                Assert.GreaterOrEqual(rect.sizeDelta.x, 100, button.name + " width at 720 reference");
            }
        }
    }
}
