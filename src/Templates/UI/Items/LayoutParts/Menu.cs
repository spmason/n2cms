using System;
using System.Collections.Generic;
using System.Text;
using N2.Integrity;

namespace N2.Templates.Items.LayoutParts
{
	[Definition("Menu", "Menu")]
	[RestrictParents(typeof(StartPage))] // The menu is placed on the start page and displayed on all underlying pages
	[AllowedZones("SiteLeft")]
	public class Menu : SidebarItem
	{
		public override string TemplateUrl
		{
			get { return "~/Layouts/Parts/Menu.ascx"; }
		}

		public override string IconUrl
		{
			get
			{
				return "~/Img/page_white_link.png";
			}
		}
	}
}