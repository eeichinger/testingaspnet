#region Imports

using System;

#endregion

namespace NUnitAspEx.Core
{
  /// <summary>
  /// class LongLivingMarshalByRefObject
  /// </summary>
  /// <author>Erich Eichinger</author>
  /// <created>02.12.2007 00:47:53</created>
  /// <version>$Id: $</version>
  internal class LongLivingMarshalByRefObject:MarshalByRefObject
  {
    // Methods
    public override object InitializeLifetimeService()
    {
      return null;
    }
  }
}
