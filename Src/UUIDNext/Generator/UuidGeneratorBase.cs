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
            //Erase upper 2 bits
            bytes[variantByte] &= 0b0011_1111;
            //Set 2 upper bits to variant
            bytes[variantByte] |= 0b1000_0000;
        }

        private void SetVersion(Span<byte> bytes)
        {
            int versionByte = BitConverter.IsLittleEndian ? 7 : 6;
            //Erase upper 4 bits
            bytes[versionByte] &= 0b0000_1111;
            //Set 4 upper bits to version
            bytes[versionByte] |= (byte)(Version << 4);
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