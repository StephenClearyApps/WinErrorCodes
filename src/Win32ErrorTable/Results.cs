using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Nito.Comparers;

namespace Win32ErrorTable
{
    public sealed class Results
    {
        public Results(List<ErrorMessage> win32Errors, List<ErrorMessage> hResultErrors, List<Facility> hResultFacilities, List<ErrorMessage> ntStatusErrors, List<Facility> ntStatusFacilities)
        {
            Win32Errors = win32Errors ?? new List<ErrorMessage>();
            HResultErrors = hResultErrors ?? new List<ErrorMessage>();
            HResultFacilities = hResultFacilities ?? new List<Facility>();
            NtStatusErrors = ntStatusErrors ?? new List<ErrorMessage>();
            NtStatusFacilities = ntStatusFacilities ?? new List<Facility>();
            Win32Range = new Range(0, 0xFFFF, null);
        }

        [JsonProperty("w")]
        public List<ErrorMessage> Win32Errors { get; }

        [JsonProperty("wr")]
        public Range Win32Range { get; }

        [JsonProperty("h")]
        public List<ErrorMessage> HResultErrors { get; }

        [JsonProperty("hf")]
        public List<Facility> HResultFacilities { get; }

        [JsonProperty("n")]
        public List<ErrorMessage> NtStatusErrors { get; }

        [JsonProperty("nf")]
        public List<Facility> NtStatusFacilities { get; }

        public bool HResultIsKnownWin32Error(uint hresultCode)
        {
            return (hresultCode & 0x1FFF0000) == 0x00070000 && Win32Errors.Any(x => x.Code == (hresultCode & 0xFFFF));
        }

        public void MergeWith(Results other)
        {
            Win32Errors.AddRange(other.Win32Errors);
            HResultErrors.AddRange(other.HResultErrors);
            HResultFacilities.AddRange(other.HResultFacilities);
            NtStatusErrors.AddRange(other.NtStatusErrors);
            NtStatusFacilities.AddRange(other.NtStatusFacilities);
            Win32Range.MergeWith(other.Win32Range);
        }
    }

    public sealed class ErrorMessage
    {
        public ErrorMessage(uint code, IList<string> ids, string text)
        {
            Code = code;
            Ids = ids;
            Text = text;
        }

        [JsonProperty("c")]
        public uint Code { get; }

        [JsonProperty("i")]
        public IList<string> Ids { get; }

        [JsonProperty("t")]
        public string Text { get; set; }
    }

    public sealed class Facility
    {
        public Facility(uint value, IList<FacilityName> names)
        {
            Value = value;
            Names = names;
        }

        [JsonProperty("v")]
        public uint Value { get; }

        [JsonProperty("n")]
        public IList<FacilityName> Names { get; }
    }

    public sealed class FacilityName
    {
        public FacilityName(string name, string description, string notes = "")
        {
            Name = name;
            Range = new Range(0, 0xFFFF, description);
            Notes = notes;
        }

        [JsonProperty("n")]
        public string Name { get; }

        [JsonProperty("r")]
        public Range Range { get; }

        [JsonProperty("o")]
        public string Notes { get; }
    }

    public sealed class Range
    {
        private readonly List<Range> _childRanges;

        public Range(uint lowerBound, uint upperBound, string description)
        {
            LowerBound = lowerBound;
            UpperBound = upperBound;
            Description = description ?? "";
            _childRanges = new List<Range>();
            if (LowerBound >= UpperBound)
                throw new InvalidOperationException("LowerBound must be smaller than UpperBound");
        }

        [JsonProperty("d")]
        public string Description { get; set; }

        [JsonProperty("l")]
        public uint LowerBound { get; }

        [JsonProperty("u")]
        public uint UpperBound { get; }

        [JsonProperty("c")]
        public IReadOnlyList<Range> ChildRanges => _childRanges;

        public void AddChildRange(Range child)
        {
            if (child.LowerBound < LowerBound || child.UpperBound > UpperBound)
                throw new InvalidOperationException("Child is outside range of parent");
            if (ChildRanges.Any(existingChild => existingChild.LowerBound < child.UpperBound && existingChild.UpperBound > child.LowerBound))
                throw new InvalidOperationException("Overlapping child ranges");
            var index = _childRanges.BinarySearch(child, ComparerBuilder.For<Range>().OrderBy(x => x.LowerBound));
            if (index >= 0)
                throw new InvalidOperationException("Comparer detected overlapping ranges");
            _childRanges.Insert(~index, child);
        }

        public void MergeWith(Range other)
        {
            if (other.LowerBound != LowerBound || other.UpperBound != UpperBound)
                throw new InvalidOperationException("Merging incompatible ranges");
            foreach (var child in other.ChildRanges)
                AddChildRange(child);
        }
    }
}
