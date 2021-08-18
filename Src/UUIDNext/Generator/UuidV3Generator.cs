using System.Security.Cryptography;
using System.Threading;

namespace UUIDNext.Generator
{
    public class UuidV3Generator : UuidNameGeneratorBase
    {
        protected override ThreadLocal<HashAlgorithm> HashAlgorithm { get; } = new(MD5.Create);

        public override byte Version => 3;
    }
}
