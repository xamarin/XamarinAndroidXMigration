#/!bin/bash

# test-xamarin-androidx-cecilfier.sh

# sh ./build.sh 
# cd ./Cecilfier/
# dotnet publish ./samples/Xamarin.AndroidX.Cecilfier.App
# cp ./samples/Xamarin.AndroidX.Cecilfier.App/bin/Debug/netcoreapp3.0/publish/* ./bin/



./bin/Xamarin.AndroidX.Cecilfier.App \
    /exact-pairs \
        ../Demo/AarxerciseDemoApp/bin/Debug/Aarxercise.dll=../Demo/AarxerciseDemoApp/bin/Debug/Aarxercise.AAA.dll \
        ../Demo/AarxerciseDemoApp/bin/Debug/ManagedAarxercise.dll:../Demo/AarxerciseDemoApp/bin/Debug/ManagedAarxercise.AAA.dll \
    /bulk-with-pattern \
        c1:d1 \
        c2=d2 \

# BabySteps not building
#        ../Demo/BabySteps/bin/Debug/BabySteps.dll:../Demo/BabySteps/bin/Debug/BabySteps.AAAA.dll \


# mono \
#     ./samples/Xamarin.AndroidX.Cecilfier.App/bin/Debug/netcoreapp3.0/Xamarin.AndroidX.Cecilfier.App.dll \
#     /exact-pairs \
#         ../Demo/BabySteps/bin/Debug/BabySteps.dll:../Demo/BabySteps/bin/Debug/BabySteps.AAAA.dll \
#         ../Demo/AarxerciseDemoApp/bin/Debug/Aarxercise.dll=../Demo/AarxerciseDemoApp/bin/Debug/Aarxercise.AAA.dll \
#         ../Demo/AarxerciseDemoApp/bin/Debug/ManagedAarxercise.dll:../Demo/AarxerciseDemoApp/bin/Debug/ManagedAarxercise.AAA.dll \
#     /bulk-with-pattern \
#         c1:d1 \
#         c2=d2 \
