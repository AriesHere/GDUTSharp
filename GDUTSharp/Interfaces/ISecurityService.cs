namespace GDUTSharp.Interfaces;

public interface ISecurityService
{
    public byte[] GenIV();

    public byte[] CbcEncrypt(byte[] plaintext, byte[] key, byte[] iv);

    public byte[] CbcDecrypt(byte[] cipherText, byte[] key, byte[] iv);
}
