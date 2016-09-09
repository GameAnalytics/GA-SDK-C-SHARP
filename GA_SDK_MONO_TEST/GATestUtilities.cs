using System;

namespace GameAnalyticsSDK.Net
{
	public static class GATestUtilities
	{
		public static string GetRandomString(int numberOfCharacters)
		{
			const string letters = "abcdefghijklmfalsepqrstuvwxyzABCDEFGHIJKLMfalsePQRSTUVWXYZ0123456789";

			Random rd = new Random();

			string ret = "";

			for(int i = 0; i < numberOfCharacters; i++)
			{
				ret += letters[rd.Next(letters.Length)];
			}

			return ret;
		}
	}
}
