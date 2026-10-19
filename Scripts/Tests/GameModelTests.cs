using NS.Core.GameModes.Basic;
using NS.Core.Model;
using NS.Utility;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Tests
{
    public class GameModelTests
    {
        [SetUp]
        public void Setup()
        {
            EditorSceneManager.OpenScene("Assets/Scenes/GameplayMain.unity");
        }

        [Test]
        public void TestGameModePresentInScene()
        {
            var gameMode = Object.FindAnyObjectByType<BasicGameMode>();
            Assert.IsNotNull(gameMode);
        }

        [TearDown]
        public void Teardown()
        {
            // EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        }

        [Test]
        public void TestEquality()
        {
            var gameMode = Object.FindAnyObjectByType<BasicGameMode>();

            var model1 = new GameModel();
            var model2 = new GameModel();
            var model3 = new GameModel();

            gameMode.UpdateGameModel(model3, TestUtils.PlacementDataA);

            Assert.IsTrue(model1.Equals(model2));
            Assert.IsTrue(model1.Equals(model3) == false);

            gameMode.UpdateGameModel(model2, TestUtils.PlacementDataA);

            Assert.IsTrue(model2.Equals(model3));
            Assert.IsTrue(model2.Equals(model1) == false);

            gameMode.UpdateGameModel(model1, TestUtils.PlacementDataB);

            Assert.IsTrue(model1.Equals(model2) == false);
            Assert.IsTrue(model1.Equals(model2) == false);
        }

        [Test]
        public void TestSerialization()
        {
            var gameMode = Object.FindAnyObjectByType<BasicGameMode>();

            var model = new GameModel();
            gameMode.UpdateGameModel(model, TestUtils.PlacementDataA);
            TestUtils.TestSerialization(model);
        }

        [Test]
        public void TestBase64Conversion()
        {
            var gameMode = Object.FindAnyObjectByType<BasicGameMode>();

            var model = new GameModel();
            gameMode.UpdateGameModel(model, TestUtils.PlacementDataA);
            var deserialized = Utils.FromBase64<GameModel>(Utils.ToBase64(model));
            Assert.IsTrue(deserialized != null);
            Assert.IsTrue(model.Equals(deserialized));
        }
    }
}
