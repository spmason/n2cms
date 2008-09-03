﻿namespace N2.Templates.Wiki.UI.WikiTemplates
{
    /// <summary>
    /// A template plugin attribute that assumes the template to exist in the 
    /// folder '/Wiki/UI/WikiTemplates'.
    /// </summary>
    public class DefaultWikiTemplateAttribute : WikiTemplateAttribute
    {
        public DefaultWikiTemplateAttribute()
            : base(null)
        {
        }

        public override string VirtualPath
        {
            get { return base.VirtualPath ?? string.Format("~/Templates/Wiki/UI/WikiTemplates/{0}.ascx", Name); }
            set { base.VirtualPath = value; }
        }
    }
}