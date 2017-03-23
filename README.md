# GA-SDK-C-SHARP
GameAnalytics Mono / .Net 4.5, Universal Windows 8 and UWP SDK.

Documentation is in the [wiki](https://github.com/GameAnalytics/GA-SDK-C-SHARP/wiki).

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
