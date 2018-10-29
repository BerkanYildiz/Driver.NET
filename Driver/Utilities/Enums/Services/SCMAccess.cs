namespace WeGotSkills.Driver.Loader.Enums.Services
{
    using System;

    [Flags]
    internal enum ScmAccess : uint
    {
        StandardRightsRequired      = 0xF0000,
        ScManagerConnect            = 0x00001,
        ScManagerCreateService      = 0x00002,
        ScManagerEnumerateService   = 0x00004,
        ScManagerLock               = 0x00008,
        ScManagerQueryLockStatus    = 0x00010,
        ScManagerModifyBootConfig   = 0x00020,

        ScManagerAllAccess          = ScmAccess.StandardRightsRequired    |
                                      ScmAccess.ScManagerConnect          |
                                      ScmAccess.ScManagerCreateService    |
                                      ScmAccess.ScManagerEnumerateService |
                                      ScmAccess.ScManagerLock             |
                                      ScmAccess.ScManagerQueryLockStatus  |
                                      ScmAccess.ScManagerModifyBootConfig
    }
}
