namespace GDUTSharp.Interfaces;

public interface ISecurityService
{
    public byte[] Encrypt(byte[] plaintext, byte[] key, byte[] iv);

    public byte[] Decrypt(byte[] cipherText, byte[] key, byte[] iv);
}
