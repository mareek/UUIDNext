using System;

namespace UUIDNext.Generator
{
    public abstract class UuidGeneratorBase
    {
        protected abstract byte Version { get; }

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
            byte previousValue = bytes[versionByte];
            int partToKeep = previousValue % 16;
            int versionPart = Version << 4;
            byte newValue = (byte)(versionPart | partToKeep);
            bytes[versionByte] = newValue;
        }

        protected static void SwitchByteOrderIfNeeded(Span<byte> guidByteArray)
        {
            if (!BitConverter.IsLittleEndian)
            {
                // On Big Endian architecture everything is in network byte order so we don't need to switch
                return;
            }

            Permut(guidByteArray, 0, 3);
            Permut(guidByteArray, 1, 2);

            Permut(guidByteArray, 5, 4);
            
            Permut(guidByteArray, 6, 7);

            static void Permut(Span<byte> array, int indexSource, int indexDest)
            {
                var temp = array[indexDest];
                array[indexDest] = array[indexSource];
                array[indexSource] = temp;
            }
        }
    }
}