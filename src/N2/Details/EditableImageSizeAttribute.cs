﻿using System.Linq;
using System.Web.UI.WebControls;
using N2.Configuration;
using N2.Engine;

namespace N2.Details
{
	public class EditableImageSizeAttribute : EditableDropDownAttribute
	{
		public EditableImageSizeAttribute(string title, int sortOrder)
			: base(title, sortOrder)
		{
		}

		[Dependency]
		public ConfigurationManagerWrapper Configuration { get; set; }

		protected override System.Web.UI.WebControls.ListItem[] GetListItems()
		{
			return Configuration
				.GetContentSection<EditSection>("edit").Images.Sizes
				.OfType<ImageSizeElement>()
				.Select(ise => new ListItem(GetText(ise), ise.Name))
				.ToArray();
		}

		private static string GetText(ImageSizeElement ise)
		{
			return ise.Name + " " + GetLengthText(ise.Width) + "," + GetLengthText(ise.Height);
		}

		private static string GetLengthText(int length)
		{
			return length == 0 ? "*" : length.ToString();
		}
	}
}
