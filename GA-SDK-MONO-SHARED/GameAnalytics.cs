using System;
using GameAnalyticsSDK.Net.Threading;
using GameAnalyticsSDK.Net.Logging;
using GameAnalyticsSDK.Net.State;
using GameAnalyticsSDK.Net.Validators;
using GameAnalyticsSDK.Net.Device;
using GameAnalyticsSDK.Net.Events;
using GameAnalyticsSDK.Net.Store;
#if UNITY_WEBGL || UNITY_TIZEN
using System.Collections.Generic;
using System.Collections;
#elif WINDOWS_UWP || WINDOWS_WSA
using Windows.UI.Xaml;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Core;
#endif

namespace GameAnalyticsSDK.Net
{
	public static class GameAnalytics
	{
		static GameAnalytics()
		{
			GADevice.Touch();
		}

#if !UNITY
        public static event Action<string, EGALoggerMessageType> OnMessageLogged;

        internal static void MessageLogged(string message, EGALoggerMessageType type)
        {
            if(OnMessageLogged != null)
            {
                OnMessageLogged(message, type);
            }
        }
#endif

#if UNITY_WEBGL || UNITY_TIZEN

		private static Queue<IEnumerator> _requestCoroutineQueue = new Queue<IEnumerator>();

		public static Queue<IEnumerator> RequestCoroutineQueue
		{
			get { return _requestCoroutineQueue; }
		}

#endif

        #region CONFIGURE

        public static void ConfigureAvailableCustomDimensions01(params string[] customDimensions)
		{
			GAThreading.PerformTaskOnGAThread("configureAvailableCustomDimensions01", () =>
			{
				if (IsSdkReady(true, false))
				{
					GALogger.W("Available custom dimensions must be set before SDK is initialized");
					return;
				}
				GAState.AvailableCustomDimensions01 = customDimensions;
			});
		}

		public static void ConfigureAvailableCustomDimensions02(params string[] customDimensions)
		{
			GAThreading.PerformTaskOnGAThread("configureAvailableCustomDimensions02", () =>
			{
				if (IsSdkReady(true, false))
				{
					GALogger.W("Available custom dimensions must be set before SDK is initialized");
					return;
				}
				GAState.AvailableCustomDimensions02 = customDimensions;
			});
		}

		public static void ConfigureAvailableCustomDimensions03(params string[] customDimensions)
		{
			GAThreading.PerformTaskOnGAThread("configureAvailableCustomDimensions03", () =>
			{
				if (IsSdkReady(true, false))
				{
					GALogger.W("Available custom dimensions must be set before SDK is initialized");
					return;
				}
				GAState.AvailableCustomDimensions03 = customDimensions;
			});
		}

		public static void ConfigureAvailableResourceCurrencies(params string[] resourceCurrencies)
		{
			GAThreading.PerformTaskOnGAThread("configureAvailableResourceCurrencies", () =>
			{
				if (IsSdkReady(true, false))
				{
					GALogger.W("Available resource currencies must be set before SDK is initialized");
					return;
				}
				GAState.AvailableResourceCurrencies = resourceCurrencies;
			});
		}

		public static void ConfigureAvailableResourceItemTypes(params string[] resourceItemTypes)
		{
			GAThreading.PerformTaskOnGAThread("configureAvailableResourceItemTypes", () =>
			{
				if (IsSdkReady(true, false))
				{
					GALogger.W("Available resource item types must be set before SDK is initialized");
					return;
				}
				GAState.AvailableResourceItemTypes = resourceItemTypes;
			});
		}

		public static void ConfigureBuild(string build)
		{
			GAThreading.PerformTaskOnGAThread("configureBuild", () =>
			{
				if (IsSdkReady(true, false))
				{
					GALogger.W("Build version must be set before SDK is initialized.");
					return;
				}
				if (!GAValidator.ValidateBuild(build))
				{
					GALogger.I("Validation fail - configure build: Cannot be null, empty or above 32 length. String: " + build);
					return;
				}
				GAState.Build = build;
			});
		}

		public static void ConfigureSdkGameEngineVersion(string sdkGameEngineVersion)
		{
			GAThreading.PerformTaskOnGAThread("configureSdkGameEngineVersion", () =>
			{
				if (IsSdkReady(true, false))
				{
					return;
				}
				if (!GAValidator.ValidateSdkWrapperVersion(sdkGameEngineVersion))
				{
					GALogger.I("Validation fail - configure sdk version: Sdk version not supported. String: " + sdkGameEngineVersion);
					return;
				}
				GADevice.SdkGameEngineVersion = sdkGameEngineVersion;
			});
		}

		public static void ConfigureGameEngineVersion(string gameEngineVersion)
		{
			GAThreading.PerformTaskOnGAThread("configureGameEngineVersion", () =>
			{
				if (IsSdkReady(true, false))
				{
					return;
				}
				if (!GAValidator.ValidateEngineVersion(gameEngineVersion))
				{
					GALogger.I("Validation fail - configure sdk version: Sdk version not supported. String: " + gameEngineVersion);
					return;
				}
				GADevice.GameEngineVersion = gameEngineVersion;
			});
		}

		public static void ConfigureUserId(string uId)
		{
			GAThreading.PerformTaskOnGAThread("configureUserId", () =>
			{
				if (IsSdkReady(true, false))
				{
					GALogger.W("A custom user id must be set before SDK is initialized.");
					return;
				}
				if (!GAValidator.ValidateUserId(uId))
				{
					GALogger.I("Validation fail - configure user_id: Cannot be null, empty or above 64 length. Will use default user_id method. Used string: " + uId);
					return;
				}

				GAState.UserId = uId;
			});
		}

		#endregion // CONFIGURE

		#region INITIALIZE

		public static void Initialize(string gameKey, string gameSecret)
		{
#if WINDOWS_UWP || WINDOWS_WSA
            CoreApplication.Suspending += OnSuspending;
            CoreApplication.Resuming += OnResuming;
#endif
            GADevice.UpdateConnectionType();

			GAThreading.PerformTaskOnGAThread("initialize", () =>
			{
				if (IsSdkReady(true, false))
				{
					GALogger.W("SDK already initialized. Can only be called once.");
					return;
				}
				if (!GAValidator.ValidateKeys(gameKey, gameSecret))
				{
					GALogger.W("SDK failed initialize. Game key or secret key is invalid. Can only contain characters A-z 0-9, gameKey is 32 length, gameSecret is 40 length. Failed keys - gameKey: " + gameKey + ", secretKey: " + gameSecret);
					return;
				}

				GAState.SetKeys(gameKey, gameSecret);

				if (!GAStore.EnsureDatabase(false))
				{
					GALogger.W("Could not ensure/validate local event database: " + GADevice.WritablePath);
				}

				GAState.InternalInitialize();
            });
		}

#if WINDOWS_UWP || WINDOWS_WSA
        private static void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
			if(!GAState.UseManualSessionHandling)
			{
				OnStop();
			}
			else
			{
				GALogger.I("OnSuspending: Not calling GameAnalytics.OnStop() as using manual session handling");
			}
            deferral.Complete();
        }

        private static void OnResuming(object sender, object e)
        {
            GAThreading.PerformTaskOnGAThread("onResuming", () =>
            {
				if(!GAState.UseManualSessionHandling)
				{
					OnResume();
				}
				else
				{
					GALogger.I("OnResuming: Not calling GameAnalytics.OnResume() as using manual session handling");
				}
            });
        }
#endif

        #endregion // INITIALIZE

        #region ADD EVENTS

        public static void AddBusinessEvent(string currency, int amount, string itemType, string itemId, string cartType)
		{
			GADevice.UpdateConnectionType();

			GAThreading.PerformTaskOnGAThread("addBusinessEvent", () =>
			{
				if (!IsSdkReady(true, true, "Could not add business event"))
				{
					return;
				}
				// Send to events
				GAEvents.AddBusinessEvent(currency, amount, itemType, itemId, cartType);
			});
		}

		public static void AddResourceEvent(EGAResourceFlowType flowType, string currency, float amount, string itemType, string itemId)
		{
			GADevice.UpdateConnectionType();

			GAThreading.PerformTaskOnGAThread("addResourceEvent", () =>
			{
				if (!IsSdkReady(true, true, "Could not add resource event"))
				{
					return;
				}

				GAEvents.AddResourceEvent(flowType, currency, amount, itemType, itemId);
			});
		}

		public static void AddProgressionEvent(EGAProgressionStatus progressionStatus, string progression01)
		{
			AddProgressionEvent(progressionStatus, progression01, "", "");
		}

		public static void AddProgressionEvent(EGAProgressionStatus progressionStatus, string progression01, double score)
		{
			AddProgressionEvent(progressionStatus, progression01, "", "", score);
		}

		public static void AddProgressionEvent(EGAProgressionStatus progressionStatus, string progression01, string progression02)
		{
			AddProgressionEvent(progressionStatus, progression01, progression02, "");
		}

		public static void AddProgressionEvent(EGAProgressionStatus progressionStatus, string progression01, string progression02, double score)
		{
			AddProgressionEvent(progressionStatus, progression01, progression02, "", score);
		}

		public static void AddProgressionEvent(EGAProgressionStatus progressionStatus, string progression01, string progression02, string progression03)
		{
			GADevice.UpdateConnectionType();

			GAThreading.PerformTaskOnGAThread("addProgressionEvent", () =>
			{
				if(!IsSdkReady(true, true, "Could not add progression event"))
				{
					return;
				}

				// Send to events
				// TODO(nikolaj): check if this cast from int to double is OK
				GAEvents.AddProgressionEvent(progressionStatus, progression01, progression02, progression03, 0, false);
			});
		}

		public static void AddProgressionEvent(EGAProgressionStatus progressionStatus, string progression01, string progression02, string progression03, double score)
		{
			GADevice.UpdateConnectionType();

			GAThreading.PerformTaskOnGAThread("addProgressionEvent", () =>
			{
				if (!IsSdkReady(true, true, "Could not add progression event"))
				{
					return;
				}

				// Send to events
				// TODO(nikolaj): check if this cast from int to double is OK
				GAEvents.AddProgressionEvent(progressionStatus, progression01, progression02, progression03, score, true);
			});
		}

		public static void AddDesignEvent(string eventId)
		{
			GADevice.UpdateConnectionType();

			GAThreading.PerformTaskOnGAThread("addDesignEvent", () =>
			{
				if(!IsSdkReady(true, true, "Could not add design event"))
				{
					return;
				}
				GAEvents.AddDesignEvent(eventId, 0, false);
			});
		}

		public static void AddDesignEvent(string eventId, double value)
		{
			GADevice.UpdateConnectionType();

			GAThreading.PerformTaskOnGAThread("addDesignEvent", () =>
			{
				if (!IsSdkReady(true, true, "Could not add design event"))
				{
					return;
				}
				GAEvents.AddDesignEvent(eventId, value, true);
			});
		}

		public static void AddErrorEvent(EGAErrorSeverity severity, string message)
		{
			GADevice.UpdateConnectionType();

			GAThreading.PerformTaskOnGAThread("addErrorEvent", () =>
			{
				if (!IsSdkReady(true, true, "Could not add error event"))
				{
					return;
				}
				GAEvents.AddErrorEvent(severity, message);
			});
		}

		#endregion // ADD EVENTS

		#region SET STATE CHANGES WHILE RUNNING

		public static void SetEnabledInfoLog(bool flag)
		{
			GAThreading.PerformTaskOnGAThread("setEnabledInfoLog", () =>
			{
				if (flag)
				{
					GALogger.InfoLog = flag;
					GALogger.I("Info logging enabled");
				}
				else
				{
					GALogger.I("Info logging disabled");
					GALogger.InfoLog = flag;
				}
			});
		}

		public static void SetEnabledVerboseLog(bool flag)
		{
			GAThreading.PerformTaskOnGAThread("setEnabledVerboseLog", () =>
			{
				if (flag)
				{
					GALogger.VerboseLog = flag;
					GALogger.I("Verbose logging enabled");
				}
				else
				{
					GALogger.I("Verbose logging disabled");
					GALogger.VerboseLog = flag;
				}
			});
		}

		public static void SetEnabledManualSessionHandling(bool flag)
		{
			GAThreading.PerformTaskOnGAThread("setEnabledManualSessionHandling", () =>
			{
				GAState.SetManualSessionHandling(flag);
			});
		}

		public static void SetCustomDimension01(string dimension)
		{
			GAThreading.PerformTaskOnGAThread("setCustomDimension01", () =>
			{
				if (!GAValidator.ValidateDimension01(dimension))
				{
					GALogger.W("Could not set custom01 dimension value to '" + dimension + "'. Value not found in available custom01 dimension values");
					return;
				}
				GAState.SetCustomDimension01(dimension);
			});
		}

		public static void SetCustomDimension02(string dimension)
		{
			GAThreading.PerformTaskOnGAThread("setCustomDimension02", () =>
			{
				if (!GAValidator.ValidateDimension02(dimension))
				{
					GALogger.W("Could not set custom02 dimension value to '" + dimension + "'. Value not found in available custom02 dimension values");
					return;
				}
				GAState.SetCustomDimension02(dimension);
			});
		}

		public static void SetCustomDimension03(string dimension)
		{
			GAThreading.PerformTaskOnGAThread("setCustomDimension03", () =>
			{
				if (!GAValidator.ValidateDimension03(dimension))
				{
					GALogger.W("Could not set custom03 dimension value to '" + dimension + "'. Value not found in available custom03 dimension values");
					return;
				}
				GAState.SetCustomDimension03(dimension);
			});
		}

		public static void SetFacebookId(string facebookId)
		{
			GAThreading.PerformTaskOnGAThread("setFacebookId", () =>
			{
				if (GAValidator.ValidateFacebookId(facebookId))
				{
					GAState.SetFacebookId(facebookId);
				}
			});
		}

		public static void SetGender(EGAGender gender)
		{
			GAThreading.PerformTaskOnGAThread("setGender", () =>
			{
				if (GAValidator.ValidateGender(gender))
				{
					GAState.SetGender(gender);
				}
			});
		}

		public static void SetBirthYear(int birthYear)
		{
			GAThreading.PerformTaskOnGAThread("setBirthYear", () =>
			{
				if (GAValidator.ValidateBirthyear(birthYear))
				{
					GAState.SetBirthYear(birthYear);
				}
			});
		}

		#endregion // SET STATE CHANGES WHILE RUNNING

		public static void StartSession()
		{
			GAThreading.PerformTaskOnGAThread("startSession", () =>
			{
				if(GAState.UseManualSessionHandling)
				{
					if(!GAState.Initialized)
					{
						return;
					}

					if(GAState.IsEnabled() && GAState.SessionIsStarted())
					{
						GAState.EndSessionAndStopQueue();
					}

					GAState.ResumeSessionAndStartQueue();
				}
			});
		}

		public static void EndSession()
		{
			if(GAState.UseManualSessionHandling)
			{
				OnStop();
			}
		}

		public static void OnStop()
		{
			GALogger.D("OnStop() called");
#if !UNITY_WEBGL && !UNITY_TIZEN
			GAThreading.PerformTaskOnGAThread("onStop", () =>
			{
				try
				{
					GAState.EndSessionAndStopQueue();
				}
				catch(Exception)
				{
				}
			});
#else
			try
            {
                GAState.EndSessionAndStopQueue();
            }
            catch (Exception)
            {
            }
#endif
        }

		public static void OnResume()
		{
			GALogger.D("OnResume() called");
#if !UNITY_WEBGL && !UNITY_TIZEN
			GAThreading.PerformTaskOnGAThread("onResume", () =>
			{
				GAState.ResumeSessionAndStartQueue();
			});
#else
			GAState.ResumeSessionAndStartQueue();
#endif
		}

		#region PRIVATE HELPERS

		private static bool IsSdkReady(bool needsInitialized)
		{
			return IsSdkReady(needsInitialized, true);
		}

		private static bool IsSdkReady(bool needsInitialized, bool warn)
		{
			return IsSdkReady(needsInitialized, warn, "");
		}

		private static bool IsSdkReady(bool needsInitialized, bool warn, String message)
		{
			if (!string.IsNullOrEmpty(message))
			{
				message = message + ": ";
			}

			// Make sure database is ready
			if (!GAStore.IsTableReady)
			{
				if (warn)
				{
					GALogger.W(message + "Datastore not initialized");
				}
				return false;
			}
			// Is SDK initialized
			if (needsInitialized && !GAState.Initialized)
			{
				if (warn)
				{
					GALogger.W(message + "SDK is not initialized");
				}
				return false;
			}
			// Is SDK enabled
			if (needsInitialized && !GAState.IsEnabled())
			{
				if (warn)
				{
					GALogger.W(message + "SDK is disabled");
				}
				return false;
			}
			return true;
		}

		#endregion // PRIVATE HELPERS
	}
}
