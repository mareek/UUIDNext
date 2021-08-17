using System.Security.Cryptography;

namespace UUIDNext.Generator
{
    public class UuidV5Generator : UuidNameGeneratorBase
    {
        protected override HashAlgorithm HashAlgorithm { get; } = SHA1.Create();

        public override byte Version => 5;
    }
}
