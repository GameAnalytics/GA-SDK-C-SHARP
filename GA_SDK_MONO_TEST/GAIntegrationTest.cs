using System;
using NUnit.Framework;
using System.Threading;
using System.Diagnostics;

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
        public void TestIntegrationBasic()
        {

            TestContext.Out.WriteLine("Running TestInitialize...");

            Assert.IsFalse(GameAnalytics.IsInitialized(), "GA SDK should not be initialized.");
            GameAnalytics.Initialize("bd624ee6f8e6efb32a054f8d7ba11618", "7f5c3f682cbd217841efba92e92ffb1b3b6612bc");


            int maxWaitTimeMs = 5000; // Maximum wait time (5 seconds)
            int waitIntervalMs = 100; // Interval between checks
            int elapsedTime = 0;

            while (!GameAnalytics.IsInitialized() && elapsedTime < maxWaitTimeMs)
            {
                Thread.Sleep(waitIntervalMs);
                elapsedTime += waitIntervalMs;
            }

            Assert.IsTrue(GameAnalytics.IsInitialized(), "GA SDK should be initialized.");
            TestContext.Out.WriteLine($"GA SDK initialized in {elapsedTime / 1000.0} seconds.");

            //Get User ID
            string userId = GameAnalytics.GetUserId();
            TestContext.Out.WriteLine($"User ID: {userId}");
            // wait 3 seconds for the SDK to send the initialization event
            Thread.Sleep(3000);

            TestContext.Out.WriteLine("Running TestSendEvent...");
            GameAnalytics.AddDesignEvent("testEvent");
            Thread.Sleep(3000);
            TestContext.Out.WriteLine("TestSendEvent completed.");

            TestContext.Out.WriteLine("Running OneTimeTearDown...");
            GameAnalytics.OnQuit();
            WaitForGameAnalyticsShutdown(timeoutMs: 5000);
            Assert.IsFalse(GameAnalytics.IsInitialized(), "GA SDK should be shut down.");
        }


        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            // TestContext.Out.WriteLine("Running OneTimeTearDown...");
            // GameAnalytics.OnQuit();
            // WaitForGameAnalyticsShutdown(timeoutMs: 5000);
            // Assert.IsFalse(GameAnalytics.IsInitialized(), "GA SDK should be shut down.");
        }

        private void WaitForGameAnalyticsShutdown(int timeoutMs)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            while (GameAnalytics.IsInitialized() && stopwatch.ElapsedMilliseconds < timeoutMs)
            {
                Thread.Sleep(100);
            }

            stopwatch.Stop();

            if (GameAnalytics.IsInitialized())
            {
                TestContext.Out.WriteLine("Timeout reached! GameAnalytics.IsInitialized() is still true.");
            }
            else
            {
                TestContext.Out.WriteLine($"GameAnalytics shut down after {stopwatch.ElapsedMilliseconds / 1000.0} seconds.");
            }
        }

        // [Test]
        // public void TestTearDown()
        // {
        //     
        // }
    }
}