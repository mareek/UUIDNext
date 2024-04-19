using System.Security.Cryptography;
using System.Threading;

namespace UUIDNext.Generator
{
    /// <summary>
    /// Generate a UUID version 5 based on RFC 9562
    /// </summary>
    internal class UuidV5Generator : UuidNameGeneratorBase
    {
        protected override ThreadLocal<HashAlgorithm> HashAlgorithm { get; } = new(SHA1.Create);

        protected override byte Version => 5;
    }
}
