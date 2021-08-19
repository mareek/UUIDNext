using System;
using System.Security.Cryptography;
using System.Threading;

namespace UUIDNext.Generator
{
    /// <summary>
    /// Generate a UUID version 3 based on RFC 4122
    /// </summary>
    [Obsolete("UUID version 3 should only be used for backward compatibility. You should use version 5 instead.")]
    public class UuidV3Generator : UuidNameGeneratorBase
    {
        protected override ThreadLocal<HashAlgorithm> HashAlgorithm { get; } = new(MD5.Create);

        protected override byte Version => 3;
    }
}
