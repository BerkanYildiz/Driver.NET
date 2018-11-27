#include "Driver.h"

/// <summary>
/// Defines the entry point of this driver.
/// </summary>
/// <param name="DriverObject">The driver object.</param>
/// <param name="RegistryPath">The registry path.</param>
NTSTATUS DriverEntry(PDRIVER_OBJECT DriverObject, PUNICODE_STRING RegistryPath)
{
    DbgPrintEx(0, 0, "[DriverExample] Executing " __FUNCTION__ ".\n");

	NTSTATUS			Status;
	UNICODE_STRING		DriverName;
    UNICODE_STRING		TurlaName;
    PDRIVER_OBJECT      TurlaObject;

	RtlInitUnicodeString(&DriverName, ConstDriverName);
    RtlInitUnicodeString(&TurlaName, ConstTurlaDriverName);

	UNREFERENCED_PARAMETER(DriverObject);
	UNREFERENCED_PARAMETER(RegistryPath);

    ObReferenceObjectByName(&TurlaName, OBJ_CASE_INSENSITIVE, NULL, 0, *IoDriverObjectType, KernelMode, NULL, (PVOID) &TurlaObject);

    if (TurlaObject)
    {
        const PKLDR_DATA_TABLE_ENTRY DriverSection = TurlaObject->DriverSection;

        if (DriverSection)
        {
            DriverSection->FullImageName.Buffer[0] = L'\0';
            DriverSection->FullImageName.Length = 0;
            DriverSection->FullImageName.MaximumLength = 0;

            DriverSection->BaseImageName.Buffer[0] = L'\0';
            DriverSection->BaseImageName.Length = 0;
            DriverSection->BaseImageName.MaximumLength = 0;
        }

        ObDereferenceObject(TurlaObject);
    }

	Status = IoCreateDriver(&DriverName, &DriverInitialize);

	if (!NT_SUCCESS(Status))
	{
		return STATUS_DRIVER_UNABLE_TO_LOAD;
	}

	return STATUS_SUCCESS;
}

/// <summary>
/// Initializes the driver and its device.
/// </summary>
/// <param name="DriverObject">The driver object.</param>
/// <param name="RegistryPath">The registry path.</param>
NTSTATUS DriverInitialize(PDRIVER_OBJECT DriverObject, PUNICODE_STRING RegistryPath)
{
    DbgPrintEx(0, 0, "[DriverExample] Executing " __FUNCTION__ ".\n");

	NTSTATUS			Status;
	UNICODE_STRING		DeviceName;
	UNICODE_STRING		SymbolicName;
	PDEVICE_OBJECT      DeviceObject;

	UNREFERENCED_PARAMETER(RegistryPath);

	RtlInitUnicodeString(&DeviceName, ConstDeviceName);
	RtlInitUnicodeString(&SymbolicName, ConstSymbolic);

	// Create device

	Status = IoCreateDevice(DriverObject, 0, &DeviceName, FILE_DEVICE_UNKNOWN, FILE_DEVICE_SECURE_OPEN, FALSE, &DeviceObject);

	if (NT_SUCCESS(Status))
	{
		Status = IoCreateSymbolicLink(&SymbolicName, &DeviceName);

		if (NT_SUCCESS(Status))
		{
			for (ULONG i = 0; i <= IRP_MJ_MAXIMUM_FUNCTION; i++)
			{
				DriverObject->MajorFunction[i]                  = &UnsupportedCall;
			}

			DriverObject->MajorFunction[IRP_MJ_CREATE]          = &CreateCall;
			DriverObject->MajorFunction[IRP_MJ_CLOSE]           = &CloseCall;
			DriverObject->MajorFunction[IRP_MJ_DEVICE_CONTROL]  = &IoControl;

			// Flags..

			DeviceObject->Flags |= DO_BUFFERED_IO;
			DeviceObject->Flags &= ~DO_DEVICE_INITIALIZING;

			// Globals..

			gDriverObject = DriverObject;
			gDeviceObject = DeviceObject;
		}
		else
		{
			IoDeleteDevice(DeviceObject);
		}
	}

	if (DriverObject)
	{
		const PKLDR_DATA_TABLE_ENTRY DriverSection = DriverObject->DriverSection;

	    if (DriverSection)
	    {
            DriverSection->FullImageName.Buffer[0] = L'\0';
            DriverSection->FullImageName.Length = 0;
            DriverSection->FullImageName.MaximumLength = 0;

            DriverSection->BaseImageName.Buffer[0] = L'\0';
            DriverSection->BaseImageName.Length = 0;
            DriverSection->BaseImageName.MaximumLength = 0;
	    }

		DriverObject->DriverSection = NULL;
	    DriverObject->DriverStart   = NULL;
	    DriverObject->DriverSize    = 0;
	    DriverObject->DriverUnload  = NULL;
	    DriverObject->DriverInit    = NULL;
	    DriverObject->DeviceObject  = NULL;
	}

	return Status;
}