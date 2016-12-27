using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ZPF
{
   public class MainViewModel : BaseViewModel
   {
      internal static string AppTitle = "Import Connaissance";
      public static string IniFileName = "Import_Connaissance.ini";

      // - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  -

      static MainViewModel _Instance = null;

      public static MainViewModel Instance
      {
         get
         {
            if (_Instance == null)
            {
               _Instance = new MainViewModel();
            };

            return _Instance;
         }

         set
         {
            _Instance = value;
         }
      }

      // - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  -

      public MainViewModel()
      {
         _Instance = this;
      }

      // - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  -
      public bool IsDebug { get; private set; }

      // - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  -

      string _ImportPath;
      public string ImportPath
      {
         get { return _ImportPath; }
         set { SetField(ref _ImportPath, value); }
      }

      // - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  -

      public System.IO.FileInfo[] ImportFiles
      {
         get
         {
            try
            {
               return new System.IO.DirectoryInfo(ImportPath).GetFiles("*.pdf");
            }
            catch
            {
               return null;
            };
         }
      }

      // - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  -

      public async Task<bool> Save()
      {
         TIniFile IniFile = new TIniFile(IniFileName);

         IniFile.WriteBool("General", "IsDebug", IsDebug);
         IniFile.WriteString("Import", "ImportPath", ImportPath);

         try
         {
            await IniFile.UpdateFile();
         }
         catch (Exception ex)
         {
            Debug.WriteLine(ex.Message);
         };

         return true;
      }

      public bool Load()
      {
         TIniFile IniFile = new TIniFile(IniFileName);

#if PCL
         await IniFile.LoadValues();
#else
         IniFile.LoadValues();
#endif

         IsDebug = IniFile.ReadBool("General", "Debug", true);
         ImportPath = IniFile.ReadString("Import", "ImportPath", "");

         Debug.WriteLine("*** IniFile loaded");
         return true;
      }

      // - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - - 

      public void ReadFormPos(Window window, bool PosOnly, bool FromBorder)
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

      public async void WriteFormPos(Window window, bool FromBorder)
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

                     await IniFile.UpdateFile();
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

                        await IniFile.UpdateFile();
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

      // - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  
   }
}
