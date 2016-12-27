using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;

#if ! WINCE
using System.Threading.Tasks;
#endif

namespace ZPF
{
   public class BaseViewModel : INotifyPropertyChanged
   {
      public BaseViewModel()
      {

      }

      #region INotifyPropertyChanged

      // - - - INotifyPropertyChanged Members - - - 

      public event PropertyChangedEventHandler PropertyChanged;
        

#if WINCE
      protected void OnPropertyChanged(string propertyName)
      {
         var handler = PropertyChanged;
         if (handler != null)
         {
            handler(this, new PropertyChangedEventArgs(propertyName));
         }
      }
#else
      protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
      {
         var handler = PropertyChanged;
         if (handler != null)
         {
            handler(this, new PropertyChangedEventArgs(propertyName));
         }
      }
#endif

      #endregion

#if WINCE
      protected bool SetField<T>(ref T field, T value, string propertyName )
      {
         if (propertyName != null)
         {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;

            OnPropertyChanged(propertyName);
            return true;
         }
         else
         {
            return false;
         };
      }
#else
      protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
      {
         if (propertyName != null)
         {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;

            OnPropertyChanged(propertyName);
            return true;
         }
         else
         {
            return false;
         };
      }

      protected bool SetFieldNN<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
      {
         if (value != null)
         {
            return SetField(ref field, value, propertyName);
         }
         else
         {
            return false;
         };
      }
#endif

#if !PCL && !WebService && !WINCE
      private DependencyObject _DependencyObject = null;
      [DB_SQL.Ignore]
      protected bool IsInDesignMode
      {
         get
         {
            if (_DependencyObject == null) _DependencyObject = new DependencyObject();
            return System.ComponentModel.DesignerProperties.GetIsInDesignMode(_DependencyObject);
         }
      }
#endif

      // - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  -
   }
}
