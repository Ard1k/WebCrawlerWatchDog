using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace WebCrawlerWatchDog
{
	public class CrawlerConfig
	{
		public CrawlPage[] UrlArray { get; set; }
		public StatsData[] StoreDefinitions { get; set; }
		public string[] UnavailableKeywords { get; set; }
		public MailSettings MailSettings { get; set; }
	}

	public class StatsData
	{
		public string Name { get; set; }
		public string Sequence { get; set; }
		[JsonIgnore]
		public string Result { get; set; }
	}

	public class CrawlPage
	{
		public string Url { get; set; }
		public string Name { get; set; }
	}

	public class MailSettings
	{
		public string SmtpServer { get; set; }
		public int Port { get; set; }
		public string SmtpUsername { get; set; }
		public string SmtpPassword { get; set; }
		public string Sender { get; set; }
		public string Recipient { get; set; }
		public string Subject { get; set; }
		public string FoundSubjectPrefix { get; set; }
		public bool EnableSsl { get; set; }
	}
}
