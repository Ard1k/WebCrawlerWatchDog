using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WebCrawlerWatchDog
{
	internal class WebCrawler
	{
		public static void Run()
		{
			StringBuilder sb = new StringBuilder();
			StringBuilder log = new StringBuilder();
			Console.WriteLine("rouming crawler v1.1");
			sb.AppendLine("rouming crawler v1.1");
			Console.WriteLine("--------------------------------------");
			sb.AppendLine("--------------------------------------");
			sb.AppendLine();

			string forumUrl = @"https://www.rouming.cz/roumingForum.php?thread=615145";

			var psc = new Dictionary<string, int>();
			int totalScores = 0;

			int pageNumber = 1;
			while (true)
			{
				string response = null;

				//if (pageNumber > 2)
				//	break; //debug

				try
				{
					response = postResponse(forumUrl, pageNumber);
				}
				catch (Exception ex)
				{
					Console.WriteLine($"Exception while getting page: {pageNumber}");
					Console.WriteLine("------------------------");
					Console.WriteLine(ex.Message);
					break;
				}

				string regexPattern = "<input type=\"text\" name=\"page\" value=\"(\\d+)\" size=\"1\" class=\"input\"/>";
				Match match = Regex.Match(response, regexPattern);

				if (match.Success)
				{
					int recievedPage = Int32.Parse(match.Groups[1].Value);

					if (recievedPage < pageNumber)
					{
						Console.WriteLine($"Finished on page: {recievedPage}");
						break;
					}

					Console.WriteLine("Processing page number: " + recievedPage);
				}
				else
				{
					Console.WriteLine("No page number found in response!");
					break;
				}

				//string regexPatternPlayerScore = @"\d+\. (\w+) \((\d+) pts\.\)";
				string regexPatternPlayerScore = @"\d+\.\s(.+?)\s\((\d+) pts\.\)";
				MatchCollection matches = Regex.Matches(response, regexPatternPlayerScore);

				Console.WriteLine($"Scores found: {matches.Count}");
				totalScores += matches.Count;

				foreach (Match m in matches)
				{
					string name = m.Groups[1].Value.Trim();
					int points = Int32.Parse(m.Groups[2].Value);

					if (psc.ContainsKey(name))
					{
						psc[name] += points;
					}
					else
					{
						psc.Add(name, points);
					}
				}

				pageNumber++;
				Thread.Sleep(1000);
			}

			var list = psc.OrderByDescending(p => p.Value).Select(s => $"{s.Key} {s.Value} pts.").ToList();

			Console.WriteLine($"Scores found: {totalScores}");
			Console.WriteLine($"Unique players: {list.Count}");
			Console.WriteLine("Leaderboards:");
			Console.WriteLine("--------------");

			int count = 1;

			foreach (var it in list)
			{
				Console.WriteLine($"{count}. " + it);
				sb.AppendLine($"{count}. " + it);
				count++;
			}

			sb.AppendLine();
			sb.AppendLine();
			sb.AppendLine();
			sb.AppendLine("--- LOG ---");
			sb.Append(log);
		}

		static string getResponse(string url)
		{
			HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
			httpWebRequest.Method = "GET";
			httpWebRequest.Headers.Add(HttpRequestHeader.AcceptCharset, "utf-8");
			HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
			if ((int)httpWebResponse.StatusCode < 200 || (int)httpWebResponse.StatusCode >= 300)
				throw new ApplicationException($"	ERROR loading page: {(int)httpWebResponse.StatusCode} response code ");
			return new StreamReader(httpWebResponse.GetResponseStream(), Encoding.UTF8).ReadToEnd();
		}

		static string postResponse(string url, int page)
		{
			var postData = "searchText=";
			postData += "&searchTopic=";
			postData += "&searchNick=";
			postData += $"&page={page}";
			postData += "&submit=J%C3%ADt";
			//postData += "&polozka=" + Uri.EscapeDataString("datastring");
			var data = Encoding.ASCII.GetBytes(postData);

			HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
			httpWebRequest.Method = "POST";
			httpWebRequest.Headers.Add(HttpRequestHeader.AcceptCharset, "utf-8");
			httpWebRequest.ContentType = "application/x-www-form-urlencoded";
			httpWebRequest.ContentLength = data.Length;

			using (var stream = httpWebRequest.GetRequestStream())
			{
				stream.Write(data, 0, data.Length);
			}

			HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
			if ((int)httpWebResponse.StatusCode < 200 || (int)httpWebResponse.StatusCode >= 300)
				throw new ApplicationException($"	ERROR loading page: {(int)httpWebResponse.StatusCode} response code ");
			return new StreamReader(httpWebResponse.GetResponseStream(), Encoding.UTF8).ReadToEnd();
		}
	}
}
