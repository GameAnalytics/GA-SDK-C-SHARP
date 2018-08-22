using System.Collections.Generic;
using NUnit.Framework;
using GameAnalyticsSDK.Net.State;

namespace GameAnalyticsSDK.Net
{
    [TestFixture]
    public class GAStateTest
    {
        [Test]
        public void TestValidateAndCleanCustomFields()
        {
            IDictionary<string, object> map;

            {
                map = new Dictionary<string, object>();
                while(map.Count < 100)
                {
                    string key = GATestUtilities.GetRandomString(4);

                    if(!map.ContainsKey(key))
                    {
                        map.Add(key, GATestUtilities.GetRandomString(4));
                    }
                }
            }
            Assert.AreEqual(50, GAState.ValidateAndCleanCustomFields(map).Count);

            {
                map = new Dictionary<string, object>();
                while (map.Count < 50)
                {
                    string key = GATestUtilities.GetRandomString(4);

                    if (!map.ContainsKey(key))
                    {
                        map.Add(key, GATestUtilities.GetRandomString(4));
                    }
                }
            }
            Assert.AreEqual(50, GAState.ValidateAndCleanCustomFields(map).Count);

            {
                map = new Dictionary<string, object>();
                map.Add(GATestUtilities.GetRandomString(4), "");
            }
            Assert.AreEqual(0, GAState.ValidateAndCleanCustomFields(map).Count);

            {
                map = new Dictionary<string, object>();
                map.Add(GATestUtilities.GetRandomString(4), GATestUtilities.GetRandomString(257));
            }
            Assert.AreEqual(0, GAState.ValidateAndCleanCustomFields(map).Count);

            {
                map = new Dictionary<string, object>();
                map.Add("", GATestUtilities.GetRandomString(4));
            }
            Assert.AreEqual(0, GAState.ValidateAndCleanCustomFields(map).Count);

            {
                map = new Dictionary<string, object>();
                map.Add("___", GATestUtilities.GetRandomString(4));
            }
            Assert.AreEqual(1, GAState.ValidateAndCleanCustomFields(map).Count);

            {
                map = new Dictionary<string, object>();
                map.Add("_&_", GATestUtilities.GetRandomString(4));
            }
            Assert.AreEqual(0, GAState.ValidateAndCleanCustomFields(map).Count);

            {
                map = new Dictionary<string, object>();
                map.Add(GATestUtilities.GetRandomString(65), GATestUtilities.GetRandomString(4));
            }
            Assert.AreEqual(0, GAState.ValidateAndCleanCustomFields(map).Count);

            {
                map = new Dictionary<string, object>();
                map.Add(GATestUtilities.GetRandomString(4), 100);
            }
            Assert.AreEqual(1, GAState.ValidateAndCleanCustomFields(map).Count);

            {
                map = new Dictionary<string, object>();
                map.Add(GATestUtilities.GetRandomString(4), new object[] { 100 });
                map.Add(GATestUtilities.GetRandomString(5), true);
            }
            Assert.AreEqual(0, GAState.ValidateAndCleanCustomFields(map).Count);

            {
                map = new Dictionary<string, object>();
                byte b = 1;
                sbyte sb = 1;
                short s = 5;
                ushort us = 5;
                float f = 9.99f;
                int i = 2;
                uint ui = 2;
                long l = 1000L;
                ulong ul = 1000L;
                double d = 6.66d;
                char c = 'a';

                map.Add(GATestUtilities.GetRandomString(1), b);
                map.Add(GATestUtilities.GetRandomString(2), sb);
                map.Add(GATestUtilities.GetRandomString(3), s);
                map.Add(GATestUtilities.GetRandomString(4), us);
                map.Add(GATestUtilities.GetRandomString(5), f);
                map.Add(GATestUtilities.GetRandomString(6), i);
                map.Add(GATestUtilities.GetRandomString(7), ui);
                map.Add(GATestUtilities.GetRandomString(8), l);
                map.Add(GATestUtilities.GetRandomString(9), ul);
                map.Add(GATestUtilities.GetRandomString(10), d);
                map.Add(GATestUtilities.GetRandomString(11), c);
            }
            Assert.AreEqual(11, GAState.ValidateAndCleanCustomFields(map).Count);
        }
    }
}
