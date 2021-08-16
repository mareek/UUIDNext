using System;
using System.Buffers;
using System.Security.Cryptography;

namespace UUIDNext.Generator
{
    public class UuidV4Generator : UuidGeneratorBase
    {
        private readonly RandomNumberGenerator _rng = RandomNumberGenerator.Create();
        
        public override byte Version => 4;

        public Guid New()
        {
            var bytes = ArrayPool<byte>.Shared.Rent(16);
            _rng.GetBytes(bytes);
            var result = CreateGuidFromBytes(bytes);
            ArrayPool<byte>.Shared.Return(bytes);
            return result;
        }
    }
}
