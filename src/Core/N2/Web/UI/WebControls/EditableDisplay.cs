#region License

/* Copyright (C) 2007 Cristian Libardo
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 */

#endregion

using System;
using System.Web.UI;
using N2.Definitions;

namespace N2.Web.UI.WebControls
{
	/// <summary>
	/// This control will either display content using the defined editor 
	/// just as the <see cref="Display"/> does or edit using the defined 
	/// editor. The GetState method on the <see cref="ControlPanel"/> determines 
	/// wether the content should be displayed or edited.
	/// </summary>
	public class EditableDisplay : Display, IEditableEditor
	{
		private IEditable editable;
		private Control editor;
		private ControlPanelState state;

		public Control Editor
		{
			get
			{
				EnsureChildControls();
				return editor;
			}
			set { editor = value; }
		}

		public ControlPanelState State
		{
			get { return state; }
			set { state = value; }
		}

		protected override void OnInit(EventArgs e)
		{
			State = ControlPanel.GetState();
			if (State == ControlPanelState.Editing)
			{
				AddEditable();
			}
			else
			{
				AddDisplayable();
			}
		}

		protected void AddEditable()
		{
			editable = GetEditable(CurrentItem.GetType());
			editor = editable.AddTo(this);
			if (!Page.IsPostBack)
				editable.UpdateEditor(CurrentItem, editor);
		}

		private IEditable GetEditable(Type itemType)
		{
			foreach (IEditable editable in N2.Context.Definitions.GetDefinition(itemType).GetEditables(Page.User))
				if (editable.Name == PropertyName)
					return editable;
			return null;
		}
	}
}