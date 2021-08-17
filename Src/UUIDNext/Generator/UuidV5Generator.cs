using System;
using System.Security.Cryptography;
using System.Text;

namespace UUIDNext.Generator
{
    public class UuidV5Generator : UuidGeneratorBase
    {
        private readonly HashAlgorithm _hashAlgorithm = SHA1.Create();

        public override byte Version => 5;

        public Guid New(Guid namespaceId, string name)
        {
            var bytes = GetUuidBytes(namespaceId, name);
            return CreateGuidFromBytes(bytes);
        }

        private byte[] GetUuidBytes(Guid namespaceId, string name)
        {
            //Convert the name to a canonical sequence of octets (as defined by the standards or conventions of its name space);
            byte[] utf8NameBytes = Encoding.UTF8.GetBytes(name); ;
            //put the name space ID in network byte order.
            byte[] namespaceBytes = namespaceId.ToByteArray();
            SwitchByteOrderIfNeeded(namespaceBytes);

            //Compute the hash of the name space ID concatenated with the name.
            byte[] bytesToHash = new byte[namespaceBytes.Length + utf8NameBytes.Length];
            Buffer.BlockCopy(namespaceBytes, 0, bytesToHash, 0, namespaceBytes.Length);
            Buffer.BlockCopy(utf8NameBytes, 0, bytesToHash, namespaceBytes.Length, utf8NameBytes.Length);

            var hash = _hashAlgorithm.ComputeHash(bytesToHash);
            SwitchByteOrderIfNeeded(hash);
            return hash;
        }

        private static void SwitchByteOrderIfNeeded(byte[] guidByteArray)
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

            static void Permut<T>(T[] array, int indexSource, int indexDest)
            {
                T temp = array[indexDest];
                array[indexDest] = array[indexSource];
                array[indexSource] = temp;
            }
        }
    }
}
