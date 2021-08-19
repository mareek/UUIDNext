using System;
using System.Security.Cryptography;

namespace UUIDNext.Generator
{
    public class UuidV4Generator : UuidGeneratorBase
    {
        private readonly RandomNumberGenerator _rng = RandomNumberGenerator.Create();

        public override byte Version => 4;

        public Guid New()
        {
            Span<byte> bytes = stackalloc byte[16];
            _rng.GetBytes(bytes);
            return CreateGuidFromBytes(bytes);
        }
    }
}
