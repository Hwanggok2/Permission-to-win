using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace StageGame.Tests
{
    public sealed class HostLayoutTests
    {
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        public void OsInsetsAreMergedOnceAndHostBandIsBelowThem(int pixelRatio)
        {
            var screen = new Vector2Int(390 * pixelRatio, 844 * pixelRatio);
            var os = new Rect(0, 34 * pixelRatio, screen.x, 751 * pixelRatio);
            var content = HostLayout.ContentArea(os, screen, new Vector2(390, 844), new Vector4(0, 59, 0, 34));
            Assert.AreEqual(new Rect(0, 34 * pixelRatio, 390 * pixelRatio, 687 * pixelRatio), content);
        }

        [Test]
        public void BrowserFallbackAndExplicitControlsUseCssCoordinates()
        {
            var size = new Vector2Int(640, 1136);
            var viewport = new Vector2(320, 568);
            Assert.AreEqual(new Rect(0, 0, 640, 1008), HostLayout.ContentArea(new Rect(0, 0, 640, 1136), size, viewport, Vector4.zero));
            Assert.AreEqual(new Rect(0, 68, 640, 732), HostLayout.ContentArea(new Rect(0, 0, 640, 1136), size, viewport, new Vector4(0, 59, 0, 34), 160));
        }

        [Test]
        public void SideInsetsMergePerEdgeAndOversizedInsetsStayInsideScreen()
        {
            var size = new Vector2Int(390, 844);
            var content = HostLayout.ContentArea(new Rect(12, 20, 370, 794), size, new Vector2(390, 844), new Vector4(5, 59, 10, 34));
            Assert.AreEqual(new Rect(12, 34, 368, 687), content);
            content = HostLayout.ContentArea(new Rect(0, 0, 390, 844), size, new Vector2(390, 844), new Vector4(500, 1000, 500, 900));
            Assert.AreEqual(0, content.width); Assert.AreEqual(0, content.height);
            Assert.LessOrEqual(content.xMax, 390); Assert.LessOrEqual(content.yMax, 844);
        }

        [Test]
        public void InvalidInsetsFallBackToZeroWithoutRemovingHostAllowance()
        {
            var content = HostLayout.ContentArea(new Rect(0, 0, 320, 568), new Vector2Int(320, 568), new Vector2(320, 568), new Vector4(float.NaN, float.PositiveInfinity, float.NegativeInfinity, -3), float.PositiveInfinity);
            Assert.AreEqual(new Rect(0, 0, 320, 504), content);
        }

        [Test]
        public void SavedGraphicsAndSecondaryLicenseMenuKeepPrefabReferences()
        {
            var root = AssetDatabase.LoadAssetAtPath<GameObject>(Editor.ProjectBuilder.Root + "Prefabs/AppRoot.prefab");
            var app = root.GetComponent<GameApp>();
            var rounded = AssetDatabase.LoadAssetAtPath<Sprite>(Editor.TabletopUiBuilder.Art + "ButtonMint.png");
            var disc = AssetDatabase.LoadAssetAtPath<Sprite>(Editor.TabletopUiBuilder.Art + "RecessedHole.png");
            Assert.NotNull(rounded); Assert.NotNull(disc);
            Assert.AreEqual(new Vector4(32, 32, 32, 32), rounded.border);
            Assert.AreSame(rounded, app.play.GetComponent<Image>().sprite);
            Assert.AreEqual(Image.Type.Sliced, app.play.GetComponent<Image>().type);
            foreach (var stone in app.stones) Assert.AreSame(disc, stone.sprite);
            Assert.AreEqual(0, app.noticesOpen.GetComponent<Image>().color.a);
            Assert.IsTrue(app.noticesOpen.GetComponent<Image>().raycastTarget);
            Assert.AreEqual(26, app.noticesOpen.GetComponentInChildren<Text>().fontSize);
            Assert.AreEqual(new Vector2(535, 110), ((RectTransform)app.noticesOpen.transform).sizeDelta);
            foreach (var close in new[] { app.resultClose, app.settingsClose, app.helpClose, app.confirmClose, app.noticesClose })
                Assert.AreEqual(Editor.ProjectBuilder.Root + "Prefabs/UI/Common/CloseButton.prefab", PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(close));
        }
    }
}
