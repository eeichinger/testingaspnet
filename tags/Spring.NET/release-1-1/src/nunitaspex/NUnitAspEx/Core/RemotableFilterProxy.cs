using NUnit.Core;

namespace NUnitAspEx.Core
{
    // TODO: revisit for later NUnit versions (current: 2.4.1)

    // remote filtering is not possible, because some ITest 
    // implementations (like NUnitTestMethod) are not serializable

    internal class RemotableFilterProxy : LongLivingMarshalByRefObject, ITestFilter
    {
        private readonly ITestFilter _wrappedFilter;

        public RemotableFilterProxy(ITestFilter wrappedFilter)
        {
            this._wrappedFilter = wrappedFilter;
        }

        public bool Pass(ITest test)
        {
            return this._wrappedFilter.Pass(test);
        }

        public bool Match(ITest test)
        {
            return this._wrappedFilter.Match(test);
        }

        public bool IsEmpty
        {
            get { return this._wrappedFilter.IsEmpty; }
        }
    }
}