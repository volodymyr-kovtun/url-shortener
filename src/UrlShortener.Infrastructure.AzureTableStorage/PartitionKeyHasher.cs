using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;

[assembly: InternalsVisibleTo("UrlShortener.Infrastructure.AzureTableStorage.Tests")]

namespace UrlShortener.Infrastructure.AzureTableStorage;

internal static class PartitionKeyHasher
{
    public static string Hash(string shortUrl)
    {
        Span<byte> hash = stackalloc byte[SHA256.HashSizeInBytes];
        SHA256.HashData(Encoding.UTF8.GetBytes(shortUrl), hash);
        return Convert.ToHexStringLower(hash[..2]);
    }
}
