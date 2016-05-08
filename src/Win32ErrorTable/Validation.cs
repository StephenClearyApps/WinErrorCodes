using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Win32ErrorTable
{
    public static class Validation
    {
        private static void WriteLine(bool silent, string text)
        {
            if (!silent)
                Console.WriteLine(text);
        }

        public static bool CheckResults(Results results, bool silent)
        {
            bool result = true;

            // Ensure all HRESULT facility codes are unique.
            var facilities = new Dictionary<uint, Facility>();
            foreach (var facility in results.HResultFacilities)
            {
                if (facilities.ContainsKey(facility.Value))
                {
                    WriteLine(silent, "Duplicated HRESULT facility code " + facility.Value);
                    result = false;
                }
                else
                {
                    facilities.Add(facility.Value, facility);
                }
            }

            // Ensure all NTSTATUS facility codes are unique.
            var ntFacilities = new Dictionary<uint, Facility>();
            foreach (var facility in results.NtStatusFacilities)
            {
                if (ntFacilities.ContainsKey(facility.Value))
                {
                    WriteLine(silent, "Duplicated HRESULT facility code " + facility.Value);
                    result = false;
                }
                else
                {
                    ntFacilities.Add(facility.Value, facility);
                }
            }

            // Ensure all error codes are unique.
            var win32codes = new Dictionary<uint, ErrorMessage>();
            foreach (var win32 in results.Win32Errors)
            {
                if (win32codes.ContainsKey(win32.Code))
                {
                    WriteLine(silent, "Duplicated Win32 code " + win32.Code);
                    result = false;
                }
                else
                {
                    win32codes.Add(win32.Code, win32);
                }
            }

            var hresultCodes = new Dictionary<uint, ErrorMessage>();
            foreach (var hresult in results.HResultErrors)
            {
                if (hresultCodes.ContainsKey(hresult.Code))
                {
                    result = false;
                    WriteLine(silent, "Duplicated HRESULT code " + hresult.Code.ToString("X8"));
                }
                else
                {
                    hresultCodes.Add(hresult.Code, hresult);
                }
            }

            var ntstatusCodes = new Dictionary<uint, ErrorMessage>();
            foreach (var ntstatus in results.NtStatusErrors)
            {
                if (ntstatusCodes.ContainsKey(ntstatus.Code))
                {
                    result = false;
                    WriteLine(silent, "Duplicated NTSTATUS code " + ntstatus.Code.ToString("X8"));
                }
                else
                {
                    ntstatusCodes.Add(ntstatus.Code, ntstatus);
                }
            }

            // Display all shared codes.
            foreach (var facility in results.HResultFacilities.Where(x => x.Names.Count > 1))
                WriteLine(silent, "HRESULT Facility " + facility.Value + " shared between " + string.Join(" ", facility.Names));
            foreach (var facility in results.NtStatusFacilities.Where(x => x.Names.Count > 1))
                WriteLine(silent, "NTSTATUS Facility " + facility.Value + " shared between " + string.Join(" ", facility.Names));
            foreach (var win32 in results.Win32Errors.Where(x => x.Ids.Count > 1))
                WriteLine(silent, "Win32 " + win32.Code + " shared between " + string.Join(" ", win32.Ids));
            foreach (var hresult in results.HResultErrors.Where(x => x.Ids.Count > 1))
                WriteLine(silent, "HRESULT " + hresult.Code.ToString("X8") + " shared between " + string.Join(" ", hresult.Ids));
            foreach (var ntstatus in results.NtStatusErrors.Where(x => x.Ids.Count > 1))
                WriteLine(silent, "NTSTATUS " + ntstatus.Code.ToString("X8") + " shared between " + string.Join(" ", ntstatus.Ids));

            // Display all codes without text.
            foreach (var win32 in results.Win32Errors.Where(x => x.Text == ""))
                WriteLine(silent, "Win32 " + string.Join(" ", win32.Ids) + " (" + win32.Code + ") has no text.");
            foreach (var hresult in results.HResultErrors.Where(x => x.Text == ""))
            {
                if (results.HResultIsKnownWin32Error(hresult.Code))
                    continue;
                WriteLine(silent, "HRESULT " + string.Join(" ", hresult.Ids) + " (" + hresult.Code.ToString("X8") + ") has no text.");
            }
            foreach (var ntstatus in results.NtStatusErrors.Where(x => x.Text == ""))
                WriteLine(silent, "NTSTATUS " + string.Join(" ", ntstatus.Ids) + " (" + ntstatus.Code.ToString("X8") + ") has no text.");

            return result;
        }

        public static bool CheckResults(IList<ErrorMessage> results, bool silent)
        {
            bool result = true;

            // Ensure all error codes are unique.
            var codes = new Dictionary<uint, ErrorMessage>();
            foreach (var code in results)
            {
                if (codes.ContainsKey(code.Code))
                {
                    result = false;
                    WriteLine(silent, "Duplicated Resource code " + code.Code);
                }
                else
                {
                    codes.Add(code.Code, code);
                }
            }

            return result;
        }

        public static bool CrossCheckWin32Results(Results headerResults, IList<ErrorMessage> resourceResults, bool silent)
        {
            // Check that both sets have all codes.
            var headerSet = headerResults.Win32Errors.Concat(headerResults.HResultErrors).Select(x => x.Code).ToList();
            var resourceSet = resourceResults.Select(x => x.Code).ToList();
            foreach (var code in headerSet.Except(resourceSet))
            {
                if (headerResults.HResultIsKnownWin32Error(code))
                    continue;
                WriteLine(silent, "Extra Header value: " + code.ToString("X8"));
            }
            foreach (var code in resourceSet.Except(headerSet))
                WriteLine(silent, "Extra Resource value: " + code.ToString("X8"));

            // Check that for each code in both sets, the text is the same.
            foreach (var code in headerSet.Intersect(resourceSet))
            {
                var headerCode = headerResults.Win32Errors.Concat(headerResults.HResultErrors).First(x => x.Code == code);
                var resourceCode = resourceResults.First(x => x.Code == code);
                if (headerCode.Text != resourceCode.Text)
                {
                    WriteLine(silent, "Text mismatch:");
                    WriteLine(silent, headerCode.Text);
                    WriteLine(silent, resourceCode.Text);
                }
            }

            return true;
        }

        public static bool CrossCheckNtStatusResults(Results headerResults, IList<ErrorMessage> resourceResults, bool silent)
        {
            // Check that both sets have all codes.
            var headerSet = headerResults.NtStatusErrors.Select(x => x.Code).ToList();
            var resourceSet = resourceResults.Select(x => x.Code).ToList();
            foreach (var code in headerSet.Except(resourceSet))
                WriteLine(silent, "Extra Header value: " + code.ToString("X8"));
            foreach (var code in resourceSet.Except(headerSet))
                WriteLine(silent, "Extra Resource value: " + code.ToString("X8"));

            // Check that for each code in both sets, the text is the same.
            foreach (var code in headerSet.Intersect(resourceSet))
            {
                var headerCode = headerResults.NtStatusErrors.First(x => x.Code == code);
                var resourceCode = resourceResults.First(x => x.Code == code);
                if (headerCode.Text != resourceCode.Text)
                {
                    WriteLine(silent, "Text mismatch:");
                    WriteLine(silent, headerCode.Text);
                    WriteLine(silent, resourceCode.Text);
                }
            }

            return true;
        }
    }
}
