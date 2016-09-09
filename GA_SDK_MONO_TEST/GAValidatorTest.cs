using System;
using NUnit.Framework;
using GameAnalyticsSDK.Net.Logging;
using GameAnalyticsSDK.Net.Validators;

namespace GameAnalyticsSDK.Net
{
	[TestFixture()]
	public class GAValidatorTest
	{
		[Test()]
		public void TestValidateCurrency()
		{
			GALogger.InfoLog = true;

			Assert.True(GAValidator.ValidateCurrency("USD"));
			Assert.True(GAValidator.ValidateCurrency("XXX"));

			Assert.False(GAValidator.ValidateCurrency("usd"));
			Assert.False(GAValidator.ValidateCurrency("US"));
			Assert.False(GAValidator.ValidateCurrency("KR"));
			Assert.False(GAValidator.ValidateCurrency("USDOLLARS"));
			Assert.False(GAValidator.ValidateCurrency("$"));
			Assert.False(GAValidator.ValidateCurrency(""));
			Assert.False(GAValidator.ValidateCurrency(null));
		}
	}
}
