using System;
using NUnit.Framework;
using GameAnalyticsSDK.Net.Logging;
using GameAnalyticsSDK.Net.Validators;
using GameAnalyticsSDK.Net.State;
using GameAnalyticsSDK.Net.Http;
using GameAnalyticsSDK.Net.Utilities;

namespace GameAnalyticsSDK.Net
{
	[TestFixture]
	public class GAValidatorTest
	{
		[Test]
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

		[Test]
		public void TestValidateResourceCurrencies()
		{
			// Store result
			bool isValid;

			// Valid resource types
			isValid = GAValidator.ValidateResourceCurrencies("gems", "gold");
			Assert.True(isValid, "Valid resource types array should succeed");

			// Invalid resource types
			isValid = GAValidator.ValidateResourceCurrencies("", "gold");
			Assert.False(isValid, "Should false to allow empty resource type");

			isValid = GAValidator.ValidateResourceCurrencies();
			Assert.False(isValid, "Should false to allow empty array");

			isValid = GAValidator.ValidateResourceCurrencies((string)null);
			Assert.False(isValid, "Should false to allow null");

			isValid = GAValidator.ValidateResourceCurrencies(null, "gold");
			Assert.False(isValid, "Should false to allow null");
		}

		[Test]
		public void TestValidateResourceItemTypes()
		{
			// Store result
			bool isValid;

			// Valid resource types
			isValid = GAValidator.ValidateResourceItemTypes("gems", "gold");
			Assert.True(isValid, "Valid resource types array should succeed");

			// Invalid resource types
			isValid = GAValidator.ValidateResourceItemTypes("", "gold");
			Assert.False(isValid, "Should falset allow empty resource type");

			isValid = GAValidator.ValidateResourceItemTypes();
			Assert.False(isValid, "Should falset allow empty array");

			isValid = GAValidator.ValidateResourceItemTypes((string)null);
			Assert.False(isValid, "Should falset allow empty null");

			isValid = GAValidator.ValidateResourceItemTypes(null, "gold");
			Assert.False(isValid, "Should falset allow empty null");
		}

		[Test]
		public void TestValidateProgressionEvent()
		{
			// Store result
			bool isValid;

			// Valids
			isValid = GAValidator.ValidateProgressionEvent(EGAProgressionStatus.Start, "world_001", "level_001", "phase_001");
			Assert.True(isValid, "Should allow progression 01-02-03");

			isValid = GAValidator.ValidateProgressionEvent(EGAProgressionStatus.Start, "world_001", "level_001", "");
			Assert.True(isValid, "Should allow progression 01-02");

			isValid = GAValidator.ValidateProgressionEvent(EGAProgressionStatus.Start, "world_001", "level_001", null);
			Assert.True(isValid, "Should allow progression 01-02");

			isValid = GAValidator.ValidateProgressionEvent(EGAProgressionStatus.Start, "world_001", "", "");
			Assert.True(isValid, "Should allow progression 01");

			isValid = GAValidator.ValidateProgressionEvent(EGAProgressionStatus.Start, "world_001", null, null);
			Assert.True(isValid, "Should allow progression 01");

			// Invalids
			isValid = GAValidator.ValidateProgressionEvent(EGAProgressionStatus.Start, "", "", "");
			Assert.False(isValid, "Should falset allow false progressions");

			isValid = GAValidator.ValidateProgressionEvent(EGAProgressionStatus.Start, null, null, null);
			Assert.False(isValid, "Should falset allow false progressions");

			isValid = GAValidator.ValidateProgressionEvent(EGAProgressionStatus.Start, "world_001", "", "phase_001");
			Assert.False(isValid, "Should falset allow progression 01-03");

			isValid = GAValidator.ValidateProgressionEvent(EGAProgressionStatus.Start, "world_001", null, "phase_001");
			Assert.False(isValid, "Should falset allow progression 01-03");

			isValid = GAValidator.ValidateProgressionEvent(EGAProgressionStatus.Start, "", "level_001", "phase_001");
			Assert.False(isValid, "Should falset allow progression 02-03");

			isValid = GAValidator.ValidateProgressionEvent(EGAProgressionStatus.Start, null, "level_001", "phase_001");
			Assert.False(isValid, "Should falset allow progression 02-03");

			isValid = GAValidator.ValidateProgressionEvent(EGAProgressionStatus.Start, "", "level_001", "");
			Assert.False(isValid, "Should falset allow progression 02");

			isValid = GAValidator.ValidateProgressionEvent(EGAProgressionStatus.Start, null, "level_001", null);
			Assert.False(isValid, "Should falset allow progression 02");

			isValid = GAValidator.ValidateProgressionEvent(EGAProgressionStatus.Start, "", "", "phase_001");
			Assert.False(isValid, "Should falset allow progression 03");

			isValid = GAValidator.ValidateProgressionEvent(EGAProgressionStatus.Start, null, null, "phase_001");
			Assert.False(isValid, "Should falset allow progression 03");
		}

		[Test]
		public void TestValidateBusinessEvent()
		{
			// Store result
			bool isValid;

			// Valid business events
			isValid = GAValidator.ValidateBusinessEvent("USD", 99, "cartType", "itemType", "itemId");
			Assert.True(isValid, "Valid business event should succeed");

			// Should allow false carttype (optional)
			isValid = GAValidator.ValidateBusinessEvent("USD", 99, "", "itemType", "itemId");
			Assert.True(isValid, "Business event cartType should be optional");

			// Should allow 0 amount
			isValid = GAValidator.ValidateBusinessEvent("USD", 0, "", "itemType", "itemId");
			Assert.True(isValid, "Business event should allow amount 0");

			// Should allow negative amount
			isValid = GAValidator.ValidateBusinessEvent("USD", -99, "", "itemType", "itemId");
			Assert.True(isValid, "Business event should allow amount less than 0");

			// Should fail on empty item type
			isValid = GAValidator.ValidateBusinessEvent("USD", 99, "", "", "itemId");
			Assert.False(isValid, "Business event should not allow empty item type");

			// Should fail on empty item id
			isValid = GAValidator.ValidateBusinessEvent("USD", 99, "", "itemType", "");
			Assert.False(isValid, "Business event should not allow empty item id");
		}

		[Test]
		public void TestValidateResourceSinkEvent()
		{
			// Set available list
			GAState.AvailableResourceCurrencies = new string[]{ "gems", "gold" };
			GAState.AvailableResourceItemTypes = new string[] { "guns", "powerups" };

			// Store result
			bool isValid;

			// Valid resource source events
			isValid = GAValidator.ValidateResourceEvent(EGAResourceFlowType.Sink, "gems", 100, "guns", "item");
			Assert.True(isValid, "Valid resource source event should succeed");

			// Valid resource source events
			isValid = GAValidator.ValidateResourceEvent(EGAResourceFlowType.Sink, "gold", 100, "powerups", "item");
			Assert.True(isValid, "Valid resource source event should succeed");

			// falset defined resource type should fail
			isValid = GAValidator.ValidateResourceEvent(EGAResourceFlowType.Sink, "iron", 100, "guns", "item");
			Assert.False(isValid, "Resource event should falset allow falsen defined resource types");

			// falset defined item type should fail
			isValid = GAValidator.ValidateResourceEvent(EGAResourceFlowType.Sink, "gems", 100, "cows", "item");
			Assert.False(isValid, "Resource event should falset allow falsen defined item types");

			// Should falset allow 0 amount
			isValid = GAValidator.ValidateResourceEvent(EGAResourceFlowType.Sink, "gems", 0, "guns", "item");
			Assert.False(isValid, "Resource event should falset allow 0 amount");

			// Should falset allow negative amount
			isValid = GAValidator.ValidateResourceEvent(EGAResourceFlowType.Sink, "gems", -10, "guns", "item");
			Assert.False(isValid, "Resource event should falset allow negative amount");

			// Should falset allow false item id
			isValid = GAValidator.ValidateResourceEvent(EGAResourceFlowType.Sink, "gems", 10, "guns", "");
			Assert.False(isValid, "Resource event should falset allow empty item id");

			isValid = GAValidator.ValidateResourceEvent(EGAResourceFlowType.Sink, "gems", 10, "guns", null);
			Assert.False(isValid, "Resource event should falset allow empty item id");

			// Should falset allow false item type
			isValid = GAValidator.ValidateResourceEvent(EGAResourceFlowType.Sink, "gems", 10, "", "item");
			Assert.False(isValid, "Resource event should falset allow empty item id");

			isValid = GAValidator.ValidateResourceEvent(EGAResourceFlowType.Sink, "gems", 10, null, "item");
			Assert.False(isValid, "Resource event should falset allow empty item id");
		}

		[Test]
		public void TestValidateDesignEvent()
		{
			// Store result
			bool isValid;

			// Valid
			isValid = GAValidator.ValidateDesignEvent("name:name", 100);
			Assert.True(isValid, "Design event should allow nil value");

			isValid = GAValidator.ValidateDesignEvent("name:name:name", 100);
			Assert.True(isValid, "Design event should allow nil value");

			isValid = GAValidator.ValidateDesignEvent("name:name:name:name", 100);
			Assert.True(isValid, "Design event should allow nil value");

			isValid = GAValidator.ValidateDesignEvent("name:name:name:name:name", 100);
			Assert.True(isValid, "Design event should allow nil value");

			isValid = GAValidator.ValidateDesignEvent("name:name", 0);
			Assert.True(isValid, "Design event should allow nil value");

			// Invalid
			isValid = GAValidator.ValidateDesignEvent("", 100);
			Assert.False(isValid, "Design event should falset allow empty event string");

			isValid = GAValidator.ValidateDesignEvent(null, 100);
			Assert.False(isValid, "Design event should falset allow empty event string");

			isValid = GAValidator.ValidateDesignEvent("name:name:name:name:name:name", 100);
			Assert.False(isValid, "Design event should falset allow more than 5 values in event string");
		}

		[Test]
		public void TestValidateErrorEvent()
		{
			// Store result
			bool isValid;

			// Valid
			isValid = GAValidator.ValidateErrorEvent(EGAErrorSeverity.Error, "This is a message");
			Assert.True(isValid, "Error event should validate");

			isValid = GAValidator.ValidateErrorEvent(EGAErrorSeverity.Error, "");
			Assert.True(isValid, "Error event should allow empty message");

			isValid = GAValidator.ValidateErrorEvent(EGAErrorSeverity.Error, null);
			Assert.True(isValid, "Error event should allow nil message");
		}

		[Test]
		public void TestValidateSdkErrorEvent()
		{
			// Store result
			bool isValid;

			// Valid
			isValid = GAValidator.ValidateSdkErrorEvent("c6cfc80ff69d1e7316bf1e0c8194eda6", "e0ae4809f70e2fa96916c7060f417ae53895f18d", EGASdkErrorType.Rejected);
			Assert.True(isValid, "Sdk error event should validate");
		}

		[Test]
		public void TestCustomDimensionsValidator()
		{
			// Store result
			bool isValid;

			// Valid
			isValid = GAValidator.ValidateCustomDimensions("abc", "def", "ghi");
			Assert.True(isValid, "Should validate custom dimensions");

			// Invalid
			isValid = GAValidator.ValidateCustomDimensions("abc", "def", "abc", "def", "abc", "def", "abc", "def", "abc", "def", "abc", "def", "abc", "def", "abc", "def", "abc", "def", "abc", "def", "abc", "def");
			Assert.False(isValid, "Should falset allow more than 20 custom dimensions");

			isValid = GAValidator.ValidateCustomDimensions("abc", "");
			Assert.False(isValid, "Should falset allow empty custom dimension value");
		}

		[Test]
		public void TestValidateSdkWrapperVersion()
		{
			Assert.False(GAValidator.ValidateSdkWrapperVersion("123"));
			Assert.False(GAValidator.ValidateSdkWrapperVersion("test 1.2.x"));
			Assert.False(GAValidator.ValidateSdkWrapperVersion("unkfalsewn 1.5.6"));
			Assert.False(GAValidator.ValidateSdkWrapperVersion("unity 1.2.3.4"));
			Assert.False(GAValidator.ValidateSdkWrapperVersion("Unity 1.2"));
			Assert.False(GAValidator.ValidateSdkWrapperVersion("corona1.2.3"));
			Assert.False(GAValidator.ValidateSdkWrapperVersion("unity x.2.3"));
			Assert.False(GAValidator.ValidateSdkWrapperVersion("unity 1.x.3"));
			Assert.False(GAValidator.ValidateSdkWrapperVersion("unity 1.2.x"));

			Assert.True(GAValidator.ValidateSdkWrapperVersion("unity 1.2.3"));
			Assert.False(GAValidator.ValidateSdkWrapperVersion("marmalade 1.2.3"));
			Assert.False(GAValidator.ValidateSdkWrapperVersion("corona 1.2.3"));
			Assert.True(GAValidator.ValidateSdkWrapperVersion("unity 1233.101.0"));
		}

		[Test]
		public void TestValidateBuild()
		{
			Assert.False(GAValidator.ValidateBuild(""));
			Assert.False(GAValidator.ValidateBuild(null));
			Assert.False(GAValidator.ValidateBuild(GATestUtilities.GetRandomString(40)));

			Assert.True(GAValidator.ValidateBuild("alpha 1.2.3"));
			Assert.True(GAValidator.ValidateBuild("ALPHA 1.2.3"));
			Assert.True(GAValidator.ValidateBuild("TES# sdf.fd3"));
		}

		[Test]
		public void TestValidateEngineVersion()
		{
			Assert.False(GAValidator.ValidateEngineVersion(""));
			Assert.False(GAValidator.ValidateEngineVersion(null));
			Assert.False(GAValidator.ValidateEngineVersion(GATestUtilities.GetRandomString(40)));
			Assert.False(GAValidator.ValidateEngineVersion("uni 1.2.3"));
			Assert.False(GAValidator.ValidateEngineVersion("unity 123456.2.3"));
			Assert.False(GAValidator.ValidateEngineVersion("unity1.2.3"));
			Assert.False(GAValidator.ValidateEngineVersion("unity 1.2.3.4"));
			Assert.False(GAValidator.ValidateEngineVersion("Unity 1.2.3"));
			Assert.False(GAValidator.ValidateEngineVersion("UNITY 1.2.3"));

			Assert.True(GAValidator.ValidateEngineVersion("unity 1.2.3"));
			Assert.True(GAValidator.ValidateEngineVersion("unity 1.2"));
			Assert.True(GAValidator.ValidateEngineVersion("unity 1"));
			Assert.False(GAValidator.ValidateEngineVersion("marmalade 1.2.3"));
			Assert.False(GAValidator.ValidateEngineVersion("xamarin 1.2.3"));
			Assert.False(GAValidator.ValidateEngineVersion("cocos2d 1.2.3"));
			Assert.False(GAValidator.ValidateEngineVersion("unreal 1.2.3"));
		}

		[Test]
		public void TestValidateKeysWithGameKey()
		{
			string validGameKey = "123456789012345678901234567890ab";
			string validSecretKey = "123456789012345678901234567890123456789a";

			string tooLongKey = "123456789012345678901234567890123456789abcdefg";

			Assert.False(GAValidator.ValidateKeys(validGameKey, null));
			Assert.False(GAValidator.ValidateKeys(validGameKey, ""));
			Assert.False(GAValidator.ValidateKeys(validGameKey, "123"));
			Assert.False(GAValidator.ValidateKeys(validGameKey, tooLongKey));

			Assert.False(GAValidator.ValidateKeys(null, validSecretKey));
			Assert.False(GAValidator.ValidateKeys("", validSecretKey));
			Assert.False(GAValidator.ValidateKeys("123", validSecretKey));
			Assert.False(GAValidator.ValidateKeys(tooLongKey, validSecretKey));

			Assert.True(GAValidator.ValidateKeys(validGameKey, validSecretKey));
		}

		[Test]
		public void TestValidateEventPartLength()
		{
			Assert.True(GAValidator.ValidateEventPartLength(GATestUtilities.GetRandomString(40), true));
			Assert.True(GAValidator.ValidateEventPartLength(GATestUtilities.GetRandomString(40), false));
			Assert.False(GAValidator.ValidateEventPartLength(GATestUtilities.GetRandomString(80), true));
			Assert.False(GAValidator.ValidateEventPartLength(GATestUtilities.GetRandomString(80), false));
			Assert.False(GAValidator.ValidateEventPartLength(null, false));
			Assert.False(GAValidator.ValidateEventPartLength("", false));
			Assert.True(GAValidator.ValidateEventPartLength("", true));

			Assert.True(GAValidator.ValidateEventPartLength("sdfdf", false));
			Assert.True(GAValidator.ValidateEventPartLength(null, true));
			Assert.True(GAValidator.ValidateEventPartLength(GATestUtilities.GetRandomString(32), true));
		}

		[Test]
		public void TestValidateEventPartCharacters()
		{
			Assert.False(GAValidator.ValidateEventPartCharacters("øææ"));
			Assert.False(GAValidator.ValidateEventPartCharacters(""));
			Assert.False(GAValidator.ValidateEventPartCharacters(null));
			Assert.False(GAValidator.ValidateEventPartCharacters("*"));
			Assert.False(GAValidator.ValidateEventPartCharacters("))&%"));

			Assert.True(GAValidator.ValidateEventPartCharacters("sdfdffdgdfg"));
		}

		[Test]
		public void TestValidateEventIdLength()
		{
			Assert.True(GAValidator.ValidateEventIdLength(GATestUtilities.GetRandomString(40)));
			Assert.True(GAValidator.ValidateEventIdLength(GATestUtilities.GetRandomString(32)));
			Assert.True(GAValidator.ValidateEventIdLength("sdfdf"));

			Assert.False(GAValidator.ValidateEventIdLength(GATestUtilities.GetRandomString(80)));
			Assert.False(GAValidator.ValidateEventIdLength(null));
			Assert.False(GAValidator.ValidateEventIdLength(""));
		}

		[Test]
		public void ValidateEventIdCharacters()
		{
			Assert.True(GAValidator.ValidateEventIdCharacters("GHj:df(g?h d_fk7-58.9)3!47"));

			Assert.False(GAValidator.ValidateEventIdCharacters("GHj:df(g?h d_fk,7-58.9)3!47"));
			Assert.False(GAValidator.ValidateEventIdCharacters(null));
			Assert.False(GAValidator.ValidateEventIdCharacters(""));
		}

		[Test]
		public void TestValidateShortString()
		{
			Assert.True(GAValidator.ValidateShortString(GATestUtilities.GetRandomString(32), false));
			Assert.True(GAValidator.ValidateShortString(GATestUtilities.GetRandomString(32), true));
			Assert.True(GAValidator.ValidateShortString(GATestUtilities.GetRandomString(10), false));
			Assert.True(GAValidator.ValidateShortString(GATestUtilities.GetRandomString(10), true));
			Assert.True(GAValidator.ValidateShortString(null, true));
			Assert.True(GAValidator.ValidateShortString("", true));

			Assert.False(GAValidator.ValidateShortString(GATestUtilities.GetRandomString(40), false));
			Assert.False(GAValidator.ValidateShortString(GATestUtilities.GetRandomString(40), true));
			Assert.False(GAValidator.ValidateShortString(null, false));
			Assert.False(GAValidator.ValidateShortString("", false));
		}

		[Test]
		public void TestValidateString()
		{
			Assert.True(GAValidator.ValidateString(GATestUtilities.GetRandomString(64), false));
			Assert.True(GAValidator.ValidateString(GATestUtilities.GetRandomString(64), true));
			Assert.True(GAValidator.ValidateString(GATestUtilities.GetRandomString(10), false));
			Assert.True(GAValidator.ValidateString(GATestUtilities.GetRandomString(10), true));
			Assert.True(GAValidator.ValidateString(null, true));
			Assert.True(GAValidator.ValidateString("", true));

			Assert.False(GAValidator.ValidateString(GATestUtilities.GetRandomString(80), false));
			Assert.False(GAValidator.ValidateString(GATestUtilities.GetRandomString(80), true));
			Assert.False(GAValidator.ValidateString(null, false));
			Assert.False(GAValidator.ValidateString("", false));
		}

		[Test]
		public void TestValidateLongString()
		{
			Assert.True(GAValidator.ValidateString(GATestUtilities.GetRandomString(64), false));
			Assert.True(GAValidator.ValidateString(GATestUtilities.GetRandomString(64), true));
			Assert.True(GAValidator.ValidateString(GATestUtilities.GetRandomString(10), false));
			Assert.True(GAValidator.ValidateString(GATestUtilities.GetRandomString(10), true));
			Assert.True(GAValidator.ValidateString(null, true));
			Assert.True(GAValidator.ValidateString("", true));

			Assert.False(GAValidator.ValidateString(GATestUtilities.GetRandomString(80), false));
			Assert.False(GAValidator.ValidateString(GATestUtilities.GetRandomString(80), true));
			Assert.False(GAValidator.ValidateString(null, false));
			Assert.False(GAValidator.ValidateString("", false));
		}

		[Test]
		public void TestValidateArrayOfStrings()
		{
			Assert.True(GAValidator.ValidateArrayOfStrings(3, 10, false, "test", GATestUtilities.GetRandomString(3), GATestUtilities.GetRandomString(10), GATestUtilities.GetRandomString(7)));
			Assert.True(GAValidator.ValidateArrayOfStrings(3, 10, true, "test", new string[0]));

	        Assert.False(GAValidator.ValidateArrayOfStrings(3, 10, false, "test", GATestUtilities.GetRandomString(3), GATestUtilities.GetRandomString(12), GATestUtilities.GetRandomString(7)));
	        Assert.False(GAValidator.ValidateArrayOfStrings(3, 10, false, "test", GATestUtilities.GetRandomString(3), "", GATestUtilities.GetRandomString(7)));
	        Assert.False(GAValidator.ValidateArrayOfStrings(3, 10, false, "test", GATestUtilities.GetRandomString(3), null, GATestUtilities.GetRandomString(7)));
	        Assert.False(GAValidator.ValidateArrayOfStrings(2, 10, false, "test", GATestUtilities.GetRandomString(3), GATestUtilities.GetRandomString(10), GATestUtilities.GetRandomString(7)));
	        Assert.False(GAValidator.ValidateArrayOfStrings(3, 10, false, "test", new string[0]));
	        Assert.False(GAValidator.ValidateArrayOfStrings(3, 10, false, "test", (string)null));
	    }

		[Test]
		public void TestValidateBirthyear()
		{
			Assert.True(GAValidator.ValidateBirthyear(1982));
			Assert.True(GAValidator.ValidateBirthyear(9999));
			Assert.True(GAValidator.ValidateBirthyear(0));

			Assert.False(GAValidator.ValidateBirthyear(10000));
			Assert.False(GAValidator.ValidateBirthyear(-1));
		}

		[Test]
		public void TestValidateClientTs()
		{
			Assert.True(GAValidator.ValidateClientTs(GAUtilities.TimeIntervalSince1970()));

			Assert.False(GAValidator.ValidateClientTs(long.MinValue));
			Assert.False(GAValidator.ValidateClientTs(long.MaxValue));
		}

		[Test]
		public void TestValidateUserId()
		{
			Assert.True(GAValidator.ValidateUserId("fhjkdfghdfjkgh"));

			Assert.False(GAValidator.ValidateUserId(null));
			Assert.False(GAValidator.ValidateUserId(""));
		}
	}
}
