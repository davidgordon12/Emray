namespace Emray.Services
{
    interface IEncryptionService
    {
        void Encrypt(string FilePath, string OutputPath);

        void Decrypt(string FilePath, string OutputPath);
    }
}
