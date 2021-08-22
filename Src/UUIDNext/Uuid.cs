using System;
using System.Runtime.CompilerServices;
using UUIDNext.Generator;

[assembly: InternalsVisibleTo("UUIDNext.Test")]

namespace UUIDNext
{
    public static class Uuid
    {
        private static readonly UuidV4Generator _v4Generator = new();
        private static readonly UuidV5Generator _v5Generator = new();
        private static readonly UuidV7Generator _v7Generator = new();

        public static Guid NewRandom() => _v4Generator.New();
        public static Guid NewNameBased(Guid namespaceId, string name) => _v5Generator.New(namespaceId, name);
        public static Guid NewSequential() => _v7Generator.New();
    }
}
