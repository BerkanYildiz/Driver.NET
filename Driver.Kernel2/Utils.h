#pragma once

#ifndef _UTILS_H
#define _UTILS_H

/// <summary>
/// Gets the driver object.
/// </summary>
/// <param name="DriverName">Name of the driver.</param>
PDRIVER_OBJECT GetDriverObject(const PUNICODE_STRING DriverName);

/// <summary>
/// Hooks the specified driver.
/// </summary>
/// <param name="TargetedDeviceName">Name of the device.</param>
/// <param name="HookedDeviceIoControl">The hooked device io control.</param>
NTSTATUS HookDriver(IN PWCHAR TargetedDeviceName, PVOID HookedDeviceIoControl);

#endif