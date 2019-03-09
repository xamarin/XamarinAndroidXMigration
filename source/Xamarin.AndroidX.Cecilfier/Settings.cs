using System;

namespace HolisticWare.Xamarin.Tools.Bindings.XamarinAndroid.AndroidX.Migraineator
{
    public class Settings
    {
        public static string WorkingDirectory
        {
            get
            {
                string wd = null;


                #if NETCOREAPP && NETCOREAPP2_1
                #if DEBUG
                wd = "./bin/Debug/netcoreapp2.1/";
                #elif RELEASE
                wd  = "./bin/Release/netcoreapp2.1/";
                #endif
                #elif NETCOREAPP && NETCOREAPP3_0
                #if DEBUG
                wd = "./bin/Debug/netcoreapp3.0/";
                #elif RELEASE
                wd = "./bin/Release/netcoreapp3.0/";
                #endif
                #else
                System.Reflection.Assembly a = System.Reflection.Assembly.GetExecutingAssembly();
                ReadOnlySpan<char> l = a.Location.AsSpan();                int i = l.LastIndexOf(System.IO.Path.DirectorySeparatorChar);
                wd = l.Slice(0, i).ToString();
                #endif

                return wd;
            }
        }
    }
}
