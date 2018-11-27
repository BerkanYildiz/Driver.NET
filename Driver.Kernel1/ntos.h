#pragma once

#ifndef _NTOS_H
#define _NTOS_H

#define MM_UNLOADED_DRIVERS_SIZE 50 // NT 5.x
#define VALID_SECTION 0x20 // Windows 10 - 1803

typedef unsigned long long QWORD;
typedef unsigned char BYTE;

typedef NTSTATUS(NTAPI OBREFERENCEOBJECTBYNAME) (
    PUNICODE_STRING ObjectPath,
    ULONG Attributes,
    PACCESS_STATE PassedAccessState OPTIONAL,
    ACCESS_MASK DesiredAccess OPTIONAL,
    POBJECT_TYPE ObjectType,
    KPROCESSOR_MODE AccessMode,
    PVOID ParseContext OPTIONAL,
    PVOID *ObjectPtr);

__declspec(dllimport) OBREFERENCEOBJECTBYNAME ObReferenceObjectByName;
__declspec(dllimport) POBJECT_TYPE *IoDriverObjectType;

typedef struct _KLDR_DATA_TABLE_ENTRY
{
	LIST_ENTRY InLoadOrderLinks;
	PVOID ExceptionTable;
	UINT32 ExceptionTableSize;
	PVOID GpValue;
	struct _NON_PAGED_DEBUG_INFO *NonPagedDebugInfo;
	PVOID ImageBase;
	PVOID EntryPoint;
	UINT32 SizeOfImage;
	UNICODE_STRING FullImageName;
	UNICODE_STRING BaseImageName;
	UINT32 Flags;
	UINT16 LoadCount;

	union
	{
		UINT16 SignatureLevel : 4;
		UINT16 SignatureType : 3;
		UINT16 Unused : 9;
		UINT16 EntireField;
	} u;

	PVOID SectionPointer;
	UINT32 CheckSum;
	UINT32 CoverageSectionSize;
	PVOID CoverageSection;
	PVOID LoadedImports;
	PVOID Spare;
	UINT32 SizeOfImageNotRounded;
	UINT32 TimeDateStamp;
} KLDR_DATA_TABLE_ENTRY, *PKLDR_DATA_TABLE_ENTRY;

typedef struct _LDR_DATA_TABLE_ENTRY {
	LIST_ENTRY InLoadOrderLinks;
	LIST_ENTRY InMemoryOrderLinks;
	LIST_ENTRY InInitializationOrderLinks;
	PVOID DllBase;
	PVOID EntryPoint;
	ULONG SizeOfImage;
	UNICODE_STRING FullDllName;
	UNICODE_STRING BaseDllName;
	ULONG Flags;
	USHORT LoadCount;
	USHORT TlsIndex;
	union {
		LIST_ENTRY HashLinks;
		struct {
			PVOID SectionPointer;
			ULONG CheckSum;
		};
	};
	union {
		struct {
			ULONG TimeDateStamp;
		};
		struct {
			PVOID LoadedImports;
		};
	};
	PVOID * EntryPointActivationContext;

	PVOID PatchInformation;

} LDR_DATA_TABLE_ENTRY, *PLDR_DATA_TABLE_ENTRY;

typedef struct _MM_UNLOADED_DRIVER
{
    UNICODE_STRING 	Name;
    PVOID 			ModuleStart;
    PVOID 			ModuleEnd;
    ULONG64 		UnloadTime;
} MM_UNLOADED_DRIVER, *PMM_UNLOADED_DRIVER;

NTSTATUS NTAPI IoCreateDriver(IN PUNICODE_STRING DriverName OPTIONAL, IN PDRIVER_INITIALIZE InitializationFunction);
NTSTATUS NTAPI MmCopyVirtualMemory
(
	PEPROCESS SourceProcess,
	PVOID SourceAddress,
	PEPROCESS TargetProcess,
	PVOID TargetAddress,
	SIZE_T BufferSize,
	KPROCESSOR_MODE PreviousMode,
	PSIZE_T ReturnSize
);

NTKERNELAPI NTSTATUS PsLookupProcessByProcessId(HANDLE ProcessId, PEPROCESS *Process);
NTKERNELAPI PVOID PsGetProcessSectionBaseAddress(__in PEPROCESS Process);
NTKERNELAPI PVOID NTAPI PsGetProcessWow64Process(IN PEPROCESS Process);
NTKERNELAPI PPEB  NTAPI PsGetProcessPeb(IN PEPROCESS Process);
NTSYSAPI PIMAGE_NT_HEADERS NTAPI RtlImageNtHeader(IN PVOID ModuleAddress);

NTSYSAPI NTSTATUS NTAPI ZwTerminateThread(_In_ HANDLE ThreadHandle, _In_ NTSTATUS ExitStatus);

NTSYSAPI ULONG RtlRandomEx(
    PULONG Seed
);

__kernel_entry NTSYSCALLAPI NTSTATUS ZwAllocateVirtualMemory(
    HANDLE    ProcessHandle,
    PVOID     *BaseAddress,
    ULONG_PTR ZeroBits,
    PSIZE_T   RegionSize,
    ULONG     AllocationType,
    ULONG     Protect
);

__kernel_entry NTSYSCALLAPI NTSTATUS ZwFreeVirtualMemory(
    IN PHANDLE ProcessHandle,
    IN OUT PVOID *BaseAddress,
    IN OUT PULONG RegionSize,
    IN ULONG FreeType
);

#endif