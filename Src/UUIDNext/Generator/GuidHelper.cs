using System;

namespace UUIDNext.Generator
{
    public static class GuidHelper
    {
        public static Guid FromBigEndianBytes(Span<byte> bytes)
        {
            SwitchByteOrderIfNeeded(bytes);
            return new Guid(bytes);
        }

        public static bool TryWriteBigEndianBytes(Guid guid, Span<byte> bytes)
        {
            if (bytes.Length < 16 || !guid.TryWriteBytes(bytes))
            {
                return false;
            }

            SwitchByteOrderIfNeeded(bytes);
            return true;
        }

        private static void SwitchByteOrderIfNeeded(Span<byte> bigEndianBytes)
        {
            if (!BitConverter.IsLittleEndian)
            {
                // On Big Endian architecture everything is in network byte order so we don't need to switch
                return;
            }

            Permut(bigEndianBytes, 0, 3);
            Permut(bigEndianBytes, 1, 2);

            Permut(bigEndianBytes, 5, 4);

            Permut(bigEndianBytes, 6, 7);

            static void Permut(Span<byte> array, int indexSource, int indexDest)
            {
                (array[indexSource], array[indexDest]) = (array[indexDest], array[indexSource]);
            }
        }
    }
}
