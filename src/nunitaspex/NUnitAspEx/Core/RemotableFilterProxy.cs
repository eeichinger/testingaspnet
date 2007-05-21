using NUnit.Core;

namespace NUnitAspEx.Core
{
	internal class RemotableFilterProxy : LongLivingMarshalByRefObject, IFilter
	{
		private IFilter _wrappedFilter;

		public RemotableFilterProxy(IFilter wrappedFilter)
		{
			this._wrappedFilter = wrappedFilter;
		}

		public bool Pass(TestSuite suite)
		{
			return this._wrappedFilter.Pass( suite );
		}

		public bool Pass(TestCase test)
		{
			return this._wrappedFilter.Pass( test );
		}

		public bool Exclude
		{
			get { return this._wrappedFilter.Exclude; }
		}
	}
}