using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static System.Environment;

namespace Win32ErrorTable
{
    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            var dllPath = Path.Combine(GetFolderPath(SpecialFolder.SystemX86), "en-US");
            var results = new Results(null, null, null, null, null);

            Console.WriteLine();
            Console.WriteLine("Basic WinError.h/kernel32.dll messages, which include most Win32 errors and HRESULTS.");

            var win32 = new WinErrorH().Process();
            Validation.CheckResults(win32);

            var kernelMessages = new Resources(Path.Combine(dllPath, "kernel32.dll.mui")).Process();
            Validation.CheckResults(kernelMessages);

            Validation.CrossCheckWin32Results(win32, kernelMessages);
            results.MergeWith(win32);

            Console.WriteLine();
            Console.WriteLine("Basic ntstatus.h/ntdll.dll messages, which include most NTSTATUS values.");

            var ntstatus = new NtStatusH().Process();
            Validation.CheckResults(ntstatus);

            var ntdllMessages = new Resources(Path.Combine(dllPath, "ntdll.dll.mui")).Process();
            Validation.CheckResults(ntdllMessages);

            Validation.CrossCheckNtStatusResults(ntstatus, ntdllMessages);
            results.MergeWith(ntstatus);

            // TODO: other Win32/HRESULT values.

            Console.WriteLine();
            Console.WriteLine("Combined.");

            Validation.CheckResults(results);
            Export.Json(results);

            Console.WriteLine();
            Console.WriteLine("Win32: " + results.Win32Errors.Count);
            Console.WriteLine("HRESULT: " + results.HResultErrors.Count);
            Console.WriteLine("NTSTATUS: " + results.NtStatusErrors.Count);
            Console.Error.WriteLine("Done.");
            Console.ReadKey();
        }

#if NO
        Links:
        https://msdn.microsoft.com/en-us/library/windows/desktop/ff485842(v=vs.85).aspx
        https://msdn.microsoft.com/en-us/library/cc231198.aspx
        https://msdn.microsoft.com/en-us/library/cc231214.aspx#Appendix_A_5

        Do we want to handle mappings? https://support.microsoft.com/en-us/kb/113996
        
        Disqus: set page.identifier: https://help.disqus.com/customer/portal/articles/472098-javascript-configuration-variables

NTSTATUS: messages in ntdll.dll / ntstatus.h
 0x80000000, 0x40000000 - Severity
 0x20000000 - Customer bit
 0x10000000 - Must be 0. If 1, then this is an HRESULT wrapper for an NTSTATUS value.
 0x0FFF0000 - Facility (only known if customer bit is 0).
 0x0000FFFF - Code.
Win32 errors: 0x0000-0xFFFF, with messages in kernel32.dll / winerror.h
HRESULT: messages in winerror.h
 0x80000000 - Severity
 0x40000000 - Must be 0. Unless bit N is 1.
 0x20000000 - Customer bit.
 0x10000000 - N bit. If set, then this is an NTSTATUS value.
 0x08000000 - Reserved. Generally 0 except for a handful of exceptions.
 0x07FF0000 - Facility (only known if Customer bit is 0 and N bit is 0).
 0x0000FFFF - Code.

0x10000000 set -> NTSTATUS in HRESULT clothing. Otherwise:
- Attempt to interpret as Win32 (only if 0xFFFF0000 is clear).
- Attempt to interpret as HRESULT.
- Attempt to interpret as Win32.
- Attempt to interpret as NTSTATUS.

        https://en.wikibooks.org/wiki/X86_Disassembly/Windows_Executable_Files

        Query: attempt to match based on hex/dec code; if no match, attempt to match based on substring. If there's exactly one match, go directly to that page with a normalized id=<hex> url.
#endif

    }
}
