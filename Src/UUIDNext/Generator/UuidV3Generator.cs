using System.Security.Cryptography;

namespace UUIDNext.Generator
{
    public class UuidV3Generator : UuidNameGeneratorBase
    {
        protected override HashAlgorithm HashAlgorithm { get; } = MD5.Create();

        public override byte Version => 3;
    }
}
