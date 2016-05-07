using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

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
        }

        [JsonProperty("w")]
        public List<ErrorMessage> Win32Errors { get; }

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
        public string Text { get; }
    }

    public sealed class Facility
    {
        public Facility(uint value, IList<string> names)
        {
            Value = value;
            Names = names;
        }

        [JsonProperty("v")]
        public uint Value { get; }

        [JsonProperty("n")]
        public IList<string> Names { get; }
    }
}
