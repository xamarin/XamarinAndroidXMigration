Xamarin AndroidX Migration
==========================

[![Build Status][pub-img]][pub-ci] [![Build Status][img]][ci]


If you are looking for the source code to the _Xamarin AndroidX bindings_,
it is available in the branch: [AndroidX @ xamarin/AndroidSupportComponents][src]

The Migration package helps apps which are transitioning to AndroidX but haven't
yet updated their own source code to the new AndroidX API's, or still depend on
third party libraries which have not been updated to use the new AndroidX API's.

Building the Xamarin.AndroidX.Migration NuGet package
-----------------------------------------------------

The packages are built using Cake, which is very simple to use. All you need is
the .NET Core tool: [Cake.Tool](https://www.nuget.org/packages/cake.tool)

Installing the tool:

```
dotnet tool install --global Cake.Tool --version 0.34.1
```

Starting the build:

```
dotnet cake
```


Add the Xamarin.AndroidX.Migration NuGet package to your app
------------------------------------------------------------

You should add the `Xamarin.AndroidX.Migration` NuGet package to your Xamarin
Android application project and any Xamarin.Android library projects it
references.


Keep the Xamarin.Android.Support.* NuGet packages
-------------------------------------------------

You will need to keep your existing references to Xamarin.Android.Support.*
NuGet packages as any remaining usage of the old Android Support API's will
require this for compiling before the migration build step does its job.  The
Android Support libraries will not be bundled into your apk.


Add references to the correct AndroidX NuGet packages
-----------------------------------------------------

The migration package contains a validation step which helps ensure you have
references to all of the AndroidX NuGet packages that the Android Support
library NuGet packages you reference in your app map to.

You will see a build error generated telling you which packages are missing
which you need to add.


When can I remove the Migration package?
----------------------------------------

As soon as all of your own source code, layout files, proguard configurations,
and manifest files are converted over to using the new AndroidX API's, and all
of the libraries you depend on have done the same, you can safely remove the
AndroidX Migration NuGet package along with all of the old Android Support
library NuGet packages.


Current Limitations or known issues
-----------------------------------

 - Android Designer may become unusable after adding AndroidX Migration package

Migration Tips and Tricks
-------------------------

Steps for migrating larger codebase (applications):

1.  Migrate from `packages.config` to `<PackageReference />` 

    Visual Studio for Windows has option for migration.

2.  Update all packagest to latest versions and test application

    Manage NuGet packages for Solution in Visual Studio for Windows can consolidate all packages for
    solution.

3.  Change options for Android application (all configurations) [TODO]

    1.  `aapt2`

    2.  `d8`

    3.  `r8`

4.  Add `Xamarin.AndroidX.Migration` package to the Android application and rebuild.

5.  Fix issues reported by migration package (packages)

    In Visual Studio for Windows unload project and edit project file (`*.csproj`) by adding code snippet
    from migration package error.

    In Visual Studio for Mac unloading is not necessary - project files can be edited directly.

6. Save, load/reload and rebuild application

    This step will most likely fail with numerous errors due to source code not being migrated (namespaces,
    fully qualified typenames).

7.  Fix source code (build errors) from step 5.

    This is manual step where Android.Support namespaces and [fully qualified] typenames must be replaced 
    with AndroidX counterparts. Use tooltips available in both IDEs (Visual Studio for Windows and Mac).

    Do not forget migrating Android.Support packagenames in layout files (`*.axml`). Failing to do so will
    cause runtime errors (inflation of binary XML file).

8.  Fixing other issues [TODO]

    1.  add `Xamarin.AndroidX.AppCompat`

        Sometimes `Xamarin.AndroidX.AppCompat.Resources` is added, but `Xamarin.AndroidX.AppCompat` is skipped.

    2.  `failed linking. resource X is private`




[src]: https://github.com/xamarin/AndroidSupportComponents/tree/AndroidX
[pub-img]: https://dev.azure.com/xamarin/public/_apis/build/status/AndroidX%20Migration%20(Public)?branchName=master
[pub-ci]: https://dev.azure.com/xamarin/public/_build/latest?definitionId=36&branchName=master
[img]: https://dev.azure.com/devdiv/DevDiv/_apis/build/status/Xamarin/Components/AndroidX%20Migration?branchName=master
[ci]: https://dev.azure.com/devdiv/DevDiv/_build/latest?definitionId=11529&branchName=master
