using Newtonsoft.Json;
using AutodecisionCore.Extensions;
using System.Security.Cryptography;
using System.Text;

namespace AutodecisionCore.Utils
{
	public static class Token
	{
		public static string GenerateSystemToken()
		{
			var secret = Environment.GetEnvironmentVariable("ISSUER_SECRET_WEBPORTAL");
			string header = "{\"alg\":\"HS256\",\"typ\":\"JWT\"}";
			var user = new UserSystemJWT();
			var claims = JsonConvert.SerializeObject(user);

			string header_base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(header)).ToBase64URLEncoding();
			string claims_base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(claims)).ToBase64URLEncoding();

			string payload = header_base64 + "." + claims_base64;

			byte[] payload_bytes = Encoding.UTF8.GetBytes(payload);
			byte[] secret_bytes = Encoding.UTF8.GetBytes(secret);

			HMACSHA256 hash = new HMACSHA256(secret_bytes);
			byte[] calculated_signature_bytes = hash.ComputeHash(payload_bytes);

			string calculated_signature = Convert.ToBase64String(calculated_signature_bytes).ToBase64URLEncoding();

			string JWT = payload + "." + calculated_signature;

			return JWT;
		}

		public static string ToBase64URLEncoding(this string base64)
		{
			char padding = '=';
			return base64.TrimEnd(padding).Replace('+', '-').Replace('/', '_');
		}

		public static Dictionary<string, string> GetHeaders()
		{
			var jwt = GenerateSystemToken();
			var dict = new Dictionary<string, string>()
		{
			{ "Authorization", $"Bearer {jwt}" }
		};

			return dict;

		}

		public class UserSystemJWT
		{
			[JsonProperty("id")] public int Id { get; set; } = JWTUserId.System;
			[JsonProperty("name")] public string Name { get; set; } = JWTUserName.System;
			[JsonProperty("iss")] public string Iss { get; set; } = JWTIssuer.Webportal;
			[JsonProperty("exp")] public long Expiration { get; set; } = DateTimeToUnixTimestamp(DateTime.Now.AddMinutes(20));
		}

		public static long DateTimeToUnixTimestamp(DateTime dateTime)
		{
			DateTime unixStart = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

			long unixTimeStampInTicks = (dateTime.ToUniversalTime() - unixStart).Ticks;
			return unixTimeStampInTicks / TimeSpan.TicksPerSecond;
		}
	}
}
