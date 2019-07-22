# Source Directory

This directory contains all the source code to build the library and apps.

**Native Java**

> These artifacts need to be built using
> `.\build.ps1 --target=NativeAssets`.

 - `com.xamarin.androidx.jetifierWrapper` - this is a more advanced wrapper for
   Google's Jetifier library.

**Managed**

> These assets are build as part of the MSBuild process.

 - `Migration` - this is the main library that jetifies and
   cecilfies the files.
 - `Tool` - this is a command-line utility to
   execute various jetification and cecilfication commands.
 - `BuildTasks` - this is a set of MSBuild tasks
   that are used to jetify and cecilfy files during a build.
