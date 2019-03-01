# Xamarin AndroidX Migration Workspace

A repository to collect code prototypes and efforts around implementing an Android Support to AndroidX migration story for Xamarin apps.


## Areas

- `Aar` - Contains a sample Android Studio project which builds an .aar to demonstrate a number of scenarios in which Android Support might be referenced in a third party API.
- `Cecilfier` - Code to rewrite .NET assemblies with binding glue which reference Android Support namespace types to instead reference their AndroidX counterparts.
- `Jetifier` - A wrapper for Google's jetifier library which rewrites .aar's including java byte code to map android support package names to androidx counterparts.
- `Mappings` - A set of tools and lists of mappings of Android Support to AndroidX including java package names, type names, and maven artifacts as well as .NET namespaces, type names, and nuget packages.

