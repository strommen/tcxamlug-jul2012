using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UsaCities
{
	public static class Extensions
	{
		public static IEnumerable<T> Distinct<T>(this IEnumerable<T> items,
			Func<T, T, bool> equals, Func<T, int> hashCode)
		{
			return items.Distinct(new DelegateEqualityComparer<T>(equals, hashCode));
		}

		private class DelegateEqualityComparer<T> : IEqualityComparer<T>
		{
			private Func<T, T, bool> equals;
			private Func<T, int> hashCode;
			public DelegateEqualityComparer(Func<T, T, bool> equals, Func<T, int> hashCode)
			{
				this.equals = equals;
				this.hashCode = hashCode;
			}

			public bool Equals(T x, T y)
			{
				return this.equals(x, y);
			}

			public int GetHashCode(T obj)
			{
				return this.hashCode(obj);
			}
		}
	}

}
