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
        private static bool Result = true;
        private static readonly string DllPath = Path.Combine(GetFolderPath(SpecialFolder.SystemX86), "en-US");

        [STAThread]
        static int Main(string[] args)
        {
            try
            {
                var results = new Results(null, null, null, null, null);

                results.MergeWith(Win32());
                results.MergeWith(NtStatus());

                // TODO: other Win32/HRESULT values.
                // TODO: MSI.
                // TODO: AdsErr.h, asferr.h, BitsMsg.h, CDOSysErr.h, CiError.h, dlnaerror.h, ehstormsg.h, Filterr.h, fltWinError.h, fsrmerr.h,
                //   functiondiscoveryerror.h, imapi2error.h, imapi2fserror.h, IntShCut.h, iscsierr.h, IssPer16.h, Mdmsg.h, Mferror.h, Mq.h, ndattrib.h,
                //   netevent.h, nserror.h, NtDsBMsg.h, ntiologc.h, oledberr.h, rtcerr.h, SCardErr.h, slerror.h, syncregistrationerrors.h, Tapi3Err.h,
                //   urlmon.h, vdserr.h, vfwmsgs.h, vsserror.h, wcmerrors.h, WdsCpMsg.h, wdsmcerr.h, WdsTptMgmtMsg.h, winbio_err.h, WindowsSearchErrors.h,
                //   wsbapperror.h, wsbonlineerror.h, wsmerror.h, wuerror.h

                Result = Result && Validation.CheckResults(results, false);
                Export.Json(results);

                Console.WriteLine();
                Console.WriteLine("Win32: " + results.Win32Errors.Count);
                Console.WriteLine("HRESULT: " + results.HResultErrors.Count);
                Console.WriteLine("NTSTATUS: " + results.NtStatusErrors.Count);
                return Result ? 0 : 1;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                return -1;
            }
        }

        static Results Win32()
        {
            var win32 = new WinErrorH().Process();
            Result = Result && Validation.CheckResults(win32, true);

            var kernelMessages = new Resources(Path.Combine(DllPath, "kernel32.dll.mui")).Process();
            Result = Result && Validation.CheckResults(kernelMessages, true);

            Result = Result && Validation.CrossCheckWin32Results(win32, kernelMessages, false);
            return win32;
        }

        static Results NtStatus()
        {
            var ntstatus = new NtStatusH().Process();
            Result = Result && Validation.CheckResults(ntstatus, true);

            var ntdllMessages = new Resources(Path.Combine(DllPath, "ntdll.dll.mui")).Process();
            Result = Result && Validation.CheckResults(ntdllMessages, true);

            Result = Result && Validation.CrossCheckNtStatusResults(ntstatus, ntdllMessages, false);

            NtStatusH.WellKnownValues(ntstatus);
            return ntstatus;
        }

#if NO
        Links:
        https://msdn.microsoft.com/en-us/library/windows/desktop/ff485842(v=vs.85).aspx
        https://msdn.microsoft.com/en-us/library/cc231198.aspx
        https://msdn.microsoft.com/en-us/library/cc231214.aspx#Appendix_A_5

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
