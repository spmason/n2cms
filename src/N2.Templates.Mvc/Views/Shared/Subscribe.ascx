<%@ Control Language="C#" AutoEventWireup="true" Inherits="N2.Web.Mvc.N2ViewUserControl<Subscribe>" %>

<%=this.EditableDisplay(m => m.Title)%>
<div class="box">
	<div class="inner">
	<%= N2.Web.Link.To(Model.SelectedFeed)
		.Text(string.Format("<img src='{0}' alt='RSS' /> {1}", VirtualPathUtility.ToAbsolute("~/Content/Img/feed.png"),
			N2.Utility.Evaluate(Model.SelectedFeed, "Title")))%>
	</div>
</div>