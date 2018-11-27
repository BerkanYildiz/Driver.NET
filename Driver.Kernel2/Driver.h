#pragma once

#ifndef _DRIVER_H
#define _DRIVER_H

// Defines..

#define ConstName L"DriverExample2"

#define ConstDriverName (L"\\Driver\\" ConstName)
#define ConstDeviceName (L"\\Device\\" ConstName)
#define ConstSymbolic (L"\\DosDevices\\" ConstName)
#define ConstTurlaDriverName L"\\Driver\\VBoxDrv"

// Includes..

#include <ntdef.h>
#include <ntddk.h>

#include "phnt.h"
#include "ntos.h"

#include "Requests.h"
#include "Utils.h"

// Globals..

PDRIVER_OBJECT gDriverObject;
PDEVICE_OBJECT gDeviceObject;

// Functions..

NTSTATUS DriverEntry(PDRIVER_OBJECT DriverObject, PUNICODE_STRING RegistryPath);
NTSTATUS DriverEntryInternal(PDRIVER_OBJECT DriverObject, PUNICODE_STRING RegistryPath);
NTSTATUS DriverInitialize(PDRIVER_OBJECT DriverObject, PUNICODE_STRING RegistryPath);
NTSTATUS UnloadDriver(const PDRIVER_OBJECT DriverObject);

#endif