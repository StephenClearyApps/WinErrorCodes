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
    // TODO: Source info (filename with path, line number).
    // TODO: HvStatus.h
    public sealed partial class NtStatusH
    {
        private static ErrorMessage CreateMessage(uint code, string messageId, IEnumerable<string> messageTextLines)
        {
            return new ErrorMessage(code, new List<string> { messageId }, string.Join("\n", messageTextLines));
        }

        private const string Identifier = "[A-Z][A-Za-z_0-9]*";

        private static readonly Regex CommentRegex = new Regex(@"^(.*)\s// (winnt|ntsubauth)$");

        private static readonly Regex FacilityHexRegex = new Regex(@"^#define (FACILITY_[A-Za-z_0-9]+)\s+0x([0-9A-Fa-f]+)$");
        private static readonly Regex FaciltiyHexRegex = new Regex(@"^#define (FACILTIY_[A-Za-z_0-9]+)\s+0x([0-9A-Fa-f]+)$");
        private static readonly Regex SeverityHexRegex = new Regex(@"^#define STATUS_SEVERITY_([A-Za-z_0-9]+)\s+0x([0-9A-Fa-f]+)$");

        private static readonly Regex MessageIdRegex = new Regex(@"^MessageId: (" + Identifier + @")$");

        private static readonly Regex DefineNtStatusHexCastRegex = new Regex(@"^#define (" + Identifier + @")\s+\(\(NTSTATUS\)0x([0-9A-Fa-f]{8})L?\)$");

        private string messageId;
        private List<string> messageText;

        private readonly List<ErrorMessage> ntstatus = new List<ErrorMessage>();
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
            Seed();

            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Windows Kits", "10", "Include");
            var selected = Directory.EnumerateDirectories(path).Select(dir => new { dir, ver = TryParseVersion(Path.GetFileName(dir)) }).OrderByDescending(x => x.ver).First();
            path = selected.dir;
            path = Path.Combine(path, "shared");
            var lines = File.ReadAllLines(Path.Combine(path, "ntstatus.h"));
            foreach (var line in lines.Select(x => x.Trim(' ', '\t', '/', '*')).Select(x =>
            {
                var match = CommentRegex.Match(x);
                return match.Success ? match.Groups[1].Value.Trim(' ', '\t') : x;
            }).Where(x => x != ""))
            {
                Match match;

                if (messageId == null)
                {
                    if (line.StartsWith("#if") || line.StartsWith("#else") || line.StartsWith("#endif") || line.StartsWith("#undef") || line.StartsWith("#pragma"))
                        continue;
                    if (IgnoredLines.Contains(line))
                        continue;

                    match = FacilityHexRegex.Match(line);
                    if (match.Success)
                    {
                        var id = match.Groups[1].Value;
                        var value = uint.Parse(match.Groups[2].Value, NumberStyles.AllowHexSpecifier);
                        var existing = facilities.FirstOrDefault(x => x.Value == value);
                        if (existing == null)
                            facilities.Add(new Facility(value, new List<FacilityName> { new FacilityName(id, FacilityDescription(id)) }));
                        else
                            existing.Names.Add(new FacilityName(id, FacilityDescription(id)));
                        continue;
                    }

                    match = FaciltiyHexRegex.Match(line);
                    if (match.Success)
                    {
                        var id = match.Groups[1].Value;
                        var value = uint.Parse(match.Groups[2].Value, NumberStyles.AllowHexSpecifier);
                        var existing = facilities.FirstOrDefault(x => x.Value == value);
                        if (existing == null)
                            facilities.Add(new Facility(value, new List<FacilityName> { new FacilityName(id, FacilityDescription(id), "Note that \"FACILTIY\" is misspelled") }));
                        else
                            existing.Names.Add(new FacilityName(id, FacilityDescription(id), "Note that \"FACILTIY\" is misspelled"));
                        continue;
                    }

                    match = SeverityHexRegex.Match(line);
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
                        match = DefineNtStatusHexCastRegex.Match(line);
                        if (match.Success)
                        {
                            //Console.WriteLine(messageId + " = " + match.Groups[2].Value);
                            ntstatus.Add(CreateMessage(uint.Parse(match.Groups[2].Value, NumberStyles.AllowHexSpecifier), messageId, messageText));
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

                Console.WriteLine("\"" + line + "\",");
                System.Windows.Forms.Clipboard.SetText("\"" + line + "\",");
                Console.ReadKey();
                messageId = null;
                messageText = null;
            }

            PostSeed();
            return new Results(null, null, null, ntstatus, facilities);
        }
    }
}
