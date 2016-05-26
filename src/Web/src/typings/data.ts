export const enum ErrorMessageType {
    Win32,
    HResult,
    NtStatus
}

/** A single error code, with its identifiers and/or message text. */
export interface ErrorMessage {
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
export interface Facility {
    /** The numeric value of this facility code */
    value: number;

    /** The array of human-readable identifiers for this facility. There is always at least one identifier. */
    names: FacilityName[];
}

/** A name of a facility. For historical reasons, multiple facility names (with different meanings) may actually have the same value. */
export interface FacilityName {
    /** The human-readable identifier for this facility name. */
    name: string;

    /** The code range for this facility. The description of this facility is stored in the description for this range. */
    range: CodeRange;

    /** Notes for this facility. */
    notes: string;
}

/** A range of defined values with a semantic meaning. This is generally a subset of a facility. Ranges do not overlap and are never empty. */
export interface CodeRange {
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
export interface Data {
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
export interface ErrorMessageDto {
    /** The error code for this message */
    c: number;

    /** The array of human-readable identifiers for this message. This array may be empty. */
    i: string[];

    /** The human-readable text of the error message. This may be null. The text may contain linefeed characters. */
    t: string;
}

/** A facility (category) of error messages, mapping a code to a meaning. */
export interface FacilityDto {
    /** The numeric value of this facility code */
    v: number;

    /** The array of human-readable identifiers for this facility. There is always at least one identifier. */
    n: FacilityNameDto[];
}

/** A name of a facility. For historical reasons, multiple facility names (with different meanings) may actually have the same value. */
export interface FacilityNameDto {
    /** The human-readable identifier for this facility name. */
    n: string;

    /** The code range for this facility. */
    r: CodeRangeDto;

    /** Notes for this facility. */
    o: string;
}

/** A range of defined values with a semantic meaning. This is generally a subset of a facility. Ranges do not overlap and are never empty. */
export interface CodeRangeDto {
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
export interface DataDto {
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

function transformErrorMessage(errorMessage: ErrorMessageDto, type: ErrorMessageType): ErrorMessage {
    return {
        code: errorMessage.c,
        identifiers: errorMessage.i,
        text: errorMessage.t,
        type
    };
}

function transformFacilityName(facilityName: FacilityNameDto): FacilityName {
    return {
        name: facilityName.n,
        range: transformRange(facilityName.r),
        notes: facilityName.o
    }
}

function transformFacility(facility: FacilityDto): Facility {
    return {
        value: facility.v,
        names: facility.n.map(transformFacilityName)
    };
}

function transformRange(range: CodeRangeDto): CodeRange {
    return {
        description: range.d,
        lowerBound: range.l,
        upperBound: range.u,
        childRanges: range.c.map(transformRange)
    };
}

export function transformData(data: DataDto): Data {
    return {
        win32: data.w.map(x => transformErrorMessage(x, ErrorMessageType.Win32)),
        win32Range: transformRange(data.wr),
        ntStatus: data.n.map(x => transformErrorMessage(x, ErrorMessageType.NtStatus)),
        hresult: data.h.map(x => transformErrorMessage(x, ErrorMessageType.HResult)),
        ntStatusFacilities: data.nf.map(transformFacility),
        hresultFacilities: data.hf.map(transformFacility)
    };
}
