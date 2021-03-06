﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace N2.Web.Mvc.Html
{
	public static class ContentHtmlExtensions
	{
		public static bool HasValue(this HtmlHelper html, string detailName)
		{
			var item = html.CurrentItem();
			return item != null && item[detailName] != null;
		}

		public static bool ValueEquals<TData>(this HtmlHelper html, string detailName, TData expectedValue)
		{
			var item = html.CurrentItem();
			if (item == null)
				return false;

			object value = item[detailName];
			if (value == null && expectedValue == null)
				return true;
			if (value == null || expectedValue == null)
				return false;
			if (!(value is TData))
				return false;
			return expectedValue.Equals((TData)value);			
		}
	}
}
