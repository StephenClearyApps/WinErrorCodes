declare const enum ErrorMessageType {
    Win32,
    HResult,
    NtStatus
}

/** A single error code, with its identifiers and/or message text. */
interface ErrorMessage {
    /** The error code for this message */
    code: number;

    /** The array of human-readable identifiers for this message. This array may be empty. */
    identifiers: string[];

    /** The human-readable text of the error message. This may be null. The text may contain linefeed characters. */
    text: string;

    /** The type (category) of error message that this is. */
    type: ErrorMessageType;
}

/** A facility (category) of error messages, mapping a code to a meaning (or more than one meanings). */
interface Facility {
    /** The numeric value of this facility code */
    value: number;

    /** The array of human-readable identifiers for this facility. There is always at least one identifier. */
    names: FacilityName[];
}

/** A name of a facility. For historical reasons, multiple facility names (with different meanings) may actually have the same value. */
interface FacilityName {
    /** The human-readable identifier for this facility name. */
    name: string;

    /** The code range for this facility. The description of this facility is stored in the description for this range. */
    range: CodeRange;

    /** Notes for this facility. */
    notes: string;
}

/** A range of defined values with a semantic meaning. This is generally a subset of a facility. Ranges do not overlap and are never empty. */
interface CodeRange {
    /** A short description of this range. */
    description: string;

    /** The (inclusive) lower bound of this range. */
    lowerBound: number;

    /** The (exclusive) upper bound of this range. */
    upperBound: number;

    /** A collection of child ranges, which may be empty. */
    childRanges: CodeRange[];
}

/** The JSON data contining all known error codes. */
interface Data {
    /** The Win32 error messages. */
    win32: ErrorMessage[];

    /** The code range for Win32 error messages. */
    win32Range: CodeRange;

    /** The NTSTATUS erorr messages. This array may contain NTATUS-encoded Win32 errors *iff* they have separate NTSTATUS identifiers. */
    ntStatus: ErrorMessage[];

    /** The NTSTATUS facilities. */
    ntStatusFacilities: Facility[];

    /** The HRESULT error messages. This array may contain HRESULT-encoded Win32 or NTSTATUS errors *iff* they have separate HRESULT identifiers. */
    hresult: ErrorMessage[];

    /** The HRESULT facilities. */
    hresultFacilities: Facility[];
}

/** A single error code, with its identifiers and/or message text. */
interface ErrorMessageDto {
    /** The error code for this message */
    c: number;

    /** The array of human-readable identifiers for this message. This array may be empty. */
    i: string[];

    /** The human-readable text of the error message. This may be null. The text may contain linefeed characters. */
    t: string;
}

/** A facility (category) of error messages, mapping a code to a meaning. */
interface FacilityDto {
    /** The numeric value of this facility code */
    v: number;

    /** The array of human-readable identifiers for this facility. There is always at least one identifier. */
    n: FacilityNameDto[];
}

/** A name of a facility. For historical reasons, multiple facility names (with different meanings) may actually have the same value. */
interface FacilityNameDto {
    /** The human-readable identifier for this facility name. */
    n: string;

    /** The code range for this facility. */
    r: CodeRangeDto;

    /** Notes for this facility. */
    o: string;
}

/** A range of defined values with a semantic meaning. This is generally a subset of a facility. Ranges do not overlap and are never empty. */
interface CodeRangeDto {
    /** A short description of this range. */
    d: string;

    /** The (inclusive) lower bound of this range. */
    l: number;

    /** The (exclusive) upper bound of this range. */
    u: number;

    /** A collection of child ranges, which may be empty. */
    c: CodeRangeDto[];
}

/** The JSON data contining all known error codes. */
interface DataDto {
    /** The Win32 error messages. */
    w: ErrorMessageDto[];

    /** The code range for Win32 error messages. */
    wr: CodeRangeDto;

    /** The NTSTATUS erorr messages. This array may contain NTATUS-encoded Win32 errors *iff* they have separate NTSTATUS identifiers. */
    n: ErrorMessageDto[];

    /** The NTSTATUS facilities. */
    nf: FacilityDto[];

    /** The HRESULT error messages. This array may contain HRESULT-encoded Win32 or NTSTATUS errors *iff* they have separate HRESULT identifiers. */
    h: ErrorMessageDto[];

    /** The HRESULT facilities. */
    hf: FacilityDto[];
}
