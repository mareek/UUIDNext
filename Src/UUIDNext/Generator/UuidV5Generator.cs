using System.Security.Cryptography;
using UUIDNext.Tools;

namespace UUIDNext.Generator;

/// <summary>
/// Generate a UUID version 5 based on RFC 9562
/// </summary>
internal class UuidV5Generator
{
    private readonly ThreadLocal<HashAlgorithm> _sha1HashAlgorithm = new(SHA1.Create);

    public Guid New(Guid namespaceId, string name)
        => UuidToolkit.CreateUuidFromName(namespaceId, name, _sha1HashAlgorithm.Value, 5);

}
