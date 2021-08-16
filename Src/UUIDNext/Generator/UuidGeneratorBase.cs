using System;

namespace UUIDNext.Generator
{
    public abstract class UuidGeneratorBase
    {
        public abstract byte Version { get; }

        protected Guid CreateGuidFromBytes(byte[] bytes)
        {
            SetVersion(bytes);
            SetVariant(bytes);
            return new Guid(bytes);
        }

        private void SetVariant(byte[] bytes)
        {
            bytes[8] |= 0b1000_0000;
            bytes[8] &= 0b1011_1111;
        }

        private void SetVersion(byte[] bytes) => bytes[6] = Version;
    }
}