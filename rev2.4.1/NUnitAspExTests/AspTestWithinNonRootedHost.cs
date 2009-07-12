using NUnitAspEx;

namespace NUnitAspExTests
{
    [AspTestFixture( VirtualPath = "/Test1", RelativePhysicalPath = "/TestWebs/Test1")]
    public class AspTestWithinNonRootedHost : AspTestWithinHostBase
    {	    
    }
}