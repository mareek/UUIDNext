using System.Security.Cryptography;
using System.Threading;

namespace UUIDNext.Generator
{
    public class UuidV5Generator : UuidNameGeneratorBase
    {
        protected override ThreadLocal<HashAlgorithm> HashAlgorithm { get; } = new(SHA1.Create);

        public override byte Version => 5;
    }
}
