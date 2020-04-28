using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Windows_Media_Controller_Library
{
	public static class TypeExtensions
	{
		public static bool isType(this Type type1, Type type2)
		{
			return type1 == type2;
		}

		public static bool ImplementsInterface(this Type type, Type ifaceType)
		{
			Type[] intf = type.GetInterfaces();
			for (int i = 0; i < intf.Length; i++)
			{
				if (intf[i] == ifaceType)
				{
					return true;
				}
			}
			return false;
		}
	}
}
