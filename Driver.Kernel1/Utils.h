#pragma once

#ifndef _UTILS_H
#define _UTILS_H

/// <summary>
/// Gets the driver object.
/// </summary>
/// <param name="DriverName">Name of the driver.</param>
PDRIVER_OBJECT GetDriverObject(const PUNICODE_STRING DriverName);

/// <summary>
/// Deletes the driver information.
/// </summary>
/// <param name="DriverEntry">The driver entry.</param>
VOID DeleteDriverPath(const PKLDR_DATA_TABLE_ENTRY DriverEntry);

/// <summary>
/// Deletes the driver information.
/// </summary>
/// <param name="DriverObject">The driver object.</param>
VOID DeleteDriverInfo(const PDRIVER_OBJECT DriverObject);

/// <summary>
/// Checks if the specified process is terminating.
/// </summary>
/// <param name="Process">The process.</param>
BOOLEAN CheckProcessTermination(PEPROCESS Process);

/// <summary>
/// Allocate new Unicode string from Paged pool
/// </summary>
/// <param name="result">Resulting string</param>
/// <param name="size">Buffer size in bytes to alloacate</param>
/// <returns>Status code</returns>
NTSTATUS SafeAllocateString(OUT PUNICODE_STRING result, IN USHORT size);

/// <summary>
/// Allocate and copy string
/// </summary>
/// <param name="result">Resulting string</param>
/// <param name="source">Source string</param>
/// <returns>Status code</returns>
NTSTATUS SafeInitString(OUT PUNICODE_STRING result, IN PUNICODE_STRING source);

/// <summary>
/// Search for substring
/// </summary>
/// <param name="source">Source string</param>
/// <param name="target">Target string</param>
/// <param name="CaseInSensitive">Case insensitive search</param>
/// <returns>Found position or -1 if not found</returns>
LONG SafeSearchString(IN PUNICODE_STRING source, IN PUNICODE_STRING target, IN BOOLEAN CaseInSensitive);

/// <summary>
/// Get file name from full path
/// </summary>
/// <param name="path">Path.</param>
/// <param name="name">Resulting name</param>
/// <returns>Status code</returns>
NTSTATUS StripPath(IN PUNICODE_STRING path, OUT PUNICODE_STRING name);

/// <summary>
/// Get directory path name from full path
/// </summary>
/// <param name="path">Path</param>
/// <param name="name">Resulting directory path</param>
/// <returns>Status code</returns>
NTSTATUS StripFilename(IN PUNICODE_STRING path, OUT PUNICODE_STRING dir);

/// <summary>
/// Check if file exists
/// </summary>
/// <param name="path">Fully qualifid path to a file</param>
/// <returns>Status code</returns>
NTSTATUS FileExists(IN PUNICODE_STRING path);

/// <summary>
/// Get module base address by name
/// </summary>
/// <param name="pProcess">Target process</param>
/// <param name="ModuleName">Nodule name to search for</param>
/// <returns>Found address, NULL if not found</returns>
PVOID GetUserModule64(IN PEPROCESS pProcess, IN PUNICODE_STRING ModuleName);

#endif