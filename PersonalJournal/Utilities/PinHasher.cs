namespace PersonalJournal.Utilities;
using System.Security.Cryptography;

public class PinHasher
{
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int Iterations = 100000;

    public static (string hash, string salt) HashPin(string pin)
    {
        byte[] saltBytes = new byte[SaltSize];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(saltBytes);
        }

        using var pbkdf2 = new Rfc2898DeriveBytes(pin, saltBytes, Iterations, HashAlgorithmName.SHA256);
        byte[] hash = pbkdf2.GetBytes(HashSize);

        return (Convert.ToBase64String(hash), Convert.ToBase64String(saltBytes));
    }

    public static bool VerifyPin(string pin, string storedHash, string storedSalt)
    {
        try
        {
            byte[] saltBytes = Convert.FromBase64String(storedSalt);
            using var pbkdf2 = new Rfc2898DeriveBytes(pin, saltBytes, Iterations, HashAlgorithmName.SHA256);
            byte[] hashToVerify = pbkdf2.GetBytes(HashSize);
            byte[] storedHashBytes = Convert.FromBase64String(storedHash);

            return CryptographicOperations.FixedTimeEquals(hashToVerify, storedHashBytes);
        }
        catch
        {
            return false;
        }
    } 
}