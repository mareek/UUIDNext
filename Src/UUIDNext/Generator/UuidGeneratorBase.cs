using System;

namespace UUIDNext.Generator
{
    internal abstract class UuidGeneratorBase
    {
        protected abstract byte Version { get; }

        protected Guid CreateGuidFromBigEndianBytes(Span<byte> bigEndianBytes)
        {
            SetVersion(bigEndianBytes);
            SetVariant(bigEndianBytes);
            return GuidHelper.FromBigEndianBytes(bigEndianBytes);
        }

        private void SetVersion(Span<byte> bigEndianBytes)
        {
            const int versionByte = 6;
            //Erase upper 4 bits
            bigEndianBytes[versionByte] &= 0b0000_1111;
            //Set 4 upper bits to version
            bigEndianBytes[versionByte] |= (byte)(Version << 4);
        }

        private void SetVariant(Span<byte> bigEndianBytes)
        {
            const int variantByte = 8;
            //Erase upper 2 bits
            bigEndianBytes[variantByte] &= 0b0011_1111;
            //Set 2 upper bits to variant
            bigEndianBytes[variantByte] |= 0b1000_0000;
        }
    }
}