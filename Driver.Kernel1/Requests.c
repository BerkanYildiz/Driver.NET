#include "Driver.h"

/// <summary>
/// Called when data has been received.
/// </summary>
/// <param name="DeviceObject">The device object.</param>
/// <param name="Irp">The irp.</param>
NTSTATUS IoControl(PDEVICE_OBJECT DeviceObject, PIRP Irp)
{
	NTSTATUS Status = STATUS_INVALID_PARAMETER;
	ULONG_PTR BytesIO = 0;
	
	const PIO_STACK_LOCATION Stack = IoGetCurrentIrpStackLocation(Irp);
	const ULONG ControlCode = Stack->Parameters.DeviceIoControl.IoControlCode;

	if (ControlCode == IO_EXAMPLE_REQUEST)
	{
        const PKERNEL_EXAMPLE_REQUEST Buffer = (PKERNEL_EXAMPLE_REQUEST) Irp->AssociatedIrp.SystemBuffer;

		if (Stack->Parameters.DeviceIoControl.InputBufferLength == 4)
		{
            DbgPrintEx(0, 0, "[DriverExample] Received IO request '%i' from user-mode.\n", Buffer->Value);
		}
        else
        {
            DbgPrintEx(0, 0, "[DriverExample] Received IO request with invalid length from user-mode.\n");
        }
	}
	else
	{
        DbgPrintEx(0, 0, "[DriverExample] Received IO request with invalid IOCTL from user-mode.\n");

		Status	= STATUS_INVALID_PARAMETER;
		BytesIO = 0;
	}

	Irp->IoStatus.Status = Status;
	Irp->IoStatus.Information = BytesIO;

	IoCompleteRequest(Irp, IO_NO_INCREMENT);

	return Irp->IoStatus.Status;
}

/// <summary>
/// Creates the call.
/// </summary>
/// <param name="DeviceObject">The device object.</param>
/// <param name="Irp">The irp.</param>
NTSTATUS CreateCall(PDEVICE_OBJECT DeviceObject, PIRP Irp)
{
	UNREFERENCED_PARAMETER(DeviceObject);

	Irp->IoStatus.Status = STATUS_SUCCESS;
	Irp->IoStatus.Information = 0;

	IoCompleteRequest(Irp, IO_NO_INCREMENT);

	return Irp->IoStatus.Status;
}

/// <summary>
/// Closes the call.
/// </summary>
/// <param name="DeviceObject">The device object.</param>
/// <param name="Irp">The irp.</param>
NTSTATUS CloseCall(PDEVICE_OBJECT DeviceObject, PIRP Irp)
{
	UNREFERENCED_PARAMETER(DeviceObject);

	Irp->IoStatus.Status = STATUS_SUCCESS;
	Irp->IoStatus.Information = 0;

	IoCompleteRequest(Irp, IO_NO_INCREMENT);

	return Irp->IoStatus.Status;
}

NTSTATUS UnsupportedCall(_In_ struct _DEVICE_OBJECT *DeviceObject, _Inout_ struct _IRP *IRP)
{
	UNREFERENCED_PARAMETER(DeviceObject);

	IRP->IoStatus.Status = STATUS_NOT_SUPPORTED;
	IRP->IoStatus.Information = 0;

	IoCompleteRequest(IRP, IO_NO_INCREMENT);

	return IRP->IoStatus.Status;
}