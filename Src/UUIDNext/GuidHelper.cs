using System;

namespace UUIDNext
{
    public static class GuidHelper
    {
        public static Guid FromBytes(Span<byte> bytes, bool bigEndian)
        {
            if (!bigEndian)
                return new(bytes);

            Span<byte> localBytes = stackalloc byte[bytes.Length];
            
            bytes.CopyTo(localBytes);
            SwitchByteOrder(localBytes);
            return new(localBytes);
        }

        public static byte[] ToByteArray(this Guid guid, bool bigEndian)
        {
            if (!bigEndian)
                return guid.ToByteArray();

            byte[] result = new byte[16];
            guid.TryWriteBytes(result, true, out var _);
            return result;
        }

        public static bool TryWriteBytes(this Guid guid, Span<byte> bytes, bool bigEndian, out int bytesWritten)
        {
            if (bytes.Length < 16 || !guid.TryWriteBytes(bytes))
            {
                bytesWritten = 0;
                return false;
            }

            if (bigEndian)
                SwitchByteOrder(bytes);

            bytesWritten = 16;
            return true;
        }

        private static void SwitchByteOrder(Span<byte> bytes)
        {
            Permut(bytes, 0, 3);
            Permut(bytes, 1, 2);

            Permut(bytes, 5, 4);

            Permut(bytes, 6, 7);

            static void Permut(Span<byte> array, int indexSource, int indexDest)
            {
                (array[indexSource], array[indexDest]) = (array[indexDest], array[indexSource]);
            }
        }
    }
}
