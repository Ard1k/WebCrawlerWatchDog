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
			Console.WriteLine("ceskamincovna.cz watchdog crawler v1.1");
			sb.AppendLine("ceskamincovna.cz watchdog crawler v1.1");
			Console.WriteLine("--------------------------------------");
			sb.AppendLine("--------------------------------------");
			sb.AppendLine();

			bool isAnyCoinAvailable = false;


			#region inicializace a kontroly configu
			if (!File.Exists("config.json"))
			{
				Console.WriteLine("CONF_ERROR - Nenalezen config file");
				return;
			}

			var configText = File.ReadAllText("config.json");
			CrawlerConfig crawlerConfig = null;
			if (!string.IsNullOrEmpty(configText))
			{
				crawlerConfig = JsonConvert.DeserializeObject<CrawlerConfig>(configText);
			}

			if (crawlerConfig == null)
			{
				Console.WriteLine("CONF_ERROR - Nezdarila se deserializace configu");
				return;
			}

			if (crawlerConfig.UrlArray == null || crawlerConfig.UrlArray.Length <= 0)
			{
				Console.WriteLine("CONF_ERROR - Nezadany zadne url ke kontrole");
				return;
			}

			if (crawlerConfig.StoreDefinitions == null || crawlerConfig.StoreDefinitions.Length <= 0)
			{
				Console.WriteLine("CONF_ERROR - Nezadany prodejny ke kontrole");
				return;
			}

			#endregion

			foreach (var page in crawlerConfig.UrlArray)
			{
				Console.WriteLine($"{page.Name ?? "UNK page"}");
				log.AppendLine($"{page.Name ?? "UNK page"}");
				try
				{
					string response = getResponse(page.Url);
					if (string.IsNullOrEmpty(response))
						throw new ApplicationException("	ERROR - invalid web response");

					response = WebUtility.HtmlDecode(response);

					foreach (StatsData s in crawlerConfig.StoreDefinitions)
					{
						string status = findAwail(response, s.Sequence);
						if (string.IsNullOrEmpty(status))
						{
							Console.WriteLine($"	WARNING - Pro obchod [{s.Name ?? "undefined"}] - nenalezen status!");
							log.AppendLine($"	WARNING - Pro obchod [{s.Name ?? "undefined"}] - nenalezen status!");
						}
						Console.WriteLine($"	{s.Name ?? "undefined"} - {status}");
						log.AppendLine($"	{s.Name ?? "undefined"} - {status}");

						bool isAvail = true;
						foreach (var uKey in crawlerConfig.UnavailableKeywords)
						{
							if (status.ToLower().Contains(uKey.ToLower()))
							{
								isAvail = false; 
								break;
							}
						}

						if (isAvail)
						{
							isAnyCoinAvailable = true;
							sb.AppendLine($" SKLADEM! Mince: {page.Name ?? "unk_page_name"} Url: {page.Url}");
							break;
						}
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.Message);
					log.AppendLine(ex.Message);
				}
			}

			sb.AppendLine();
			sb.AppendLine();
			sb.AppendLine();
			sb.AppendLine("--- LOG ---");
			sb.Append(log);

			if (!string.IsNullOrEmpty(crawlerConfig.MailSettings?.SmtpServer))
			{
				Console.WriteLine("Sending mail");
				try
				{
					var smtpClient = new SmtpClient(crawlerConfig.MailSettings?.SmtpServer, crawlerConfig.MailSettings?.Port > 0 ? crawlerConfig.MailSettings.Port : 587)
					{
						Credentials = new NetworkCredential(crawlerConfig.MailSettings?.SmtpUsername ?? "unknown", crawlerConfig.MailSettings?.SmtpPassword ?? "unknown"),
						EnableSsl = crawlerConfig.MailSettings?.EnableSsl ?? false //pokud nebude v JSONU je default false!
					};

					string subject = crawlerConfig.MailSettings?.Subject ?? String.Empty;
					if (isAnyCoinAvailable)
						subject = (crawlerConfig.MailSettings?.FoundSubjectPrefix ?? String.Empty) + " " + subject;

					smtpClient.Send(crawlerConfig.MailSettings?.Sender ?? "unknow", crawlerConfig.MailSettings?.Recipient ?? "unknown", subject, sb.ToString());
				}
				catch (Exception ex)
				{
					Console.WriteLine("ERROR sending email: " + ex.Message ?? String.Empty);
				}
				Console.WriteLine("Mail sent!");
			}
		}

		static string findAwail(string response, string store_name)
		{
			int store_index = response.IndexOf(store_name);
			if (store_index < 0)
				return null;
			int i1 = response.IndexOf("</div>", store_index);
			if (i1 < 0)
				return null;
			int i2 = response.IndexOf("</div>", i1 + 6);
			if (i2 < 0)
				return null;
			int i3 = response.Substring(0, i2 - 6).LastIndexOf('>');
			if (i3 < 0)
				return null;
			return response.Substring(i3+1, i2 - i3 - 1).Replace("\r", String.Empty).Replace("\n", String.Empty).Trim();
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
	}
}
