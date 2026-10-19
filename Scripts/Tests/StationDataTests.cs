using NS.Core;
using NS.Core.Blueprints;
using NUnit.Framework;
using UnityEngine;

namespace Tests
{
    public class StationDataTests
    {
        [Test]
        public void TestEquality()
        {
            var station1 = new StationData(ResourceType.Wood, 1, ResourceType.None, 0);
            var station2 = new StationData(ResourceType.Wood, 1, ResourceType.None, 0);
            Assert.IsTrue(station1.Equals(station2));

            Assert.IsTrue(TestUtils.StationGivesWood.Equals(TestUtils.StationGivesSteel) == false);
            Assert.IsTrue(TestUtils.StationGivesWood.Equals(TestUtils.StationWantsWood) == false);
            Assert.IsTrue(TestUtils.StationWantsWoodGivesSteel.Equals(TestUtils.StationWantsWood) == false);
            Assert.IsTrue(TestUtils.StationWantsWoodGivesSteel.Equals(TestUtils.StationWantsSteel) == false);
            Assert.IsTrue(TestUtils.StationWantsWoodGivesSteel.Equals(TestUtils.StationGivesWood) == false);
            Assert.IsTrue(TestUtils.StationWantsWoodGivesSteel.Equals(TestUtils.StationGivesSteel) == false);
        }

        [Test]
        public void TestSerialization()
        {
            TestUtils.TestSerialization(TestUtils.StationGivesWood);
            TestUtils.TestSerialization(TestUtils.StationGivesSteel);
            TestUtils.TestSerialization(TestUtils.StationWantsWood);
            TestUtils.TestSerialization(TestUtils.StationWantsSteel);
            TestUtils.TestSerialization(TestUtils.StationWantsWoodGivesSteel);
        }
    }
}
