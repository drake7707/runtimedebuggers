## Project Description
Allows you to inject a library in another process, to inspect all the loaded assemblies, view and edit fields, properties,dump resources, ...

--
**02 jan 2014**

There are a few bugs that crept in the release I made, mainly due to my struggle with the resource files and .NET 2 <-> .NET 4 (missing icons and such). The resource pane also seems to be borked and there's still a System.Diagnostics.Debugger.Break() without an System.Diagnostics.Debugger.IsAttached, so if you hit that one, it will generate an exception. I demoted the current release to beta, and will make a new version once the issues are resolved.
Update: the above issues are resolved in the source code
--

Note: if you don't have C++ installed then you can ignore the ManagedInjector project. I made 4 binaries (the 3.5 ones with vs2008, the 4.0 ones with vs2010) and added them to the Injector project as copy to output.

## Videos

**Overview**
{video:url=http://www.youtube.com/watch?v=4VPuk9RYmA8,type=youtube}

**IL Debugging**
{video:url=http://www.youtube.com/watch?v=MsroEJUHiFQ,type=youtube}

## Screenshots

**Inject into**
![](http://i.imgur.com/6VVOCrp.png)

**Assembly explorer**
![](http://i.imgur.com/Cj6esLd.png)
![](http://i.imgur.com/6z4Jssuh.png)
![](http://i.imgur.com/y5bOVSIh.png)

**Locals & watches**
![](http://i.imgur.com/d0KpREz.png)
![](http://i.imgur.com/dKsbrwj.png)

**IL Debugging (very big WIP)**
![](http://i.imgur.com/4uNQfR5h.png)

**Decrypt strings**
![](http://i.imgur.com/1qmoWQf.gif)

