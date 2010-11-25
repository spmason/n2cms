using System;
using System.Collections.Generic;
using System.Xml;
using N2.Definitions;

namespace N2.Persistence.Serialization
{
	public class AttachmentXmlWriter : IXmlWriter
	{
		private readonly IAttributeExplorer explorer;

		public AttachmentXmlWriter(IAttributeExplorer explorer)
		{
			this.explorer = explorer;
		}

		#region IXmlWriter Members

		public void Write(ContentItem item, XmlTextWriter writer)
		{
			IList<IAttachmentHandler> attachments = explorer.Find<IAttachmentHandler>(item.GetContentType());
			if (attachments.Count > 0)
			{
				using (new ElementWriter("attachments", writer))
				{
					foreach (IAttachmentHandler attachment in attachments)
					{
						using (var attachmentElement = new ElementWriter("attachment", writer))
						{
							attachmentElement.WriteAttribute("name", attachment.Name);
							attachment.Write(item, writer);
						}
					}
				}
			}
		}

		#endregion
	}
}