using System;
using GameAnalyticsSDK.Net.Threading;
using GameAnalyticsSDK.Net.Logging;
using System.Collections.Generic;
using GameAnalyticsSDK.Net.Store;
using GameAnalyticsSDK.Net.Utilities;
using GameAnalyticsSDK.Net.Http;
using GameAnalyticsSDK.Net.State;
using GameAnalyticsSDK.Net.Validators;
using System.Globalization;

namespace GameAnalyticsSDK.Net.Events
{
	internal class GAEvents
	{
		#region Fields and properties

		private static readonly GAEvents _instance = new GAEvents();
		private const string CategorySessionStart = "user";
		private const string CategorySessionEnd = "session_end";
		private const string CategoryDesign = "design";
		private const string CategoryBusiness = "business";
		private const string CategoryProgression = "progression";
		private const string CategoryResource = "resource";
		private const string CategoryError = "error";
		private bool isRunning;
		private bool keepRunning;
		private const double ProcessEventsIntervalInSeconds = 8.0;
		private const int MaxEventCount = 500;

		private static GAEvents Instance
		{
			get
			{
				return _instance;
			}
		}

		#endregion // Fields and properties

		private GAEvents()
		{
		}

		#region Public methods

		public static void StopEventQueue()
		{
			Instance.keepRunning = false;
		}

		public static void EnsureEventQueueIsRunning()
		{
			Instance.keepRunning = true;

			if(!Instance.isRunning)
			{
				Instance.isRunning = true;
				GAThreading.ScheduleTimer(ProcessEventsIntervalInSeconds, "processEventQueue", ProcessEventQueue);
			}
		}

		public static void AddSessionStartEvent()
		{
			string categorySessionStart = CategorySessionStart;

			// Event specific data
			JSONClass eventDict = new JSONClass();
			eventDict["category"] = categorySessionStart;

			// Increment session number  and persist
			GAState.IncrementSessionNum();
			GAStore.SetState(GAState.SessionNumKey, GAState.SessionNum.ToString(CultureInfo.InvariantCulture));

			// Add custom dimensions
			AddDimensionsToEvent(eventDict);

			// Add to store
			AddEventToStore(eventDict);

			// Log
			GALogger.I("Add SESSION START event");

			// Send event right away
			ProcessEvents(categorySessionStart, false);
		}

		public static void AddSessionEndEvent()
		{
			long session_start_ts = GAState.SessionStart;
			long client_ts_adjusted = GAState.GetClientTsAdjusted();
			long sessionLength = client_ts_adjusted - session_start_ts;

			if(sessionLength < 0)
			{
				// Should never happen.
				// Could be because of edge cases regarding time altering on device.
				GALogger.W("Session length was calculated to be less then 0. Should not be possible. Resetting to 0.");
				sessionLength = 0;
			}

			// Event specific data
			JSONClass eventDict = new JSONClass();
			eventDict["category"] = CategorySessionEnd;
			eventDict.Add("length", new JSONData(sessionLength));

			// Add custom dimensions
			AddDimensionsToEvent(eventDict);

			// Add to store
			AddEventToStore(eventDict);

			// Log
			GALogger.I("Add SESSION END event.");

			// Send all event right away
			ProcessEvents("", false);
		}

		public static void AddBusinessEvent(
			string currency,
			int amount,
			string itemType,
			string itemId,
			string cartType
		)
		{
			// Validate event params
			if (!GAValidator.ValidateBusinessEvent(currency, amount, cartType, itemType, itemId))
			{
				GAHTTPApi.Instance.SendSdkErrorEvent(EGASdkErrorType.Rejected);
				return;
			}

			// Create empty eventData
			JSONClass eventDict = new JSONClass();

			// Increment transaction number and persist
			GAState.IncrementTransactionNum();
			GAStore.SetState(GAState.TransactionNumKey, GAState.TransactionNum.ToString(CultureInfo.InvariantCulture));

			// Required
			eventDict["event_id"] = itemType + ":" + itemId;
			eventDict["category"] = CategoryBusiness;
			eventDict["currency"] = currency;
			eventDict.Add("amount", new JSONData(amount));
			eventDict.Add(GAState.TransactionNumKey, new JSONData(GAState.TransactionNum));

			// Optional
			if (!string.IsNullOrEmpty(cartType))
			{
				eventDict.Add("cart_type", cartType);
			}

			// Add custom dimensions
			AddDimensionsToEvent(eventDict);

			// Log
			GALogger.I("Add BUSINESS event: {currency:" + currency + ", amount:" + amount + ", itemType:" + itemType + ", itemId:" + itemId + ", cartType:" + cartType + "}");

			// Send to store
			AddEventToStore(eventDict);
		}

		public static void AddResourceEvent(EGAResourceFlowType flowType, string currency, double amount, string itemType, string itemId)
		{
			// Validate event params
			if (!GAValidator.ValidateResourceEvent(flowType, currency, (long)amount, itemType, itemId))
			{
				GAHTTPApi.Instance.SendSdkErrorEvent(EGASdkErrorType.Rejected);
				return;
			}

			// If flow type is sink reverse amount
			if (flowType == EGAResourceFlowType.Sink)
			{
				amount *= -1;
			}

			// Create empty eventData
			JSONClass eventDict = new JSONClass();

			// insert event specific values
			string flowTypeString = ResourceFlowTypeToString(flowType);
			eventDict["event_id"] = flowTypeString + ":" + currency + ":" + itemType + ":" + itemId;
			eventDict["category"] = CategoryResource;
			eventDict.Add("amount", new JSONData(amount));

			// Add custom dimensions
			AddDimensionsToEvent(eventDict);

			// Log
			GALogger.I("Add RESOURCE event: {currency:" + currency + ", amount:" + amount + ", itemType:" + itemType + ", itemId:" + itemId + "}");

			// Send to store
			AddEventToStore(eventDict);
		}

		public static void AddProgressionEvent(EGAProgressionStatus progressionStatus, string progression01, string progression02, string progression03, double score, bool sendScore)
		{
			string progressionStatusString = ProgressionStatusToString(progressionStatus);

			// Validate event params
			if (!GAValidator.ValidateProgressionEvent(progressionStatus, progression01, progression02, progression03))
			{
				GAHTTPApi.Instance.SendSdkErrorEvent(EGASdkErrorType.Rejected);
				return;
			}

			// Create empty eventData
			JSONClass eventDict = new JSONClass();

			// Progression identifier
			string progressionIdentifier;

			if (string.IsNullOrEmpty(progression02))
			{
				progressionIdentifier = progression01;
			}
			else if (string.IsNullOrEmpty(progression03))
			{
				progressionIdentifier = progression01 + ":" + progression02;
			}
			else
			{
				progressionIdentifier = progression01 + ":" + progression02 + ":" + progression03;
			}

			// Append event specifics
			eventDict["category"] = CategoryProgression;
			eventDict["event_id"] = progressionStatusString + ":" + progressionIdentifier;

			// Attempt
			double attempt_num = 0;

			// Add score if specified and status is not start
			if (sendScore && progressionStatus != EGAProgressionStatus.Start)
			{
				eventDict.Add("score", new JSONData(score));
			}

			// Count attempts on each progression fail and persist
			if (progressionStatus == EGAProgressionStatus.Fail)
			{
				// Increment attempt number
				GAState.IncrementProgressionTries(progressionIdentifier);
			}

			// increment and add attempt_num on complete and delete persisted
			if (progressionStatus == EGAProgressionStatus.Complete)
			{
				// Increment attempt number
				GAState.IncrementProgressionTries(progressionIdentifier);

				// Add to event
				attempt_num = GAState.GetProgressionTries(progressionIdentifier);
				eventDict.Add("attempt_num", new JSONData(attempt_num));

				// Clear
				GAState.ClearProgressionTries(progressionIdentifier);
			}

			// Add custom dimensions
			AddDimensionsToEvent(eventDict);

			// Log
			GALogger.I("Add PROGRESSION event: {status:" + progressionStatusString + ", progression01:" + progression01 + ", progression02:" + progression02 + ", progression03:" + progression03 + ", score:" + score + ", attempt:" + attempt_num + "}");

			// Send to store
			AddEventToStore(eventDict);
		}

		public static void AddDesignEvent(string eventId, double value, bool sendValue)
		{
			// Validate
			if (!GAValidator.ValidateDesignEvent(eventId, value))
			{
				GAHTTPApi.Instance.SendSdkErrorEvent(EGASdkErrorType.Rejected);
				return;
			}

			// Create empty eventData
			JSONClass eventData = new JSONClass();

			// Append event specifics
			eventData["category"] = CategoryDesign;
			eventData["event_id"] = eventId;

			if(sendValue)
			{
				eventData.Add("value", new JSONData(value));
			}

			// Log
			GALogger.I("Add DESIGN event: {eventId:" + eventId + ", value:" + value + "}");

			// Send to store
			AddEventToStore(eventData);
		}

		public static void AddErrorEvent(EGAErrorSeverity severity, string message)
		{
			string severityString = ErrorSeverityToString(severity);

			// Validate
			if (!GAValidator.ValidateErrorEvent(severity, message))
			{
				GAHTTPApi.Instance.SendSdkErrorEvent(EGASdkErrorType.Rejected);
				return;
			}

			// Create empty eventData
			JSONClass eventData = new JSONClass();

			// Append event specifics
			eventData["category"] = CategoryError;
			eventData["severity"] = severityString;
			eventData["message"] = message;

			// Log
			GALogger.I("Add ERROR event: {severity:" + severityString + ", message:" + message + "}");

			// Send to store
			AddEventToStore(eventData);
		}

		#endregion // Public methods

		#region Private methods

		private static void ProcessEventQueue()
		{
			ProcessEvents("", true);
			if(Instance.keepRunning)
			{
				GAThreading.ScheduleTimer(ProcessEventsIntervalInSeconds, "processEventQueue", ProcessEventQueue);
			}
			else
			{
				Instance.isRunning = false;
			}
		}

#if WINDOWS_UWP || WINDOWS_WSA
        private async static void ProcessEvents(string category, bool performCleanUp)
#else
        private static void ProcessEvents(string category, bool performCleanUp)
#endif
        {
			try
			{
				string requestIdentifier = Guid.NewGuid().ToString();
				
				string selectSql;
				string updateSql;
				string deleteSql = "DELETE FROM ga_events WHERE status = '" + requestIdentifier + "'";
				string putbackSql = "UPDATE ga_events SET status = 'new' WHERE status = '" + requestIdentifier + "';";
				
				// Cleanup
				if(performCleanUp)
				{
					CleanupEvents();
					FixMissingSessionEndEvents();
				}
				
				// Prepare SQL
				string andCategory = "";
				if(!string.IsNullOrEmpty(category))
				{
					andCategory = " AND category='" + category + "' ";
				}
				selectSql = "SELECT event FROM ga_events WHERE status = 'new' " + andCategory + ";";
				updateSql = "UPDATE ga_events SET status = '" + requestIdentifier + "' WHERE status = 'new' " + andCategory + ";";
				
				// Get events to process
				JSONArray events = GAStore.ExecuteQuerySync(selectSql);
				
				// Check for errors or empty
				if(events == null)
				{
					GALogger.I("Event queue: No events to send");
					return;
				}
				
				// Check number of events and take some action if there are too many?
				if(events.Count > MaxEventCount)
				{
					// Make a limit request
					selectSql = "SELECT client_ts FROM ga_events WHERE status = 'new' " + andCategory + " ORDER BY client_ts ASC LIMIT 0," + MaxEventCount + ";";
					events = GAStore.ExecuteQuerySync(selectSql);
					if(events == null)
					{
						return;
					}
				
					// Get last timestamp
					JSONNode lastItem = events[events.Count - 1];
					string lastTimestamp = lastItem["client_ts"].AsString;

					// Select again
					selectSql = "SELECT event FROM ga_events WHERE status = 'new' " + andCategory + " AND client_ts<='" + lastTimestamp + "';";
					events = GAStore.ExecuteQuerySync(selectSql);
					if (events == null)
					{
						return;
					}

					// Update sql
					updateSql = "UPDATE ga_events SET status='" + requestIdentifier + "' WHERE status='new' " + andCategory + " AND client_ts<='" + lastTimestamp + "';";
				}

				// Log
				GALogger.I("Event queue: Sending " + events.Count + " events.");

				// Set status of events to 'sending' (also check for error)
				if (GAStore.ExecuteQuerySync(updateSql) == null)
				{
					return;
				}

				// Create payload data from events
				List<JSONNode> payloadArray = new List<JSONNode>();

				for (int i = 0; i < events.Count; ++i)
				{
					JSONNode ev = events[i];
					JSONNode eventDict = JSONNode.LoadFromBase64(ev["event"].AsString);
					if (eventDict.Count != 0)
					{
						payloadArray.Add(eventDict);
					}
				}

                // send events
#if WINDOWS_UWP || WINDOWS_WSA
                KeyValuePair<EGAHTTPApiResponse, JSONNode> result = await GAHTTPApi.Instance.SendEventsInArray(payloadArray);
#else
                KeyValuePair<EGAHTTPApiResponse, JSONNode> result = GAHTTPApi.Instance.SendEventsInArray(payloadArray);
#endif

                ProcessEvents(result.Key, result.Value, putbackSql, deleteSql, payloadArray.Count);
            }
            catch (Exception e)
			{
				GALogger.E("Error during ProcessEvents(): " + e);
			}
		}

		public static void ProcessEvents(EGAHTTPApiResponse responseEnum, JSONNode dataDict, string putbackSql, string deleteSql, int eventCount)
		{
			if(responseEnum == EGAHTTPApiResponse.Ok)
			{
				// Delete events
				GAStore.ExecuteQuerySync(deleteSql);
				GALogger.I("Event queue: " + eventCount + " events sent.");
			}
			else
			{
				// Put events back (Only in case of no response)
				if(responseEnum == EGAHTTPApiResponse.NoResponse)
				{
					GALogger.W("Event queue: Failed to send events to collector - Retrying next time");
					GAStore.ExecuteQuerySync(putbackSql);
					// Delete events (When getting some anwser back always assume events are processed)
				}
				else
				{
					if(dataDict != null)
					{
						JSONNode json = null;
						IEnumerator<JSONNode> enumerator = dataDict.Childs.GetEnumerator();
						if(enumerator.MoveNext())
						{
							json = enumerator.Current;
						}

						if(responseEnum == EGAHTTPApiResponse.BadRequest && json is JSONArray)
						{
							GALogger.W("Event queue: " + eventCount + " events sent. " + dataDict.Count + " events failed GA server validation.");
						}
						else
						{
							GALogger.W("Event queue: Failed to send events.");
						}
					}
					else
					{
						GALogger.W("Event queue: Failed to send events.");
					}

					GAStore.ExecuteQuerySync(deleteSql);
				}
			}

            UpdateSessionTime();
        }

		private static void CleanupEvents()
		{
			GAStore.ExecuteQuerySync("UPDATE ga_events SET status = 'new';");
		}

		private static void FixMissingSessionEndEvents()
		{
			// Get all sessions that are not current
			Dictionary<string, object> parameters = new Dictionary<string, object>();
			parameters.Add("$session_id", GAState.SessionId);


            string sql = "SELECT timestamp, event FROM ga_session WHERE session_id != $session_id;";
			JSONArray sessions = GAStore.ExecuteQuerySync(sql, parameters);

			if (sessions == null)
			{
				return;
			}

			GALogger.I(sessions.Count + " session(s) located with missing session_end event.");

			// Add missing session_end events
			for (int i = 0; i < sessions.Count; ++i)
			{
				JSONNode session = sessions[i];
				JSONNode sessionEndEvent = JSONNode.LoadFromBase64(session["event"].AsString);
				long event_ts = sessionEndEvent["client_ts"].AsLong;
				long start_ts = session["timestamp"].AsLong;

				long length = event_ts - start_ts;
				length = Math.Max(0, length);

				GALogger.D("fixMissingSessionEndEvents length calculated: " + length);

				sessionEndEvent["category"] = CategorySessionEnd;
				sessionEndEvent.Add("length", new JSONData(length));

				// Add to store
				AddEventToStore(sessionEndEvent.AsObject);
			}
        }

#if WINDOWS_WSA
        private async static void AddEventToStore(JSONClass eventData)
#else
        private static void AddEventToStore(JSONClass eventData)
#endif
        {
			// Check if datastore is available
			if (!GAStore.IsTableReady)
			{
				GALogger.W("Could not add event: SDK datastore error");
				return;
			}

			// Check if we are initialized
			if (!GAState.Initialized)
			{
				GALogger.W("Could not add event: SDK is not initialized");
				return;
			}

			try
			{
                // Check db size limits (10mb)
                // If database is too large block all except user, session and business
                if (GAStore.IsDbTooLargeForEvents && !GAUtilities.StringMatch(eventData["category"].AsString, "^(user|session_end|business)$"))
				{
					GALogger.W("Database too large. Event has been blocked.");
					return;
				}

				// Get default annotations
				JSONClass ev = GAState.GetEventAnnotations();

				// Create json with only default annotations
				string jsonDefaults = ev.SaveToBase64();

				// Merge with eventData
				foreach(KeyValuePair<string,JSONNode> pair in eventData)
				{
					ev.Add(pair.Key, pair.Value);
				}

				// Create json string representation
				string json = ev.ToString();

				// output if VERBOSE LOG enabled

				GALogger.II("Event added to queue: " + json);

				// Add to store
				Dictionary<string, object> parameters = new Dictionary<string, object>();
				parameters.Add("$status", "new");
				parameters.Add("$category", ev["category"].Value);
				parameters.Add("$session_id", ev["session_id"].Value);
				parameters.Add("$client_ts", ev["client_ts"].Value);
				parameters.Add("$event", ev.SaveToBase64());
				string sql = "INSERT INTO ga_events (status, category, session_id, client_ts, event) VALUES($status, $category, $session_id, $client_ts, $event);";

				GAStore.ExecuteQuerySync(sql, parameters);

				// Add to session store if not last
				if (eventData["category"].AsString.Equals(CategorySessionEnd))
				{
					parameters.Clear();
					parameters.Add("$session_id", ev["session_id"].Value);
					sql = "DELETE FROM ga_session WHERE session_id = $session_id;";
					GAStore.ExecuteQuerySync(sql, parameters);
				}
				else
				{
					sql = "INSERT OR REPLACE INTO ga_session(session_id, timestamp, event) VALUES($session_id, $timestamp, $event);";
					parameters.Clear();
					parameters.Add("$session_id", ev["session_id"].Value);
					parameters.Add("$timestamp", GAState.SessionStart);
					parameters.Add("$event", jsonDefaults);
					GAStore.ExecuteQuerySync(sql, parameters);
				}
			}
			catch (Exception e)
			{
				GALogger.E("addEventToStoreWithEventData: error using json");
				GALogger.E(e.ToString());
			}
		}

		private static void AddDimensionsToEvent(JSONClass eventData)
		{
			if (eventData == null)
			{
				return;
			}
			// add to dict (if not nil)
			if (!string.IsNullOrEmpty(GAState.CurrentCustomDimension01))
			{
				eventData["custom_01"] = GAState.CurrentCustomDimension01;
			}
			if (!string.IsNullOrEmpty(GAState.CurrentCustomDimension02))
			{
				eventData["custom_02"] = GAState.CurrentCustomDimension02;
			}
			if (!string.IsNullOrEmpty(GAState.CurrentCustomDimension03))
			{
				eventData["custom_03"] = GAState.CurrentCustomDimension03;
			}
		}

		private static string ResourceFlowTypeToString(EGAResourceFlowType value)
		{
			switch(value)
			{
				case EGAResourceFlowType.Source:
					{
						return "Source";
					}

				case EGAResourceFlowType.Sink:
					{
						return "Sink";
					}

				default:
					{
						return "";
					}
			}
		}

		private static string ProgressionStatusToString(EGAProgressionStatus value)
		{
			switch(value)
			{
				case EGAProgressionStatus.Start:
					{
						return "Start";
					}

				case EGAProgressionStatus.Complete:
					{
						return "Complete";
					}

				case EGAProgressionStatus.Fail:
					{
						return "Fail";
					}

				default:
					{
						return "";
					}
			}
		}

        private static void UpdateSessionTime()
        {
            JSONClass ev = GAState.GetEventAnnotations();
            string jsonDefaults = ev.SaveToBase64();
            string sql = "INSERT OR REPLACE INTO ga_session(session_id, timestamp, event) VALUES($session_id, $timestamp, $event);";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("$session_id", ev["session_id"].Value);
            parameters.Add("$timestamp", GAState.SessionStart);
            parameters.Add("$event", jsonDefaults);
            GAStore.ExecuteQuerySync(sql, parameters);
        }

		private static string ErrorSeverityToString(EGAErrorSeverity value)
		{
			switch(value)
			{
				case EGAErrorSeverity.Debug:
					{
						return "debug";
					}

				case EGAErrorSeverity.Info:
					{
						return "info";
					}

				case EGAErrorSeverity.Warning:
					{
						return "warning";
					}

				case EGAErrorSeverity.Error:
					{
						return "error";
					}

				case EGAErrorSeverity.Critical:
					{
						return "critical";
					}

				default:
					{
						return "";
					}
			}
		}

#endregion // Private methods
	}
}

