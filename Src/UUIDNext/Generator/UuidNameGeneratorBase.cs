﻿using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace UUIDNext.Generator
{
    internal abstract class UuidNameGeneratorBase : UuidGeneratorBase
    {
        protected abstract ThreadLocal<HashAlgorithm> HashAlgorithm { get; }

        public Guid New(Guid namespaceId, string name)
        {
            //Convert the name to a canonical sequence of octets (as defined by the standards or conventions of its name space);
            var utf8NameByteCount = Encoding.UTF8.GetByteCount(name.Normalize(NormalizationForm.FormC));
#if NET472_OR_GREATER
            byte[] utf8NameBytes = new byte[utf8NameByteCount];
            Encoding.UTF8.GetBytes(name, 0, name.Length, utf8NameBytes, 0);

            //put the name space ID in network byte order.
            Span<byte> namespaceBytes = stackalloc byte[16];
            namespaceId.TryWriteBytes(namespaceBytes, bigEndian: true, out var _);

            //Compute the hash of the name space ID concatenated with the name.
            int bytesToHashCount = namespaceBytes.Length + utf8NameBytes.Length;
            byte[] bytesToHash = new byte[bytesToHashCount];
            namespaceBytes.CopyTo(bytesToHash);
            utf8NameBytes.CopyTo(bytesToHash, namespaceBytes.Length);

            HashAlgorithm hashAlgorithm = HashAlgorithm.Value;
            var hash = hashAlgorithm.ComputeHash(bytesToHash);

            return CreateGuidFromBigEndianBytes(hash.AsSpan(0 , 16));
#else
            Span<byte> utf8NameBytes = (utf8NameByteCount > 256) ? new byte[utf8NameByteCount] : stackalloc byte[utf8NameByteCount];
            Encoding.UTF8.GetBytes(name, utf8NameBytes);

            //put the name space ID in network byte order.
            Span<byte> namespaceBytes = stackalloc byte[16];
            namespaceId.TryWriteBytes(namespaceBytes, bigEndian: true, out var _);

            //Compute the hash of the name space ID concatenated with the name.
            int bytesToHashCount = namespaceBytes.Length + utf8NameBytes.Length;
            Span<byte> bytesToHash = (utf8NameByteCount > 256) ? new byte[bytesToHashCount] : stackalloc byte[bytesToHashCount];
            namespaceBytes.CopyTo(bytesToHash);
            utf8NameBytes.CopyTo(bytesToHash[namespaceBytes.Length..]);

            HashAlgorithm hashAlgorithm = HashAlgorithm.Value;
            Span<byte> hash = stackalloc byte[hashAlgorithm.HashSize / 8];
            hashAlgorithm.TryComputeHash(bytesToHash, hash, out var _);

            return CreateGuidFromBigEndianBytes(hash[0..16]);
#endif
        }
    }
}