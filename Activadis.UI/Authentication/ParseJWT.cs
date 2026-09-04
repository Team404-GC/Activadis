using System.Security.Claims;
using System.Text.Json;

namespace Activadis.UI.Authentication
{
    public static class ParseJWT
    {
        public static List<Claim> GetClaims(string token)
        {
            List<Claim> claims = new List<Claim>();
            Dictionary<string, JsonElement>? dictionary = token
                .GetPayload()
                .ToValidPayload()
                .ToBase64()
                .ToBytesOfJSON()
                .ToDictionaryOfJSON();

            if (dictionary is null)
                return claims;

            foreach (KeyValuePair<string, JsonElement> pair in dictionary)
                claims.AddPair(pair);

            return claims;
        }

        private static string? GetPayload(this string token)
        {
            if (token is null)
                return null;

            List<string> jwt = token.Split('.').ToList();
            if (jwt.Count != 3)
                return null;

            return jwt[1];
        }

        private static string? ToValidPayload(this string? payload)
        {
            if (payload is null)
                return null;

            if (payload.Length % 4 == 0)
                return payload;

            int amount = 4 - payload.Length % 4;
            return payload + new string('=', amount);
        }

        private static string? ToBase64(this string? payload)
        {
            if (payload is null)
                return null;

            return payload.Replace('-', '+').Replace('_', '/');
        }

        private static byte[] ToBytesOfJSON(this string? base64)
        {
            if (base64 is null)
                return [];

            try
            {
                return Convert.FromBase64String(base64);
            }
            catch
            {
                return [];
            }
        }

        private static Dictionary<string, JsonElement>? ToDictionaryOfJSON(this byte[] bytes)
        {
            if (bytes.Length <= 0)
                return null;

            return JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(bytes);
        }

        private static List<Claim> AddPair(this List<Claim> claims, KeyValuePair<string, JsonElement> pair)
        {
            if (pair.Value.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in pair.Value.EnumerateArray())
                {
                    claims.Add(new Claim(pair.Key, item.GetValue()));
                }
            }
            else if (pair.Value.ValueKind != JsonValueKind.Null)
            {
                claims.Add(new Claim(pair.Key, pair.Value.GetValue()));
            }

            return claims;
        }

        private static string GetValue(this JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.String)
                return element.GetString() ?? string.Empty;

            return element.ToString();
        }
    }
}
