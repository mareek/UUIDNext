using System;
using UUIDNext.Generator;

namespace UUIDNext
{
    public static class Uuid
    {
        private static readonly UuidV4Generator _v4Generator = new();
        private static readonly UuidV5Generator _v5Generator = new();
        private static readonly UuidV7Generator _v7Generator = new();

        public static Guid NewV4() => _v4Generator.New();
        public static Guid NewV5(Guid namespaceId, string name) => _v5Generator.New(namespaceId, name);
        public static Guid NewV6() => throw new NotImplementedException();
        public static Guid NewV7() => _v7Generator.New();
    }
}
