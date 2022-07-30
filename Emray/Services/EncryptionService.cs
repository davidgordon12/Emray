using System.Security.Cryptography;

namespace Emray.Services
{
    internal class EncryptionService : IEncryptionService
    {
        CspParameters _cspp { get; }
        RSACryptoServiceProvider _rsa { get; set; }

        public EncryptionService()
        {
#pragma warning disable CA1416
            _cspp = new CspParameters();
            _cspp.KeyContainerName = "Key01";
            _rsa = new RSACryptoServiceProvider(_cspp)
            {
                PersistKeyInCsp = true
            };
        }

        bool IEncryptionService.Encrypt(string FilePath, string OutputPath)
        {
            if (_rsa == null)
                return false;

            string file = FilePath;

            Aes aes = Aes.Create();
            ICryptoTransform transform = aes.CreateEncryptor();

            byte[] keyEncrypted = _rsa.Encrypt(aes.Key, false);

            int lKey = keyEncrypted.Length;
            byte[] LenK = BitConverter.GetBytes(lKey);
            int lIV = aes.IV.Length;
            byte[] LenIV = BitConverter.GetBytes(lIV);

            string outFile = Path.ChangeExtension(file, ".enc");

            using (var outFs = new FileStream(outFile, FileMode.Create))
            {
                outFs.Write(LenK, 0, 4);
                outFs.Write(LenIV, 0, 4);
                outFs.Write(keyEncrypted, 0, lKey);
                outFs.Write(aes.IV, 0, lIV);

                using (var outStreamEncrypted =
                    new CryptoStream(outFs, transform, CryptoStreamMode.Write))
                {
                    int count = 0;
                    int offset = 0;

                    int blockSizeBytes = aes.BlockSize / 8;
                    byte[] data = new byte[blockSizeBytes];
                    int bytesRead = 0;

                    using (var inFs = new FileStream(FilePath, FileMode.Open))
                    {
                        do
                        {
                            count = inFs.Read(data, 0, blockSizeBytes);
                            offset += count;
                            outStreamEncrypted.Write(data, 0, count);
                            bytesRead += blockSizeBytes;
                        } while (count > 0);
                    }
                    outStreamEncrypted.FlushFinalBlock();
                    return true;
                }
            }
        }

        bool IEncryptionService.Decrypt(string FilePath, string OutputPath)
        {
            if (_rsa == null)
                return false;

            Aes aes = Aes.Create();

            byte[] LenK = new byte[4];
            byte[] LenIV = new byte[4];

            string outFile =
                Path.ChangeExtension(FilePath.Replace("Encrypt", "Decrypt"), ".txt");

            using (var inFs = new FileStream(FilePath, FileMode.Open))
            {
                inFs.Seek(0, SeekOrigin.Begin);
                inFs.Read(LenK, 0, 3);
                inFs.Seek(4, SeekOrigin.Begin);
                inFs.Read(LenIV, 0, 3);

                int lenK = BitConverter.ToInt32(LenK, 0);
                int lenIV = BitConverter.ToInt32(LenIV, 0);

                int startC = lenK + lenIV + 8;
                int lenC = (int)inFs.Length - startC;

                byte[] KeyEncrypted = new byte[lenK];
                byte[] IV = new byte[lenIV];

                inFs.Seek(8, SeekOrigin.Begin);
                inFs.Read(KeyEncrypted, 0, lenK);
                inFs.Seek(8 + lenK, SeekOrigin.Begin);
                inFs.Read(IV, 0, lenIV);

                Directory.CreateDirectory(OutputPath);
                byte[] KeyDecrypted = _rsa.Decrypt(KeyEncrypted, false);

                ICryptoTransform transform = aes.CreateDecryptor(KeyDecrypted, IV);

                using (var outFs = new FileStream(outFile, FileMode.Create))
                {
                    int count = 0;
                    int offset = 0;

                    int blockSizeBytes = aes.BlockSize / 8;
                    byte[] data = new byte[blockSizeBytes];

                    inFs.Seek(startC, SeekOrigin.Begin);
                    using (var outStreamDecrypted =
                        new CryptoStream(outFs, transform, CryptoStreamMode.Write))
                    {
                        do
                        {
                            count = inFs.Read(data, 0, blockSizeBytes);
                            offset += count;
                            outStreamDecrypted.Write(data, 0, count);
                        } while (count > 0);

                        outStreamDecrypted.FlushFinalBlock();
                    }
                }
                return true;
            }
        }
    }
}