﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Win32ErrorTable
{
    partial class NtStatusH
    {
        /// <summary>
        /// The known lines that are ignored in NtStatus.h. Take care changing this! Other constants may need to be changed.
        /// </summary>
        private static string[] IgnoredLines =
        {
            "++ BUILD Version: 0005    // Increment this if a change has global effects",
            "Copyright (c) Microsoft Corporation. All rights reserved.",
            "Module Name:",
            "ntstatus.h",
            "Abstract:",
            "Constant definitions for the NTSTATUS values.",
            "Author:",
            "Portable Systems Group 30-Mar-1989",
            "Revision History:",
            "Notes:",
            "This file is generated by the MC tool from the ntstatus.mc file.",
            "Please add new error values to the end of the file. To do otherwise",
            "will jumble the error values.",
            "Search for \"**** New SYSTEM error codes can be inserted here ****\" to",
            "find where to insert new error codes",
            "--",
            "#define _NTSTATUS_",
            "begin_ntsecapi",
            "lint -save -e767 */  // Don't complain about different definitions",
            "Please update FACILITY_MAXIMUM_VALUE when adding new facility values.",
            "(This value should be greater than the highest value above)",
            "Facility 0x17 is reserved and used in isolation lib as",
            "PIE=0x17:FACILITY_MANIFEST_ERROR_CODE",
            "Standard Success values",
            "The success status codes 0 - 63 are reserved for wait completion status.",
            "FacilityCodes 0x5 - 0xF have been allocated by various drivers.",
            "Values are 32 bit values laid out as follows:",
            "3 3 2 2 2 2 2 2 2 2 2 2 1 1 1 1 1 1 1 1 1 1",
            "1 0 9 8 7 6 5 4 3 2 1 0 9 8 7 6 5 4 3 2 1 0 9 8 7 6 5 4 3 2 1 0",
            "+---+-+-+-----------------------+-------------------------------+",
            "|Sev|C|R|     Facility          |               Code            |",
            "where",
            "Sev - is the severity code",
            "00 - Success",
            "01 - Informational",
            "10 - Warning",
            "11 - Error",
            "C - is the Customer code flag",
            "R - is a reserved bit",
            "Facility - is the facility code",
            "Code - is the facility's status code",
            "Define the facility codes",
            "#define FACILITY_MAXIMUM_VALUE           0xEB",
            "Define the severity codes",
            "#define STATUS_WAIT_0                           ((NTSTATUS)0x00000000L)",
            "The success status codes 128 - 191 are reserved for wait completion",
            "status with an abandoned mutant object.",
            "#define STATUS_ABANDONED                        ((NTSTATUS)0x00000080L)",
            "The success status codes 256, 257, 258, and 258 are reserved for",
            "User APC, Kernel APC, Alerted, and Timeout.",
            "Standard Information values",
            "Standard Warning values",
            "Note:  Do NOT use the value 0x80000000L, as this is a non-portable value",
            "for the NT_SUCCESS macro. Warning values start with a code of 1.",
            "Standard Error values",
            "Network specific errors.",
            "Status codes raised by the Cache Manager which must be considered as",
            "\"expected\" by its callers.",
            "============================================================",
            "NOTE: The following ABIOS error code should be reserved on",
            "non ABIOS kernel. Eventually, I will remove the ifdef",
            "ABIOS.",
            "Available range of NTSTATUS codes",
            "Directory Service specific Errors",
            "++",
            "MessageId's 0x030c - 0x031f (inclusive) are reserved for future **STORAGE",
            "copy protection errors.",
            "MessageId's 0x0323 - 0x034f (inclusive) are reserved for other future copy",
            "protection errors.",
            "MessageId up to 0x400 is reserved for smart cards",
            "MessageId 0x042E is reserved and used in isolation lib as",
            "MessageId=0x042E Facility=System Severity=ERROR SymbolicName=STATUS_VERSION_PARSE_ERROR", // TODO
            "Language=English",
            "A version number could not be parsed.",
            ".",
            "New SYSTEM error codes can be inserted here",
            "MessageId's 0xa010 - 0xa07f (inclusive) are reserved for TCPIP errors.",
            "MessageId's 0xa014 - 0xa07f (inclusive) are reserved for TCPIP errors.",
            "MessageId's 0xa100 - 0xa120 (inclusive) are for the SMB Hash Generation Service.",
            "MessageId's 0xa121 - 0xa140 (inclusive) are for GPIO (General Purpose I/O) controller related errors.",
            "MessageId's 0xa141 - 0xa160 (inclusive) are for run levels support.",
            "MessageId's 0xa200 - 0xa280 (inclusive) are reserved for app container specific messages.",
            "MessageId's 0xa281 - 0xa2a0 (inclusive) are reserved for Fast Cache specific messages.",
            "MessageId's 0xa2a1 - 0xa300 (inclusive) are for File System Filters Supported Features specific messages.",
            "Debugger error values",
            "RPC error values",
            "ACPI error values",
            "Terminal Server specific Errors",
            "IO error values",
            "MUI error values",
            "Filter Manager error values",
            "Translation macro for converting:",
            "HRESULT --> NTSTATUS",
            "#define FILTER_FLT_NTSTATUS_FROM_HRESULT(x) ((NTSTATUS) (((x) & 0xC0007FFF) | (FACILITY_FILTER_MANAGER << 16) | 0x40000000))",
            "Side-by-side (SXS) error values",
            "Cluster error values",
            "Transaction Manager error values",
            "CLFS (common log file system) error values",
            "XDDM Video Facility Error codes (videoprt.sys)",
            "Monitor Facility Error codes (monitor.sys)",
            "Graphics Facility Error codes (dxg.sys, dxgkrnl.sys)",
            "Common Windows Graphics Kernel Subsystem status codes {0x0000..0x00ff}",
            "Video Memory Manager (VidMM) specific status codes {0x0100..0x01ff}",
            "Video GPU Scheduler (VidSch) specific status codes {0x0200..0x02ff}",
            "Video Present Network Management (VidPNMgr) specific status codes {0x0300..0x03ff}",
            "Port specific status codes {0x0400..0x04ff}",
            "OPM, PVP and UAB status codes {0x0500..0x057F}",
            "Monitor Configuration API status codes {0x0580..0x05DF}",
            "OPM, UAB, PVP and DDC/CI shared status codes {0x25E0..0x25FF}",
            "Full Volume Encryption Error codes (fvevol.sys)",
            "FWP error codes (fwpkclnt.sys)",
            "NDIS error codes (ndis.sys)",
            "NDIS error codes (802.11 wireless LAN)",
            "NDIS informational codes(ndis.sys)",
            "NDIS Chimney Offload codes (ndis.sys)",
            "TPM hardware errors {0x0000..0x003ff}",
            "TPM vendor specific hardware errors {0x0400..0x04ff}",
            "TPM non-fatal hardware errors {0x0800..0x08ff}",
            "TPM software Error codes (tpm.sys)",
            "Platform Crypto Provider Error Codes (PCPTPM12.dll and future platform crypto providers)",
            "Remote TPM Error Codes",
            "Hypervisor error codes - changes to these codes must be reflected in HvStatus.h",
            "Virtualization status codes - these codes are used by the Virtualization Infrustructure Driver (VID) and other components",
            "of the virtualization stack.",
            "Errors:",
            "Warnings:",
            "IPSEC error codes (tcpip.sys)",
            "Volume manager status codes (volmgr.sys and volmgrx.sys)",
            "WARNINGS",
            "ERRORS",
            "Boot Code Data (BCD) status codes",
            "vhdparser error codes (vhdmp.sys)",
            "Vhd warnings.",
            "Resume Key Filter (RKF) error codes.",
            "RDBSS / MiniRdr internal error codes.",
            "Bluetooth Attribute Protocol Warnings",
            "Secure Boot error messages.",
            "System Integrity Policy error messages.",
            "Clip modern app and windows licensing error messages.",
            "Audio error messages.",
            "Spaceport success codes (spaceport.sys)",
            "Spaceport error codes (spaceport.sys)",
            "Volsnap status codes (volsnap.sys)",
            "Sdbus status codes (sdbus.sys)",
            "Shared VHDX status codes (svhdxflt.sys)",
            "SMB status codes",
            "Embedded Security Core",
            "Reserved id values 0x0001 - 0x00FF",
            "0x8xxx",
            "0x4xxx",
            "Virtual Secure Mode (VSM)",
            "Map a WIN32 error value into an NTSTATUS",
            "Note: This assumes that WIN32 errors fall in the range -32k to 32k.",
            "#define __NTSTATUS_FROM_WIN32(x) ((NTSTATUS)(x) <= 0 ? ((NTSTATUS)(x)) : ((NTSTATUS) (((x) & 0x0000FFFF) | (FACILITY_NTWIN32 << 16) | ERROR_SEVERITY_ERROR)))",
            "__inline NTSTATUS_FROM_WIN32(long x) { return x <= 0 ? (NTSTATUS)x : (NTSTATUS) (((x) & 0x0000FFFF) | (FACILITY_NTWIN32 << 16) | ERROR_SEVERITY_ERROR);}",
            "#define NTSTATUS_FROM_WIN32(x) __NTSTATUS_FROM_WIN32(x)",
            "lint -restore */  // Resume checking for different macro definitions",
            "end_ntsecapi",
        };
    }
}
