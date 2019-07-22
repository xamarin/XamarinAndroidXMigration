# Source Directory

This directory contains all the source code to build the library and apps.

**Native Java**

> These artifacts need to be built using 
> `.\build.ps1 --target=NativeAssets`.

 - `com.xamarin.androidx.jetifierWrapper` - this is a more advanced wrapper for
   Google's Jetifier library.

**Managed**

> These assets are build as part of the MSBuild process.

 - `Xamarin.AndroidX.Migration` - this is the main library that jetifies and
   cecilfies the files.
 - `Xamarin.AndroidX.Migration.Tool` - this is a command-line utility to
   execute various jetification and cecilfication commands.
 - `Xamarin.AndroidX.Migration.BuildTasks` - this is a set of MSBuild tasks
   that are used to jetify and cecilfy files during a build.
