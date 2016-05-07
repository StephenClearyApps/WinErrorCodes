using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Constants;

namespace Win32ErrorTable
{
    public sealed class Resources
    {
        private readonly string filename;
        private MemoryMappedViewAccessor view;
        IMAGE_DOS_HEADER dosHeader;
        IMAGE_NT_HEADERS32 ntHeader;
        IMAGE_SECTION_HEADER[] sectionHeaders;
        IMAGE_SECTION_HEADER rsrc;
        private readonly List<ErrorMessage> results = new List<ErrorMessage>();

        public Resources(string filename)
        {
            this.filename = filename;
        }

        public IList<ErrorMessage> Process()
        {
            using (var stream = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var mapping = MemoryMappedFile.CreateFromFile(stream, null, 0, MemoryMappedFileAccess.Read, HandleInheritability.None, true))
            using (view = mapping.CreateViewAccessor(0, 0, MemoryMappedFileAccess.Read))
            {
                view.Read(0, out dosHeader);
                if (!dosHeader.IsValid)
                    return results;

                view.Read(dosHeader.e_lfanew, out ntHeader);
                if (!ntHeader.IsValid)
                    return results;

                var sectionBaseOffset = dosHeader.e_lfanew + 24 + ntHeader.FileHeader.SizeOfOptionalHeader;
                sectionHeaders = new IMAGE_SECTION_HEADER[ntHeader.FileHeader.NumberOfSections];
                view.ReadArray(sectionBaseOffset, sectionHeaders, 0, sectionHeaders.Length);
                foreach (var sectionHeader in sectionHeaders)
                {
                    if (sectionHeader.Section != ".rsrc")
                        continue;

                    rsrc = sectionHeader;
                    IMAGE_RESOURCE_DIRECTORY typeTable;
                    view.Read(sectionHeader.PointerToRawData, out typeTable);
                    var entryOffset = sectionHeader.PointerToRawData + 16 + typeTable.NumberOfNameEntries * 8;
                    for (int j = 0; j != typeTable.NumberOfIdEntries; ++j)
                    {
                        IMAGE_RESOURCE_DIRECTORY_ENTRY idEntry;
                        view.Read(entryOffset + j * 8, out idEntry);
                        if (idEntry.Id != RT_MESSAGETABLE)
                            continue;

                        DumpResources(idEntry.SubdirectoryOffset);
                    }
                }
            }
            return results;
        }

        void DumpResources(uint tableOffset)
        {
            IMAGE_RESOURCE_DIRECTORY table;
            view.Read(rsrc.PointerToRawData + tableOffset, out table);
            var entryOffset = rsrc.PointerToRawData + tableOffset + 16;
            for (int j = 0; j != table.NumberOfNameEntries; ++j)
            {
                IMAGE_RESOURCE_DIRECTORY_ENTRY nameEntry;
                view.Read(entryOffset + j * 8, out nameEntry);
                //var nameLength = view.ReadUInt16(rsrc.PointerToRawData + nameEntry.NameOffset);
                //var nameChars = new char[nameLength];
                //view.ReadArray(rsrc.PointerToRawData + nameEntry.NameOffset + 2, nameChars, 0, nameLength);
                //var name = new string(nameChars);
                if (nameEntry.IsSubdirectory)
                {
                    DumpResources(nameEntry.SubdirectoryOffset);
                }
                else
                {
                    DumpStrings(nameEntry.DataOffset);
                }
            }
            entryOffset += (uint)table.NumberOfNameEntries * 8;
            for (int j = 0; j != table.NumberOfIdEntries; ++j)
            {
                IMAGE_RESOURCE_DIRECTORY_ENTRY idEntry;
                view.Read(entryOffset + j * 8, out idEntry);
                if (idEntry.IsSubdirectory)
                {
                    DumpResources(idEntry.SubdirectoryOffset);
                }
                else
                {
                    DumpStrings(idEntry.DataOffset);
                }
            }
        }

        void DumpStrings(uint dataEntryOffset)
        {
            IMAGE_RESOURCE_DATA_ENTRY dataEntry;
            view.Read(rsrc.PointerToRawData + dataEntryOffset, out dataEntry);

            var dataOffset = RvaToFileOffset(dataEntry.DataRva);
            var numberOfBlocks = view.ReadUInt32(dataOffset);
            for (var blockIndex = 0; blockIndex != numberOfBlocks; ++blockIndex)
            {
                MESSAGE_RESOURCE_BLOCK block;
                view.Read(dataOffset + 4 + blockIndex * 12, out block);
                var messageOffset = dataOffset + block.OffsetToEntries;
                for (var messageId = block.LowId; messageId <= block.HighId; ++messageId)
                {
                    MESSAGE_RESOURCE_ENTRY entry;
                    view.Read(messageOffset, out entry);
                    messageOffset += entry.Length;
                    if (entry.Length == 4)
                        continue;
                    // TODO: should we properly handle code pages?
                    if ((entry.Flags & MessageResourceEntryFlags.Unicode) != 0)
                    {
                        var chars = new char[(entry.Length - 4) / 2];
                        view.ReadArray(messageOffset - entry.Length + 4, chars, 0, chars.Length);
                        var str = new string(chars).TrimEnd('\0');
                        if (str != "")
                            results.Add(new ErrorMessage(messageId, new string[0], str.Trim().Replace("\r\n", "\n").Replace("\n\n", "\n")));
                    }
                    else
                    {
                        var chars = new byte[entry.Length - 4];
                        view.ReadArray(messageOffset - entry.Length + 4, chars, 0, chars.Length);
                        var str = Encoding.ASCII.GetString(chars);
                        if (str != "")
                            results.Add(new ErrorMessage(messageId, new string[0], str.Trim().Replace("\r\n", "\n").Replace("\n\n", "\n")));
                    }
                }
            }
        }

        uint RvaToFileOffset(uint rva)
        {
            foreach (var sectionHeader in sectionHeaders)
            {
                if (rva >= sectionHeader.VirtualAddress && rva < sectionHeader.VirtualAddress + sectionHeader.VirtualSize)
                {
                    return rva - sectionHeader.VirtualAddress + sectionHeader.PointerToRawData;
                }
            }
            throw new InvalidOperationException("Could not evaluate RVA " + rva);
        }
    }
}
