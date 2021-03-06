#region License

/* Copyright (C) 2007 Cristian Libardo
 *
 * This is free software; you can redistribute it and/or modify it
 * under the terms of the GNU Lesser General Public License as
 * published by the Free Software Foundation; either version 2.1 of
 * the License, or (at your option) any later version.
 */

#endregion

using System;
using System.Collections.Generic;
using N2.Definitions;

namespace N2.Integrity
{
	/// <summary>
	/// A class decoration used to restrict which items may be placed under 
	/// which. When this attribute intersects with 
	/// <see cref="AllowedChildrenAttribute"/>, the union of these two are 
	/// considered to be allowed.</summary>
	[AttributeUsage(AttributeTargets.Class)]
	public class RestrictParentsAttribute : TypeIntegrityAttribute, IInheritableDefinitionRefiner
	{
		/// <summary>Initializes a new instance of the RestrictParentsAttribute which is used to restrict which types of items may be added below which.</summary>
		public RestrictParentsAttribute()
		{
			RefinementOrder = RefineOrder.Before;
		}

		/// <summary>Initializes a new instance of the RestrictParentsAttribute which is used to restrict which types of items may be added below which.</summary>
		/// <param name="allowedTypes">Defines wether all types of items are allowed as parent items.</param>
		public RestrictParentsAttribute(AllowedTypes allowedTypes)
			: this()
		{
			if (allowedTypes == AllowedTypes.All)
				Types = null;
			else
				Types = new Type[0];
		}

		/// <summary>Initializes a new instance of the RestrictParentsAttribute which is used to restrict which types of items may be added below which.</summary>
		/// <param name="allowedParentTypes">A list of allowed types. Null is interpreted as all types are allowed.</param>
		public RestrictParentsAttribute(params Type[] allowedParentTypes)
			: this()
		{
			Types = allowedParentTypes;
		}

		/// <summary>Initializes a new instance of the RestrictParentsAttribute which is used to restrict which types of items may be added below which.</summary>
		/// <param name="allowedParentType">A list of allowed types. Null is interpreted as all types are allowed.</param>
		public RestrictParentsAttribute(Type allowedParentType)
			: this()
		{
			Types = new [] { allowedParentType };
		}

		/// <summary>Changes allowed parents on the item definition.</summary>
		/// <param name="currentDefinition">The definition to alter.</param>
		/// <param name="allDefinitions">All definitions.</param>
		public override void Refine(ItemDefinition currentDefinition, IList<ItemDefinition> allDefinitions)
		{
			foreach (ItemDefinition definition in allDefinitions)
			{
				if (IsAssignable(definition.ItemType))
					definition.AddAllowedChild(currentDefinition);
			}
		}
	}
}