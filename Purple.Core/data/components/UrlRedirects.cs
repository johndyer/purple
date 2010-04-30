using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Purple.Core
{
	public class UrlRedirects
	{
		private static List<UrlRedirect> _activeUrlRedirects = null;
		public static List<UrlRedirect> ActiveUrlRedirects
		{
			get
			{
				if (_activeUrlRedirects == null)
					ReloadActiveRedirects();

				return _activeUrlRedirects;
			}
		}

		private static void ReloadActiveRedirects()
		{
			List<UrlRedirect> redirects = PurpleDataProviderManager.Provider.GetUrlRedirects();
			redirects.RemoveAll(delegate(UrlRedirect url) { return !url.IsActive; });
			_activeUrlRedirects = redirects;
		}
		
	
		public static Guid AddUrlRedirect(UrlRedirect urlRedirect)
		{
			PurpleDataProviderManager.Provider.AddUrlRedirect(urlRedirect);

			ReloadActiveRedirects();

			return urlRedirect.RedirectID;
		}

		public static void UpdateUrlRedirect(UrlRedirect urlRedirect)
		{
			PurpleDataProviderManager.Provider.UpdateUrlRedirect(urlRedirect);

			ReloadActiveRedirects();
		}

		public static void DeleteUrlRedirect(UrlRedirect urlRedirect)
		{
			PurpleDataProviderManager.Provider.DeleteUrlRedirect(urlRedirect);

			ReloadActiveRedirects();
		}

		public static UrlRedirect GetUrlRedirect(string url)
		{
			return PurpleDataProviderManager.Provider.GetUrlRedirect(url);
		}

		public static UrlRedirect GetUrlRedirect(Guid redirectID)
		{
			return PurpleDataProviderManager.Provider.GetUrlRedirect(redirectID);
		}
	}
}
