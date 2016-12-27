using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Net;
using ZPF;
using ERPAudixis;

public class DataModul
{
#if DEBUG
   public static string ConnectionString = @"Data Source=rti2.zpf.fr\SQLEXPRESS;Initial Catalog=Audixis_ERP09;Persist Security Info=True;User ID=sa;Password=MossIsTheBoss2";
  // public static string ConnectionString = @"Data Source=rti2.zpf.fr\SQLEXPRESS;Initial Catalog=Audixis_ERP_Old;Persist Security Info=True;User ID=sa;Password=MossIsTheBoss2";
  // public static string ConnectionString = @"Data Source=califabe.audixis.fr\SQLEXPRESS;Initial Catalog=Audixis_ERP;Persist Security Info=True;User ID=sa;Password=Audixis%2015%";

#else
    //public static string ConnectionString = @"Data Source=rti2.zpf.fr\SQLEXPRESS;Initial Catalog=Audixis_ERP09;Persist Security Info=True;User ID=sa;Password=MossIsTheBoss2";
    public static string ConnectionString = @"Data Source=califabe.audixis.fr\SQLEXPRESS;Initial Catalog=Audixis_ERP;Persist Security Info=True;User ID=sa;Password=Audixis%2015%";
#endif

   public static DbConnection SQLConnection;


#if DEBUG
   public static string IniFileName = @"ERPAudixis.ini";
#else
   public static string IniFileName = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\ERPAudixis.ini";
#endif

   public static bool Init()
   {
      bool Result = true;

      //
      // - - -  - - - 

      TIniFile ini = new TIniFile(IniFileName);

      if (!File.Exists(IniFileName))
      {
         ini.WriteString("General", "Created", DateTime.Now.ToString());

         ini.UpdateFile();
      };

      //
      // - - -  - - - 

      if (SQLConnection == null)
      {
         try
         {
            SQLConnection = DB_SQL.Open(DB_SQL.DBType.SQLServer, ConnectionString);
            DB_SQL.Connection = SQLConnection;
         }
         catch (Exception ex)
         {
            DB_SQL.Connection = null;
            Result = false;
            AuditTrail.Write(ErrorLevel.Error, ex.ToString());
         };
      };

      AuditTrailViewModel.Instance.Init(DB_SQL.Connection);

      //
      // - - -  - - - 

      return Result;
   }

   // - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - - 

   public static bool AutoOpen = false;

   public static bool RestoreMenuState = true;
   public static bool SingleSubMenu = false;

   public static string MenuState = "";
   public static string Style = "Default";
   public static bool LaunchExcel = true;
   public static string ReportPath = "";

   public static bool Load()
   {
      TIniFile ini = new TIniFile(IniFileName);

      AutoOpen = ini.ReadBool("General", "AutoOpen", AutoOpen);
      Style = ini.ReadString("General", "Style", Style);

      LaunchExcel = ini.ReadBool("Export", "LaunchExcel", LaunchExcel);
      ReportPath = ini.ReadString("Export", "ReportPath", ReportPath);

      RestoreMenuState = ini.ReadBool("Menu", "RestoreMenuState", RestoreMenuState);
      SingleSubMenu = ini.ReadBool("Menu", "SingleSubMenu", SingleSubMenu);
      MenuState = ini.ReadString("Menu", "MenuState", MenuState);

      // - - -

      MenuState = (RestoreMenuState ? MenuState : "");

      // - - -  - - - 

      string fmt = Environment.CurrentDirectory + @"\Styles\{0}.xaml";

      TStrings FileNames = new TStrings();
      FileNames.Add(string.Format(fmt, Style));

      (Application.Current as App).DynamicLoadStyles(FileNames);

      //
      // - - -  - - - 

      return true;
   }

   public static async Task<bool> Save()
   {
      TIniFile ini = new TIniFile(IniFileName);

      ini.WriteBool("General", "AutoOpen", AutoOpen);
      ini.WriteString("General", "Style", Style);

      ini.WriteBool("Export", "LaunchExcel", LaunchExcel);
      ini.WriteString("Export", "ReportPath", ReportPath);

      ini.WriteBool("Menu", "RestoreMenuState", RestoreMenuState);
      ini.WriteBool("Menu", "SingleSubMenu", SingleSubMenu);
      ini.WriteString("Menu", "MenuState", MenuState);

      return await ini.UpdateFile();
   }

   // - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - - 

   public static void ReadFormPos(Window window, bool PosOnly, bool FromBorder)
   {
      TIniFile IniFile = new TIniFile(IniFileName);
      TStrings Strings = new TStrings();

      if (FromBorder)
      {
         bool AlignLeft = IniFile.ReadBool(window.Name, "AlignLeft", false);
         bool AlignTop = IniFile.ReadBool(window.Name, "AlignTop", false);
         int MarginX = IniFile.ReadInteger(window.Name, "MarginX", 15);
         int MarginY = IniFile.ReadInteger(window.Name, "MarginY", 15);

         if (MarginX < 0) MarginX = 0;
         if (MarginY < 0) MarginY = 0;

         if (AlignLeft)
         {
            window.Left = MarginX;
         }
         else
         {
            window.Left = System.Windows.SystemParameters.PrimaryScreenWidth - window.Width - MarginX;
         };

         if (AlignTop)
         {
            window.Top = MarginY;
         }
         else
         {
            window.Top = System.Windows.SystemParameters.PrimaryScreenHeight - window.Height - MarginY;
         };

         return;
      };

      window.Left = IniFile.ReadInteger(window.Name, "Left", (int)window.Left);
      window.Top = IniFile.ReadInteger(window.Name, "Top", (int)window.Top);

      if (!PosOnly)
      {
         window.Width = IniFile.ReadInteger(window.Name, "Width", (int)window.Width);
         window.Height = IniFile.ReadInteger(window.Name, "Height", (int)window.Height);
      };

      //if (window.Left < 0)
      if (window.Left < System.Windows.SystemParameters.VirtualScreenLeft)
      {
         window.Left = 0;
      };

      //if (window.Top < 0)
      if (window.Top < System.Windows.SystemParameters.VirtualScreenTop)
      {
         window.Top = 0;
      };

      if (!PosOnly)
      {
         if (window.Left > System.Windows.SystemParameters.WorkArea.Width)
         {
            window.Left = System.Windows.SystemParameters.WorkArea.Width - window.Width;
         };

         if (window.Top > System.Windows.SystemParameters.WorkArea.Height)
         {
            window.Top = System.Windows.SystemParameters.WorkArea.Height - window.Height;
         };

         if (window.Width > System.Windows.SystemParameters.WorkArea.Width)
         {
            window.Width = System.Windows.SystemParameters.WorkArea.Width;
         };

         if (window.Height > System.Windows.SystemParameters.WorkArea.Height)
         {
            window.Height = System.Windows.SystemParameters.WorkArea.Height;
         };
      };

      int windowState = IniFile.ReadInteger(window.Name, "WindowState", (int)(WindowState.Normal));

      if (windowState == (int)(WindowState.Maximized))
      {
         window.WindowState = WindowState.Maximized;
      }
   }

   public static void WriteFormPos(Window window, bool FromBorder)
   {
      if (IniFileName != "")
      {
         try
         {
            if (FromBorder)
            {
               bool AlignLeft = true;
               bool AlignTop = true;
               double MarginX = 0;
               double MarginY = 0;

               if ((window.Left + (window.Width / 2)) > System.Windows.SystemParameters.PrimaryScreenWidth / 2)
               {
                  // right border
                  AlignLeft = false;
                  MarginX = System.Windows.SystemParameters.PrimaryScreenWidth - window.Left - window.Width;
               }
               else
               {
                  MarginX = window.Left;
               };

               if ((window.Top + (window.Height / 2)) > System.Windows.SystemParameters.PrimaryScreenHeight / 2)
               {
                  // bottom border
                  AlignTop = false;
                  MarginY = System.Windows.SystemParameters.PrimaryScreenHeight - window.Top - window.Height;
               }
               else
               {
                  MarginY = window.Top;
               };

               TIniFile IniFile = new TIniFile(IniFileName);

               try
               {
                  IniFile.WriteBool(window.Name, "AlignLeft", AlignLeft);
                  IniFile.WriteBool(window.Name, "AlignTop", AlignTop);
                  IniFile.WriteInteger(window.Name, "MarginX", (int)MarginX);
                  IniFile.WriteInteger(window.Name, "MarginY", (int)MarginY);

                  IniFile.UpdateFile();
               }
               catch
               {
                  //on E: Exception do
                  //{
                  //   MessageDlg('Erreur fatale (WriteFormPos): '  + #13 + #10
                  //               + E.Message, mtError, [mbOk], 0);
                  //   Application.Terminate;
                  //};
               };
            }
            else
            {
               {
                  TIniFile IniFile = new TIniFile(IniFileName);

                  try
                  {
                     IniFile.WriteInteger(window.Name, "WindowState", (int)(window.WindowState));
                     IniFile.WriteInteger(window.Name, "Left", (int)window.Left);
                     IniFile.WriteInteger(window.Name, "Top", (int)window.Top);
                     IniFile.WriteInteger(window.Name, "Width", (int)window.Width);
                     IniFile.WriteInteger(window.Name, "Height", (int)window.Height);

                     IniFile.UpdateFile();
                  }
                  catch
                  {
                     //on E: Exception do
                     //{
                     //   MessageDlg('Erreur fatale (WriteFormPos): '  + #13 + #10
                     //               + E.Message, mtError, [mbOk], 0);
                     //   Application.Terminate;
                     //};
                  };
               };
            };
         }
         catch
         {
         };
      };
   }
}
