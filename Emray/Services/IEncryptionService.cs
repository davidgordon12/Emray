namespace Emray.Services
{
    interface IEncryptionService
    {
        string Encrypt(string FilePath, string OutputPath);

        string Decrypt(string FilePath, string OutputPath);
    }
}
