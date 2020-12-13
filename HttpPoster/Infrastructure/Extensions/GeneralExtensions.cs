using System.Collections.Generic;
using System.Text.Json;

namespace HttpPoster.Infrastructure.Extensions
{
	public static class GeneralExtensions
	{
		public static Dictionary<string, dynamic> ToJson(this string json)
		{
			return JsonSerializer.Deserialize<Dictionary<string, dynamic>>(json);
		}
	}
}
