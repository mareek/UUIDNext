using System;

namespace UUIDNext.Generator
{
    public abstract class UuidGeneratorBase
    {
        public abstract byte Version { get; }

        protected Guid CreateGuidFromBytes(Span<byte> bytes)
        {
            SetVersion(bytes);
            SetVariant(bytes);
            return new Guid(bytes);
        }

        private void SetVariant(Span<byte> bytes)
        {
            const int variantByte = 8;
            bytes[variantByte] |= 0b1000_0000;
            bytes[variantByte] &= 0b1011_1111;
        }

        private void SetVersion(Span<byte> bytes)
        {
            int versionByte = BitConverter.IsLittleEndian ? 7 : 6;
            byte previousValues = bytes[versionByte];
            int partToKeep = previousValues % 16;
            int versionPart = Version * 16;
            byte newValue = (byte)(versionPart + partToKeep);
            bytes[versionByte] = newValue;
        }
    }
}