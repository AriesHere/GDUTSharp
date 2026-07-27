namespace GDUTSharp.Interfaces;

public interface ISecurityService
{
    public string CbcEncrypt(string plaintext, string key);

    public string CbcDecrypt(string cipherText, string key);
}
