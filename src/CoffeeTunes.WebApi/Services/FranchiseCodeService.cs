using System.Security.Cryptography;
using System.Text;

namespace CoffeeTunes.WebApi.Services;

public static class FranchiseCodeService
{
    public static string Generate(Guid groupId)
    {
        // Hash the GUID to spread entropy uniformly
        var bytes = Encoding.UTF8.GetBytes(groupId.ToString("N")); // 32 hex chars, no dashes
        using var sha = SHA256.Create();
        var hash = sha.ComputeHash(bytes);

        // Take first 6 bytes (48 bits), then mask down to 42 bits so it fits exactly into 8 base36 chars:
        // 36^8 ≈ 2.8e12 ≈ 2^41.4 -> 42 bits cover the range.
        ulong value = 0;
        for (int i = 0; i < 6; i++)
            value = (value << 8) | hash[i];

        value &= ((1UL << 42) - 1); // keep only 42 bits

        return ToBase36(value, 8);
    }

    // Encodes an unsigned value to base36 and pads to 'length' characters (uppercase).
    private static string ToBase36(ulong value, int length)
    {
        const string alphabet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        char[] result = new char[length];
        for (int i = length - 1; i >= 0; i--)
        {
            result[i] = alphabet[(int)(value % 36)];
            value /= 36;
        }
        return new string(result);
    }
}