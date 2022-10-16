using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace WebCrawlerWatchDog
{
	internal class WebCrawler
	{
		public class StatsData
		{
			public string Name { get; set; }
			public string Sequence { get; set; }
			public string Result { get; set; }
		}

		public static void Run()
		{
			StatsData[] stores = new StatsData[]
			{
				new StatsData
				{
					Name = "Na Příkopě 820/24, Praha 1",
					Sequence = "820/24, Praha 1"
				},
				new StatsData
				{
					Name = "Havelská 503/19, Praha 1",
					Sequence = "503/19, Praha 1"
				},
				new StatsData
				{
					Name = "Roosveltova 419/20, Brno - střed",
					Sequence = "419/20, Brno - střed"
				},
				new StatsData
				{
					Name = "Mírové náměstí 15, Jablonec nad Nisou",
					Sequence = "15, Jablonec nad Nisou"
				},
				new StatsData
				{
					Name = "Suché Mýto 1, Bratislava",
					Sequence = "1, Bratislava"
				}
			};

			Console.WriteLine("ceskamincovna.cz watchdog crawler v1.0");
			Console.WriteLine("--------------------------------------");
			string url = "https://ceskamincovna.cz/stribrna-mince-psi-plemena---border-kolie-proof-389-14127-d";
			string response = getResponse(url);
			response = WebUtility.HtmlDecode(response);
			//Console.WriteLine(response);
			int dostupna = 0;
			int nedostupna = 0;
			foreach (StatsData s in stores)
			{
				string status = findAwail(response, s.Sequence);
				Console.WriteLine(s.Name + " - " + status);
				if (status == "nedostupný")
					nedostupna++;
				else
					dostupna++;
			}
			Console.WriteLine("--------------------------------------");
			Console.WriteLine($"Dostupna: {dostupna} Nedostupna: {nedostupna}");
			Console.ReadKey();
		}

		static string findBetween(string s1, string s2, string data)
		{
			int num = data.IndexOf(s1) + s1.Length;
			int num2 = data.IndexOf(s2, num);
			return data.Substring(num, num2 - num);
		}

		static string findAwail(string response, string store_name)
		{
			int store_index = response.IndexOf(store_name);
			int i1 = response.IndexOf("</div>", store_index);
			int i2 = response.IndexOf("</div>", i1 + 6);
			int i3 = response.Substring(0, i2 - 6).LastIndexOf('>');
			return response.Substring(i3+1, i2 - i3 - 1).Replace("\r", String.Empty).Replace("\n", String.Empty).Trim();
		}

		static string getResponse(string url)
		{
			HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
			httpWebRequest.Method = "GET";
			httpWebRequest.Headers.Add(HttpRequestHeader.AcceptCharset, "utf-8");
			HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
			return new StreamReader(httpWebResponse.GetResponseStream(), Encoding.UTF8).ReadToEnd();
		}
	}
}
