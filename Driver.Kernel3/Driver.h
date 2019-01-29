#pragma once

#include <ntddk.h>
#include "WinOS.h"
#include "Utils.h"

/// <summary>
/// Defines the entry point of the driver.
/// </summary>
/// <param name="DriverObject">The driver object.</param>
/// <param name="RegistryPath">The registry path.</param>
NTSTATUS DriverEntry(PDRIVER_OBJECT DriverObject, PUNICODE_STRING RegistryPath);

/// <summary>
/// Initializes the communication system.
/// </summary>
NTSTATUS InitializeCommunication();