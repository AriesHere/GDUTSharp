namespace GDUTSharp.Interfaces;

public interface ISecurityService
{
    public byte[] GenIV();

    public string CbcEncrypt(string plaintext, byte[] key, byte[] iv);

    public string CbcDecrypt(string cipherText, byte[] key, byte[] iv);
}
