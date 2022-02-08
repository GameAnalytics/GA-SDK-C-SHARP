| Mono        | UWP           | WSA  |
| :-------------: |:-------------:| :-----:|
| [![NuGet](https://img.shields.io/nuget/v/GameAnalytics.Mono.SDK.svg)](https://www.nuget.org/packages/GameAnalytics.Mono.SDK) [![NuGet](https://img.shields.io/nuget/dt/GameAnalytics.Mono.SDK.svg?label=nuget%20downloads)](https://www.nuget.org/packages/GameAnalytics.Mono.SDK)      | [![NuGet](https://img.shields.io/nuget/v/GameAnalytics.UWP.SDK.svg)](https://www.nuget.org/packages/GameAnalytics.UWP.SDK) [![NuGet](https://img.shields.io/nuget/dt/GameAnalytics.UWP.SDK.svg?label=nuget%20downloads)](https://www.nuget.org/packages/GameAnalytics.UWP.SDK) | [![NuGet](https://img.shields.io/nuget/v/GameAnalytics.WSA.SDK.svg)](https://www.nuget.org/packages/GameAnalytics.WSA.SDK) [![NuGet](https://img.shields.io/nuget/dt/GameAnalytics.WSA.SDK.svg?label=nuget%20downloads)](https://www.nuget.org/packages/GameAnalytics.WSA.SDK) |

[![MIT license](http://img.shields.io/badge/license-MIT-brightgreen.svg)](http://opensource.org/licenses/MIT)

# GA-SDK-C-SHARP
GameAnalytics Mono / .Net 4.5, Universal Windows 8 and UWP SDK.

Documentation can be found [here](https://gameanalytics.com/docs/c-sharp-sdk).

> :information_source:<br>
>
> This repository is open-source and can be built to Mono / .Net 4.5, Universal Windows Platform (UWP) and Universal Windows 8.1 (Windows 8.1 and Windows Phone 8.1).    
>
> How to build: Click [here](How-to-build)    
>
> **Mono / .Net 4.5**:    
> Supported platforms: Windows, Mac OS X and Linux    
> Requirements:  Mono / .Net 4.5 or higher    
>
> **UWP**:    
> Requirements: Windows 10 Universal SDK    
>
> **Universal Windows 8.1**:    
> Requirements: Windows 8 or higher

Changelog
---------
<!--(CHANGELOG_TOP)-->
**3.3.5**
* changed event uuid field name

**3.3.4**
* added event uuid to events sent

**3.3.3**
* updated dependencies to fix errors for uwp

**3.3.2**
* added error events to be sent for invalid custom event fields used
* added optional mergeFields argument to event methods to merge with global custom fields instead of overwriting them

**3.3.1**
* fixed missing custom event fields for when trying to fix missing session end events

**3.3.0**
* added global custom event fields function to allow to add custom fields to events sent automatically by the SDK

**3.2.1**
* added functionality to force a new user in a/b testing without having to uninstall app first, simply use custom user id function to set a new user id which hasn’t been used yet

**3.2.0**
* added custom event fields feature

**3.1.2**
* updated client ts validator

**3.1.1**
* fixed null exception in some cases in gastore

**3.1.0**
* added support for .Net Core

**3.0.8**
* added session_num to init request

**3.0.7**
* removed facebook, gender and birthyear methods

**3.0.6**
* A/B testing fix

**3.0.5**
* A/B testing fixes

**3.0.4**
* remote configs fixes

**3.0.3**
* remote configs fix

**3.0.2**
* validator fix

**3.0.1**
* small bug fix for http requests

**3.0.0**
* Remote Config calls have been updated and the old calls have deprecated. Please see GA documentation for the new SDK calls and migration guide
* A/B testing support added

**2.1.7**
* session length fixes

**2.1.6**
* fixed logger crash bug

**2.1.5**
* thread bug fix

**2.1.4**
* session length fixes

**2.1.3**
* small correction for logging

**2.1.2**
* threading fixes

**2.1.1**
* fixes events to being sent

**2.1.0**
* added enable/disable event submission function

**2.0.6**
* removed manual session handling check for startsession and endsession for non-uwp and non-wsa builds

**2.0.5**
* fixed validation for business events

**2.0.4**
* fixed json deserializing bugs

**2.0.3**
* fixes to shutdown freeze

**2.0.2**
* updated dependencies for UWP library

**2.0.1**
* updated dependencies for UWP library

**2.0.0**
* added command center functionality
* fixed event json for computers set up to use commas instead of periods for decimal numbers

**1.1.12**
* small fix for uwp and wsa for db path

**1.1.11**
* added custom dimensions to design and error events
* fixed db path issue for unity version
* updated nlog for mono version

**1.1.10**
* fixed not allowing to add events when session has not started
* fixed session length bug

**1.1.9**
* small correction to use int instead of double for session num

**1.1.8**
* bug fix for end session when using manual session handling

**1.1.7**
* session length precision improvement

**1.1.6**
* minor improvements on background thread

**1.1.5**
* changed persistent path (uwp, wsa)

**1.1.3**
* possible to set custom dimensions and demographics before initialize

**1.1.2**
* Bug fix to design and progression events when not sending score/value
* Session length bug fix

**1.1.1**
* Fixes to validation error for Windows Universal 8.1 (Windows Phone 8 and Windows 8)

**1.1.0**
* Added support for Universal Windows 8 (Windows Phone 8 and Windows 8)

**1.0.13**
* Initial version

Folderstructure
---------------

* **3rd** - third party libraries
  * **Mono.Data.Sqlite/Unity** - specific compiled Mono.Data.Sqlite to use with Unity to look for sqlite native methods in **__Internal** dll
  * **Mono** - Unity 4 and 5 versions of System.Core.dll used for Unity builds
  * **System.Data.SQLite** - System.Data.SQLite compiled with UseInteropDll=false and UseSqliteStandard=true
  * **Unity** - Unity 4 and 5 libraries
* **GA-SDK-MONO-SHARED** - Shared code between projects
* **GA-SDK-MONO-UNITY-SHARED** - Shared code between Unity projects
* **GA-SDK-UNITY-MONO_4.x** - project to compile DLL for Unity 4
* **GA-SDK-UNITY-MONO_5.x** - project to compile DLL for Unity 5
* **GA_SDK_MONO** - project to compile DLL for Mono / .Net 4.5
* **GA_SDK_UWP** - project to compile DLL for UWP (requires Windows 10 and Windows 10 Universal SDK installed)
* **GA-SDK-WSA** - project to compile DLL for Universal Windows 8 (requires Windows 8 or higher)

Usage of the SDK
----------------

Add this to the top of each class you use the GameAnalytics SDK in:

``` c-sharp
 using GameAnalyticsSDK.Net;
```

#### Configuration

Example:

```c-sharp
GameAnalytics.SetEnabledInfoLog(true);
GameAnalytics.SetEnabledVerboseLog(true);

GameAnalytics.ConfigureBuild("0.10");

GameAnalytics.ConfigureAvailableResourceCurrencies("gems", "gold");
GameAnalytics.ConfigureAvailableResourceItemTypes("boost", "lives");

GameAnalytics.ConfigureAvailableCustomDimensions01("ninja", "samurai");
GameAnalytics.ConfigureAvailableCustomDimensions02("whale", "dolpin");
GameAnalytics.ConfigureAvailableCustomDimensions03("horde", "alliance");
```

#### Initialization

Example:

```c-sharp
GameAnalytics.Initialize("<your_game_key>", "<your_secret_key>");
```

#### Send events

Example:

```c-sharp
GameAnalytics.AddDesignEvent("testEvent");
GameAnalytics.AddBusinessEvent("USD", 100, "boost", "super_boost", "shop");
GameAnalytics.AddResourceEvent(EGAResourceFlowType.Source, "gems", 10, "lives", "extra_life");
GameAnalytics.AddProgressionEvent(EGAProgressionStatus.Start, "progression01", "progression02");
```
