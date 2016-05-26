using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Win32ErrorTable
{
    // TODO: Document all ranges.
    // TODO: Source info (filename with path, line number).
    // TODO: asn1code.h, lmerr.h, FabricCommon.idl, tcerror.h, WinSock.h, WinSock2.h, txdtc.h, issperr.h, mferror.mc, dlnaerror.mc, nserror.mc, neterror.mc
    public sealed class WinErrorH
    {
        private static ErrorMessage CreateMessage(uint code, string messageId, IEnumerable<string> messageTextLines)
        {
            return new ErrorMessage(code, new List<string> { messageId }, string.Join("\n", messageTextLines));
        }

        private const string Identifier = "[A-Z][A-Za-z_0-9]*";

        private static readonly Regex CommentRegex = new Regex(@"^(.*)\s// (dderror|winnt)$");
        private static readonly Regex CommentHexDefinitionRegex = new Regex(@"^" + Identifier + @"\s+0x[0-9A-Fa-f]{8}$");

        private static readonly Regex RangeRegex = new Regex(@"^#define (" + Identifier + @"_(E_FIRST|E_LAST|S_FIRST|S_LAST))\s+[0-9A-Fa-fxLHRESULT()]+$");

        private static readonly Regex FacilityRegex = new Regex(@"^#define (FACILITY_[A-Za-z_0-9]+)\s+(\d+)$");
        private static readonly Regex SeverityRegex = new Regex(@"^#define SEVERITY_([A-Za-z_0-9]+)\s+(\d+)$");

        private static readonly Regex MessageIdRegex = new Regex(@"^MessageId: (" + Identifier + @")$");

        private static readonly Regex DefineDecRegex = new Regex(@"^#define (" + Identifier + @")\s+(\d+)L?$");
        private static readonly Regex DefineHresultHexCastRegex = new Regex(@"^#define (" + Identifier + @")\s+\(\(HRESULT\)0x([0-9A-Fa-f]{8})L?\)$");
        private static readonly Regex DefineHresultDecCastRegex = new Regex(@"^#define (" + Identifier + @")\s+\(\(HRESULT\)([0-9]+)L?\)$");
        private static readonly Regex DefineHresultHexTypedefRegex = new Regex(@"^#define (" + Identifier + @")\s+_HRESULT_TYPEDEF_\(0x([0-9A-Fa-f]{8})L?\)$");
        private static readonly Regex DefineHresultHexNdisTypedefRegex = new Regex(@"^#define (" + Identifier + @")\s+_NDIS_ERROR_TYPEDEF_\(0x([0-9A-Fa-f]{8})L?\)$");
        private static readonly Regex DefineAliasRegex = new Regex(@"^#define (" + Identifier + @")\s+(" + Identifier + @")$");
        private static readonly Regex DefineHresultFromWin32SymbolRegex = new Regex(@"^#define (" + Identifier + @")\s+HRESULT_FROM_WIN32\((" + Identifier + @")\)$");

        private static readonly Regex SymbolicNameRegex = new Regex(@"^SymbolicName=(" + Identifier + @")");
        private static readonly Regex SymbolicNameMessageIdRegex = new Regex(@"^MessageId: 0x([0-9A-Fa-f]{8})L? \(No symbolic name defined\)$");

        private string messageId;
        private List<string> messageText;
        private bool processingSymbolicNames;
        private uint? symbolicNameCode;

        private readonly List<ErrorMessage> win32 = new List<ErrorMessage>();
        private readonly List<ErrorMessage> hresult = new List<ErrorMessage>();
        private readonly List<Facility> facilities = new List<Facility>();

        private static Version TryParseVersion(string s)
        {
            Version result;
            if (Version.TryParse(s, out result))
                return result;
            return null;
        }

        public Results Process()
        {
            // Seed some well-known HRESULT values.
            hresult.Add(CreateMessage(0, "S_OK", new[] { "Success." }));
            hresult[0].Ids.Add("NOERROR");
            hresult.Add(CreateMessage(1, "S_FALSE", new[] { "Some methods use S_FALSE to mean, roughly, a negative condition that is not a failure. It can also indicate a \"no-op\" - the method succeeded, but had no effect." }));

            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Windows Kits", "10", "Include");
            var selected = Directory.EnumerateDirectories(path).Select(dir => new { dir, ver = TryParseVersion(Path.GetFileName(dir)) }).OrderByDescending(x => x.ver).First();
            path = selected.dir;
            path = Path.Combine(path, "shared");
            var lines = File.ReadAllLines(Path.Combine(path, "winerror.h"));
            foreach (var line in lines.Select(x => x.Trim(' ', '\t', '/', '*')).Select(x =>
            {
                var match = CommentRegex.Match(x);
                return match.Success ? match.Groups[1].Value.Trim(' ', '\t') : x;
            }).Where(x => x != "" && !CommentHexDefinitionRegex.IsMatch(x)))
            {
                Match match;
                if (processingSymbolicNames)
                {
                    if (line == "End XACT_DTC_CONSTANTS enumerated values defined in txdtc.h")
                    {
                        processingSymbolicNames = false;
                        hresult.Add(CreateMessage(symbolicNameCode.Value, messageId, messageText));
                        messageId = null;
                        symbolicNameCode = null;
                        messageText = null;
                        continue;
                    }

                    if (messageId == null)
                    {
                        // SymbolicName parsing uses "SymbolicName=" instead of MessageId.
                        match = SymbolicNameRegex.Match(line);
                        if (match.Success)
                        {
                            messageId = match.Groups[1].Value;
                            continue;
                        }
                    }
                    else if (symbolicNameCode == null)
                    {
                        // SymbolicName parsing uses "MessageId:" as the code.
                        match = SymbolicNameMessageIdRegex.Match(line);
                        if (match.Success)
                        {
                            symbolicNameCode = uint.Parse(match.Groups[1].Value, NumberStyles.AllowHexSpecifier);
                            continue;
                        }
                    }
                    else if (messageText == null)
                    {
                        if (line == "MessageText:")
                        {
                            messageText = new List<string>();
                            continue;
                        }
                    }
                    else
                    {
                        match = SymbolicNameRegex.Match(line);
                        if (match.Success)
                        {
                            hresult.Add(CreateMessage(symbolicNameCode.Value, messageId, messageText));
                            messageId = match.Groups[1].Value;
                            symbolicNameCode = null;
                            messageText = null;
                            continue;
                        }

                        messageText.Add(line);
                        continue;
                    }
                }

                match = RangeRegex.Match(line);
                if (match.Success)
                {
                    if (RangeLines.Contains(line))
                        continue;
                    Console.WriteLine("Unrecognized range:");
                    Console.WriteLine("\"" + line + "\",");
                    System.Windows.Forms.Clipboard.SetText("\"" + line + "\",");
                    Console.ReadKey();
                    continue;
                }

                if (messageId == null)
                {
                    if (line.StartsWith("#if") || line.StartsWith("#else") || line.StartsWith("#endif") || line.StartsWith("#undef") || line.StartsWith("#pragma"))
                        continue;
                    if (IgnoredLines.Contains(line))
                        continue;

                    match = FacilityRegex.Match(line);
                    if (match.Success)
                    {
                        var value = uint.Parse(match.Groups[2].Value);
                        var existing = facilities.FirstOrDefault(x => x.Value == value);
                        if (existing == null)
                            facilities.Add(new Facility(value, new List<FacilityName> { new FacilityName(match.Groups[1].Value, "") }));
                        else
                            existing.Names.Add(new FacilityName(match.Groups[1].Value, ""));
                        continue;
                    }

                    match = SeverityRegex.Match(line);
                    if (match.Success)
                        continue;

                    match = MessageIdRegex.Match(line);
                    if (match.Success)
                    {
                        messageId = match.Groups[1].Value;
                        continue;
                    }
                }
                else if (messageText == null)
                {
                    if (line == "MessageText:")
                    {
                        messageText = new List<string>();
                        continue;
                    }
                }
                else
                {
                    if (line.StartsWith("#define " + messageId))
                    {
                        match = DefineDecRegex.Match(line);
                        if (match.Success)
                        {
                            //Console.WriteLine(messageId + " = " + match.Groups[2].Value);
                            win32.Add(CreateMessage(uint.Parse(match.Groups[2].Value), messageId, messageText));
                            messageId = null;
                            messageText = null;
                            continue;
                        }

                        match = DefineHresultHexTypedefRegex.Match(line);
                        if (match.Success)
                        {
                            //Console.WriteLine(messageId + " = " + match.Groups[2].Value);
                            hresult.Add(CreateMessage(uint.Parse(match.Groups[2].Value, NumberStyles.AllowHexSpecifier), messageId, messageText));
                            messageId = null;
                            messageText = null;
                            continue;
                        }

                        match = DefineHresultHexNdisTypedefRegex.Match(line);
                        if (match.Success)
                        {
                            //Console.WriteLine(messageId + " = " + match.Groups[2].Value);
                            hresult.Add(CreateMessage(uint.Parse(match.Groups[2].Value, NumberStyles.AllowHexSpecifier), messageId, messageText));
                            messageId = null;
                            messageText = null;
                            continue;
                        }
                    }
                    else
                    {
                        messageText.Add(line);
                        continue;
                    }
                }

                match = DefineDecRegex.Match(line);
                if (match.Success)
                {
                    var code = uint.Parse(match.Groups[2].Value);
                    var existing = win32.FirstOrDefault(x => x.Code == code);
                    if (existing != null)
                    {
                        //Console.WriteLine(match.Groups[1].Value + " = " + existing.MessageIds.First() + " = " + code);
                        existing.Ids.Add(match.Groups[1].Value);
                        continue;
                    }
                }

                match = DefineHresultHexCastRegex.Match(line);
                if (match.Success)
                {
                    var code = uint.Parse(match.Groups[2].Value, NumberStyles.AllowHexSpecifier);
                    var existing = hresult.FirstOrDefault(x => x.Code == code);
                    if (existing != null)
                    {
                        //Console.WriteLine(match.Groups[1].Value + " = " + existing.MessageIds.First() + " = " + code);
                        existing.Ids.Add(match.Groups[1].Value);
                        continue;
                    }
                    else
                    {
                        hresult.Add(CreateMessage(code, match.Groups[1].Value, Enumerable.Empty<string>()));
                        continue;
                    }
                }

                match = DefineHresultDecCastRegex.Match(line);
                if (match.Success)
                {
                    var code = uint.Parse(match.Groups[2].Value);
                    var existing = hresult.FirstOrDefault(x => x.Code == code);
                    if (existing != null)
                    {
                        //Console.WriteLine(match.Groups[1].Value + " = " + existing.MessageIds.First() + " = " + code);
                        var name = match.Groups[1].Value;
                        if (name != "S_OK" && name != "S_FALSE") // These are well-known, seeded values.
                            existing.Ids.Add(name);
                        continue;
                    }
                    else
                    {
                        hresult.Add(CreateMessage(code, match.Groups[1].Value, Enumerable.Empty<string>()));
                        continue;
                    }
                }

                match = DefineAliasRegex.Match(line);
                if (match.Success)
                {
                    var symbol = match.Groups[2].Value;
                    var existing = win32.Concat(hresult).FirstOrDefault(x => x.Ids.Contains(symbol));
                    if (existing != null)
                    {
                        //Console.WriteLine(match.Groups[1].Value + " = " + symbol);
                        if (match.Groups[1].Value != "DNS_ERROR_RCODE_LAST")
                            existing.Ids.Add(match.Groups[1].Value);
                        continue;
                    }
                }

                match = DefineHresultFromWin32SymbolRegex.Match(line);
                if (match.Success)
                {
                    var symbol = match.Groups[2].Value;
                    var existing = win32.FirstOrDefault(x => x.Ids.Contains(symbol));
                    if (existing != null)
                    {
                        //Console.WriteLine(match.Groups[1].Value + " = HRESULT wrapper for " + match.Groups[2].Value);
                        hresult.Add(CreateMessage(0x80070000 | (existing.Code & 0xFFFF), match.Groups[1].Value, Enumerable.Empty<string>()));
                        continue;
                    }
                }

                if (line == "Begin XACT_DTC_CONSTANTS enumerated values defined in txdtc.h")
                {
                    processingSymbolicNames = true;
                    continue;
                }

                Console.WriteLine("Unrecognized:");
                Console.WriteLine("\"" + line + "\",");
                System.Windows.Forms.Clipboard.SetText("\"" + line + "\",");
                Console.ReadKey();
                messageId = null;
                messageText = null;
            }

            return new Results(win32, hresult, facilities, null, null);
        }

        private static string[] RangeLines =
        {
            "#define OLE_E_FIRST ((HRESULT)0x80040000L)",
            "#define OLE_E_LAST  ((HRESULT)0x800400FFL)",
            "#define OLE_S_FIRST ((HRESULT)0x00040000L)",
            "#define OLE_S_LAST  ((HRESULT)0x000400FFL)",
            "#define DRAGDROP_E_FIRST 0x80040100L",
            "#define DRAGDROP_E_LAST  0x8004010FL",
            "#define DRAGDROP_S_FIRST 0x00040100L",
            "#define DRAGDROP_S_LAST  0x0004010FL",
            "#define CLASSFACTORY_E_FIRST  0x80040110L",
            "#define CLASSFACTORY_E_LAST   0x8004011FL",
            "#define CLASSFACTORY_S_FIRST  0x00040110L",
            "#define CLASSFACTORY_S_LAST   0x0004011FL",
            "#define MARSHAL_E_FIRST  0x80040120L",
            "#define MARSHAL_E_LAST   0x8004012FL",
            "#define MARSHAL_S_FIRST  0x00040120L",
            "#define MARSHAL_S_LAST   0x0004012FL",
            "#define DATA_E_FIRST     0x80040130L",
            "#define DATA_E_LAST      0x8004013FL",
            "#define DATA_S_FIRST     0x00040130L",
            "#define DATA_S_LAST      0x0004013FL",
            "#define VIEW_E_FIRST     0x80040140L",
            "#define VIEW_E_LAST      0x8004014FL",
            "#define VIEW_S_FIRST     0x00040140L",
            "#define VIEW_S_LAST      0x0004014FL",
            "#define REGDB_E_FIRST     0x80040150L",
            "#define REGDB_E_LAST      0x8004015FL",
            "#define REGDB_S_FIRST     0x00040150L",
            "#define REGDB_S_LAST      0x0004015FL",
            "#define CAT_E_FIRST     0x80040160L",
            "#define CAT_E_LAST      0x80040161L",
            "#define CS_E_FIRST     0x80040164L",
            "#define CS_E_LAST      0x8004016FL",
            "#define CACHE_E_FIRST     0x80040170L",
            "#define CACHE_E_LAST      0x8004017FL",
            "#define CACHE_S_FIRST     0x00040170L",
            "#define CACHE_S_LAST      0x0004017FL",
            "#define OLEOBJ_E_FIRST     0x80040180L",
            "#define OLEOBJ_E_LAST      0x8004018FL",
            "#define OLEOBJ_S_FIRST     0x00040180L",
            "#define OLEOBJ_S_LAST      0x0004018FL",
            "#define CLIENTSITE_E_FIRST     0x80040190L",
            "#define CLIENTSITE_E_LAST      0x8004019FL",
            "#define CLIENTSITE_S_FIRST     0x00040190L",
            "#define CLIENTSITE_S_LAST      0x0004019FL",
            "#define INPLACE_E_FIRST     0x800401A0L",
            "#define INPLACE_E_LAST      0x800401AFL",
            "#define INPLACE_S_FIRST     0x000401A0L",
            "#define INPLACE_S_LAST      0x000401AFL",
            "#define ENUM_E_FIRST        0x800401B0L",
            "#define ENUM_E_LAST         0x800401BFL",
            "#define ENUM_S_FIRST        0x000401B0L",
            "#define ENUM_S_LAST         0x000401BFL",
            "#define CONVERT10_E_FIRST        0x800401C0L",
            "#define CONVERT10_E_LAST         0x800401CFL",
            "#define CONVERT10_S_FIRST        0x000401C0L",
            "#define CONVERT10_S_LAST         0x000401CFL",
            "#define CLIPBRD_E_FIRST        0x800401D0L",
            "#define CLIPBRD_E_LAST         0x800401DFL",
            "#define CLIPBRD_S_FIRST        0x000401D0L",
            "#define CLIPBRD_S_LAST         0x000401DFL",
            "#define MK_E_FIRST        0x800401E0L",
            "#define MK_E_LAST         0x800401EFL",
            "#define MK_S_FIRST        0x000401E0L",
            "#define MK_S_LAST         0x000401EFL",
            "#define CO_E_FIRST        0x800401F0L",
            "#define CO_E_LAST         0x800401FFL",
            "#define CO_S_FIRST        0x000401F0L",
            "#define CO_S_LAST         0x000401FFL",
            "#define EVENT_E_FIRST        0x80040200L",
            "#define EVENT_E_LAST         0x8004021FL",
            "#define EVENT_S_FIRST        0x00040200L",
            "#define EVENT_S_LAST         0x0004021FL",
            "#define XACT_E_FIRST   0x8004D000",
            "#define XACT_E_LAST    0x8004D02B",
            "#define XACT_S_FIRST   0x0004D000",
            "#define XACT_S_LAST    0x0004D010",
            "#define CONTEXT_E_FIRST        0x8004E000L",
            "#define CONTEXT_E_LAST         0x8004E02FL",
            "#define CONTEXT_S_FIRST        0x0004E000L",
            "#define CONTEXT_S_LAST         0x0004E02FL",
        };

        private static string[] IgnoredLines =
        {
            "winerror.h --  error code definitions for the Win32 API functions",
            "Copyright (c) Microsoft Corp. All rights reserved.",
            "#define _WINERROR_",
            "#define FORCEINLINE __forceinline",
            "#define FORCEINLINE __inline",
            "#include <specstrings.h>",
            "Note: There is a slightly modified layout for HRESULT values below,",
            "after the heading \"COM Error Codes\".",
            "Search for \"**** Available SYSTEM error codes ****\" to find where to",
            "insert new error codes",
            "Values are 32 bit values laid out as follows:",
            "3 3 2 2 2 2 2 2 2 2 2 2 1 1 1 1 1 1 1 1 1 1",
            "1 0 9 8 7 6 5 4 3 2 1 0 9 8 7 6 5 4 3 2 1 0 9 8 7 6 5 4 3 2 1 0",
            "+-+-+-+-+-+---------------------+-------------------------------+",
            "|S|R|C|N|r|    Facility         |               Code            |",
            "+-+-+-+-+-+---------------------+-------------------------------+",
            "where",
            "S - Severity - indicates success/fail",
            "0 - Success",
            "1 - Fail (COERROR)",
            "R - reserved portion of the facility code, corresponds to NT's",
            "second severity bit.",
            "C - reserved portion of the facility code, corresponds to NT's",
            "C field.",
            "N - reserved portion of the facility code. Used to indicate a",
            "mapped NT status value.",
            "r - reserved portion of the facility code. Reserved for internal",
            "use. Used to indicate HRESULT values that are not status",
            "values, but are instead message ids for display strings.",
            "Facility - is the facility code",
            "Code - is the facility's status code",
            "Define the facility codes",
            "Define the severity codes",
            "Available SYSTEM error codes",
            "Capability Authorization Error codes",
            "0450 to 0460",
            "Do not use ID's 1266 - 1270 as the symbolicNames have been moved to SEC_E_",
            "SECURITY Error codes",
            "1299 to 1399",
            "WinUser Error codes",
            "1400 to 1499",
            "EventLog Error codes",
            "1500 to 1549",
            "Class Scheduler Error codes",
            "1550 to 1599",
            "MSI Error codes",
            "1600 to 1699",
            "RPC Error codes",
            "1700 to 1999",
            "OpenGL Error codes",
            "2000 to 2009",
            "Image Color Management Error codes",
            "2010 to 2049",
            "Winnet32 Error codes",
            "2100 to 2999",
            "The range 2100 through 2999 is reserved for",
            "network status codes. See lmerr.h for a",
            "complete listing",
            "Win32 Spooler Error codes",
            "3000 to 3049",
            "CopyFile ext. Error codes",
            "3050 to 3059",
            "Available",
            "3060 to 3199",
            "the message range",
            "3200 to 3299",
            "is reserved and used in isolation lib",
            "3300 to 3899",
            "IO Error Codes",
            "3900 to 3999",
            "Wins Error codes",
            "4000 to 4049",
            "PeerDist Error codes",
            "4050 to 4099",
            "DHCP Error codes",
            "4100 to 4149",
            "4150 to 4199",
            "WMI Error codes",
            "4200 to 4249",
            "app container Specific Error Codes",
            "4250 to 4299",
            "RSM (Media Services) Error codes",
            "4300 to 4349",
            "Remote Storage Service Error codes",
            "4350 to 4389",
            "Reparse Point Error codes",
            "4390 to 4399",
            "Fast Cache Specific Error Codes",
            "4400 to 4419",
            "SecureBoot Error codes",
            "4420 to 4439",
            "File System Supported Features Error Codes",
            "4440 to 4499",
            "Single Instance Store (SIS) Error codes",
            "4500 to 4549",
            "System Integrity Error codes",
            "4550 to 4559",
            "VSM Error codes",
            "4560 to 4569",
            "4570 to 4599",
            "Cluster Error codes",
            "5000 to 5999",
            "Codes from 4300 through 5889 overlap with codes in ds\\published\\inc\\apperr2.w.",
            "Do not add any more error codes in that range.",
            "EFS Error codes",
            "6000 to 6099",
            "BROWSER Error codes",
            "6100 to 6199",
            "This message number is for historical purposes and cannot be changed or re-used.",
            "Task Scheduler Error codes",
            "NET START must understand",
            "6200 to 6249",
            "6250 to 6599",
            "Common Log (CLFS) Error codes",
            "6600 to 6699",
            "Transaction (KTM) Error codes",
            "6700 to 6799",
            "Transactional File Services (TxF)",
            "Error codes",
            "6800 to 6899",
            "6900 to 6999",
            "Terminal Server Error codes",
            "7000 to 7099",
            "Windows Fabric Error Codes",
            "7100 to 7499",
            "defined in FabricCommon.idl",
            "Traffic Control Error Codes",
            "7500 to 7999",
            "defined in: tcerror.h",
            "Active Directory Error codes",
            "8000 to 8999",
            "FACILITY_FILE_REPLICATION_SERVICE",
            "FACILITY DIRECTORY SERVICE",
            "8223 unused",
            "8319 unused",
            "End of Active Directory Error Codes",
            "8000 to  8999",
            "DNS Error codes",
            "9000 to 9999",
            "=============================",
            "Facility DNS Error Messages",
            "DNS response codes.",
            "#define DNS_ERROR_RESPONSE_CODES_BASE 9000",
            "#define DNS_ERROR_MASK 0x00002328 // 9000 or DNS_ERROR_RESPONSE_CODES_BASE",
            "DNSSEC errors",
            "#define DNS_ERROR_DNSSEC_BASE 9100",
            "Packet format",
            "#define DNS_ERROR_PACKET_FMT_BASE 9500",
            "General API errors",
            "#define DNS_ERROR_GENERAL_API_BASE 9550",
            "Zone errors",
            "#define DNS_ERROR_ZONE_BASE 9600",
            "Datafile errors",
            "#define DNS_ERROR_DATAFILE_BASE 9650",
            "Database errors",
            "#define DNS_ERROR_DATABASE_BASE 9700",
            "Operation errors",
            "#define DNS_ERROR_OPERATION_BASE 9750",
            "Secure update",
            "#define DNS_ERROR_SECURE_BASE 9800",
            "Setup errors",
            "#define DNS_ERROR_SETUP_BASE 9850",
            "Directory partition (DP) errors",
            "#define DNS_ERROR_DP_BASE 9900",
            "DNS RRL errors from 9911 to 9920",
            "DNS ZoneScope errors from 9951 to 9970",
            "DNS Policy errors from 9971 to 9999",
            "End of DNS Error Codes",
            "WinSock Error Codes",
            "10000 to 11999",
            "WinSock error codes are also defined in WinSock.h",
            "and WinSock2.h, hence the IFDEF",
            "#define WSABASEERR 10000",
            "End of WinSock Error Codes",
            "12000 to 12999",
            "Start of IPSec Error codes",
            "13000 to 13999",
            "These must stay as a unit.",
            "Do NOT change this final value.  It is used in a public API structure",
            "Extended upper bound for IKE errors to accomodate new errors",
            "Following error codes are returned by IPsec kernel.",
            "End of IPSec Error codes",
            "Start of Side By Side Error Codes",
            "14000 to 14999",
            "End of Side By Side Error Codes",
            "Start of WinEvt Error codes",
            "15000 to 15079",
            "Start of Wecsvc Error codes",
            "15080 to 15099",
            "Start of MUI Error codes",
            "15100 to 15199",
            "Start of Monitor Configuration API error codes",
            "15200 to 15249",
            "End of Monitor Configuration API error codes",
            "Start of Syspart error codes",
            "15250 - 15299",
            "Start of Vortex error codes",
            "15300 - 15320",
            "Start of GPIO error codes",
            "15321 - 15340",
            "Start of Run Level error codes",
            "15400 - 15500",
            "Start of Com Task error codes",
            "15501 - 15510",
            "APPX Caller Visible Error Codes",
            "15600-15699",
            "AppModel Error Codes",
            "15700-15720",
            "Appx StateManager Codes",
            "15800-15840",
            "Application Partition Codes",
            "15841-15860",
            "Windows Store Codes",
            "15861-15880",

            "COM Error Codes",
            "The return value of COM functions and methods is an HRESULT.",
            "This is not a handle to anything, but is merely a 32-bit value",
            "with several fields encoded in the value. The parts of an",
            "HRESULT are shown below.",
            "Many of the macros and functions below were orginally defined to",
            "operate on SCODEs. SCODEs are no longer used. The macros are",
            "still present for compatibility and easy porting of Win16 code.",
            "Newly written code should use the HRESULT macros and functions.",
            "HRESULTs are 32 bit values layed out as follows:",
            "Severity values",
            "Generic test for success on any status value (non-negative numbers",
            "indicate success).",
            "#define SUCCEEDED(hr) (((HRESULT)(hr)) >= 0)",
            "and the inverse",
            "#define FAILED(hr) (((HRESULT)(hr)) < 0)",
            "Generic test for error on any status value.",
            "#define IS_ERROR(Status) (((unsigned long)(Status)) >> 31 == SEVERITY_ERROR)",
            "Return the code",
            "#define HRESULT_CODE(hr)    ((hr) & 0xFFFF)",
            "#define SCODE_CODE(sc)      ((sc) & 0xFFFF)",
            "Return the facility",
            "#define HRESULT_FACILITY(hr)  (((hr) >> 16) & 0x1fff)",
            "#define SCODE_FACILITY(sc)    (((sc) >> 16) & 0x1fff)",
            "Return the severity",
            "#define HRESULT_SEVERITY(hr)  (((hr) >> 31) & 0x1)",
            "#define SCODE_SEVERITY(sc)    (((sc) >> 31) & 0x1)",
            "Create an HRESULT value from component pieces",
            "#define MAKE_HRESULT(sev,fac,code) \\",
            "((HRESULT) (((unsigned long)(sev)<<31) | ((unsigned long)(fac)<<16) | ((unsigned long)(code))) )",
            "#define MAKE_SCODE(sev,fac,code) \\",
            "((SCODE) (((unsigned long)(sev)<<31) | ((unsigned long)(fac)<<16) | ((unsigned long)(code))) )",
            "Map a WIN32 error value into a HRESULT",
            "Note: This assumes that WIN32 errors fall in the range -32k to 32k.",
            "Define bits here so macros are guaranteed to work",
            "#define FACILITY_NT_BIT                 0x10000000",
            "HRESULT_FROM_WIN32(x) used to be a macro, however we now run it as an inline function",
            "to prevent double evaluation of 'x'. If you still need the macro, you can use __HRESULT_FROM_WIN32(x)",
            "#define __HRESULT_FROM_WIN32(x) ((HRESULT)(x) <= 0 ? ((HRESULT)(x)) : ((HRESULT) (((x) & 0x0000FFFF) | (FACILITY_WIN32 << 16) | 0x80000000)))",
            "#define _HRESULT_DEFINED",
            "typedef _Return_type_success_(return >= 0) long HRESULT;",
            "FORCEINLINE _Translates_Win32_to_HRESULT_(x) HRESULT HRESULT_FROM_WIN32(unsigned long x) { return (HRESULT)(x) <= 0 ? (HRESULT)(x) : (HRESULT) (((x) & 0x0000FFFF) | (FACILITY_WIN32 << 16) | 0x80000000);}",
            "#define HRESULT_FROM_WIN32(x) __HRESULT_FROM_WIN32(x)",
            "Map an NT status value into a HRESULT",
            "#define HRESULT_FROM_NT(x)      ((HRESULT) ((x) | FACILITY_NT_BIT))",
            "OBSOLETE functions",
            "HRESULT functions",
            "As noted above, these functions are obsolete and should not be used.",
            "Extract the SCODE from a HRESULT",
            "#define GetScode(hr) ((SCODE) (hr))",
            "Convert an SCODE into an HRESULT.",
            "#define ResultFromScode(sc) ((HRESULT) (sc))",
            "PropagateResult is a noop",
            "#define PropagateResult(hrPrevious, scBase) ((HRESULT) scBase)",
            "End of OBSOLETE functions.",
            "---------------------- HRESULT value definitions -----------------",
            "HRESULT definitions",
            "#define _HRESULT_TYPEDEF_(_sc) _sc",
            "#define _HRESULT_TYPEDEF_(_sc) ((HRESULT)_sc)",
            "#define NOERROR             0", // Handled in the initial seeding
            "Error definitions follow",
            "Codes 0x4000-0x40ff are reserved for OLE",
            "Success codes",
            "FACILITY_ITF",
            "Codes 0x0-0x01ff are reserved for the OLE group of",
            "interfaces.",
            "Generic OLE errors that may be returned by many inerfaces",
            "Old OLE errors",
            "Class Store Error Codes",
            "TXF & CRM errors start 4d080.",
            "OleTx Success codes.",
            "Old OLE Success Codes",
            "Task Scheduler errors",
            "FACILITY_WINDOWS",
            "Codes 0x0200-0x02ff are reserved for the APPX errors",
            "Codes 0x0300-0x030f are reserved for background task error codes.",
            "FACILITY_DISPATCH",
            "FACILITY_STORAGE",
            "++",
            "MessageId's 0x0305 - 0x031f (inclusive) are reserved for **STORAGE",
            "copy protection errors.",
            "--",
            "FACILITY_RPC",
            "Codes 0x0-0x11 are propagated from 16 bit OLE.",
            "Additional Security Status Codes",
            "Facility=Security",
            "end of Additional Security Status Codes",
            "FACILITY_SSPI",
            "Provided for backwards compatibility",
            "The range 0x5000-0x51ff is reserved for XENROLL errors.",
            "Error codes for mssipotf.dll",
            "Most of the error codes can only occur when an error occurs",
            "during font file signing",
            "Note that additional FACILITY_SSPI errors are in issperr.h",
            "FACILITY_CERT",
            "FACILITY_MEDIASERVER",
            "Also known as FACILITY_MF and FACILITY_NS",
            "The error codes are defined in mferror.mc, dlnaerror.mc, nserror.mc, and neterror.mc",
            "FACILITY_SETUPAPI",
            "Since these error codes aren't in the standard Win32 range (i.e., 0-64K), define a",
            "macro to map either Win32 or SetupAPI error codes into an HRESULT.",
            "#define HRESULT_FROM_SETUPAPI(x) ((((x) & (APPLICATION_ERROR_MASK|ERROR_SEVERITY_ERROR)) == (APPLICATION_ERROR_MASK|ERROR_SEVERITY_ERROR)) \\",
            "? ((HRESULT) (((x) & 0x0000FFFF) | (FACILITY_SETUPAPI << 16) | 0x80000000))                               \\",
            ": HRESULT_FROM_WIN32(x))",
            "FACILITY_SCARD",
            "Facility SCARD Error Messages",
            "These are warning codes.",
            "FACILITY_COMPLUS",
            "===============================",
            "Facility COMPLUS Error Messages",
            "The following are the subranges  within the COMPLUS facility",
            "0x400 - 0x4ff               COMADMIN_E_CAT",
            "0x600 - 0x6ff               COMQC errors",
            "0x700 - 0x7ff               MSDTC errors",
            "0x800 - 0x8ff               Other COMADMIN errors",
            "COMPLUS Admin errors",
            "COMPLUS Queued component errors",
            "The range 0x700-0x7ff is reserved for MSDTC errors.",
            "More COMADMIN errors from 0x8",
            "FACILITY_WER",
            "FACILITY_USERMODE_FILTER_MANAGER",
            "Translation macro for converting FilterManager error codes only from:",
            "NTSTATUS  --> HRESULT",
            "#define FILTER_HRESULT_FROM_FLT_NTSTATUS(x) (ASSERT((x & 0xfff0000) == 0x001c0000),(HRESULT) (((x) & 0x8000FFFF) | (FACILITY_USERMODE_FILTER_MANAGER << 16)))",
            "Facility Graphics Error Messages",
            "The following are the subranges within the Graphics facility",
            "0x0000 - 0x0fff     Display Driver Loader driver & Video Port errors (displdr.sys, videoprt.sys)",
            "0x1000 - 0x1fff     Monitor Class Function driver errors             (monitor.sys)",
            "0x2000 - 0x2fff     Windows Graphics Kernel Subsystem errors         (dxgkrnl.sys)",
            "0x3000 - 0x3fff               Desktop Window Manager errors",
            "0x2000 - 0x20ff      Common errors",
            "0x2100 - 0x21ff      Video Memory Manager (VidMM) subsystem errors",
            "0x2200 - 0x22ff      Video GPU Scheduler (VidSch) subsystem errors",
            "0x2300 - 0x23ff      Video Display Mode Management (VidDMM) subsystem errors",
            "Display Driver Loader driver & Video Port errors {0x0000..0x0fff}",
            "Desktop Window Manager errors {0x3000..0x3fff}",
            "Monitor class function driver errors {0x1000..0x1fff}",
            "Windows Graphics Kernel Subsystem errors {0x2000..0x2fff}",
            "TODO: Add DXG Win32 errors here",
            "Common errors {0x2000..0x20ff}",
            "Video Memory Manager (VidMM) subsystem errors {0x2100..0x21ff}",
            "Video GPU Scheduler (VidSch) subsystem errors {0x2200..0x22ff}",
            "Video Present Network Management (VidPNMgr) subsystem errors {0x2300..0x23ff}",
            "Port specific status codes {0x2400..0x24ff}",
            "OPM, UAB and PVP specific error codes {0x2500..0x257f}",
            "Monitor Configuration API error codes {0x2580..0x25DF}",
            "OPM, UAB, PVP and DDC/CI shared error codes {0x25E0..0x25ff}",
            "FACILITY_NAP",
            "TPM Services and TPM Software Error Messages",
            "The TPM services and TPM software facilities are used by the various",
            "TPM software components. There are two facilities because the services",
            "errors are within the TCG-defined error space and the software errors",
            "are not.",
            "The following are the subranges within the TPM Services facility.",
            "The TPM hardware errors are defined in the document",
            "TPM Main Specification 1.2 Part 2 TPM Structures.",
            "The TBS errors are slotted into the TCG error namespace at the TBS layer.",
            "0x0000 - 0x08ff     TPM hardware errors",
            "0x4000 - 0x40ff     TPM Base Services errors (tbssvc.dll)",
            "The following are the subranges within the TPM Software facility. The TBS",
            "has two classes of errors - those that can be returned (the public errors,",
            "defined in the TBS spec), which are in the TPM services facility,  and",
            "those that are internal or implementation specific, which are here in the",
            "TPM software facility.",
            "0x0000 - 0x00ff     TPM device driver errors (tpm.sys)",
            "0x0100 - 0x01ff     TPM API errors (tpmapi.lib)",
            "0x0200 - 0x02ff     TBS internal errors (tbssvc.dll)",
            "0x0300 - 0x03ff     TPM Physical Presence errors",
            "TPM hardware error codes {0x0000..0x08ff}",
            "This space is further subdivided into hardware errors, vendor-specific",
            "errors, and non-fatal errors.",
            "TPM hardware errors {0x0000..0x003ff}",
            "TPM vendor specific hardware errors {0x0400..0x04ff}",
            "TPM non-fatal hardware errors {0x0800..0x08ff}",
            "TPM Base Services error codes {0x4000..0x40ff}",
            "TPM API error codes {0x0100..0x01ff}",
            "TBS implementation error codes {0x0200..0x02ff}",
            "TPM Physical Presence implementation error codes {0x0300..0x03ff}",
            "Platform Crypto Provider (PCPTPM12.dll and future platform crypto providers)  error codes {0x0400..0x04ff}",
            "If the application is designed to use TCG defined TPM return codes",
            "then undefine the Windows defined codes for the same symbols. The application",
            "declares usage of TCG return codes by defining WIN_OMIT_TSS_TPM_RETURN_CODES",
            "before including windows.h",
            "=======================================================",
            "Facility Performance Logs & Alerts (PLA) Error Messages",
            "Full Volume Encryption Error Messages",
            "Windows Filtering Platform Error Messages",
            "Web Services Platform Error Codes",
            "NDIS error codes (ndis.sys)",
            "#define _NDIS_ERROR_TYPEDEF_(_sc)  _sc",
            "#define _NDIS_ERROR_TYPEDEF_(_sc)  (DWORD)(_sc)",
            "NDIS error codes (802.11 wireless LAN)",
            "NDIS informational code (ndis.sys)",
            "NDIS Chimney Offload codes (ndis.sys)",
            "Hypervisor error codes",
            "Virtualization error codes - these codes are used by the Virtualization Infrustructure Driver (VID) and other components",
            "of the virtualization stack.",
            "VID errors (0x0001 - 0x00ff)",
            "Host compute service errors (0x0100-0x01ff)",
            "Virtual networking errors (0x0200-0x02ff)",
            "VID warnings (0x0000 - 0x00ff):",
            "Volume manager error codes mapped from status codes",
            "WARNINGS",
            "ERRORS",
            "Boot Code Data (BCD) error codes",
            "Vhd error codes - These codes are used by the virtual hard diskparser component.",
            "Errors:",
            "Warnings:",
            "Facility Scripted Diagnostics (SDIAG) Error Messages",
            "Facility Windows Push Notifications (WPN) Error Messages",
            "MBN error codes",
            "Profile related error messages",
            "SMS related error messages",
            "P2P error codes",
            "record error codes",
            "eventing error",
            "searching error",
            "certificate verification error codes",
            "Contacts APIs error code",
            "Special success codes",
            "Pnrp helpers errors",
            "AppInvite APIs error code",
            "Serverless presence error codes",
            "UI error codes",
            "Bluetooth Attribute Protocol Warnings",
            "Audio errors",
            "StateRepository errors",
            "Spaceport errors",
            "Success",
            "Errors",
            "Volsnap errors",
            "Tiering errors",
            "Embedded Security Core",
            "Reserved id values 0x0001 - 0x00FF",
            "0x8xxx",
            "0x4xxx",
            "Clip modern app and windows licensing error messages.",
            "Facility Direct* Error Messages",
            "DXGI status (success) codes",
            "DXGI error codes",
            "DXGI errors that are internal to the Desktop Window Manager",
            "DXGI DDI",
            "Direct3D10",
            "Direct3D11",
            "Direct3D12",
            "Direct2D",
            "DirectWrite",
            "Windows Codecs",
            "MIL/DWM",
            "Composition engine errors",
            "MIL AV Specific errors",
            "MIL Bitmap Effet errors",
            "DWM specific errors",
            "DirectComposition",
            "OnlineId",
            "Facility Shell Error codes",
            "Sync Engine File Error Codes",
            "Sync Engine Stream Resolver Errors",
            "Sync Engine Global Errors",
            "EAS",
            "WebSocket",
            "Touch and Pen Input Platform Error Codes",
            "Internet",
            "Debuggers",
            "Sdbus",
            "JScript",
            "WEP - Windows Encryption Providers",
            "device lock feature - requires encryption software to use something like a TPM or a secure location to store failed counts of the password in an interactive logon to lock out the device",
            "Shared VHDX status codes (svhdxflt.sys)",
            "SMB status codes",
            "WININET.DLL errors - propagated as HRESULT's using FACILITY=WIN32",
            "SQLite",
            "FACILITY_UTC",
        };
    }
}
