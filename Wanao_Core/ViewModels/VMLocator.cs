using System;

namespace Wanao_Core
{
   public class VMLocator
   {
      public static void Clean()
      {
         _Import = null;

      }

      static ImportViewModel _Import = null;

      public static ImportViewModel Import
      {
         get
         {
            if (_Import == null)
            {
               _Import = new ImportViewModel();
            }

            return _Import;
         }
      }

   }
}
