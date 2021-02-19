using Dapper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CryptoAnalyzer.Chan
{
	public class ThreadData
	{
		public HashSet<string> PossibleCoins = new HashSet<string>();
		public int Replies { get; set; }
		public DateTimeOffset LastUpdate { get; set; }

		public static async Task<Dictionary<int, ThreadData>> GetFromDB()
		{
			using(var conn = Context.OpenDatabaseConnection())
			{
				return JsonConvert.DeserializeObject <Dictionary<int, ThreadData>>( await conn.QuerySingleAsync<string>("SELECT Value FROM dbo.Content WHERE Name = 'BizThreadData'"));
			}
		}

		public static async Task CleanAndStore(Dictionary<int, ThreadData> threadData)
		{
			var cleanDic = threadData.Where(x => x.Value.LastUpdate > DateTimeOffset.UtcNow.AddDays(-1)).ToDictionary(x=>x.Key, x=> x.Value);
			using (var conn = Context.OpenDatabaseConnection())
			{
				await conn.ExecuteAsync("UPDATE dbo.Content SET Value = @Val WHERE Name = 'BizThreadData'",
					new { Val = new DbString() { IsAnsi = true, Value = JsonConvert.SerializeObject(cleanDic) }});
			}
		}
	}
}
