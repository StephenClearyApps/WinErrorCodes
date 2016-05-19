using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Win32ErrorTable
{
    partial class NtStatusH
    {
        private readonly Dictionary<string, string> FacilityDescriptions = new Dictionary<string, string>
        {
            { "FACILITY_SXS_ERROR_CODE", "SxS (side-by-side)" },
            { "FACILITY_TRANSACTION", "Transaction manager" },
            { "FACILITY_COMMONLOG", "CLFS (common log file system)" },
            { "FACILITY_VIDEO", "XDDM video (videoprt.sys)" },
            { "FACILITY_MONITOR", "Monitor (monitor.sys)" },
            { "FACILITY_GRAPHICS_KERNEL", "Graphics (dxg.sys, dxgkrnl.sys)" },
            { "FACILITY_FVE_ERROR_CODE", "Full volume encryption (fvevol.sys)" },
            { "FACILITY_FWP_ERROR_CODE", "(fwpkclnt.sys)" },
            { "FACILITY_NDIS_ERROR_CODE", "(ndis.sys)" },
            { "FACILITY_TPM", "Trusted Platform Module (TPM) / Platform Crypto Provider (PCP) (PCPTPM12.dll and future platform crypto providers)" },
            { "FACILITY_IPSEC", "(tcpip.sys)" },
            { "FACILITY_VOLMGR", "Volume manager (volmgr.sys and volmgrx.sys)" },
            { "FACILITY_BCD_ERROR_CODE", "Boot Code Data (BCD)" },
            { "FACILITY_RDBSS", "RDBSS / MiniRdr" },
            { "FACILITY_BTH_ATT", "Bluetooth Attribute Protocol" },
            { "FACILITY_SYSTEM_INTEGRITY", "System integrity policy" },
            { "FACILITY_LICENSING", "CLiP modern app and windows licensing" },
            { "FACILITY_SPACES", "Spaceport (spaceport.sys)" },
            { "FACILITY_VOLSNAP", "(volsnap.sys)" },
            { "FACILITY_SDBUS", "(sdbus.sys)" },
            { "FACILITY_SHARED_VHDX", "(svhdxflt.sys)" },
            { "FACILITY_SECURITY_CORE", "Embedded security core" },
            { "FACILITY_VSM", "Virtual Secure Mode (VSM)" },
        };

        private string FacilityDescription(string facilityName)
        {
            string result;
            if (FacilityDescriptions.TryGetValue(facilityName, out result))
                return result;
            return "";
        }
        
        /// <summary>
        /// Seed known values into NtStatus.h results.
        /// </summary>
        private void Seed()
        {
            // Wait completion status codes.
            for (int i = 0; i < 64; ++i)
                ntstatus.Add(new ErrorMessage((uint)i, new List<string> { "STATUS_WAIT_0 + " + i }, "The object at index " + i + " has satisfied the wait."));

            // Wait abandoned status codes.
            for (int i = 128; i < 192; ++i)
                ntstatus.Add(new ErrorMessage((uint)i, new List<string> { "STATUS_ABANDONED + " + (i - 128) }, "The object at index " + i + " has been abandoned."));

            ntstatus.Add(new ErrorMessage(0xC000042E, new List<string> { "STATUS_VERSION_PARSE_ERROR" }, "A version number could not be parsed.")); // ntstatus.h:8964
        }

        private void PostSeed()
        {
            var facilityNames = facilities.SelectMany(x => x.Names);

            var system = new FacilityName("", "System");
            facilities.Add(new Facility(0, new List<FacilityName> { system }));
            system.Range.AddChildRange(new Range(0x030D, 0x0320, "Storage copy protection")); // ntstatus.h:8085
            system.Range.AddChildRange(new Range(0x0323, 0x0350, "Non-storage copy protection")); // ntstatus.h:8118
            system.Range.AddChildRange(new Range(0x0390, 0x0400, "Smart card")); // ntstatus.h:8582
            system.Range.AddChildRange(new Range(0xA010, 0xA080, "TCP/IP")); // ntstatus.h:10256,10306
            system.Range.AddChildRange(new Range(0xA100, 0xA121, "SMB hash generation service")); // ntstatus.h:10407
            system.Range.AddChildRange(new Range(0xA121, 0xA141, "GPIO (General Purpose I/O) controller")); // ntstatus.h:10430
            system.Range.AddChildRange(new Range(0xA141, 0xA161, "Run levels support")); // ntstatus.h:10498
            system.Range.AddChildRange(new Range(0xA200, 0xA281, "App container")); // ntstatus.h:10560
            system.Range.AddChildRange(new Range(0xA281, 0xA2A1, "Fast cache")); // ntstatus.h:10592
            system.Range.AddChildRange(new Range(0xA2A1, 0xA301, "File system filters supported features")); // ntstatus.h:10642
            
            var graphics = facilityNames.Single(x => x.Name == "FACILITY_GRAPHICS_KERNEL").Range;
            graphics.AddChildRange(new Range(0x0000, 0x0100, "Common windows graphics kernel subsystem"));
            graphics.AddChildRange(new Range(0x0100, 0x0200, "Video memory manager (VidMM)"));
            graphics.AddChildRange(new Range(0x0200, 0x0300, "Video GPU scheduler (VidSch)"));
            graphics.AddChildRange(new Range(0x0300, 0x0400, "Video present network management (VidPNMgr)"));
            graphics.AddChildRange(new Range(0x0400, 0x0500, "Port specific"));
            graphics.AddChildRange(new Range(0x0500, 0x0580, "OPM, PVP and UAB"));
            graphics.AddChildRange(new Range(0x0580, 0x05E0, "Monitor configuration API"));
            graphics.AddChildRange(new Range(0x25E0, 0x2600, "OPM, UAB, PVP and DDC/CI"));

            var tpm = facilityNames.Single(x => x.Name == "FACILITY_TPM").Range;
            tpm.AddChildRange(new Range(0x0000, 0x0400, "TPM hardware errors"));
            tpm.AddChildRange(new Range(0x0400, 0x0500, "TPM vendor specific hardware errors"));
            tpm.AddChildRange(new Range(0x0800, 0x0900, "TPM non-fatal hardware errors"));

            facilities.Add(new Facility(0x3A, new List<FacilityName> { new FacilityName("", "vhdparser (vhdparser.sys)") }));
        }
    }
}
