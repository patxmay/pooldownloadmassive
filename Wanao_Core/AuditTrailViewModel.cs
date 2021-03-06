using System.Collections.ObjectModel;
using ZPF;
using System.ComponentModel;
using System.Runtime.CompilerServices;

// Analysis disable once CheckNamespace
using System.Collections.Generic;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;

#if NETFX_CORE 
using System.Threading.Tasks;
using Windows.Foundation;
#endif

#if WebService
using System.Web;
using System.Data;
#endif

#if PCL
using PCLStorage;
using System.Threading.Tasks;
#else
using System.Data.Common;
#endif

public enum ErrorLevel { Debug = 1, Log = 2, Warning = 3, Error = 4, Critical = 5 }
public enum MessageBoxType { Info, Warning, Error, Confirmation }

public class AuditTrailItem
{
   public AuditTrailItem()
   {
      TimeStamp = DateTime.Now;
      Level = ErrorLevel.Log;
      Tag = "";
   }

   public DateTime TimeStamp { get; set; }
   public ErrorLevel Level { get; set; }
   public string Tag { get; set; }
   public string Message { get; set; }
   public string Data { get; set; }
   public string TerminalID { get; internal set; }
   public string FKUser { get; set; }
   public string ItemID { get; set; }
   public string ItemType { get; set; }

#if SqlServer || MySQL
   [DB_SQL.Ignore]
#endif
#if SQLLITE
   [SQLite.Net.Attributes.IgnoreAttribute]
#endif
   public bool HasData { get { return !string.IsNullOrEmpty(Data); } }
}


#if AT_TXT
#endif


public class AuditTrailViewModel : BaseViewModel
{
#if DEBUG
   public static int MaxLines = 5000;
   public static int MaxSize = 512 * 1024;
#else
   public static int MaxLines = 500;
   public static int MaxSize = 256 * 1024;
#endif

   public static string DateTimeFormat = "HHmmss";

   // - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  -

   static AuditTrailViewModel _Instance = null;

   public static AuditTrailViewModel Instance
   {
      get
      {
         if (_Instance == null)
         {
            _Instance = new AuditTrailViewModel();
         };

         return _Instance;
      }

      set
      {
         _Instance = value;
      }
   }

   public AuditTrailViewModel()
   {
      FileName = "AuditTrail.txt";

      if (_Instance == null)
      {
         _Instance = this;
      };

#if WINCE
      AuditTrail = new Collection<AuditTrailItem>();
#else
      AuditTrail = new ObservableCollection<AuditTrailItem>();

      InitDB();
#endif
   }

   // - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - - 

   public void InitDB()
   {
#if ! WINCE
#if SqlServer || MySQL
      if (DB_SQL.Connection != null)
      {
         CreateTable();
      };
#endif
#if SQLLITE
      if (DB_SQL.SQLiteConnection != null)
      {
         DB_SQL.SQLiteConnection.CreateTable<AuditTrailItem>();
      };
#endif
#endif
   }

   public TStrings GetLastLines(int NbLines = 10)
   {
      TStrings Result = new TStrings();

      foreach (var l in AuditTrail)
      {
         Result.Add( string.Format("{0} {1} {2} {3} {4} ", l.TimeStamp.ToString("dd HH:mm"), l.Level.ToString(), l.Tag, l.Message, l.Data )); 
      };

      return Result;
   }

   // - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - - 

#if SqlServer || MySQL
   private bool CreateTable()
   {
      bool Result = true;

      if (Result)
      {
         if (DB_SQL.Connection.GetType().ToString().Contains("MsSqlConnection") || DB_SQL.Connection.GetType().ToString() == "System.Data.SqlClient.SqlConnection")
         {
            if (DB_SQL.QuickQuery("SELECT count( TABLE_NAME ) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE' AND TABLE_NAME='AuditTrail'") == "0")
            {
               TStrings SQL = new TStrings();

               SQL.Text = GetResource("ViewModel.AuditTrailViewModel.Create.MSSQL.sql");
               Result = DB_SQL.RunScript(DB_SQL.Connection, "", SQL);

               if (!Result)
               {
                  this.Write(new AuditTrailItem() { Level = ErrorLevel.Error, Message = DB_SQL.LastError + Environment.NewLine + DB_SQL.LastQuery });

                  if (Debugger.IsAttached)
                  {
                     Debugger.Break();
                  };

                  return false;
               }
            };
         };

         if (DB_SQL.Connection.GetType().ToString().Contains("MySqlConnection"))
         {
            if (DB_SQL.QuickQuery("SHOW TABLES LIKE 'AuditTrail'") != "AuditTrail")
            {
               TStrings SQL = new TStrings();

               SQL.Text = GetResource("ViewModel.AuditTrailViewModel.Create.MySQL.sql");
               Result = DB_SQL.RunScript(DB_SQL.Connection, "", SQL);

               if (!Result)
               {
                  this.Write(new AuditTrailItem() { Level = ErrorLevel.Error, Message = DB_SQL.LastError + Environment.NewLine + DB_SQL.LastQuery });

                  if (Debugger.IsAttached)
                  {
                     Debugger.Break();
                  };

                  return false;
               }
            };
         };
      };

      return Result;
   }

   string GetResource(string ResourceName)
   {
      string Result = "";

      string CodeBase = Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetExecutingAssembly().CodeBase);
      if (!ResourceName.StartsWith(CodeBase))
      {
         ResourceName = CodeBase + "." + ResourceName;
      }

      using (Stream stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(ResourceName))
      using (StreamReader reader = new StreamReader(stream))
      {
         Result = reader.ReadToEnd();
      };

      return Result;
   }
#endif

   // - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -

#if WebService
   ErrorLevel _Level = ErrorLevel.Warning;
   public ErrorLevel Level
   {
      get { return AuditTrailViewModel.Instance._Level; }
      set { SetField(ref AuditTrailViewModel.Instance._Level, value); }
   }

   string _Terminal = "";
   public string Terminal
   {
      get { return AuditTrailViewModel.Instance._Terminal; }
      set { SetField(ref AuditTrailViewModel.Instance._Terminal, value); }
   }

   string _Event = "";
   public string Event
   {
      get { return AuditTrailViewModel.Instance._Event; }
      set { SetField(ref AuditTrailViewModel.Instance._Event, value); }
   }


   public DataSet GetAudittrailList()
   {
      String SQL = "SELECT TOP 500 AuditTrail.*, UserAccount.Login FROM AuditTrail LEFT OUTER JOIN UserAccount ON AuditTrail.FKUser = UserAccount.PK WHERE 1=1 ORDER BY AuditTrail.PK DESC";
      String Where = "1=1";

      // - -  - -

      Where += string.Format(" and Level>={0} ", (int)(Level));

      if (!string.IsNullOrEmpty(Terminal))
      {
         Where += string.Format(" and TerminalID = '{0}' ", Terminal);
      };

      if (!string.IsNullOrEmpty(Event) && Event != "01/01/1900 00:00:00")
      {
         Where += string.Format(" and ItemType like '{0}%' ", Event.Left(16));
      };

      SQL = SQL.Replace("1=1", Where);

      // - -  - -

      DataTable dt = DB_SQL.QuickQueryView(SQL) as DataTable;

      return DataTable2DataSet(dt);
   }

   public DataSet GetTerminalList()
   {
      String SQL = "SELECT ' ' AS Name, '' AS Value UNION SELECT DISTINCT TerminalID as Name, TerminalID as Value FROM AuditTrail ORDER BY Name";

      DataTable dt = DB_SQL.QuickQueryView(SQL) as DataTable;

      return DataTable2DataSet(dt);
   }

   public DataSet GetEventList()
   {
      String SQL = @"
SELECT ' ' AS Name, '' AS Value, 1 as SortOrder
UNION 
SELECT Distinct (Tag + ' ' + ItemType) as Name, CONVERT(datetime, ItemType, 104) as Value, 2 as SortOrder FROM AuditTrail where ItemType is not null 
order by SortOrder, Value desc
";

      DataTable dt = DB_SQL.QuickQueryView(SQL) as DataTable;

      return DataTable2DataSet(dt);
   }

   public DataSet GetLastImportList()
   {
      String SQL = "SELECT * FROM AuditTrail where [ItemType] = (select TOP 1[ItemType] from AuditTrail where Tag = 'IMPORT' order by PK desc) and Level > 2";

      DataTable dt = DB_SQL.QuickQueryView(SQL) as DataTable;

      return DataTable2DataSet(dt);
   }

   private DataSet DataTable2DataSet(DataTable dataTable)
   {
      DataSet Result = new DataSet();
      Result.Tables.Add(dataTable);
      return Result;
   }
#endif

   // - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -

#if WINCE
   public Collection<AuditTrailItem> AuditTrail { get; set; }
#else
   public ObservableCollection<AuditTrailItem> AuditTrail { get; set; }
#endif

   // - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  -

#if !PCL
   DbConnection _Connection = null;

   public void Init(DbConnection connection)
   {
      _Connection = connection;
   }
#endif

   // - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  -

   Func<MessageBoxType, string, string, bool> _MsgCallBack = null;

   public void InitMsgCallBack(Func<MessageBoxType, string, string, bool> MsgCallBack)
   {
      _MsgCallBack = MsgCallBack;
   }

   // - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  -

   public string FileName { get; set; }

   string _User = "";
   public string User
   {
      get { return _User; }
      set { SetField(ref _User, value, "User"); }
   }



   // - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  -

   string AuditTrailLevel2String(ErrorLevel Level)
   {
      switch (Level)
      {
         default:
         case ErrorLevel.Debug: return "   ";
         case ErrorLevel.Log: return "*  ";
         case ErrorLevel.Warning: return "** ";
         case ErrorLevel.Error: return "***";
         case ErrorLevel.Critical: return "!!!";
      };
   }

   // - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  -

#if PCL
   public async Task Write(AuditTrailItem auditTrailItem)
   {
      AuditTrail.Add(auditTrailItem);
      await AddToFile(auditTrailItem);

#if SQLLITE
      if (DB_SQL.SQLiteConnection != null)
      {
         DB_SQL.Insert(auditTrailItem);
      };
#endif
   }

#else
   public bool Write(AuditTrailItem auditTrailItem)
   {
#if WebService
#elif WINCE
      AuditTrail.Add(auditTrailItem);
#else
      System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
      {
         AuditTrail.Add(auditTrailItem);

         OnPropertyChanged("AuditTrail");
      }));
#endif

      // - - -

      AddToFile(auditTrailItem);

      // - - -

      if (_Connection != null)
      {
         try
         {
#if WINCE
            if (Debugger.IsAttached)
            {
               Debugger.Break();
            };
#else

            if (string.IsNullOrEmpty(auditTrailItem.FKUser))
            {
               if (_User != null)
               {
                  auditTrailItem.FKUser = _User;
               }
            };

#if WebService
            if (string.IsNullOrEmpty(auditTrailItem.TerminalID))
            {
               if (HttpContext.Current.Session != null)
               {
                  auditTrailItem.TerminalID = HttpContext.Current.Request.UserHostAddress;
               }
            };
#else
            if (string.IsNullOrEmpty(auditTrailItem.TerminalID))
            {
               auditTrailItem.TerminalID = System.Environment.MachineName;
            };
#endif

            if (_Connection.State == System.Data.ConnectionState.Closed)
            {
               if (Debugger.IsAttached)
               {
                  Debugger.Break();
               };

               _Connection.Open();
            };

            if (!DB_SQL.QuickInsert(_Connection, "AuditTrail", auditTrailItem))
            {
               Debug.WriteLine(DB_SQL.LastError);
               Debug.WriteLine(DB_SQL.LastQuery);

               if (Debugger.IsAttached)
               {
                  Debugger.Break();
               };
            };
#endif
         }
         catch (Exception ex)
         {
            Debug.WriteLine(DB_SQL.LastError);
            Debug.WriteLine(DB_SQL.LastQuery);

            if (Debugger.IsAttached)
            {
               Debugger.Break();
            };
         };
      };

      return true;
   }
#endif

   // - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  -

#if PCL
   public async Task AddToFile(AuditTrailItem auditTrailItem)
   {
#endif
#if NETFX_CORE || WINDOWS_PHONE
   public async Task<AsyncStatus> AddToFile(AuditTrailItem auditTrailItem)
   {
#endif
#if WPF || WINCE || WebService
   private void AddToFile(AuditTrailItem auditTrailItem)
   {
#endif
      try
      {
         string Line = String.Format("{0} {1} {2}", auditTrailItem.TimeStamp.ToString(DateTimeFormat), AuditTrailLevel2String(auditTrailItem.Level), auditTrailItem.Message);

         System.Diagnostics.Debug.WriteLine(Line);

#if Desktop || WPF
         StreamWriter SW;
         SW = File.AppendText(FileName);
         SW.WriteLine(Line);

         if ((auditTrailItem.Level == ErrorLevel.Critical) && (auditTrailItem.HasData))
         {
            SW.WriteLine(auditTrailItem.Data);
            SW.WriteLine("");
         };

         SW.Close();
#endif

#if PCL
         IFolder rootFolder = FileSystem.Current.LocalStorage;
         IFile file = await rootFolder.CreateFileAsync(FileName, CreationCollisionOption.OpenIfExists);

         using (Stream writer = await file.OpenAsync(FileAccess.ReadAndWrite, System.Threading.CancellationToken.None))
         {
            byte[] array = Encoding.UTF8.GetBytes(Line + "\r\n");

            writer.Seek(0, SeekOrigin.End);
            await writer.WriteAsync(array, 0, array.Length);

            await writer.FlushAsync();
         }
#endif

#if NETFX_CORE || WINDOWS_PHONE
#if DEBUG
         System.Diagnostics.Debug.WriteLine(Line);
#endif
         StorageFolder myFolder = Windows.Storage.ApplicationData.Current.LocalFolder;

         using (Stream s = await myFolder.OpenStreamForWriteAsync(Path.GetFileName(FileName), CreationCollisionOption.OpenIfExists))
         {
            byte[] array = Encoding.UTF8.GetBytes(Line + Environment.NewLine);

            s.Seek(0, SeekOrigin.End);
            s.Write(array, 0, array.Length);

            await s.FlushAsync();
         };
#endif
      }
      catch
      {
      }

#if NETFX_CORE || WINDOWS_PHONE
      return AsyncStatus.Completed;
#endif
   }

   // - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  -

   public void AuditTrailCallBack(ErrorLevel Level, string Line)
   {
      AuditTrail.Add(new AuditTrailItem() { TimeStamp = DateTime.Now, Level = Level, Message = Line });
   }

   // - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  -

#if PCL
   public async Task Clear()
   {
      AuditTrail.Clear();

      try
      {
         IFolder rootFolder = FileSystem.Current.LocalStorage;
         IFile file = await rootFolder.CreateFileAsync(FileName, CreationCollisionOption.OpenIfExists);

         if (file != null)
         {
            await file.DeleteAsync(System.Threading.CancellationToken.None);
         };
      }
      catch
      {
      };
   }
#endif

#if Desktop || WPF
   public void Clear()
   {
      AuditTrail.Clear();

      try
      {
         File.Delete(FileName);
      }
      catch { };
   }
#endif

#if NETFX_CORE || WINDOWS_PHONE
   public async void Clear()
   {
     AuditTrail.Clear();

     try
      {
         StorageFile sf = await ApplicationData.Current.LocalFolder.GetFileAsync(FileName);
         await sf.DeleteAsync(StorageDeleteOption.Default);
      }
      catch { };
   }
#endif

   // - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  -

#if Desktop || WINCE || WPF
   public void Clean()
   {
      while (AuditTrail.Count > MaxLines) AuditTrail.RemoveAt(0);

      try
      {
         StreamReader streamReader = null;

         try
         {
            streamReader = new StreamReader(FileName);
            System.Collections.ArrayList lines = new System.Collections.ArrayList();

            string line;

            while ((line = streamReader.ReadLine()) != null)
            {
               lines.Add(line);
            }

            streamReader.Close();

            if (lines.Count > MaxLines)
            {
               StreamWriter streamWriter = new StreamWriter(FileName);

               for (int i = lines.Count - MaxLines; i < lines.Count; i++)
               {
                  streamWriter.WriteLine(lines[i]);
               };

               streamWriter.Close();
            };
         }
         catch (OutOfMemoryException ex)
         {
            streamReader.Close();

            StreamWriter streamWriter = new StreamWriter(FileName);
            streamWriter.WriteLine("*** AuditTrail.Clean() ***");
            streamWriter.WriteLine(ex.ToString());
            streamWriter.Close();
         };
      }
      catch { };
   }
#endif

#if NETFX_CORE || WINDOWS_PHONE || PCL
#if PCL
   public async Task Clean()
#else
   public async Task<AsyncStatus> Clean()
#endif
   {
      while (AuditTrail.Count > MaxLines) AuditTrail.RemoveAt(0);
      Exception ex = null;

      try
      {
         TStrings Lines = new TStrings();

#if WINDOWS_PHONE
         //Lines.LoadFromFile(FileName, Encoding.UTF8, MaxSize);
         Lines.LoadFromFile(FileName, Encoding.UTF8 );
#endif

#if NETFX_CORE || PCL
         await Lines.LoadFromFile(FileName, Encoding.UTF8, MaxSize);
#endif

         while (Lines.Count > MaxLines)
         {
            Lines.Delete(0);
         }
         ;

#if WINDOWS_PHONE
         Lines.SaveToFile(FileName, Encoding.UTF8);
         Write("","");
#endif

#if NETFX_CORE || PCL
         Lines.Add("\r\n");
         await Lines.SaveToFile(FileName, Encoding.UTF8);
#endif
      }
      catch (Exception _ex)
      {
         ex = _ex;
      };

      if (ex != null)
      {
         try
         {
#if PCL
#else
            StorageFolder myFolder = Windows.Storage.ApplicationData.Current.LocalFolder;

            using (Stream s = await myFolder.OpenStreamForWriteAsync(Path.GetFileName(FileName), CreationCollisionOption.ReplaceExisting))
            {
               using (StreamWriter sw = new StreamWriter(s, Encoding.UTF8))
               {
                  sw.WriteLine(new String('-', 40));
                  sw.WriteLine(DateTime.Now.ToString("dd/MM/yy HH:mm:ss"));
                  sw.WriteLine("*** AuditTrail.Clean() ***");
                  sw.WriteLine(ex.ToString());
               };
            };
#endif
         }
         catch
         {
         };
      };

#if PCL
#else
      return AsyncStatus.Completed;
#endif
   }

   public Task Add(string message)
   {
      throw new NotImplementedException();

      //#if SQLLITE
      //      if (DB_SQL.SQLiteConnection != null)
      //      {
      //         DB_SQL.Insert(auditTrailItem);
      //      };
      //#endif
   }
#endif

   // - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  -
}


public static class AuditTrail
{
#if PCL
   public static async Task Write(ErrorLevel errorLevel, string Message,
                                  [CallerMemberName] string memberName = "",
                                  [CallerFilePath] string sourceFilePath = "",
                                  [CallerLineNumber] int sourceLineNumber = 0)
   {
      await Write(new AuditTrailItem()
      {
         Level = errorLevel,
         Message = Message,
      });
   }


   public static async Task Add(string Message)
   {
      await AuditTrailViewModel.Instance.Add(Message);
   }
#else
   public static void Write(ErrorLevel errorLevel, string Message)
   {
      Write(new AuditTrailItem()
      {
         Tag = "",
         Level = errorLevel,
         Message = Message,
      });
   }
#endif


#if PCL
   public static async Task Write(string Tag, string Message)
   {
      await Write(new AuditTrailItem()
      {
         Tag = Tag,
         Message = Message,
      });
   }
#else
   public static void Write(string Tag, string Message)
   {
      Write(new AuditTrailItem()
      {
         Tag = Tag,
         Message = Message,
      });
   }

   public static void Write(string Tag, string Message, string Data)
   {
      Write(new AuditTrailItem()
      {
         Tag = Tag,
         Message = Message,
         Data = Data
      });
   }
#endif


#if PCL
   public static async Task Write(ErrorLevel errorLevel, Exception ex)
   {
      await Write(errorLevel, ex, "");
   }
#else
   public static void Write(ErrorLevel errorLevel, Exception ex)
   {
      Write(errorLevel, ex, "");
   }
#endif


#if PCL
   public static async Task Write(ErrorLevel errorLevel, Exception ex, string Data)
   {
      await Write(new AuditTrailItem()
      {
         Level = errorLevel,
         Message = ex.Message,
         Data = (string.IsNullOrEmpty(Data) ? ex.StackTrace : Data + Environment.NewLine + Environment.NewLine + ex.StackTrace),
      });
   }
#else
   public static void Write(ErrorLevel errorLevel, Exception ex, string Data)
   {
      Write(new AuditTrailItem()
      {
         Level = errorLevel,
         Message = ex.Message,
         Data = (string.IsNullOrEmpty(Data) ? ex.StackTrace : Data + Environment.NewLine + Environment.NewLine + ex.StackTrace),
      });
   }
#endif

#if PCL
   public static async Task Write(AuditTrailItem auditTrailItem)
   {
      await AuditTrailViewModel.Instance.Write(auditTrailItem);
   }
#else
   public static void Write(AuditTrailItem auditTrailItem)
   {
      AuditTrailViewModel.Instance.Write(auditTrailItem);
   }
#endif

   // - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  -

   public static void MessageBox(ErrorLevel errorLevel, string Message)
   {
      Write(ErrorLevel.Log, Message);
      BackboneViewModel.Instance.MessageBox(MessageBoxType.Info, Message);
   }

   public static void MessageBox(ErrorLevel errorLevel, string Message, string Data)
   {
      Write(new AuditTrailItem { Level = errorLevel, Tag = "", Message = Message, Data = Data });

      switch (errorLevel)
      {
         case ErrorLevel.Critical:
         case ErrorLevel.Error:
            BackboneViewModel.Instance.MessageBox(MessageBoxType.Error, Message);
            break;

         case ErrorLevel.Warning:
            BackboneViewModel.Instance.MessageBox(MessageBoxType.Warning, Message);
            break;

         default:
            BackboneViewModel.Instance.MessageBox(MessageBoxType.Info, Message);
            break;
      };
   }

   public static void MessageBox(ErrorLevel errorLevel, Exception ex)
   {
      MessageBox(errorLevel, ex.Message, ex.ToString());
   }

#if WINCE || WPF || WebService
   public static void MessageBoxSQL()
   {
      MessageBox(ErrorLevel.Warning, DB_SQL.LastError, DB_SQL.LastQuery);
   }
#endif

   // - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  -

#if NETFX_CORE || WINDOWS_PHONE
   public static async Task<AsyncStatus> WriteHeader(string ProgName, string ProgVersion, string TerminalID)
   {
      await AuditTrail.WriteNH(ErrorLevel.Log, new String('-', 40));
      await AuditTrail.WriteNH(ErrorLevel.Log, DateTime.Now.ToString("dd/MM/yy HH:mm:ss"));
      await AuditTrail.WriteNH(ErrorLevel.Log, String.Format("{0} {1}", ProgName, ProgVersion));
#endif
#if PCL
   public static async Task WriteHeader(string ProgName, string ProgVersion, string TerminalID)
   {
      await AuditTrail.Write(ErrorLevel.Log, new String('-', 40), "");
      await AuditTrail.Write(ErrorLevel.Log, DateTime.Now.ToString("dd/MM/yy HH:mm:ss"), "");
      await AuditTrail.Write(ErrorLevel.Log, String.Format("{0} {1}", ProgName, ProgVersion), "");
#endif
#if WINCE || WPF || WebService
   public static void WriteHeader(string ProgName, string ProgVersion, string TerminalID)
   {
      AuditTrail.Write(ErrorLevel.Log, new String('-', 40));
      AuditTrail.Write(ErrorLevel.Log, DateTime.Now.ToString("dd/MM/yy HH:mm:ss"));
      AuditTrail.Write(ErrorLevel.Log, String.Format("{0} {1}", ProgName, ProgVersion));
#endif

#if PocketPC || WINCE
      if (String.IsNullOrEmpty(TerminalID))
      {
         AuditTrail.Write(ErrorLevel.Log, String.Format("Terminal {0}", TerminalID));
      };

      AuditTrail.Write(ErrorLevel.Log, "OEMInfo  " + Platform.GetOemInfo());
      AuditTrail.Write(ErrorLevel.Log, "Type     " + Platform.GetPlatformType());
#endif

#if NETFX_CORE || WINDOWS_PHONE
      return AsyncStatus.Completed;
#endif
   }

   // - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  -

   static int StartStamp = System.Environment.TickCount;
   static int LapStamp = System.Environment.TickCount;

   public static void InitTimeStamp()
   {
      StartStamp = System.Environment.TickCount;
      LapStamp = StartStamp;
   }

#if NETFX_CORE || WINDOWS_PHONE || PCL
   public static async Task<string> WriteTimeStamp(string Message)
#endif
#if PocketPC || WINCE || DESKTOP || WPF || WebService
   public static string WriteTimeStamp(string Message)
#endif
   {
      int Stamp = System.Environment.TickCount;
      TimeSpan dt = TimeSpan.FromMilliseconds(Stamp - LapStamp);
      LapStamp = Stamp;

      var ai = new AuditTrailItem
      {
         Message = string.Format("{0:00}:{1:00}:{2:00}.{3:000} {4}",
         dt.Hours, dt.Minutes, dt.Seconds, dt.Milliseconds,
         Message)
      };

#if NETFX_CORE || WINDOWS_PHONE || PCL
      await AuditTrailViewModel.Instance.Write(ai);
#endif
#if PocketPC || WINCE
      AuditTrailViewModel.Instance.Write(ai);
#endif

      return ai.Message;
   }


#if NETFX_CORE || WINDOWS_PHONE || PCL
   public static async Task<string> WriteTotalTime(string Message)
#endif
#if PocketPC || WINCE || DESKTOP || WPF || WebService
   public static string WriteTotalTime(string Message)
#endif
   {
      int Stamp = System.Environment.TickCount;
      TimeSpan dt = TimeSpan.FromMilliseconds(Stamp - StartStamp);
      LapStamp = Stamp;

      var ai = new AuditTrailItem
      {
         Message = string.Format("{0:00}:{1:00}:{2:00}.{3:000} {4}",
         dt.Hours, dt.Minutes, dt.Seconds, dt.Milliseconds,
         Message)
      };

#if NETFX_CORE || WINDOWS_PHONE || PCL
      await AuditTrailViewModel.Instance.Write(ai);
#endif
#if PocketPC || WINCE
      AuditTrailViewModel.Instance.Write(ai);
#endif

      return ai.Message;
   }


   // - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  -

#if !WindowsCE
   public static void Debug(string Msg, [CallerMemberName] string memberName = "", [CallerLineNumber] int sourceLineNumber = 0)
   {
      System.Diagnostics.Debug.WriteLine(string.Format("[{0} {1:d4}] {2}", memberName, sourceLineNumber, Msg));
   }

#if PCL
   public static async Task WriteIfMsg(ErrorLevel errorLevel, string Message,
                                  [CallerMemberName] string memberName = "",
                                  [CallerFilePath] string sourceFilePath = "",
                                  [CallerLineNumber] int sourceLineNumber = 0)
   {
      if (!string.IsNullOrEmpty(Message))
      {
         await Write(errorLevel, Message, memberName, sourceFilePath, sourceLineNumber);
      };
   }

#else
   public static void WriteIfMsg(ErrorLevel errorLevel, string Message,
                                  [CallerMemberName] string memberName = "",
                                  [CallerFilePath] string sourceFilePath = "",
                                  [CallerLineNumber] int sourceLineNumber = 0)
   {
      if (!string.IsNullOrEmpty(Message))
      {
         Write(new AuditTrailItem()
         {
            Level = errorLevel,
            Message = Message,
            Data = string.Format("[{0} {1:d4}]", memberName, sourceLineNumber),
         });
      };
   }
#endif

#endif

   // - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  -
}


