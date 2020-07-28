using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Mighty.Dynamic.Tests.MySql
{
    public static class WhenDevart
    {
#if NETCOREAPP
        // just read it once or at worst a few times (no lock)
        private static string licenseKey;
#endif

        // Adding this license key this way is not working in .NET Core 1.0 (only) - Devart still behaves as if
        // it was unlicensed; I have not had time to debug why and I think it probably isn't very important any
        // more... therefore I am leaving Devart tests disabled in MightyTests.csproj for .NET Core 1.0 only.
        // Note also, the .NET Core 1.0 Devart tests definitely did pass originally using the time-limited
        // Devart trial license, and so presumably (especially since they're all still working in the other
        // .NET Core versions) would pass if whatever the .NET Core 1.0 Devart license problem is, was fixed.
        public static string AddLicenseKey(string connectionString, string providerName)
        {
#if NETCOREAPP
            const string devartProvider = "Devart.Data.MySql";
            // add license key if devart on .NET Core
            if (providerName == devartProvider)
            {
                const string filename = "devart.core.license.key";
                if (licenseKey == null)
                {
                    bool failed = false;
                    try
                    {
                        licenseKey = File.ReadAllText(filename);
                    }
                    catch (FileNotFoundException)
                    {
                        failed = true;
                    }
                    if (string.IsNullOrEmpty(licenseKey)) failed = true;
                    if (failed)
                    {
                        Assert.Fail($"{devartProvider} tests on .NET Core require a valid Devart activation key; if you have one, place the value in file {filename} in the root of the tests project, if not you can change _DISABLE_DEVART to DISABLE_DEVART for each .NET Core build in MightyTests.csproj to disable these tests.");
                    }
                }
                connectionString = $"{connectionString};LicenseKey={licenseKey}";
            }
#endif

                        // insert provider name in connection string
                        return string.Format(connectionString, providerName);
        }
    }
}
