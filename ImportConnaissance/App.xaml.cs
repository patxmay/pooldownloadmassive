using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Wanao
{
   /// <summary>
   /// Interaction logic for App.xaml
   /// </summary>
   public partial class App : Application
   {
      private void Application_Startup(object sender, StartupEventArgs e)
      {
         AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
      }

      void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
      {
         Exception ex = e.ExceptionObject as Exception;

         //AuditTrail.Write(new AuditTrailItem
         //{
         //   Level = ErrorLevel.Critical,
         //   Tag = "UnhandledException",
         //   Message = ex.Message + " (1)",
         //   Data = ex.StackTrace + Environment.NewLine + Environment.NewLine + ex.Source
         //});

         MessageBox.Show(ex.Message, "Uncaught Thread Exception", MessageBoxButton.OK, MessageBoxImage.Error);
      }

      private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
      {
         Exception ex = e.Exception;

         //AuditTrail.Write(new AuditTrailItem
         //{
         //   Level = ErrorLevel.Critical,
         //   Tag = "UnhandledException",
         //   Message = ex.Message + " (2)",
         //   Data = ex.StackTrace + Environment.NewLine + Environment.NewLine + ex.Source
         //});

         if (false)
         {
            //Handling the exception within the UnhandledExcpeiton handler.
            MessageBox.Show(e.Exception.Message, "Exception Caught", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
         }
         else
         {
            //If you do not set e.Handled to true, the application will close due to crash.
            MessageBox.Show("Application is going to close!", "Uncaught Exception");
            e.Handled = false;
         }
      }
   }
}
