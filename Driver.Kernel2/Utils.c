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
/// Hooks the specified driver.
/// </summary>
/// <param name="TargetedDeviceName">Name of the device.</param>
/// <param name="HookedDeviceIoControl">The hooked device io control.</param>
NTSTATUS HookDriver(IN PWCHAR TargetedDeviceName, PVOID HookedDeviceIoControl)
{
    NTSTATUS status = STATUS_SUCCESS;

    UNICODE_STRING deviceName = { 0 };
    PFILE_OBJECT pFileObj   = NULL;
    PDEVICE_OBJECT pDevObj  = NULL;

    RtlInitUnicodeString(&deviceName, TargetedDeviceName);

    status = IoGetDeviceObjectPointer(&deviceName, FILE_READ_DATA, &pFileObj, &pDevObj);

    if (!NT_SUCCESS(status))
    {
        return status;
    }

    gHookCtx.pDrvObj        = pDevObj->DriverObject;
    gHookCtx.pFileObj       = pFileObj;

    gHookCtx.pOldHandler    = gHookCtx.pDrvObj->MajorFunction[IRP_MJ_DEVICE_CONTROL];
    gHookCtx.pDrvObj->MajorFunction[IRP_MJ_DEVICE_CONTROL] = HookedDeviceIoControl;

    //g_HookCtx.pOldHandler = (PDRIVER_DISPATCH)InterlockedExchange64(
    //    (PLONG64)&g_HookCtx.pDrvObj->MajorFunction[IRP_MJ_DEVICE_CONTROL],
    //    (LONG64)&HookedDeviceControl);

    gHookCtx.IsHooked       = TRUE;

    return status;
}