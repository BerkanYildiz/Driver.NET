﻿namespace Driver.Native.Enums.Memory
{
    public enum MemoryPagePermissions : uint 
    { 
        PAGE_EXECUTE = 0x00000010, 
        PAGE_EXECUTE_READ = 0x00000020, 
        PAGE_EXECUTE_READWRITE = 0x00000040, 
        PAGE_EXECUTE_WRITECOPY = 0x00000080, 
        PAGE_NOACCESS = 0x00000001, 
        PAGE_READONLY = 0x00000002, 
        PAGE_READWRITE = 0x00000004, 
        PAGE_WRITECOPY = 0x00000008, 
        PAGE_GUARD = 0x00000100, 
        PAGE_NOCACHE = 0x00000200, 
        PAGE_WRITECOMBINE = 0x00000400 
    }
}