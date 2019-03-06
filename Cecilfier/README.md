# Cecilfier

Managed (.NET) Jettifier (.NET Core 3 app).

## Run

### Options

Mono.Options with options runs:

```
Xamarin.AndroidX.Cecilfier.App \
    /exact-pairs:

```

References/Links:

*   

*   

### Tests of published app

Published app (.NET Core 3):

```bash
cd ./Cecilfier
samples/Xamarin.AndroidX.Cecilfier.App/bin/Debug/netcoreapp3.0/publish/Xamarin.AndroidX.Cecilfier.App
```

Running with mono:

```bash
mono samples/Xamarin.AndroidX.Cecilfier.App/bin/Debug/netcoreapp3.0/Xamarin.AndroidX.Cecilfier.App.dll 
```

## Publish

```bash
dotnet publish samples/Xamarin.AndroidX.Cecilfier.App/
```

RID (Runtime identifiers):

*   https://docs.microsoft.com/en-us/dotnet/core/rid-catalog#rid-graph

## Install

```bash
cp \
    samples/Xamarin.AndroidX.Cecilfier.App/bin/Debug/netcoreapp3.0/publish/Xamarin.AndroidX.Cecilfier.App* \
    ./bin/
```