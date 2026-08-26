namespace TmsApi.Infrastructure.Services;

public class CryptoDemoService
{
    public string HashUserPassword(string plainText)
    {
        // BCrypt automatically generates a unique salt
        // and stores it as part of the resulting hash.
        return BCrypt.Net.BCrypt.HashPassword(
            plainText,
            workFactor: 12);
    }

    public bool VerifyUserPassword(
        string plainText,
        string hashedDbPassword)
    {
        return BCrypt.Net.BCrypt.Verify(
            plainText,
            hashedDbPassword);
    }
}