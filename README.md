# GA-SDK-C-SHARP
GameAnalytics Mono / .Net 4.5 and UWP SDK.

Mono / .Net 4.5
---------------

###Supported platforms:

* Windows
* Mac OS X
* Linux

###Requirements:

* Mono / .Net 4.5 or higher

###Dependencies:

* NLog

###How to install

####Using Nuget

Add **GameAnalytics.Mono.SDK** package from Nuget package manager. Nothing further needs to be done. Copying of files to the output directory happens automatically.

####Manual installation

Open **GA-SDK-MONO.sln** and compile the **GA_SDK_MONO** project. Add the **GameAnalytics.Mono.dll**, **System.Data.SQLite.dll** and **NLog.dll** as references to your project. Remember to copy **native/win32/sqlite3.dll** and **native/win64/sqlite3.dll** for when using on Windows platform to output directory, they should be copied to **x86/sqlite3.dll** and **x64/sqlite3.dll** under the output directory.

Universal Windows Platform (UWP)
--------------------------------

###Requirements:

* Windows 10 Universal SDK

###Dependencies:

* Microsoft.Data.Sqlite
* Microsoft.NetCore.UniversalWindowsPlatform
* MetroLog

###How to install

####Using Nuget

Add **GameAnalytics.UWP.SDK** package from Nuget package manager. Nothing further needs to be done. Copying of files to the output directory happens automatically.

####Manual installation

Open **GA-SDK-UWP.sln** and compile the **GA_SDK_UWP** project. Add the **GameAnalytics.UWP.dll** as references to your UWP project. Add the following Nuget packages: **Microsoft.Data.Sqlite**, **MetroLog** and **Microsoft.NetCore.UniversalWindowsPlatform**.


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

Usage of the SDK
----------------

Add this to the top of each class you use the GameAnalytics SDK in:

``` c-sharp
 using GameAnalyticsSDK.Net;
```

####Configuration

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

####Initialization

Example:

```c-sharp
GameAnalytics.Initialize("<your_game_key>", "<your_secret_key>");
```

####Send events

Example:

```c-sharp
GameAnalytics.AddDesignEvent("testEvent");
GameAnalytics.AddBusinessEvent("USD", 100, "boost", "super_boost", "shop");
GameAnalytics.AddResourceEvent(EGAResourceFlowType.Source, "gems", 10, "lives", "extra_life");
GameAnalytics.AddProgressionEvent(EGAProgressionStatus.Start, "progression01", "progression02");
```

Changelog
---------

**1.0.13**
* Initial version