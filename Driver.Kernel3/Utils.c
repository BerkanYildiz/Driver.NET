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

    return Status;
}

/// <summary>
/// Hides the capcom driver
/// </summary>
NTSTATUS HideCapcom()
{
    NTSTATUS        Status = STATUS_SUCCESS;
    UNICODE_STRING	CapcomName;
    PDRIVER_OBJECT  Capcom;

    RtlInitUnicodeString(&CapcomName, L"\\Driver\\Capcom");

    ObReferenceObjectByName(&CapcomName, OBJ_CASE_INSENSITIVE, NULL, 0, *IoDriverObjectType, KernelMode, NULL, (PVOID) &Capcom);

    if (Capcom)
    {
        const PKLDR_DATA_TABLE_ENTRY DriverSection = Capcom->DriverSection;

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

        ObDereferenceObject(Capcom);
    }
    else
    {
        Status = STATUS_NOT_FOUND;
    }

    return Status;
}