using System;

namespace UUIDNext.Generator
{
    public static class GuidHelper
    {
        public static Guid FromBigEndianBytes(Span<byte> bytes)
        {
            SwitchByteOrder(bytes);
            return new Guid(bytes);
        }

        public static bool TryWriteBigEndianBytes(Guid guid, Span<byte> bytes)
        {
            if (bytes.Length < 16 || !guid.TryWriteBytes(bytes))
            {
                return false;
            }

            SwitchByteOrder(bytes);
            return true;
        }

        private static void SwitchByteOrder(Span<byte> bigEndianBytes)
        {
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
