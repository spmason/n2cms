#region License
/* Copyright (C) 2007 Cristian Libardo
 *
 * This is free software; you can redistribute it and/or modify it
 * under the terms of the GNU Lesser General Public License as
 * published by the Free Software Foundation; either version 2.1 of
 * the License, or (at your option) any later version.
 *
 * This software is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with this software; if not, write to the Free
 * Software Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA
 * 02110-1301 USA, or see the FSF site: http://www.fsf.org.
 */
#endregion

using System;

namespace N2.Details
{
    /// <summary>
    /// A content detail. A number of content details can be associated with one content item.
    /// </summary>
    /// <remarks>Usually content details are created below the hood when working with primitive .NET types against a contnet item.</remarks>
	[Serializable]
	public abstract class ContentDetail: ICloneable
	{
		#region Constuctors
		/// <summary>Creates a new (empty) instance of the content detail.</summary>
		public ContentDetail()
		{
			id = 0;
            enclosingItem = null; 
			name = String.Empty;
		}
		#endregion

		#region Private Fields
		private int id;
        private ContentItem enclosingItem; 
		private string name; 
		private DetailCollection collection;
		#endregion

		#region Public Properties

        /// <summary>Gets or sets the detil's primary key.</summary>
		public virtual int ID
		{
			get { return id; }
			set { id = value; }

		}

        /// <summary>Gets or sets the name of the detail.</summary>
		public virtual string Name
		{
			get { return name; }
			set	{ name = value; }
		}

        /// <summary>Gets or sets this details' value.</summary>
        public abstract object Value
        {
            get;
            set;
        }

		/// <summary>Gets the type of value associated with this item.</summary>
		public abstract Type ValueType
		{
			get;
		}

		/// <summary>Gets whether this items belongs to an <see cref="N2.Details.DetailCollection"/>.</summary>
		public virtual bool IsInCollection
		{
			get { return EnclosingCollection != null; }
		}

		/// <summary>Gets or sets the content item that this detail belong to.</summary>
		/// <remarks>Usually this is assigned by a content item which encapsulates the usage of details</remarks>
		public virtual N2.ContentItem EnclosingItem
		{
			get { return enclosingItem; }
			set { enclosingItem = value; }
		}

		/// <summary>Gets or sets the <see cref="N2.Details.DetailCollection"/> associated with this detail. This value can be null which means it's a named detail directly on the item.</summary>
		public virtual DetailCollection EnclosingCollection
		{
			get { return collection; }
			set { collection = value; }
		}
		#endregion 
	
		#region Static Methods
		/// <summary>Creates a new content detail of the appropriated type based on the given value.</summary>
		/// <param name="item">The item that will enclose the new detail.</param>
		/// <param name="name">The name of the detail.</param>
		/// <param name="value">The value of the detail. This will determine what type of content detail will be returned.</param>
		/// <returns>A new content detail whose type depends on the type of value.</returns>
		public static ContentDetail New(ContentItem item, string name, object value)
		{
			if (value == null)
				throw new ArgumentNullException("value");

			Type t = value.GetType();
			if (t == typeof(bool))
				return new BooleanDetail(item, name, (bool)value);
			if (t == typeof(int))
				return new IntegerDetail(item, name, (int)value);
			if (t == typeof(double))
				return new DoubleDetail(item, name, (double)value);
			if (t == typeof(DateTime))
				return new DateTimeDetail(item, name, (DateTime)value);
			if (t == typeof(string))
				return new StringDetail(item, name, (string)value);
			if (t.IsSubclassOf(typeof(ContentItem)))
				return new LinkDetail(item, name, (ContentItem)value);
			
			return new ObjectDetail(item, name, value);
		}

		/// <summary>Gets the name of the property on the detail class that can encapsulate the given value.</summary>
		/// <param name="value">The value for which the to retrieve the associated property.</param>
		/// <returns>The name of the property on the detail class that can encapsulate the given value.</returns>
		public static string GetAssociatedPropertyName(object value)
		{
			if (value is bool)
				return "BoolValue";
			else if (value is int)
				return "IntValue";
			else if (value is double)
				return "DoubleValue";
			else if (value is DateTime)
				return "DateTimeValue";
			else if (value is string)
				return "StringValue";
			else if (value is ContentItem)
				return "LinkedItem";
			else
				return "Value";
		}
		#endregion

        #region Equals, HashCode and ToString Overrides

		/// <summary>Checks details for equality.</summary>
		/// <returns>True if details have the same ID.</returns>
        public override bool Equals( object obj )
		{
			if( this == obj ) return true;
			ContentDetail other = obj as ContentDetail;
			return other != null && id != 0 && id == other.id;
		}

    	int? hashCode;
		/// <summary>Gets a hash code based on the ID.</summary>
		/// <returns>A hash code.</returns>
		public override int GetHashCode()
		{
			if (!hashCode.HasValue)
				hashCode = (id > 0 ? id.GetHashCode() : base.GetHashCode());
			return hashCode.Value;
		}

		/// <summary>Returns this details value's ToString result.</summary>
		/// <returns>The value to string.</returns>
        public override string ToString()
        {
            return null == this.Value
				? base.ToString()
				: this.Value.ToString();
        }
        #endregion

        #region ICloneable Members

        /// <summary>Creates a cloned object with the id set to 0.</summary>
        /// <returns>A new ContentDetail with the same Name and Value.</returns>
		public virtual ContentDetail Clone()
        {
            ContentDetail cloned = (ContentDetail)Activator.CreateInstance(this.GetType());
            cloned.ID = 0;
            cloned.Name = this.Name;
            cloned.Value = this.Value;
            return cloned;
        }

        object ICloneable.Clone()
        {
            return this.Clone();
        }

        #endregion

		public void AddTo(ContentItem newEnclosingItem)
		{
			AddTo((DetailCollection)null);

			if (newEnclosingItem == EnclosingItem)
				return;

			RemoveFromEnclosingItem();

			if (newEnclosingItem != null)
			{
				EnclosingItem = newEnclosingItem;
				newEnclosingItem.Details.Add(Name, this);
			}
		}

		internal void RemoveFromEnclosingItem()
		{
			if (EnclosingItem != null)
				EnclosingItem.Details.Remove(Name);
		}

		public void AddTo(DetailCollection newEnclosingCollection)
		{
			RemoveFromEnclosingCollection();

			if (newEnclosingCollection != null)
				newEnclosingCollection.Add(newEnclosingCollection);
		}

		internal void RemoveFromEnclosingCollection()
		{
			if (EnclosingCollection != null)
				EnclosingCollection.Remove(this);
		}
    }
}
