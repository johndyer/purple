#region Using

using System;
using System.IO;
using System.Net.Mail;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Configuration;
using System.Globalization;
using System.Web;
using System.Web.Configuration;
using System.Threading;
using System.Reflection;
using System.Collections;
using System.Xml;
using System.Net;


#endregion

namespace XmlProviders
{
	/// <summary>
	/// Utilities for the entire solution to use.
	/// </summary>
	public static class Utils
	{

		/// <summary>
		/// Strips all illegal characters from the specified title.
		/// </summary>
		public static string RemoveIllegalCharacters(string text)
		{
			if (string.IsNullOrEmpty(text))
				return text;

			text = text.Replace(":", string.Empty);
			text = text.Replace("/", string.Empty);
			text = text.Replace("?", string.Empty);
			text = text.Replace("#", string.Empty);
			text = text.Replace("[", string.Empty);
			text = text.Replace("]", string.Empty);
			text = text.Replace("@", string.Empty);
			text = text.Replace("*", string.Empty);
			text = text.Replace(".", string.Empty);
			text = text.Replace(",", string.Empty);
			text = text.Replace("\"", string.Empty);
			text = text.Replace("&", string.Empty);
			text = text.Replace("'", string.Empty);
			text = text.Replace(" ", "-");
			text = RemoveDiacritics(text);
			text = RemoveExtraHyphen(text);

			return HttpUtility.UrlEncode(text).Replace("%", string.Empty);
		}

		private static string RemoveExtraHyphen(string text)
		{
			if (text.Contains("--"))
			{
				text = text.Replace("--", "-");
				return RemoveExtraHyphen(text);
			}

			return text;
		}

		private static String RemoveDiacritics(string text)
		{
			String normalized = text.Normalize(NormalizationForm.FormD);
			StringBuilder sb = new StringBuilder();

			for (int i = 0; i < normalized.Length; i++)
			{
				Char c = normalized[i];
				if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
					sb.Append(c);
			}

			return sb.ToString();
		}

		private static readonly Regex STRIP_HTML = new Regex("<[^>]*>", RegexOptions.Compiled);
		/// <summary>
		/// Strips all HTML tags from the specified string.
		/// </summary>
		/// <param name="html">The string containing HTML</param>
		/// <returns>A string without HTML tags</returns>
		public static string StripHtml(string html)
		{
			if (string.IsNullOrEmpty(html))
				return string.Empty;

			return STRIP_HTML.Replace(html, string.Empty);
		}

		private static readonly Regex REGEX_BETWEEN_TAGS = new Regex(@">\s+", RegexOptions.Compiled);
		private static readonly Regex REGEX_LINE_BREAKS = new Regex(@"\n\s+", RegexOptions.Compiled);

		/// <summary>
		/// Removes the HTML whitespace.
		/// </summary>
		/// <param name="html">The HTML.</param>
		public static string RemoveHtmlWhitespace(string html)
		{
			if (string.IsNullOrEmpty(html))
				return string.Empty;

			html = REGEX_BETWEEN_TAGS.Replace(html, "> ");
			html = REGEX_LINE_BREAKS.Replace(html, string.Empty);

			return html.Trim();
		}


		#region Password Util
		/// <summary>
		/// Encrypts a string using the SHA256 algorithm.
		/// </summary>
		public static string HashPassword(string plainMessage)
		{
			byte[] data = Encoding.UTF8.GetBytes(plainMessage);
			using (HashAlgorithm sha = new SHA256Managed())
			{
				byte[] encryptedBytes = sha.TransformFinalBlock(data, 0, data.Length);
				return Convert.ToBase64String(sha.Hash);
			}
		}
		#endregion

	}
}
