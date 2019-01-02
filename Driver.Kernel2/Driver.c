#include "Driver.h"

/// <summary>
/// Defines the entry point of this driver.
/// </summary>
/// <param name="DriverObject">The driver object.</param>
/// <param name="RegistryPath">The registry path.</param>
NTSTATUS DriverEntry(PDRIVER_OBJECT _DriverObject, PUNICODE_STRING _RegistryPath)
{
    UNREFERENCED_PARAMETER(_DriverObject);
    UNREFERENCED_PARAMETER(_RegistryPath);

    DbgPrintEx(0, 0, "[DriverExample] Executing " __FUNCTION__ ".\n");

	NTSTATUS			Status;
	UNICODE_STRING		DriverName;
    UNICODE_STRING		TurlaName;
    PDRIVER_OBJECT      Turla;
    PDRIVER_OBJECT		Driver;
    PDEVICE_OBJECT		Device;

    RtlInitUnicodeString(&TurlaName, L"\\Driver\\VBoxDrv");
    RtlInitUnicodeString(&DriverName, L"\\Driver\\BasicDisplay");

    ObReferenceObjectByName(&TurlaName,  OBJ_CASE_INSENSITIVE, NULL, 0, *IoDriverObjectType, KernelMode, NULL, (PVOID) &Turla);
    ObReferenceObjectByName(&DriverName, OBJ_CASE_INSENSITIVE, NULL, 0, *IoDriverObjectType, KernelMode, NULL, (PVOID) &Driver);

    // Remove Turla DLL file name..

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

        ObDereferenceObject(Turla);
    }

    // Hook the Driver..

    if (Driver)
    {
        // HookDriver("", &IoControl);
        DbgPrintEx(0, 0, "[DriverExample] Found the targeted driver.\n");

        if (Driver->DeviceObject == NULL)
        {
            DbgPrintEx(0, 0, "[DriverExample] We have to create a new device.\n");
        }
        else
        {
            Device = Driver->DeviceObject;

            if (Device->Type == 3)
            {
                OBJECT_NAME_INFORMATION DeviceInfo;
                ULONG                   DeviceInfoLength;

                DbgPrintEx(0, 0, "[DriverExample] The driver extension match the known one.\n");
                DbgPrintEx(0, 0, "[DriverExample] Device->ActiveThreadCount = %ld.\n", Device->ActiveThreadCount);

                Status = ObQueryNameString(&Device, &DeviceInfo, 0, &DeviceInfoLength);

                if (NT_SUCCESS(Status))
                {
                    DbgPrintEx(0, 0, "[DriverExample] Device->Name = %wZ.\n", &DeviceInfo.Name);
                }
            }
            else
            {
                DbgPrintEx(0, 0, "[DriverExample] The driver extension does not match the known one.\n");
            }
        }

        ObDereferenceObject(Driver);
    }
    else
    {
        DbgPrintEx(0, 0, "[DriverExample] Couldn't find the targeted driver.\n");
    }

    // End the suffering..

	if (!NT_SUCCESS(Status))
	{
		return STATUS_DRIVER_UNABLE_TO_LOAD;
	}

	return STATUS_SUCCESS;
}