using System.Security.Cryptography;

namespace Emray.Services
{
    interface IEncryptionService
    {
        bool Encrypt(string FilePath, string OutputPath);
        bool Decrypt(string FilePath, string OutputPath);
    }
}
