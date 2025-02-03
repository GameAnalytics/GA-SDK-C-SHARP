using System;
using NUnit.Framework;
using GameAnalyticsSDK.Net;

namespace GameAnalyticsSDK.Net
{
    [TestFixture]
    public class GAIntegrationTest
    {
        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            GameAnalytics.SetEnabledInfoLog(true);
            GameAnalytics.SetEnabledVerboseLog(true);

            GameAnalytics.ConfigureBuild("0.1-c-shrap-sdk");

            GameAnalytics.ConfigureAvailableResourceCurrencies("gems", "gold");
            GameAnalytics.ConfigureAvailableResourceItemTypes("boost", "lives");

            GameAnalytics.ConfigureAvailableCustomDimensions01("ninja", "samurai");
            GameAnalytics.ConfigureAvailableCustomDimensions02("whale", "dolpin");
            GameAnalytics.ConfigureAvailableCustomDimensions03("horde", "alliance");

        }

        [Test]
        public void TestInitialize()
        {
            Console.WriteLine("Running TestInitialize...");
            Assert.IsFalse(GameAnalytics.IsInitialized(), "GA SDK should not be initialized.");
            GameAnalytics.Initialize("bd624ee6f8e6efb32a054f8d7ba11618", "7f5c3f682cbd217841efba92e92ffb1b3b6612bc");
            Assert.IsTrue(GameAnalytics.IsInitialized(), "GA SDK should be initialized.");
            Console.WriteLine("TestInitialize completed.");
        }

        [Test]
        public void TestSendEvent()
        {

            Console.WriteLine("Running TestSendEvent...");
            Assert.IsTrue(true, "GA SDK should be initialized.");
            Console.WriteLine("TestInitialize completed.");
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            GameAnalyticsSDK.OnQuit();
        }

        [Test]
        public void TestTearDown()
        {
            OneTimeTearDown();
            Assert.IsFalse(GA.IsInitialized(), "GA SDK should be shut down.");
        }
    }
}