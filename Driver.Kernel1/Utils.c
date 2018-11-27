#include "Driver.h"

/// <summary>
/// Gets the driver object.
/// </summary>
/// <param name="DriverName">Name of the driver.</param>
PDRIVER_OBJECT GetDriverObject(const PUNICODE_STRING DriverName)
{
    PDRIVER_OBJECT DriverObject;

    if (NT_SUCCESS(ObReferenceObjectByName(DriverName, OBJ_CASE_INSENSITIVE, NULL, 0, *IoDriverObjectType, KernelMode, NULL, (PVOID*) &DriverObject)))
    {
        return DriverObject;
    }

    return NULL;
}

/// <summary>
/// Deletes the driver path information.
/// </summary>
/// <param name="DriverEntry">The driver entry.</param>
VOID ExDeleteDriverPath(const PKLDR_DATA_TABLE_ENTRY DriverEntry)
{
    DriverEntry->FullImageName.Buffer[0] = L'\0';
    DriverEntry->FullImageName.Length = 0;
    DriverEntry->FullImageName.MaximumLength = 0;

    DriverEntry->BaseImageName.Buffer[0] = L'\0';
    DriverEntry->BaseImageName.Length = 0;
    DriverEntry->BaseImageName.MaximumLength = 0;
}

/// <summary>
/// Deletes the driver information.
/// </summary>
/// <param name="DriverObject">The driver object.</param>
VOID ExDeleteDriverInfo(const PDRIVER_OBJECT DriverObject)
{
    DriverObject->DriverSection = NULL;
    DriverObject->DriverStart   = NULL;
    DriverObject->DriverSize    = 0;
    DriverObject->DriverUnload  = NULL;
    DriverObject->DriverInit    = NULL;
    DriverObject->DeviceObject  = NULL;
}

/// <summary>
/// Checks if the specified process is terminating.
/// </summary>
/// <param name="Process">The process.</param>
BOOLEAN CheckProcessTermination(PEPROCESS Process)
{
    LARGE_INTEGER zeroTime = { 0 };
    return KeWaitForSingleObject(Process, Executive, KernelMode, FALSE, &zeroTime) == STATUS_WAIT_0;
}

/// <summary>
/// Allocate new Unicode string from Paged pool
/// </summary>
/// <param name="result">Resulting string</param>
/// <param name="size">Buffer size in bytes to alloacate</param>
/// <returns>Status code</returns>
NTSTATUS SafeAllocateString(OUT PUNICODE_STRING result, IN USHORT size)
{
    if (result == NULL || size == 0)
        return STATUS_INVALID_PARAMETER;

    result->Buffer = ExAllocatePoolWithTag(PagedPool, size, (ULONG) 'NagG');
    result->Length = 0;
    result->MaximumLength = size;

    if (result->Buffer)
        RtlZeroMemory(result->Buffer, size);
    else
        return STATUS_NO_MEMORY;

    return STATUS_SUCCESS;
}

/// <summary>
/// Allocate and copy string
/// </summary>
/// <param name="result">Resulting string</param>
/// <param name="source">Source string</param>
/// <returns>Status code</returns>
NTSTATUS SafeInitString(OUT PUNICODE_STRING result, IN PUNICODE_STRING source)
{
    if (result == NULL || source == NULL || source->Buffer == NULL)
        return STATUS_INVALID_PARAMETER;

    // No data to copy
    if (source->Length == 0)
    {
        result->Length = result->MaximumLength = 0;
        result->Buffer = NULL;
        return STATUS_SUCCESS;
    }

    result->Buffer = ExAllocatePoolWithTag(PagedPool, source->MaximumLength, (ULONG) 'NagG');
    result->Length = source->Length;
    result->MaximumLength = source->MaximumLength;

    memcpy(result->Buffer, source->Buffer, source->Length);

    return STATUS_SUCCESS;
}

/// <summary>
/// Search for substring
/// </summary>
/// <param name="source">Source string</param>
/// <param name="target">Target string</param>
/// <param name="CaseInSensitive">Case insensitive search</param>
/// <returns>Found position or -1 if not found</returns>
LONG SafeSearchString(IN PUNICODE_STRING source, IN PUNICODE_STRING target, IN BOOLEAN CaseInSensitive)
{
    if (source == NULL || target == NULL || source->Buffer == NULL || target->Buffer == NULL)
        return STATUS_INVALID_PARAMETER;

    // Size mismatch
    if (source->Length < target->Length)
        return -1;

    USHORT diff = source->Length - target->Length;
    for (USHORT i = 0; i < diff; i++)
    {
        if (RtlCompareUnicodeStrings(
            source->Buffer + i / sizeof(WCHAR),
            target->Length / sizeof(WCHAR),
            target->Buffer,
            target->Length / sizeof(WCHAR),
            CaseInSensitive
        ) == 0)
        {
            return i;
        }
    }

    return -1;
}

/// <summary>
/// Get file name from full path
/// </summary>
/// <param name="path">Path.</param>
/// <param name="name">Resulting name</param>
/// <returns>Status code</returns>
NTSTATUS StripPath(IN PUNICODE_STRING path, OUT PUNICODE_STRING name)
{
    if (path == NULL || name == NULL)
        return STATUS_INVALID_PARAMETER;

    // Empty string
    if (path->Length < 2)
    {
        *name = *path;
        return STATUS_NOT_FOUND;
    }

    for (USHORT i = (path->Length / sizeof(WCHAR)) - 1; i != 0; i--)
    {
        if (path->Buffer[i] == L'\\' || path->Buffer[i] == L'/')
        {
            name->Buffer = &path->Buffer[i + 1];
            name->Length = name->MaximumLength = path->Length - (i + 1) * sizeof(WCHAR);
            return STATUS_SUCCESS;
        }
    }

    *name = *path;
    return STATUS_NOT_FOUND;
}

/// <summary>
/// Get directory path name from full path
/// </summary>
/// <param name="path">Path</param>
/// <param name="name">Resulting directory path</param>
/// <returns>Status code</returns>
NTSTATUS StripFilename(IN PUNICODE_STRING path, OUT PUNICODE_STRING dir)
{
    if (path == NULL || dir == NULL)
        return STATUS_INVALID_PARAMETER;

    // Empty string
    if (path->Length < 2)
    {
        *dir = *path;
        return STATUS_NOT_FOUND;
    }

    for (USHORT i = (path->Length / sizeof(WCHAR)) - 1; i != 0; i--)
    {
        if (path->Buffer[i] == L'\\' || path->Buffer[i] == L'/')
        {
            dir->Buffer = path->Buffer;
            dir->Length = dir->MaximumLength = i * sizeof(WCHAR);
            return STATUS_SUCCESS;
        }
    }

    *dir = *path;
    return STATUS_NOT_FOUND;
}

/// <summary>
/// Check if file exists
/// </summary>
/// <param name="path">Fully qualifid path to a file</param>
/// <returns>Status code</returns>
NTSTATUS FileExists(IN PUNICODE_STRING path)
{
    HANDLE hFile = NULL;
    IO_STATUS_BLOCK statusBlock = { 0 };
    OBJECT_ATTRIBUTES obAttr = { 0 };
    InitializeObjectAttributes(&obAttr, path, OBJ_KERNEL_HANDLE, NULL, NULL);

    NTSTATUS status = ZwCreateFile(
        &hFile, FILE_READ_DATA | SYNCHRONIZE, &obAttr,
        &statusBlock, NULL, FILE_ATTRIBUTE_NORMAL,
        FILE_SHARE_READ, FILE_OPEN, FILE_SYNCHRONOUS_IO_NONALERT, NULL, 0
    );

    if (NT_SUCCESS(status))
        ZwClose(hFile);

    return status;
}

/// <summary>
/// Get module base address by name
/// </summary>
/// <param name="pProcess">Target process</param>
/// <param name="ModuleName">Nodule name to search for</param>
/// <returns>Found address, NULL if not found</returns>
PVOID GetUserModule64(IN PEPROCESS pProcess, IN PUNICODE_STRING ModuleName)
{
    if (pProcess == NULL)
        return NULL;

    __try
    {
        LARGE_INTEGER time = { 0 };
        time.QuadPart = -250ll * 10 * 1000;

        PPEB pPeb = PsGetProcessPeb(pProcess);

        if (!pPeb)
        {
            DbgPrintEx(0, 0, "%s: No PEB present. Aborting\n", __FUNCTION__);
            return NULL;
        }

        for (INT i = 0; !pPeb->Ldr && i < 10; i++)
        {
            DbgPrintEx(0, 0, "%s: Loader not intialized, waiting\n", __FUNCTION__);
            KeDelayExecutionThread(KernelMode, TRUE, &time);
        }

        if (!pPeb->Ldr)
        {
            DbgPrintEx(0, 0, "%s: Loader was not intialized in time. Aborting\n", __FUNCTION__);
            return NULL;
        }

        for (PLIST_ENTRY pListEntry = pPeb->Ldr->InLoadOrderModuleList.Flink; pListEntry != &pPeb->Ldr->InLoadOrderModuleList; pListEntry = pListEntry->Flink)
        {
            PLDR_DATA_TABLE_ENTRY pEntry = CONTAINING_RECORD(pListEntry, LDR_DATA_TABLE_ENTRY, InLoadOrderLinks);

            if (RtlCompareUnicodeString(&pEntry->BaseDllName, ModuleName, TRUE) == 0)
            {
                return pEntry->DllBase;
            }
        }
    }
    __except (EXCEPTION_EXECUTE_HANDLER)
    {
        DbgPrintEx(0, 0, "%s: Exception, Code: 0x%X\n", __FUNCTION__, GetExceptionCode());
    }

    return NULL;
}