using System;
using UUIDNext.Generator;

namespace UUIDNext
{
    public static class Uuid
    {
        private static readonly UuidV4Generator _v4Generator = new UuidV4Generator();

        public static Guid NewV4() => _v4Generator.New();
        public static Guid NewV6() => throw new NotImplementedException();
        public static Guid NewV7() => throw new NotImplementedException();
    }
}
