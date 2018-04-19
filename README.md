[logo]: https://raw.githubusercontent.com/Geeksltd/Zebble.NavBarTabs/master/Shared/NuGet/Icon.png "Zebble.NavBarTabs"


## Zebble.NavBarTabs

![logo]

A Zebble plugin to allows you to have a NavBar at the top, and tabs at the bottom.


[![NuGet](https://img.shields.io/nuget/v/Zebble.NavBarTabs.svg?label=NuGet)](https://www.nuget.org/packages/Zebble.NavBarTabs/)

> With this plugin you can make some tab pages with Icon and text for all of platforms in a Zebble application.

<br>


### Setup
* Available on NuGet: [https://www.nuget.org/packages/Zebble.NavBarTabs/](https://www.nuget.org/packages/Zebble.NavBarTabs/)
* Install in your platform client projects.
* Available for iOS, Android and UWP.
<br>


### Api Usage

Just like NavBarPage, the tabs component is shared between different pages that inherit from this page type. This has some benefits:

Only one version of the tab component is rendered natively.
Transition between pages is more smooth.
The tabs component stays fixed positioned and stays out of the transition animations.
#### Location of the tabs
By default the location of the Tabs component is at the bottom for Windows and iOS, and at the top for Android.

If you want to customise this behaviour you can set the boundaries of the following in your PreRender() code:

Tabs component
The page content container (BodyScroller)
```csharp
<z-Component z-type="Page1" z-base="Templates.Default" z-namespace="UI.Pages"
    Title="Page 1" data-TopMenu="MainMenu" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
    xsi:noNamespaceSchemaLocation="./../.zebble-schema.xml">

  <z-place inside="Body">

    <!--Contents-->

  </z-place>

</z-Component>
```

If you want to modify tabs, you can change the `MainTabs.zbl` file which is located under Templates folder of application UI shared project.
```xml
<z-Component z-type="MainTabs" z-base="Zebble.Tabs" z-namespace="UI.Modules"
    
    xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
    xsi:noNamespaceSchemaLocation="../../Views/.zebble-schema.xml">

  <Tabs.Tab z-of="Pages.Page1" Label.Text="Page 1" Icon.Path="Images/Icons/Contacts.png" />
  <Tabs.Tab z-of="Pages.Page2" Label.Text="Page 2" Icon.Path="Images/Icons/Types.png" />
  <Tabs.Tab z-of="Pages.Page3" Label.Text="Page 3" Icon.Path="Images/Icons/Video.png" />
  <Tabs.Tab z-of="Pages.Page4" Label.Text="Page 4" Icon.Path="Images/Icons/Contacts.png" />
  <Tabs.Tab z-of="Pages.Page5" Label.Text="Page 5" Icon.Path="Images/Icons/Settings.png" />

</z-Component>
```

### Properties
| Property     | Type         | Android | iOS | Windows |
| :----------- | :----------- | :------ | :-- | :------ |
| DataSource           | string          | x       | x   | x       |
| LazyLoadOffset | int | x | x | x |


### Events
| Event             | Type                                          | Android | iOS | Windows |
| :-----------      | :-----------                                  | :------ | :-- | :------ |
| Flashed            | AsyncEvent    | x       | x   | x       |
| Initializing            | AsyncEvent    | x       | x   | x       |
| LongPressed            | AsyncEvent    | x       | x   | x       |
| PanFinished            | AsyncEvent    | x       | x   | x       |
| Panning            | AsyncEvent   | x       | x   | x       |
| PreRendered            | AsyncEvent    | x       | x   | x       |
| Swiped            | AsyncEvent   | x       | x   | x       |
| Tapped            | AsyncEvent    | x       | x   | x       |

