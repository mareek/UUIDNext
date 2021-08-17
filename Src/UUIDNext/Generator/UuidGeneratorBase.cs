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
            return new Guid(bytes.AsSpan(0..16));
        }

        private void SetVariant(byte[] bytes)
        {
            const int variantByte = 8;
            bytes[variantByte] |= 0b1000_0000;
            bytes[variantByte] &= 0b1011_1111;
        }

        private void SetVersion(byte[] bytes)
        {
            const int versionByte = 7;
            byte previousValues = bytes[versionByte];
            int partToKeep = previousValues % 16;
            int versionPart = Version * 16;
            byte newValue = (byte)(versionPart + partToKeep);
            bytes[versionByte] = newValue;
        }
    }
}