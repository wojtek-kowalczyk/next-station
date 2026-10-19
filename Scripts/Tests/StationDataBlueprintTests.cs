using NS.Core;
using NS.Core.Blueprints;
using NUnit.Framework;
using UnityEngine;

namespace Tests
{
    public class StationDataBlueprintTests
    {
        [Test]
        public void EqualBlueprints_AreEqual()
        {
            var a = new StationDataBlueprint(
                ResourceType.Wood,
                new Vector2Int(2, 5),
                ResourceType.Steel,
                new Vector2Int(1, 3));

            var b = new StationDataBlueprint(
                ResourceType.Wood,
                new Vector2Int(2, 5),
                ResourceType.Steel,
                new Vector2Int(1, 3));

            Assert.IsTrue(a.Equals(b));
        }

        [Test]
        public void DifferentWants_AreNotEqual()
        {
            var a = new StationDataBlueprint(
                ResourceType.Wood,
                new Vector2Int(2, 5),
                ResourceType.Steel,
                new Vector2Int(1, 3));

            var b = new StationDataBlueprint(
                ResourceType.People,
                new Vector2Int(2, 5),
                ResourceType.Steel,
                new Vector2Int(1, 3));

            Assert.IsFalse(a.Equals(b));
        }

        [Test]
        public void DifferentWantsAmount_AreNotEqual()
        {
            var a = new StationDataBlueprint(
                ResourceType.Wood,
                new Vector2Int(2, 5),
                ResourceType.Steel,
                new Vector2Int(1, 3));

            var b = new StationDataBlueprint(
                ResourceType.Wood,
                new Vector2Int(3, 5),
                ResourceType.Steel,
                new Vector2Int(1, 3));

            Assert.IsFalse(a.Equals(b));
        }

        [Test]
        public void DifferentGives_AreNotEqual()
        {
            var a = new StationDataBlueprint(
                ResourceType.Wood,
                new Vector2Int(2, 5),
                ResourceType.Steel,
                new Vector2Int(1, 3));

            var b = new StationDataBlueprint(
                ResourceType.Wood,
                new Vector2Int(2, 5),
                ResourceType.People,
                new Vector2Int(1, 3));

            Assert.IsFalse(a.Equals(b));
        }

        [Test]
        public void DifferentGivesAmount_AreNotEqual()
        {
            var a = new StationDataBlueprint(
                ResourceType.Wood,
                new Vector2Int(2, 5),
                ResourceType.Steel,
                new Vector2Int(1, 3));

            var b = new StationDataBlueprint(
                ResourceType.Wood,
                new Vector2Int(2, 5),
                ResourceType.Steel,
                new Vector2Int(2, 3));

            Assert.IsFalse(a.Equals(b));
        }

        [Test]
        public void Equals_SameReference_ReturnsTrue()
        {
            var a = new StationDataBlueprint(
                ResourceType.Wood,
                new Vector2Int(2, 5),
                ResourceType.Steel,
                new Vector2Int(1, 3));

            Assert.IsTrue(a.Equals(a));
        }

        [Test]
        public void Equals_Null_ReturnsFalse()
        {
            var a = new StationDataBlueprint(
                ResourceType.Wood,
                new Vector2Int(2, 5),
                ResourceType.Steel,
                new Vector2Int(1, 3));

            Assert.IsFalse(a.Equals(null));
        }

        [Test]
        public void IntegerConstructor_EqualToEquivalentVectorConstructor()
        {
            var a = new StationDataBlueprint(
                ResourceType.Wood,
                5,
                ResourceType.Steel,
                3);

            var b = new StationDataBlueprint(
                ResourceType.Wood,
                new Vector2Int(5, 5),
                ResourceType.Steel,
                new Vector2Int(3, 3));

            Assert.IsTrue(a.Equals(b));
        }

        [Test]
        public void TestSerialization()
        {
            TestUtils.TestSerialization(new StationDataBlueprint(
                ResourceType.Wood,
                5,
                ResourceType.Steel,
                3));

            TestUtils.TestSerialization(new StationDataBlueprint(
                ResourceType.Wood,
                new Vector2Int(5, 9),
                ResourceType.Steel | ResourceType.People | ResourceType.Gold,
                new Vector2Int(3, 5)));
        }
    }
}
