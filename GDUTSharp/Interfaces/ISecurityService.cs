namespace GDUTSharp.Interfaces;

public interface ISecurityService
{
    public const int IV_LEN = 16;

    public byte[] GenIV();

    public byte[] AesCbcEncrypt(byte[] plaintext, byte[] key, byte[] iv);

    public byte[] AesCbcDecrypt(byte[] cipherText, byte[] key, byte[] iv);
}
