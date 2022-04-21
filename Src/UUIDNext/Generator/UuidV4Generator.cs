using System;
using System.Security.Cryptography;

namespace UUIDNext.Generator
{
    /// <summary>
    /// Generate a UUID version 4 based on RFC 4122
    /// </summary>
    public class UuidV4Generator : UuidGeneratorBase
    {
        protected override byte Version => 4;

        public Guid New()
        {
            Span<byte> bytes = stackalloc byte[16];
            RandomNumberGenerator.Fill(bytes);
            return CreateGuidFromBigEndianBytes(bytes);
        }
    }
}
