#pragma once

#ifndef _DRIVER_H
#define _DRIVER_H

// Includes..

#include <ntdef.h>
#include <ntddk.h>

#include "phnt.h"
#include "ntos.h"

#include "Requests.h"
#include "Utils.h"

// Defines..

#define ConstName L"DriverExample2"

#define ConstDriverName (L"\\Driver\\" ConstName)
#define ConstDeviceName (L"\\Device\\" ConstName)
#define ConstSymbolic (L"\\DosDevices\\" ConstName)

// Globals..

PDRIVER_OBJECT          gDriverObject;
PDEVICE_OBJECT          gDeviceObject;
HOOK_DRIVER_CONTEXT     gHookCtx;

// Functions..

NTSTATUS DriverEntry(PDRIVER_OBJECT _DriverObject, PUNICODE_STRING _RegistryPath);

#endif