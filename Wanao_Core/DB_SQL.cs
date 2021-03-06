#if DEBUG
#if WINCE
#else
// #define TRACE_SQL
#endif
#endif

#if PocketPC
#define SqlCe
#endif

#if WebService
#define SqlServer
using System.Configuration;
#endif

using System;
using System.Data;
using System.Data.Common;


#if PocketPC || WINCE
using System.Text;
using System.Data.SqlClient;
using ZPF;
#else

using System.Data.OleDb;

#if WPF
using ZPF;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Diagnostics;
#else
#if Desktop
using System.Windows.Forms;
#else
#endif
#endif
#endif

#if SqlCe
using System.Data.SqlServerCe;
using System.Reflection;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
#endif

#if SQLite
using System.Data.SQLite;
#endif

#if SqlServer
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Reflection;
using System.Diagnostics;
using System.Linq;
#endif

#if MySQL
using MySql.Data.MySqlClient;
#endif

#if Oracle
using System.Data.OracleClient;
#endif


#if PostgreSQL
using Npgsql; 
#endif


namespace ZPF
{
   /// <summary>
   /// DB_SQLCE is the helper class for "Microsoft SQL Server Compact Edition" ...
   /// DB_SQL is the helper class for "Microsoft SQL Server (express or not)" ...
   /// 
   /// 22/01/07 - ME  - Creation based on Delphi stuff ...
   /// 23/01/07 - ME  - Silent (error) logging
   /// 10/06/08 - ME  - QuickQueryList: Name=Value if more than one field
   /// 10/06/08 - ME  - Created based on DB_SQL.cs
   /// 28/10/08 - ME  - Fusion DB_SQL & DB_SQLCE
   /// 19/01/09 - ME  - Add: Conditional compiler switchs for webservice
   /// 24/04/09 - ME  - Add: string GetConnectionString( string ConnectionStringName )
   /// 02/07/09 - ME  - Add: MySQL support & "define" MySQL, SqlCe, SqlServer 
   /// 30/07/09 - ME  - BugFix: NewConnection( ... )
   /// 02/10/09 - ME  - Add: Filter the comments ( -- ) at the end of script lines
   /// 08/10/09 - ME  - Add: string GetSchema( DbConnection dbConnection )
   /// 08/10/09 - ME  - Add: string GetTableStructure( DbConnection dbConnection, string TableName )
   /// 08/11/09 - ME  - Add: string QuickUpdate( DbConnection Connection, string TableName, string WhereCondition, TStrings Record )
   /// 14/01/10 - ME  - Add: string LastError
   /// 16/03/11 - ME  - Add: string ImportDataTable( DbConnection dbConnection, string TableName, DataTable dt, bool p )
   /// 02/08/11 - ME  - Add: types in QuickQueryRec
   /// 18/09/12 - ME  - Add: Use of default( DB_SQL.Connection) connection 
   /// 03/10/12 - ME  - BugFix: QuickInsert: Add: StringToSQL
   /// 03/10/12 - ME  - BugFix: QuickUpdate: Add: StringToSQL
   /// 23/10/12 - ME  - Add: QuickQueryRec: Double
   /// 07/02/14 - ME  - Add: QuickQueryRec: Single
   /// 21/03/14 - ME  - Add: string QuickInsert(string TableName, Object obj) - Update: string QuickInsert(string TableName, TStrings Record)
   /// 28/05/14 - ME  - Add: System.Drawing.Image QuickQueryImage(DbConnection dbConnection, string SQL)
   /// 05/06/14 - ME  - BugFix: QuickQueryImage(DbConnection dbConnection, string SQL) - Reader.Close();
   /// 12/06/14 - ME  - Add: T QuickQuery<T>(DbConnection dbConnection, string SQL)
   /// 26/01/15 - ME  - Add: Support for OleDB
   /// 09/02/15 - ME  - Add: Support for Oracle
   /// 09/02/15 - ME  - Reengenering: DbConnection Open(DBType dbType, string ConnectionString)
   /// 07/12/15 - ME  - Add: bool QuickDelete(DbConnection dbConnection, string TableName, Object obj)
   /// 20/12/15 - ME  - Add: support for DataRow: TStrings ObjToRecord(object obj, ObjToRecordActionType Action= ObjToRecordActionType.IncludePK, bool Hmm = true, bool StringToSQL = true)
   /// 09/01/16 - ME  - Add: bool QuickCheck(DbConnection dbConnection, string TableName, Object obj)
   /// 03/02/16 - ME  - Add: DataSet QuickQueryDataSet(string SQL);
   /// 26/06/16 - ME  - Add: int QuickQueryInt(string SQL)
   /// 26/06/16 - ME  - Add: int QuickQueryInt(string SQL, object[] args)
   /// 26/06/16 - ME  - Add: string Update(object Object)
   /// 02/07/16 - ME  - Add: bool Delete(object Object)
   /// 02/07/16 - ME  - Add: bool Delete(DbConnection dbConnection, object Object)
   /// 02/07/16 - ME  - Add: bool Delete(DbConnection dbConnection, string TableName, object Object)
   /// 01/11/16 - ME  - Add: ZPF.TStrings DataRowToRecord(DataRow row)
   /// 15/11/16 - ME  - Add: support for SQLite
   /// 24/11/16 - ME  - Bugfix (SQLite): string DateTimeToSQL(DateTime dateTime)
   /// 01/12/16 - ME  - Change: string Update(object Object) --> bool Update(object Object)
   /// 04/12/16 - ME  - Add: Transaction, Commit, Rollback
   /// 04/12/16 - CMO  - Add: support for PostgreSQL
   /// 
   /// <para>© 2005 .. 2016 ZePocketForge.com, SAS ZPF.</para>
   /// 
   /// </summary>

   public class DB_SQL
   {
      public static bool IsConsole = false;
      public static TStrings Log = new TStrings();
      public static String LastQuery = "";
      public static String LastError = "";

      public static DbConnection Connection = null;
      public static String ConnectionString = "";

      // - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - - 

      [AttributeUsage(AttributeTargets.Property)]
      public class PrimaryKeyAttribute : Attribute
      {
      }

      [AttributeUsage(AttributeTargets.Property)]
      public class IgnoreAttribute : Attribute
      {
      }

      public static int QuickQueryInt(string SQL)
      {
         return QuickQueryInt(SQL, null);
      }

      public static int QuickQueryInt(string SQL, object[] args)
      {
         int Result = 0;

         if (args == null)
         {
            int.TryParse(QuickQuery(SQL), out Result);
         }
         else
         {
            int.TryParse(QuickQuery(SQL, args), out Result);
         };

         return Result;
      }

      [AttributeUsage(AttributeTargets.Property)]
      public class MaxLengthAttribute : Attribute
      {
         public MaxLengthAttribute(int Length)
         {
         }
      }

      // - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - - 

      public static bool RunScript(DbConnection Connection, string ScriptPath, TStrings Script, bool ExecSelect)
      {
         bool Result = true;
         LastQuery = "";
         if (false) Log.Clear();

         for (int i = Script.Count - 1; i >= 0; i--)
         {
            Script[i] = Script[i].Trim();

            if (
               (Script[i].StartsWith("--"))
                  || (Script[i] == ""))
            {
               Script.Delete(i);
            }
            else
            {
               Script.SetObject(i, i + 1);
            };
         };

         //
         // - - -  - - -

         //for( int i=0; i < Script.Count; i++ )
         //{
         //   Console.WriteLine( "{0,5} {1}", (int)Script.GetObject( i), Script[ i ] );
         //};

         //
         // - - -  - - -

         string SQL = "";

         for (int i = 0; i < Script.Count; i++)
         {
            if (Script[i][0] == '@')
            {
               string FileName;

               if (Script[i][1] == '@')
               {
                  // - - - Content - - -

                  if (ScriptPath != "")
                  {
                     FileName = ScriptPath + @"\" + Script[i].Substring(2);
                  }
                  else
                  {
                     FileName = Script[i].Substring(2);
                  };

                  FileName = FileName.TrimEnd(';');

                  TStrings Content = new TStrings();
                  Content.LoadFromFile(FileName);
                  Script[i] = StringToSQL(Content.Text);
               }
               else
               {
                  // - - - Sub-script - - -

                  if (ScriptPath != "")
                  {
                     FileName = ScriptPath + @"\" + Script[i].Substring(1);
                  }
                  else
                  {
                     FileName = Script[i].Substring(1);
                  };

                  FileName = FileName.TrimEnd(';');

                  Log.Add("");
                  Log.Add(String.Format(" - -  - - Begin @{0} - -  - - ", FileName));
                  Log.Add("");

                  Result = RunScript(Connection, FileName);

                  Log.Add("");
                  Log.Add(String.Format(" - -  - - End @{0} - -  - - ", FileName));
                  Log.Add("");

                  Script[i] = " ";
               };

            };

            SQL += "\r\n" + Script[i];

            if (!Script[i].EndsWith(";"))
            {
               string st = Script[i];
               st = st.Replace(" --", "--");

               if (st.IndexOf(";--") != -1)
               {
                  Script[i] = st.Substring(0, st.IndexOf(";--")) + ";";
               };
            };

            if (Script[i].EndsWith(";"))
            {
               try
               {
                  int AffectedRows = -1;

                  LastQuery = SQL.TrimEnd(';');

                  DbCommand Command = NewCommand(SQL, Connection);
                  // Command.CommandTimeout = 120;

                  if (SQL.TrimStart(new char[] { ' ', '\n', '\r' }).ToUpper().StartsWith("SELECT"))
                  {
                     if (ExecSelect)
                     {
                        try
                        {
                           AffectedRows = DoSelect(Command);
                        }
                        catch (Exception ex)
                        {
                           string Msg = ex.Message;

                           if (IsConsole)
                           {
                              Console.WriteLine("Error (S): {0}", Msg);
                              Console.WriteLine("");
                           };

                           Log.Add(String.Format("Error (S): {0}", Msg));
                           Log.Add("");
                        };
                     };
                  }
                  else
                  {
#if SqlCe
                            if (Command.Connection is SqlCeConnection)
                            {
                                // 
                                if (SQL.ToUpper().IndexOf("CREATE TRIGGER") > -1 && SQL.ToUpper().IndexOf("CREATE TRIGGER") < 10)
                                {
                                    AffectedRows = -1;
                                    Log.Add("Skipped CREATE TRIGGER");
                                }
                                else
                                {
                                    AffectedRows = Command.ExecuteNonQuery();
                                };
                            }
                            else
#endif
                     {
                        AffectedRows = Command.ExecuteNonQuery();
                     };
                  };

                  if (AffectedRows != -1)
                  {
                     if (IsConsole)
                     {
                        Console.WriteLine("{0} Affected Row(s)", AffectedRows);
                        Console.WriteLine("");
                     };

                     Log.Add(String.Format("{0} Affected Row(s)", AffectedRows));
                     Log.Add("");
                  };
               }
               catch (Exception ex)
               {
                  Result = false;
                  string Msg = ex.Message;

                  if (IsConsole)
                  {
                     Console.WriteLine("{0,5} {1}", (int)Script.GetObject(i), SQL);
                     Console.WriteLine("{0,5} {1}", "", Msg);
                  };

                  //if( ex.Data != null )
                  //{
                  //   Msg += "\n";

                  //   foreach( DictionaryEntry de in ex.Data )
                  //   {
                  //      Msg +=  String.Format( "{0}={1}\n", de.Key, de.Value );
                  //   };
                  //}

                  Log.Add(String.Format("{0,5} {1}", (int)Script.GetObject(i), SQL));
                  Log.Add(String.Format("{0,5} {1}", "", Msg));
               };

               SQL = "";
            }
            else
            {
            };
         };

         return Result;
      }

      public static bool RunScript(DbConnection Connection, string ScriptPath, TStrings Script)
      {
         return RunScript(Connection, ScriptPath, Script, true);
      }

      public static bool RunScript(DbConnection Connection, string ScriptName)
      {
         TStrings Script = new TStrings();
         Script.LoadFromFile(ScriptName);

         return RunScript(Connection, System.IO.Path.GetDirectoryName(ScriptName), Script);
      }

      // - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - - 

      internal static int DoSelect(DbCommand Command)
      {
         DbDataReader Reader = NewReader(Command);
         int AffectedRows = 0;

         DataTable dt = Reader.GetSchemaTable();
         TStrings Cols = new TStrings();
         string st = "";

         foreach (DataRow row in dt.Rows)
         {
            int Ind = Cols.Add((string)row["ColumnName"]);
            long l = Convert.ToInt64(row["ColumnSize"].ToString());

            int Size = 20;

            if (l < 20)
            {
               Size = (int)l;
            };

            if (row["DataType"].ToString() == "System.Int32")
            {
               if (Size < 5) Size = 5;
            };

            if (row["DataType"].ToString() == "System.DateTime")
            {
               Size = 19;
            };

            Cols.SetObject(Ind, Size);

            try
            {
               st += Convert.ToString(row["ColumnName"]).PadRight((int)Size).Substring(0, (int)Size) + " ";
            }
            catch
            {
               if (IsConsole)
               {
                  Console.WriteLine("Size=" + Size.ToString());
                  Console.WriteLine("ColumnName=" + Convert.ToString(row["ColumnName"]));
               };

               Log.Add("Size=" + Size.ToString());
               Log.Add("ColumnName=" + Convert.ToString(row["ColumnName"]));
            };
         };

         if (IsConsole)
         {
            Console.WriteLine(st);
         };

         Log.Add(st);

         while (Reader.Read())
         {
            st = "";
            for (int x = 0; x < Reader.FieldCount; x++)
            {
               int Size = (int)Cols.GetObject(x);
               // st += String.Format( "{0," + ((int)Cols.GetObject( x )).ToString()  + "} ", Reader[ x ].ToString() );

               st += Reader[x].ToString().PadRight((int)Size + 1).Substring(0, Size + 1);
            };

            if (IsConsole)
            {
               Console.WriteLine(st);
            };
            Log.Add(st);

            AffectedRows += 1;
         };

         Reader.Close();

         return AffectedRows;
      }

      // - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - - 

      public static DbTransaction Transaction()
      {
         return Transaction(DB_SQL.Connection);
      }

      public static DbTransaction Transaction(DbConnection dbConnection)
      {
#if SqlCe
         if (dbConnection.GetType().Name == "SqlCeConnection")
         {
            throw new NotImplementedException();
         };
#endif

#if SqlServer
         if (dbConnection is SqlConnection)
         {
            throw new NotImplementedException();
         };
#endif

#if SQLite
         if (dbConnection is SQLiteConnection)
         {
            var c = (dbConnection as SQLiteConnection);
            return c.BeginTransaction();
         };
#endif

#if MySQL
         if (dbConnection is MySqlConnection)
         {
            throw new NotImplementedException();
         };
#endif

#if OleDB
         if (dbConnection is OleDbConnection)
         {
            throw new NotImplementedException();
         };
#endif

#if Oracle
         if (dbConnection is OracleConnection)
         {
            throw new NotImplementedException();
         };
#endif

         return null;
      }

      public static void Commit(DbTransaction dbTransaction)
      {
#if SqlCe
         if (dbTransaction.GetType().Name == "SqlCeTransaction")
         {
            throw new NotImplementedException();
         };
#endif

#if SqlServer
         if (dbTransaction is SqlTransaction)
         {
            throw new NotImplementedException();
         };
#endif

#if SQLite
         if (dbTransaction is SQLiteTransaction)
         {
            var t = (dbTransaction as SQLiteTransaction);
            t.Commit();
         };
#endif

#if MySQL
         if (dbTransaction is MySqlTransaction)
         {
            throw new NotImplementedException();
         };
#endif

#if OleDB
         if (dbTransaction is OleDbTransaction)
         {
            throw new NotImplementedException();
         };
#endif

#if Oracle
         if (dbTransaction is OracleTransaction)
         {
            throw new NotImplementedException();
         };
#endif
      }

      public static void Rollback(DbTransaction dbTransaction)
      {
#if SqlCe
         if (dbTransaction.GetType().Name == "SqlCeTransaction")
         {
            throw new NotImplementedException();
         };
#endif

#if SqlServer
         if (dbTransaction is SqlTransaction)
         {
            throw new NotImplementedException();
         };
#endif

#if SQLite
         if (dbTransaction is SQLiteTransaction)
         {
            var t = (dbTransaction as SQLiteTransaction);
            t.Rollback();
         };
#endif

#if MySQL
         if (dbTransaction is MySqlTransaction)
         {
            throw new NotImplementedException();
         };
#endif

#if OleDB
         if (dbTransaction is OleDbTransaction)
         {
            throw new NotImplementedException();
         };
#endif

#if Oracle
         if (dbTransaction is OracleTransaction)
         {
            throw new NotImplementedException();
         };
#endif
      }

      // - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - - 

#if SqlServer || MySQL || DNN
      /// <summary>
      /// Retrieve ConnectionString from the web.config file
      /// </summary>
      /// <param name="str">Name of the connection</param>
      /// <remarks>Need a reference to the System.Configuration Namespace</remarks>
      /// <returns></returns>
#if Desktop || WPF
#else
      public static string GetConnectionString(string ConnectionStringName)
      {
         //variable to hold our return value
         string Result = string.Empty;

         //check if a value was provided
         if (!string.IsNullOrEmpty(ConnectionStringName))
         {
            //name provided so search for that connection
            Result = ConfigurationManager.ConnectionStrings[ConnectionStringName].ConnectionString;
         };

         //return the value
         return Result;
      }
#endif

      // DB_SQL.Connection = DB_SQL.Open( GetConnectionString( "ConnectionStringName" );
#endif

      // - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  

      public enum DBType { Oracle, SQLServer, MySQL, OleDB, SQLite }

#if WindowsCE
        public static DbConnection Open(DBType dbType, string ConnectionString)
      {
          bool CopyContext = true;
#else
      public static DbConnection Open(DBType dbType, string ConnectionString, bool CopyContext = true)
      {
#endif
         DbConnection Result = null;
         LastError = "";

         try
         {
            if (dbType == DBType.Oracle)
            {
#if Oracle
               Result = new OracleConnection(ConnectionString);
#endif
            };

            if (dbType == DBType.SQLServer)
            {
#if SqlServer
               Result = new SqlConnection(ConnectionString);
#endif
            };

            if (dbType == DBType.SQLite)
            {
#if SQLite
               // https://www.connectionstrings.com/sqlite-net-provider/
               Result = new SQLiteConnection(ConnectionString);
#endif
            };

            if (dbType == DBType.MySQL)
            {
#if MySQL
               Result = new MySqlConnection(ConnectionString);
#endif
            };

            if (dbType == DBType.OleDB)
            {
#if OleDB
               Result = new OleDbConnection(ConnectionString);
#endif
            };

            if (CopyContext)
            {
               DB_SQL.Connection = Result;
               DB_SQL.ConnectionString = ConnectionString;
            };

            Result.Open();

            return Result;
         }
         catch (Exception ex)
         {
            LastError = ex.Message;
            //MessageBox.Show(ex.Message);
            if (Debugger.IsAttached)
            {
               Debugger.Break();
            };
         };

         return null;
      }

      public static void DumpDataBase(string FileName)
      {
         string SQL = DB_SQL.GetSchema(DB_SQL.Connection, true, false, false);

         TStrings tables = new TStrings();
         tables = DB_SQL.QuickQueryList(SQL);

         DB_SQL.QuickQuery2File(FileName, "Select GetDate()");

         for (int i = 0; i < tables.Count; i++)
         {
            DB_SQL.QuickQuery2File(FileName, "select * from " + tables.Names(i), true, tables.Names(i));
         };
      }

      public static void QuickQuery2File(string FileName, string SQL)
      {
         QuickQuery2File(FileName, SQL, false);
      }

      public static void QuickQuery2File(string FileName, string SQL, bool Append)
      {
         QuickQuery2File(FileName, SQL, Append, null);
      }

      public static void QuickQuery2File(string FileName, string SQL, bool Append, string Header)
      {
         Log.Clear();

         if (Header != null)
         {
            Log.Add("");
            Log.Add("");
            Log.Add((new string('-', 3) + " " + Header + " " + new string('-', 80)).Substring(0, 80));
            Log.Add("");
         };

         DbCommand Command = NewCommand(SQL, Connection);

         try
         {
            int AffectedRows = DB_SQL.DoSelect(Command);

            Log.Add(string.Format("{0} Affected Row(s)", AffectedRows));
         }
         catch (Exception ex)
         {
            string Msg = ex.Message;

            Log.Add(string.Format("Error (S): {0}", Msg));
         };

         if (Append)
         {
            TStrings file = new TStrings();
            file.LoadFromFile(FileName);
            file.Text = file.Text + Environment.NewLine + Log.Text;
            file.SaveToFile(FileName);
         }
         else
         {
            Log.SaveToFile(FileName);
         };
      }

      public static string QuickQuery(string SQL)
      {
         return QuickQuery(DB_SQL.Connection, SQL);
      }

      public static string QuickQuery(string SQL, object[] args)
      {
         throw new NotImplementedException();

         return QuickQuery(DB_SQL.Connection, SQL);
      }

      public static string QuickQuery(DbConnection Connection, string SQL)
      {
         string Result = "";
         LastError = "";
         LastQuery = SQL;

         DbCommand Command = NewCommand(SQL, Connection);

         if (SQL.TrimStart(new char[] { ' ', '\n', '\r' }).ToUpper().StartsWith("SHOW"))
         {
            var o = Command.ExecuteScalar();

            if (o != null)
            {
               Result = o.ToString();
            }
            else
            {
               Result = "";
            }
         }
         else if (SQL.TrimStart(new char[] { ' ', '\n', '\r' }).ToUpper().StartsWith("SELECT"))
         {
            DbDataReader Reader = NewReader(Command);

            if (Reader == null)
            {
               Result = LastError;
               return Result;
            };

            if (Reader.Read())
            {
               Result = Reader[0].ToString();
            };

            Reader.Close();
         }
         else
         {
            try
            {
               Result = Command.ExecuteNonQuery().ToString();
            }
            catch (Exception ex)
            {
               LastError = ex.Message;
               Result = ex.Message;
               //ToDo Exception handeling in DB_SQL.QuickQuery( DbConnection Connection, string SQL )
            };
         };

         return Result;
      }

      // - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -

      public static TStrings QuickQueryList(string SQL)
      {
         return QuickQueryList(Connection, SQL);
      }

      public static TStrings QuickQueryList(DbConnection Connection, string SQL)
      {
         TStrings Result = new TStrings();

         DbCommand Command = NewCommand(SQL, Connection);
         DbDataReader Reader = NewReader(Command);

         if (Reader != null)
         {
            while (Reader.Read())
            {
               if (Reader.FieldCount > 1)
               {
                  Result.Add(Reader[0].ToString().Trim() + "=" + Reader[1].ToString().TrimEnd());
               }
               else
               {
                  Result.Add(Reader[0].ToString().TrimEnd());
               };
            };

            Reader.Close();
         };

         return Result;
      }

      // - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -

      public static TStrings QuickQueryRec(string SQL)
      {
         return QuickQueryRec(Connection, SQL, false);
      }

      public static TStrings QuickQueryRec(DbConnection Connection, string SQL)
      {
         return QuickQueryRec(Connection, SQL, false);
      }

      public static TStrings QuickQueryRec(DbConnection Connection, string SQL, bool SchemaOnly)
      {
         TStrings Result = new TStrings();

         DbCommand Command = NewCommand(SQL, Connection);
         using (DbDataReader Reader = NewReader(Command))
         {
            if (Reader == null) return null;

            using (DataTable dt = Reader.GetSchemaTable())
            {
               if (dt != null)
               {
                  foreach (DataRow row in dt.Rows)
                  {
                     Result.Add((string)row["ColumnName"] + "=");
                  };
               };
            };

            if (Reader.Read())
            {
               for (int i = 0; i < Reader.FieldCount; i++)
               {
                  try
                  {
                     if (Reader.GetFieldType(i).ToString() == "System.Int32")
                     {
                        Result[i] = Result[i] + Reader.GetInt32(i).ToString();
                     }
                     else if (Reader.GetFieldType(i).ToString() == "System.Boolean")
                     {
                        try
                        {
                           Result[i] = Result[i] + Reader.GetBoolean(i).ToString();
                        }
                        catch
                        {
                           Result[i] = Result[i] + false.ToString();
                        };
                     }
                     else if (Reader.GetFieldType(i).ToString() == "System.DateTime")
                     {
                        Result[i] = Result[i] + Reader.GetDateTime(i).ToString("dd/MM/yyyy hh:mm:ss");
                     }
                     else if (Reader.GetFieldType(i).ToString() == "System.Guid")
                     {
                        Result[i] = Result[i] + Reader.GetGuid(i).ToString();
                     }
                     else if (Reader.GetFieldType(i).ToString() == "System.Int16")
                     {
                        Result[i] = Result[i] + Reader.GetInt16(i).ToString();
                     }
                     else if (Reader.GetFieldType(i).ToString() == "System.Decimal")
                     {
                        Result[i] = Result[i] + Reader.GetDecimal(i).ToString();
                     }
                     else if (Reader.GetFieldType(i).ToString() == "System.Int32")
                     {
                        Result[i] = Result[i] + Reader.GetInt32(i).ToString();
                     }
                     else if (Reader.GetFieldType(i).ToString() == "System.Int64")
                     {
                        Result[i] = Result[i] + Reader.GetInt64(i).ToString();
                     }
                     else if (Reader.GetFieldType(i).ToString() == "System.Double")
                     {
                        Result[i] = Result[i] + Reader.GetDouble(i).ToString();
                     }
                     else if (Reader.GetFieldType(i).ToString() == "System.Single")
                     {
                        Result[i] = Result[i] + Reader.GetFloat(i).ToString();
                     }
                     else
                     {
                        if (Reader.IsDBNull(i))
                        {
                        }
                        else
                        {
                           Result[i] = Result[i] + Reader.GetString(i).TrimEnd(' ');
                        };
                     };
                  }
                  catch (Exception ex)
                  {
                     LastError = ex.Message;
                  };
               };
            }
            else
            {
               if (!SchemaOnly) Result.Clear();
            };
         };

         // Reader.Close();

         return Result;
      }

      // - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -
      public static DataSet QuickQueryDataSet(string SQL)
      {
         var dataSet = new DataSet();

         // Create a new data adapter based on the specified query.
         using (DbDataAdapter dataAdapter = NewDataAdapter(SQL, Connection))
         {
            try
            {
               dataAdapter.Fill(dataSet);
            }
            catch (Exception ex)
            {
               LastError = ex.Message;

               return null;
            };
         };

         return dataSet;
      }

      // - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -

#if WindowsCE
      public static object QuickQueryView(string SQL)
      {
         return QuickQueryView(Connection, SQL);
      }
#else
      public static object QuickQueryView(string SQL, bool NoSchema = true)
      {
         return QuickQueryView(Connection, SQL, NoSchema);
      }
#endif

#if WindowsCE
        public static object QuickQueryView(DbConnection Connection, string SQL )
      {
         bool NoSchema = true;
#else
      public static object QuickQueryView(DbConnection Connection, string SQL, bool NoSchema = true)
      {
#endif
         DB_SQL.LastError = "";
         DB_SQL.LastQuery = SQL;

         // Create a new data adapter based on the specified query.
         using (DbDataAdapter dataAdapter = NewDataAdapter(SQL, Connection))
         {
            // Create a command builder to generate SQL update, insert, and
            // delete commands based on selectCommand. These are used to
            // update the database.

            // Populate a new data table and bind it to the BindingSource.
            using (DataTable table = new DataTable())
            {
               table.Locale = System.Globalization.CultureInfo.InvariantCulture;

               try
               {
                  if (!NoSchema)
                  {
                     dataAdapter.FillSchema(table, SchemaType.Source);
                  };

                  dataAdapter.Fill(table);
                  dataAdapter.Dispose();
               }
               catch (Exception ex)
               {
                  LastError = ex.Message;

                  return null;
               };

               return table;
            };
         };
      }


      public static string StringToSQL(string text)
      {
         string Result = text;

         if (text != null)
         {
            Result = Result.Replace("'", "''");
            //Result = Result.Replace(@"\'", @"\\'");
            Result = Result.Replace(@"\", @"\\");
         };

#if MySQL
         if (Connection is MySqlConnection)
         {
            Result = Result.Replace("’", "’’");
         };
#endif

         return Result;
      }

      public static string DateTimeToSQL(DateTime dateTime)
      {
         string Result = "";

         // Result = String.Format( "convert( datetime, '01/02/1900 00:00', 103 )", );
         // Result = String.Format( "convert( datetime, '{0}', 103 )", dateTime.ToString() );

#if MySQL
         if (Connection is MySqlConnection)
         {
            Result = String.Format("STR_TO_DATE('{0}', '%d/%m/%Y %H:%i:%s')", dateTime.ToString("dd/MM/yyyy HH:mm:ss"));
         }
         else
#endif
#if SQLite
         if (Connection is SQLiteConnection)
         {
            // '2016-12-24 12:34:10
            Result = String.Format("{0}", dateTime.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss"));
         }
         else
#endif
         {
            Result = String.Format("convert( datetime, '{0}', 103 )", dateTime.ToString("dd/MM/yyyy HH:mm:ss"));
         };


         return Result;
      }

      public static string BoolToSQL(bool Value)
      {
         return Value ? "1" : "0";
      }

      internal static string FormatString(string Text, int Max)
      {
         if (Text.Length > Max)
         {
            return Text.Substring(0, Max);
         }
         else
         {
            return Text;
         };
      }

      internal static TStrings Reader2List(DbDataReader Reader)
      {
         TStrings Result = new TStrings();

         for (int i = 0; i < Reader.FieldCount; i++)
         {
            //if( Reader.GetDataTypeName( i ) == "UniqueIdentifier" )
            //{
            //   Result.Add( Reader.GetName( i ) + "=" + Reader.GetGuid( i ).ToString() );
            //}
            //else
            {
               Result.Add(Reader.GetName(i) + "=" + Reader.GetValue(i).ToString());
            };
         };

         return Result;
      }

      // - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - - 

      public static DbDataAdapter NewDataAdapter(string SQL, DbConnection Connection)
      {
         DbDataAdapter Result = null;

#if SqlCe
            if (Connection.GetType().Name == "SqlCeConnection")
            {
                Result = new SqlCeDataAdapter(SQL, (Connection as SqlCeConnection));
            };
#endif

#if SqlServer
         if (Connection is SqlConnection)
         {
            Result = new SqlDataAdapter(SQL, (Connection as SqlConnection));
         };
#endif

#if SQLite
         if (Connection is SQLiteConnection)
         {
            Result = new SQLiteDataAdapter(SQL, (Connection as SQLiteConnection));
         };
#endif

#if MySQL
         if (Connection is MySqlConnection)
         {
            Result = new MySqlDataAdapter(SQL, (Connection as MySqlConnection));
         };
#endif

#if OleDB
         if (Connection is OleDbConnection)
         {
            Result = new OleDbDataAdapter(SQL, (Connection as OleDbConnection));
         };
#endif

#if Oracle
         if (Connection is OracleConnection)
         {
            Result = new OracleDataAdapter(SQL, (Connection as OracleConnection));
         };
#endif

         return Result;
      }

      public static DbDataAdapter NewDataAdapter(DbCommand Command)
      {
         DbDataAdapter Result = null;

#if SqlCe
            if (Command is SqlCeCommand)
            {
                Result = new SqlCeDataAdapter(Command as SqlCeCommand);
            };
#endif

#if SqlServer
         if (Command is SqlCommand)
         {
            Result = new SqlDataAdapter(Command as SqlCommand);
         };
#endif

#if SQLite
         if (Command is SQLiteCommand)
         {
            Result = new SQLiteDataAdapter(Command as SQLiteCommand);
         };
#endif

#if MySQL
         if (Command is MySqlCommand)
         {
            Result = new MySqlDataAdapter(Command as MySqlCommand);
         };
#endif

#if OleDB
         if (Connection is OleDbConnection)
         {
            Result = new OleDbDataAdapter(Command as OleDbCommand);
         };
#endif

#if Oracle
         if (Connection is OracleConnection)
         {
            Result = new OracleDataAdapter(Command as OracleCommand);
         };
#endif

         return Result;
      }

      public static DbConnection NewConnection(string ConnectionString)
      {
         DbConnection Result = null;

         //Oracle: Password=SGAMES;Persist Security Info=True;User ID=SGAMES;Data Source=TranseptSGAMES

         if (ConnectionString.ToUpper().IndexOf("PROVIDER=") != -1)
         {
#if PocketPC || WINCE
#else
            Result = new OleDbConnection(ConnectionString);
            Result.Open();
#endif
         }
         else if (ConnectionString.ToUpper().IndexOf(".SDF") != -1)
         {
#if SqlCe
                Result = new SqlCeConnection(ConnectionString);
                Result.Open();
#endif
         }
         else if (ConnectionString.ToUpper().IndexOf("SERVER=") != -1)
         {
#if MySQL
            Result = new MySqlConnection(ConnectionString);
            Result.Open();
#else
#if SqlServer
            Result = new SqlConnection(ConnectionString);
            Result.Open();
#endif
#endif
         }
         else if (ConnectionString.ToUpper().IndexOf("DATA SOURCE=") != -1 && ConnectionString.ToUpper().IndexOf("VERSION") != -1)
         {
#if SQLite
            Result = new SQLiteConnection(ConnectionString);
            Result.Open();
#endif
         }
         else
         {
#if SqlServer
            Result = new SqlConnection(ConnectionString);
            Result.Open();
#endif
         };

         return Result;
      }

      public static DbCommand NewCommand(string SQL)
      {
         return NewCommand(SQL, Connection);
      }

#if TRACE_SQL
   public static DbCommand NewCommand(string SQL, DbConnection Connection, [CallerMemberName] string memberName = "")
#else
      public static DbCommand NewCommand(string SQL, DbConnection Connection)
#endif
      {
         DbCommand Result = null;

#if TRACE_SQL
      AuditTrail.Write("SQL", SQL.Replace("\n", " ").Replace("\r", " "), memberName);
#endif

#if SqlCe
            if (Connection.GetType().Name == "SqlCeConnection")
            {
                Result = new SqlCeCommand(SQL, (Connection as SqlCeConnection));
            };
#endif

#if MySQL
         if (Connection is MySqlConnection)
         {
            Result = new MySqlCommand(SQL, (Connection as MySqlConnection));
         };
#endif

#if SqlServer
         if (Connection is SqlConnection)
         {
            Result = new SqlCommand(SQL, (Connection as SqlConnection));
         };
#endif

#if SQLite 
         if (Connection is SQLiteConnection)
         {
            Result = new SQLiteCommand(SQL, (Connection as SQLiteConnection));
         };
#endif

#if OleDB
         if (Connection is OleDbConnection)
         {
            Result = new OleDbCommand(SQL, (Connection as OleDbConnection));
         };
#endif

#if Oracle
         if (Connection is OracleConnection)
         {
            Result = new OracleCommand(SQL, (Connection as OracleConnection));
         };
#endif

         return Result;
      }

      public static DbDataReader NewReader(DbCommand Command)
      {
         DbDataReader Result = null;

         try
         {
#if SqlCe
                if (Command is SqlCeCommand)
                {
                    Result = (Command as SqlCeCommand).ExecuteReader();

                    //if( Result == null )
                    //{
                    //   Result = (Command as SqlCeCommand).ExecuteReader( CommandBehavior.SchemaOnly );
                    //};
                };
#endif

#if SqlServer
            if (Command is SqlCommand)
            {
               Result = (Command as SqlCommand).ExecuteReader();
            };
#endif

#if SQLite
            if (Command is SQLiteCommand)
            {
               Result = (Command as SQLiteCommand).ExecuteReader();
            };
#endif

#if MySQL
            if (Command is MySqlCommand)
            {
               Result = (Command as MySqlCommand).ExecuteReader();
            };
#endif

#if OleDB
            if (Command is OleDbCommand)
            {
               Result = (Command as OleDbCommand).ExecuteReader();
            };
#endif

#if Oracle
            if (Command is OracleCommand)
            {
               Result = (Command as OracleCommand).ExecuteReader();
            };
#endif
         }
         catch (Exception ex)
         {
            LastError = ex.ToString();
         };

         return Result;
      }

      // - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - - 

      internal static DbCommandBuilder NewCommandBuilder(DbDataAdapter dataAdapter)
      {
         DbCommandBuilder Result = null;

#if SqlCe
            if (dataAdapter is SqlCeDataAdapter)
            {
                Result = new SqlCeCommandBuilder((dataAdapter as SqlCeDataAdapter));
            };
#endif

#if SqlServer
         if (dataAdapter is SqlDataAdapter)
         {
            Result = new SqlCommandBuilder((dataAdapter as SqlDataAdapter));
         };
#endif

#if SQLite
         if (dataAdapter is SQLiteDataAdapter)
         {
            Result = new SQLiteCommandBuilder((dataAdapter as SQLiteDataAdapter));
         };
#endif

#if MySQL
         if (dataAdapter is MySqlDataAdapter)
         {
            Result = new MySqlCommandBuilder((dataAdapter as MySqlDataAdapter));
         };
#endif

#if OleDB
         if (dataAdapter is OleDbDataAdapter)
         {
            Result = new OleDbCommandBuilder((dataAdapter as OleDbDataAdapter));
         };
#endif

#if Oracle
         if (dataAdapter is OracleDataAdapter)
         {
            Result = new OracleCommandBuilder((dataAdapter as OracleDataAdapter));
         };
#endif
         return Result;
      }

      public static string GetSchema(DbConnection dbConnection, bool Tables, bool Views, bool System)
      {
         string Result = "";

#if SqlCe
            if (dbConnection.GetType().Name == "SqlCeConnection")
            {
                Result = "SELECT TABLE_NAME as [List of Tables], Description as [Description] FROM INFORMATION_SCHEMA.TABLES order by TABLE_NAME";
            };
#endif

#if SqlServer
         if (dbConnection is SqlConnection)
         {
            Result = "SELECT TABLE_NAME as 'List of Tables' FROM INFORMATION_SCHEMA.TABLES WHERE 1=1";
            if (!Tables) Result += " and Table_Type <> 'BASE TABLE' ";
            if (!Views) Result += " and Table_Type <> 'VIEW' ";
            if (!System) Result += " and Table_Type <> 'SYSTEM VIEW' ";
            Result += " order by TABLE_NAME";
         };
#endif

#if SQLite
         //ToDo: http://stackoverflow.com/questions/4770716/reading-sqlite-table-information-in-c-net
         throw new NotImplementedException();
#endif

#if MySQL
         if (dbConnection is MySqlConnection)
         {
            if (QuickQuery(dbConnection, "select version();").StartsWith("4."))
            {
               Result = "select version() as 'Attention: Unable to retrieve schema on version';";
            }
            else
            {
               Result = "SELECT distinct TABLE_NAME as 'List of Tables' FROM INFORMATION_SCHEMA.TABLES WHERE 1=1";
               if (!Tables) Result += " and Table_Type <> 'BASE TABLE' ";
               if (!Views) Result += " and Table_Type <> 'VIEW' ";
               if (!System) Result += " and Table_Type <> 'SYSTEM VIEW' ";
               Result += " order by TABLE_NAME";
            };
         };
#endif

#if OleDB
         if (dbConnection is OleDbConnection)
         {
            Result = "select TABLE_NAME as \"List of Tables\", COMMENTS as \"Description\"  from user_tab_comments WHERE 1=1";
            if (!Tables) Result += " and Table_Type <> 'TABLE' ";
            if (!Views) Result += " and Table_Type <> 'VIEW' ";
            if (!System) Result += " and Table_Type <> 'SYSTEM VIEW' ";
            Result += " order by TABLE_NAME";
         };
#endif

#if Oracle
         if (dbConnection is OracleConnection)
         {
            Result = "select TABLE_NAME as \"List of Tables\", COMMENTS as \"Description\"  from user_tab_comments WHERE 1=1";
            if (!Tables) Result += " and Table_Type <> 'TABLE' ";
            if (!Views) Result += " and Table_Type <> 'VIEW' ";
            if (!System) Result += " and Table_Type <> 'SYSTEM VIEW' ";
            Result += " order by TABLE_NAME";
         };
#endif

         return Result;
      }

      internal static string GetSchema(DbConnection dbConnection)
      {
         return GetSchema(dbConnection, true, true, false);
      }

      internal static string GetTableStructure(DbConnection dbConnection, string TableName)
      {
         string Result = "";

#if SqlCe
            if (dbConnection.GetType().Name == "SqlCeConnection")
            {
                Result = String.Format(
                        " SELECT Column_NAME as [Fields], Data_Type as [Type], Is_Nullable as [Null], Column_HasDefault as [HasDefault], Column_Default as [Default], Column_Flags as [Flags], Character_Maximum_Length as [C max length], AutoInc_Min, AutoInc_Max, AutoInc_Next, AutoInc_Seed, AutoInc_Increment, Description as [Description]"
                      + " FROM INFORMATION_SCHEMA.Columns"
                      + " Where Table_Name = '{0}'"
                      + " Order by Ordinal_Position",
                      TableName);
            };
#endif

#if SqlServer
         if (dbConnection is SqlConnection)
         {
            Result = String.Format(
                    " SELECT Column_NAME as [Fields], Data_Type as [Type], Is_Nullable as [Null],Column_Default as [Default], Character_Maximum_Length as [C max length]"
                  + " FROM INFORMATION_SCHEMA.Columns"
                  + " Where Table_Name = '{0}'"
                  + " Order by Ordinal_Position",
                  TableName);
         };
#endif

#if SQLite
         //ToDo: http://stackoverflow.com/questions/4770716/reading-sqlite-table-information-in-c-net
         throw new NotImplementedException();
#endif

#if MySQL
         if (dbConnection is MySqlConnection)
         {
            Result = String.Format(
                    " SELECT Column_NAME as 'Fields', concat( Column_Type, ' ', Extra ) as 'Type', Is_Nullable as 'Null',Column_Default as 'Default', Character_Maximum_Length as 'C max length', Column_Key as 'PK' "
                  + " FROM INFORMATION_SCHEMA.Columns"
                  + " Where Table_Name = '{0}'"
                  + " Order by Ordinal_Position",
                  TableName);
         };
#endif

#if OleDB
         if (dbConnection is OleDbConnection)
         {
            Result = String.Format(
                    " SELECT Column_NAME as \"Fields\", Data_Type as \"Type\", Nullable as \"Null\",Data_Default as \"Default\", Char_Length as \"C max length\""
                  + " FROM user_tab_columns"
                  + " Where Table_Name = '{0}'"
                  + " Order by Column_ID",
                  TableName);
         };
#endif

#if Oracle
         if (dbConnection is OracleConnection)
         {
            Result = String.Format(
                    " SELECT Column_NAME as \"Fields\", Data_Type as \"Type\", Nullable as \"Null\",Data_Default as \"Default\", Char_Length as \"C max length\""
                  + " FROM user_tab_columns"
                  + " Where Table_Name = '{0}'"
                  + " Order by Column_ID",
                  TableName);
         };
#endif
         return Result;
      }

      internal static string GetDBName(string ConnectionString)
      {
         string Result = "";

         if (ConnectionString.IndexOf("Initial Catalog=") != -1)
         {
            Result = ConnectionString.Substring(ConnectionString.IndexOf("Initial Catalog=") + ("Initial Catalog=").Length).Split(';')[0];
         };

         if (ConnectionString.ToUpper().IndexOf("DATABASE=") != -1)
         {
            Result = ConnectionString.Substring(ConnectionString.ToUpper().IndexOf("DATABASE=") + ("DATABASE=").Length).Split(';')[0];
         };

         if (ConnectionString.ToUpper().IndexOf("DATA SOURCE=") != -1)
         {
            Result = ConnectionString.Substring(ConnectionString.ToUpper().IndexOf("DATA SOURCE=") + ("DATA SOURCE=").Length).Split(';')[0];
         };

         if (ConnectionString.EndsWith(".sdf") || ConnectionString.EndsWith(".mdf"))
         {
            Result = System.IO.Path.GetFileName(ConnectionString);
         };

         return Result;
      }

      // - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - - 

      public static bool Update(object Object)
      {
         string TableName = Object.GetType().ToString();

         TStrings Record = ObjToRecord(DB_SQL.Connection, Object, ObjToRecordActionType.ExcludePK, Hmm: false, StringToSQL: false);
         TStrings PK = ObjToRecord(DB_SQL.Connection, Object, ObjToRecordActionType.OnlyPK);
         string Where = PK.Text;
         Where = Where.Substring(0, Where.Length - 1).Replace(",", " and ");

         return QuickUpdate(TableName, Where, Record) == "1";
      }

      public static string QuickUpdate(string TableName, string WhereCondition, TStrings Record)
      {
         return QuickUpdate(Connection, TableName, WhereCondition, Record);
      }

      public static string QuickUpdate(DbConnection Connection, string TableName, string WhereCondition, TStrings Record)
      {
         string Result = "";
         // UPDATE TABLE NAME SET COLUMN NAME = VALUE [ WHERE CONDITION ]

         if (!String.IsNullOrEmpty(WhereCondition)) WhereCondition = " Where " + WhereCondition;

         for (int i = 0; i < Record.Count; i++)
         {
            string Value = Record.ValueFromIndex(i);

            if (Value.StartsWith("convert(") && Value.EndsWith(")"))
            {
               // Nothing to do
            }
            else if (Value.StartsWith("STR_TO_DATE(") && Value.EndsWith(")"))
            {
               // Nothing to do
            }
            else if (Value == "null")
            {
               // Nothing to do
            }
            else
            {
               Record[i] = String.Format("{0}='{1}'", Record.Names(i), DB_SQL.StringToSQL(Record.ValueFromIndex(i)));
            };

            if (i < Record.Count - 1) Record[i] += ",";
         };

         string SQL = String.Format("Update {0} Set {1} {2}", TableName, Record.Text, WhereCondition);

         Result = QuickQuery(Connection, SQL);

         return Result;
      }

      // - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - - 

      public static List<T> Query<T>(string SQL) where T : class, new()
      {
         return Query<T>(SQL, null);
      }

      public static List<T> Query<T>(string SQL, object[] args) where T : class, new()
      {
         LastError = "";

         try
         {
            List<T> Result = new List<T>();

            if (args == null)
            {
               // return SQLiteConnection.Query<T>(SQL);

               foreach (var r in (DB_SQL.QuickQueryView(SQL) as DataTable).AsEnumerable())
               {
                  var t = new T();
                  t.CopyDataRowValues(r);
                  Result.Add(t);
               };
            }
            else
            {
               // return SQLiteConnection.Query<T>(SQL, args);
               throw new NotImplementedException();
            };

            return Result;
         }
         catch (Exception ex)
         {
            LastError = ex.Message;

            return null;
         }
      }

      // - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - - 

      public static bool Insert(object Object)
      {
         string TableName = Object.GetType().ToString();

         TStrings Record = ObjToRecord(DB_SQL.Connection, Object, ObjToRecordActionType.ExcludePK, Hmm: false, StringToSQL: false);
         // TStrings PK = ObjToRecord(Object, ObjToRecordActionType.OnlyPK);
         //string Where = PK.Text;
         //Where = Where.Substring(0, Where.Length - 1).Replace(",", " and ");

         return QuickInsert(TableName, Record) == "1";
      }

      public static string QuickInsert(string TableName, TStrings Record)
      {
         return QuickInsert(Connection, TableName, Record);
      }

      public static string QuickInsert(DbConnection dbConnection, string TableName, TStrings Record)
      {
         string Result = "";
         //Insert into DBLists ( List, Param, Value, ValueType ) Values( 'DBSchema', 'Version',   '0.98', 1 );

         string FieldList = "";
         string ValueList = "";

         string format = "{0}";

#if SqlServer
         if (dbConnection is SqlConnection)
         {
            format = "[{0}]";
         };
#endif

#if SQLite
         if (dbConnection is SQLiteConnection)
         {
            format = "[{0}]";
         };
#endif

#if MySQL
         if (dbConnection is MySqlConnection)
         {
            format = "`{0}`";
         };
#endif

         for (int i = 0; i < Record.Count; i++)
         {
            FieldList += string.Format(format, Record.Names(i));

            string Value = Record.ValueFromIndex(i);
            if (Value.StartsWith("convert(") && Value.EndsWith(")"))
            {
               ValueList += Value;
            }
            else if (Value.StartsWith("STR_TO_DATE(") && Value.EndsWith(")"))
            {
               ValueList += Value;
            }
            else if (Value == "null")
            {
               ValueList += Value;
            }
            else if ((Value == "0") || (Value == "1"))
            {
               ValueList += Value;
            }
            else
            {
               ValueList += String.Format("'{0}'", DB_SQL.StringToSQL(Value));
            };

            if (i < Record.Count - 1)
            {
               FieldList += ", ";
               ValueList += ", ";
            };
         };

         string SQL = String.Format("Insert into {0} ({1}) Values( {2} )", TableName, FieldList, ValueList);

         Result = QuickQuery(dbConnection, SQL);

         return Result;
      }

      // - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - - 

      public static string ImportDataTable(DbConnection dbConnection, string TableName, DataTable dt, bool Overwrite)
      {
         string Result = "OK";

         if (dt == null) return "DataTable is null";

         if (Overwrite) DB_SQL.QuickQuery(dbConnection, String.Format("delete from {0}", TableName));

         foreach (DataRow Row in dt.Rows)
         {
            string PK = Row["PK"].ToString();

            if (DB_SQL.QuickQuery(dbConnection, String.Format("Select PK from {0} where PK='{1}'", TableName, PK)) == PK)
            {
               // Update
               string SQL = String.Format("update {0} set ", TableName);

               for (int i = 0; i < Row.ItemArray.Length; i++)
               {
                  if (Row.Table.Columns[i].ToString() != "PK")
                  {
                     string Value = Row[i].ToString().TrimEnd();

                     if (Value != "")
                     {
                        if (Row.Table.Columns[i].DataType.Name == "Boolean")
                        {
                           SQL += String.Format("{0}={1}", Row.Table.Columns[i].ToString(), DB_SQL.BoolToSQL(Convert.ToBoolean(Value)));
                        }
                        else if (Row.Table.Columns[i].DataType.Name == "DateTime")
                        {
                           //ToDo: MySQL
                           SQL += String.Format("{0}={1}", Row.Table.Columns[i].ToString(), DB_SQL.DateTimeToSQL(Convert.ToDateTime(Value)));
                        }
                        else
                        {
                           SQL += String.Format("{0}='{1}'", Row.Table.Columns[i].ToString(), DB_SQL.StringToSQL(Value));
                        };

                        SQL += ((i < Row.ItemArray.Length - 1) ? ", " : " ");
                     };
                  };
               };

               SQL += String.Format("where PK='{0}'", PK);

               if (DB_SQL.QuickQuery(dbConnection, SQL) != "1")
               {
                  // Oups ...
                  Result = SQL;
               };
            }
            else
            {
               // Insert
               string SQL = "";
               string Fields = "";
               string Values = "";

               for (int i = 0; i < Row.ItemArray.Length; i++)
               {
                  string Value = Row[i].ToString().TrimEnd();

                  if (Value != "")
                  {
                     Fields += Row.Table.Columns[i].ToString();

                     if (Row.Table.Columns[i].DataType.Name == "Boolean")
                     {
                        Values += String.Format("{0}", DB_SQL.BoolToSQL(Convert.ToBoolean(Value)));
                     }
                     else if (Row.Table.Columns[i].DataType.Name == "DateTime")
                     {
                        Values += String.Format("{0}", DB_SQL.DateTimeToSQL(Convert.ToDateTime(Value)));
                     }
                     else
                     {
                        Values += String.Format("'{0}'", DB_SQL.StringToSQL(Value));
                     };

                     Fields += ", ";
                     Values += ", ";
                  };
               };

               if (Fields.EndsWith(", "))
               {
                  Fields = Fields.Substring(0, Fields.Length - 2);
                  Values = Values.Substring(0, Values.Length - 2);
               };

               SQL = String.Format("insert into {0} ({1}) Values ({2})", TableName, Fields, Values);

               if (DB_SQL.QuickQuery(dbConnection, SQL) != "1")
               {
                  // Oups ...
                  Result = SQL;
               };
            };
         };

         return Result;
      }

      // - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - - 

#if SqlCe
        static SqlCeEngine Engine = null;

        static public bool OpenDataBase(string DatabaseFile, string SchemaFile)
        {
            bool Result = true;

            Engine = new SqlCeEngine(String.Format("Data Source='{0}';", DatabaseFile));

            if (!File.Exists(DatabaseFile))
            {
                Engine.CreateDatabase();

                // Setup a connection to the database
                DB_SQL.Connection = new SqlCeConnection(Engine.LocalConnectionString);
                DB_SQL.Connection.Open();

                if (!File.Exists(SchemaFile))
                {
                    LastError = String.Format("File ({0}) not found!", SchemaFile);
                    Result = false;

                    // Application.Exit();
                    return false;
                }
                else
                {
                    if (!DB_SQL.RunScript(DB_SQL.Connection, SchemaFile))
                    {
                        DB_SQL.Log.SaveToFile(SchemaFile + ".log");

                        //AuditTrail.GetInstance().Connection = Connection;
                        //AuditTrail.GetInstance().Log( "DB Creation", DB_SQL.Log.Text );
                    };
                };
            }
            else
            {
                // Setup a connection to the database
                DB_SQL.Connection = new SqlCeConnection(Engine.LocalConnectionString);
                DB_SQL.Connection.Open();
            };

            //AuditTrail.GetInstance().Connection = Connection;

            return Result;
        }
#endif

      // - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - - 

#if NETFX_CORE || WebService || WPF || Desktop
      public static IEnumerable<PropertyInfo> GetAllProperties(Type t)
      {
         if (t == null)
            return Enumerable.Empty<PropertyInfo>();

         BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly;
         return t.GetProperties(flags).Concat(GetAllProperties(t.BaseType));
      }

      public static IEnumerable<FieldInfo> GetAllFields(Type t)
      {
         if (t == null)
            return Enumerable.Empty<FieldInfo>();

         BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly;
         return t.GetFields(flags).Concat(GetAllFields(t.BaseType));
      }

      private static object _DataRowToObj(object obj, DataRow row)
      {
         IEnumerable<PropertyInfo> list = GetAllProperties(obj.GetType());

         foreach (PropertyInfo pi in list)
         {
            if (pi.SetMethod == null) continue;

            if (pi.PropertyType.FullName == "System.String")
            {
               pi.SetValue(obj, row[pi.Name].ToString());
            }
            else if (pi.PropertyType.FullName == "System.Boolean")
            {
               pi.SetValue(obj, row[pi.Name].ToString() != "0");
            }
            else if (pi.PropertyType.FullName == "System.Int16")
            {
               pi.SetValue(obj, Int16.Parse(row[pi.Name].ToString()));
            }
            else if (pi.PropertyType.FullName == "System.Int32")
            {
               pi.SetValue(obj, Int32.Parse(row[pi.Name].ToString()));
            }
            else if (pi.PropertyType.FullName == "System.Int64")
            {
               pi.SetValue(obj, Int64.Parse(row[pi.Name].ToString()));
            }
            else if (pi.PropertyType.FullName == "System.DateTime")
            {
               if (row[pi.Name].ToString().Length > 0)
               {
                  pi.SetValue(obj, DateTime.Parse(row[pi.Name].ToString()));
               };
            }
            else if (pi.PropertyType.FullName == "System.Windows.Media.Imaging.BitmapImage")
            {
               // 
            }
            else
            {
               if (row[pi.Name].ToString() != "")
               {
                  if (pi.PropertyType.BaseType.Name == "Enum")
                  {
                     pi.SetValue(obj, Int32.Parse(row[pi.Name].ToString()));
                  }
                  else
                  {
                     throw new NotImplementedException();
                  };
               };
            };
         };

         return obj;
      }

      public static object RecordToObj(object obj, TStrings Record, bool DBIgnore = false)
      {
         IEnumerable<PropertyInfo> list = GetAllProperties(obj.GetType());

         foreach (PropertyInfo pi in list)
         {
            if (pi.SetMethod == null) continue;

            bool ignore = pi.GetCustomAttributes(typeof(IgnoreAttribute), true).Count() > 0;
            if (DBIgnore && ignore) continue;

            try
            {
               if (pi.PropertyType.FullName == "System.String")
               {
                  pi.SetValue(obj, Record[pi.Name]);
               }
               else if (pi.PropertyType.FullName == "System.Boolean")
               {
                  if (!string.IsNullOrEmpty(Record[pi.Name]))
                     pi.SetValue(obj, Boolean.Parse(Record[pi.Name]));
               }
               else if (pi.PropertyType.FullName == "System.Byte")
               {
                  if (!string.IsNullOrEmpty(Record[pi.Name]))
                     pi.SetValue(obj, Byte.Parse(Record[pi.Name]));
               }
               else if (pi.PropertyType.FullName == "System.Int16")
               {
                  if (!string.IsNullOrEmpty(Record[pi.Name]))
                     pi.SetValue(obj, Int16.Parse(Record[pi.Name]));
               }
               else if (pi.PropertyType.FullName == "System.Int32")
               {
                  if (!string.IsNullOrEmpty(Record[pi.Name]))
                     pi.SetValue(obj, Int32.Parse(Record[pi.Name]));
               }
               else if (pi.PropertyType.FullName == "System.Int64")
               {
                  if (!string.IsNullOrEmpty(Record[pi.Name]))
                     pi.SetValue(obj, Int64.Parse(Record[pi.Name]));
               }
               else if (pi.PropertyType.FullName == "System.UInt32")
               {
                  if (!string.IsNullOrEmpty(Record[pi.Name]))
                     pi.SetValue(obj, UInt32.Parse(Record[pi.Name]));
               }
               else if (pi.PropertyType.FullName == "System.UInt16")
               {
                  if (!string.IsNullOrEmpty(Record[pi.Name]))
                     pi.SetValue(obj, UInt16.Parse(Record[pi.Name]));
               }
               else if (pi.PropertyType.FullName == "System.UInt32")
               {
                  if (!string.IsNullOrEmpty(Record[pi.Name]))
                     pi.SetValue(obj, UInt32.Parse(Record[pi.Name]));
               }
               else if (pi.PropertyType.FullName == "System.UInt64")
               {
                  if (!string.IsNullOrEmpty(Record[pi.Name]))
                     pi.SetValue(obj, UInt64.Parse(Record[pi.Name]));
               }
               else if (pi.PropertyType.FullName == "System.Double")
               {
                  if (!string.IsNullOrEmpty(Record[pi.Name]))
                     pi.SetValue(obj, Double.Parse(Record[pi.Name]));
               }
               else if (pi.PropertyType.FullName == "System.Decimal")
               {
                  if (!string.IsNullOrEmpty(Record[pi.Name]))
                     pi.SetValue(obj, Decimal.Parse(Record[pi.Name]));
               }
               else if (pi.PropertyType.FullName == "System.Boolean")
               {
                  if (Record[pi.Name].Length == 1)
                  {
                     bool Val = true;
                     Val = Record[pi.Name] != "0";
                     pi.SetValue(obj, Val);
                  }
                  else
                  {
                     pi.SetValue(obj, bool.Parse(Record[pi.Name]));
                  };
               }
               else if (pi.PropertyType.FullName == "System.DateTime")
               {
                  if (Record[pi.Name].ToString().Length > 0)
                  {
                     try
                     {
                        pi.SetValue(obj, DateTime.Parse(Record[pi.Name]));
                     }
                     catch
                     {
                        pi.SetValue(obj, DateTime.Parse(Record[pi.Name], new System.Globalization.CultureInfo("fr-FR", false)));
                     };
                  };
               }
               else if (pi.PropertyType.FullName == "System.Windows.Media.Imaging.BitmapImage")
               {
                  // 
               }
               else
               {
                  if (Record[pi.Name] != "")
                  {
                     if (pi.PropertyType.BaseType.Name == "Enum")
                     {
                        pi.SetValue(obj, Int32.Parse(Record[pi.Name]));
                     }
                     else
                     {
                        throw new NotImplementedException();
                     };
                  };
               };
            }
            catch (Exception ex)
            {
               AuditTrail.Write(ErrorLevel.Error, ex);

               if (Debugger.IsAttached)
               {
                  Debugger.Break();
               };
            };
         };

         return obj;
      }

      public enum ObjToRecordActionType { ExcludePK, ExcludeNull, IncludePK, OnlyPK }

      public static ZPF.TStrings ObjToRecord(DbConnection dbConnection, object obj, ObjToRecordActionType Action = ObjToRecordActionType.IncludePK, bool Hmm = true, bool StringToSQL = true, bool ExcludeNull = false)
      {
         TStrings Record = new TStrings();
         string FormatStr = (Hmm ? "{0}={1}," : "{0}={1}");

         if (obj is DataRow)
         {
            DataRow dr = (obj as DataRow);

            var Columns = dr.Table.Columns;
            var ItemArray = dr.ItemArray;

            for (int i = 0; i < Columns.Count; i++)
            {
               var item = ItemArray[i];
               bool ignore = false;
               bool PK = ((dr.Table.PrimaryKey.FirstOrDefault() != null && dr.Table.PrimaryKey.FirstOrDefault().ColumnName == Columns[i].ColumnName)) || Columns[i].AutoIncrement;

               switch (Action)
               {
                  case ObjToRecordActionType.ExcludePK:
                     if (PK) ignore = true;
                     break;

                  case ObjToRecordActionType.IncludePK:
                     break;

                  case ObjToRecordActionType.OnlyPK:
                     if (!PK) ignore = true;
                     break;
               };

               if (!ignore)
               {
                  if (ExcludeNull && item is System.DBNull)
                  {

                  }
                  else if (item is System.DBNull)
                  {
                     Record.Add(String.Format(FormatStr, Columns[i].ColumnName, "NULL"));
                  }
                  else
                  {
                     switch (Columns[i].DataType.ToString())
                     {
                        case "System.Boolean":
                           if (item.ToString() == "")
                           {
                              Record.Add(String.Format(FormatStr, Columns[i].ColumnName, '0'));
                           }
                           else
                           {
                              Record.Add(String.Format(FormatStr, Columns[i].ColumnName, (Boolean.Parse(item.ToString()) ? '1' : '0')));
                           }
                           break;

                        case "System.UInt32":
                        case "System.UInt64":
                        case "System.Int32":
                        case "System.Int64":
                        case "System.Float":
                        case "System.Double":
                        case "System.Decimal":
                           Record.Add(String.Format(FormatStr, Columns[i].ColumnName, item.ToString().Replace(",", ".")));
                           break;

                        case "System.DateTime":
                           //ToDo: MySQL
#if SQLite
                           if (dbConnection is SQLiteConnection)
                           {
                              Record.Add(String.Format("{0}='{1}',", Columns[i].ColumnName, DB_SQL.DateTimeToSQL(Convert.ToDateTime(item.ToString()))));
                           }
                           else
#endif
                           {
                              Record.Add(String.Format(FormatStr, Columns[i].ColumnName, DB_SQL.DateTimeToSQL(Convert.ToDateTime(item.ToString()))));
                           };
                           break;

                        case "System.String":
                           if (Hmm)
                           {
                              Record.Add(String.Format("{0}='{1}',", Columns[i].ColumnName, DB_SQL.StringToSQL(item.ToString())));
                           }
                           else
                           {
                              Record.Add(String.Format(FormatStr, Columns[i].ColumnName, item.ToString()));
                           };
                           break;

                        default:
                           Debug.WriteLine(item.GetType().ToString());

                           if (Debugger.IsAttached)
                           {
                              Debugger.Break();
                           };
                           break;
                     };
                  };
               }
            }
         }
         else
         {
            IEnumerable<PropertyInfo> list = GetAllProperties(obj.GetType());

            foreach (PropertyInfo pi in list)
            {
               bool PK = pi.GetCustomAttributes(typeof(PrimaryKeyAttribute), true).Count() > 0;
               bool ignore = pi.GetCustomAttributes(typeof(IgnoreAttribute), true).Count() > 0;

               switch (Action)
               {
                  case ObjToRecordActionType.ExcludePK:
                     if (PK) ignore = true;
                     break;

                  case ObjToRecordActionType.IncludePK:
                     break;

                  case ObjToRecordActionType.OnlyPK:
                     if (!PK) ignore = true;
                     break;
               };

               if (!ignore)
               {
                  if (pi.GetValue(obj) != null)
                  {
                     if (pi.PropertyType.IsEnum)
                     {
                        Record.Add(String.Format(FormatStr, pi.Name, ((int)pi.GetValue(obj)).ToString()));
                     }
                     else if (pi.PropertyType.FullName == "System.String")
                     {
                        string st = (StringToSQL ? DB_SQL.StringToSQL(pi.GetValue(obj).ToString()) : pi.GetValue(obj).ToString());

                        if (Hmm)
                        {
                           Record.Add(String.Format("{0}='{1}',", pi.Name, st));
                        }
                        else
                        {
                           Record.Add(String.Format(FormatStr, pi.Name, st));
                        };
                     }
                     else if (
                                 pi.PropertyType.FullName == "System.Int16"
                                 || pi.PropertyType.FullName == "System.Int32"
                                 || pi.PropertyType.FullName == "System.Int64"
                                 || pi.PropertyType.FullName == "System.UInt32"
                                 || pi.PropertyType.FullName == "System.UInt64"
                                 || pi.PropertyType.FullName == "System.Float"
                                 || pi.PropertyType.FullName == "System.Decimal"
                                 || pi.PropertyType.FullName == "System.Double"
                                 || pi.PropertyType.FullName == "System.Single"
                              )
                     {
                        Record.Add(String.Format(FormatStr, pi.Name, pi.GetValue(obj).ToString().Replace(",", ".")));
                     }
                     else if (pi.PropertyType.FullName == "System.Byte")
                     {
                        Record.Add(String.Format(FormatStr, pi.Name, pi.GetValue(obj).ToString()));
                     }
                     else if (pi.PropertyType.FullName == "System.DateTime")
                     {
                        DateTime dt = (DateTime)(pi.GetValue(obj));

                        if (dt == DateTime.MinValue)
                        {
                           Record.Add(String.Format(FormatStr, pi.Name, "null"));
                        }
                        else
                        {
                           Record.Add(String.Format(FormatStr, pi.Name, DB_SQL.DateTimeToSQL(dt)));
                        };
                     }
                     else if (pi.PropertyType.FullName == "System.Boolean")
                     {
                        bool val = (bool)(pi.GetValue(obj));

                        Record.Add(String.Format(FormatStr, pi.Name, (val ? "1" : "0")));
                     }
                     else if (pi.PropertyType.FullName.StartsWith("System.Collections.Generic.IList"))
                     {
                     }
                     else
                     {
                        if (Debugger.IsAttached)
                        {
                           Debug.WriteLine(string.Format("{0}: {1} <-- {2}", pi.Name, pi.PropertyType.FullName, ""));
                           Debugger.Break();
                        };
                     };
                  }; // null
               };
            };
         }

         return Record;
      }

      public static ZPF.TStrings DataRowToRecord(DataRow row)
      {
         return RowToRecord(row, false, true);
      }

      public static ZPF.TStrings RowToRecord(DataRow row, bool Hmm = true, bool NoStringToSQL = false)
      {
         TStrings Record = new TStrings();

         string FormatStr = (Hmm ? "{0}={1}," : "{0}={1}");

         for (int i = 0; i < row.ItemArray.Length; i++)
         {
            if (row.Table.Columns[i].DataType.FullName == "System.String")
            {
               if (Hmm)
               {
                  Record.Add(String.Format("{0}='{1}',", row.Table.Columns[i].ColumnName, (NoStringToSQL ? row[i].ToString() : DB_SQL.StringToSQL(row[i].ToString()))));
               }
               else
               {
                  Record.Add(String.Format(FormatStr, row.Table.Columns[i].ColumnName, (NoStringToSQL ? row[i].ToString() : DB_SQL.StringToSQL(row[i].ToString()))));
               };
            }
            else if (
                        (row.Table.Columns[i].DataType.FullName == "System.Int64")
                        || (row.Table.Columns[i].DataType.FullName == "System.Int32")
                        || (row.Table.Columns[i].DataType.FullName == "System.Byte")
                     )
            {
               if (row[i].ToString() == "")
               {
                  Record.Add(String.Format(FormatStr, row.Table.Columns[i].ColumnName, "null"));
               }
               else
               {
                  Record.Add(String.Format(FormatStr, row.Table.Columns[i].ColumnName, row[i].ToString()));
               };
            }
            else if (row.Table.Columns[i].DataType.FullName == "System.Boolean")
            {
               if (row[i].ToString() == "")
               {
                  Record.Add(String.Format(FormatStr, row.Table.Columns[i].ColumnName, "null"));
               }
               else
               {
                  Record.Add(String.Format(FormatStr, row.Table.Columns[i].ColumnName, ((bool)row[i] ? '1' : '0')));
               };
            }
            else if (row.Table.Columns[i].DataType.FullName == "System.DateTime")
            {
               if (row[i].ToString() == "")
               {
                  Record.Add(String.Format(FormatStr, row.Table.Columns[i].ColumnName, "null"));
               }
               else
               {
                  DateTime dt = (DateTime)(row[i]);

                  if (dt == DateTime.MinValue)
                  {
                     Record.Add(String.Format(FormatStr, row.Table.Columns[i].ColumnName, "null"));
                  }
                  else
                  {
                     Record.Add(String.Format(FormatStr, row.Table.Columns[i].ColumnName, DB_SQL.DateTimeToSQL(dt)));
                  };
               };
            }
            else
            {
               Record.Add(string.Format("{0}={1}", row.Table.Columns[i].ColumnName, row[i].ToString()));
            };
         };

         foreach (DataColumn dc in row.Table.Columns)
         {
            //if (pi.GetValue(obj) != null)
            //{
            //   if (pi.PropertyType.FullName == "System.String")
            //   {
            //      if (Hmm)
            //      {
            //         Record.Add(String.Format("{0}='{1}',", pi.Name, DB_SQL.StringToSQL(pi.GetValue(obj).ToString())));
            //      }
            //      else
            //      {
            //         Record.Add(String.Format(FormatStr, pi.Name, DB_SQL.StringToSQL(pi.GetValue(obj).ToString())));
            //      };
            //   }
            //   else if (pi.PropertyType.FullName == "System.Int32")
            //   {
            //      Record.Add(String.Format(FormatStr, pi.Name, pi.GetValue(obj).ToString()));
            //   }
            //   else if (pi.PropertyType.FullName == "System.Byte")
            //   {
            //      Record.Add(String.Format(FormatStr, pi.Name, pi.GetValue(obj).ToString()));
            //   }
            //   else if (pi.PropertyType.FullName == "System.DateTime")
            //   {
            //      DateTime dt = (DateTime)(pi.GetValue(obj));

            //      if (dt == DateTime.MinValue)
            //      {
            //         Record.Add(String.Format(FormatStr, pi.Name, "null"));
            //      }
            //      else
            //      {
            //         Record.Add(String.Format(FormatStr, pi.Name, DB_SQL.DateTimeToSQL(dt)));
            //      };
            //   }
            //   else
            //   {
            //      //ToDo: bug
            //   };
            //}; // null
         };

         return Record;
      }

      public static List<T> QuickQueryList<T>(DbConnection dbConnection, string SQL)
      {
         return null;

         //TStrings Record = DB_SQL.QuickQueryRec(dbConnection, SQL);

         //try
         //{
         //   if (Record.Count > 0)
         //   {


         //      var obj = Activator.CreateInstance(typeof(T));

         //      obj = RecordToObj(obj, Record, DBIgnore);


         //      return (T)obj;
         //   }
         //   else
         //   {
         //      return default(T);
         //   };
         //}
         //catch (Exception ex)
         //{
         //   AuditTrail.Write(ErrorLevel.Error, ex, SQL);
         //   return default(T);
         //};

      }

      public static T QuickQuery<T>(DbConnection dbConnection, string SQL, bool DBIgnore = false)
      {
         TStrings Record = DB_SQL.QuickQueryRec(dbConnection, SQL);

         try
         {
            if (Record.Count > 0)
            {
               var obj = Activator.CreateInstance(typeof(T));

               obj = RecordToObj(obj, Record, DBIgnore);

               return (T)obj;
            }
            else
            {
               return default(T);
            };
         }
         catch (Exception ex)
         {
            AuditTrail.Write(ErrorLevel.Error, ex, SQL);
            return default(T);
         };
      }

      public static T DataRowToObj<T>(DataRow row)
      {
         if (row != null)
         {
            var obj = Activator.CreateInstance(typeof(T));

            obj = _DataRowToObj(obj, row);

            return (T)obj;
         }
         else
         {
            return default(T);
         };
      }

      public static string QuickUpdate(string TableName, DataRow Row)
      {
         TStrings Record = RowToRecord(Row);

         Record[Record.Count - 1] = Record[Record.Count - 1].Substring(0, Record[Record.Count - 1].Length - 1);

         //ToDo: get primary key from meta
         string Where = Record[0];
         Where = Where.Substring(0, Where.Length - 1);

         Record.Delete(0);

         // UPDATE TABLE NAME SET COLUMN NAME = VALUE [ WHERE CONDITION ]

         if (!String.IsNullOrEmpty(Where)) Where = " Where " + Where;

         string SQL = String.Format("Update {0} Set {1} {2}", TableName, Record.Text, Where);
         return QuickQuery(Connection, SQL);
      }

      public static string QuickInsert(string TableName, DataRow Row, bool RemovePK = true)
      {
         TStrings Record = RowToRecord(Row, false, true);

         if (Record.Text.Contains("03430-NETTO-1-COSNE "))
         {
         };

         //ToDo: get primary key from meta
         if (RemovePK) Record.Delete(0);

         return QuickInsert(Connection, TableName, Record);
      }
      public static bool QuickUpdate(string TableName, Object obj)
      {
         return QuickUpdate(Connection, TableName, obj);
      }

      public static bool QuickUpdate(DbConnection dbConnection, string TableName, Object obj)
      {
         TStrings Record = ObjToRecord(dbConnection, obj, ObjToRecordActionType.ExcludePK);
         TStrings PK = ObjToRecord(dbConnection, obj, ObjToRecordActionType.OnlyPK);

         Record[Record.Count - 1] = Record[Record.Count - 1].Substring(0, Record[Record.Count - 1].Length - 1);

         string Where = PK.Text;
         Where = Where.Substring(0, Where.Length - 1).Replace(",", " and ");

         string format = "{0}={1}";

#if SqlServer
         if (dbConnection is SqlConnection)
         {
            format = "[{0}]={1}";
         };
#endif

#if SQLite
         if (dbConnection is SQLiteConnection)
         {
            format = "[{0}]={1}";
         };
#endif

#if MySQL
         if (dbConnection is MySqlConnection)
         {
            format = "`{0}`={1}";
         };
#endif

         for (int i = 0; i < Record.Count; i++)
         {
            Record[i] = string.Format(format, Record.Names(i), Record.ValueFromIndex(i));
         }

         // UPDATE TABLE NAME SET COLUMN NAME = VALUE [ WHERE CONDITION ]

         if (!String.IsNullOrEmpty(Where)) Where = " Where " + Where;

         string SQL = String.Format("Update {0} Set {1} {2}", TableName, Record.Text, Where);

         return QuickQuery(dbConnection, SQL) == "1";
      }

      public static bool QuickCheck(DbConnection dbConnection, string TableName, Object obj)
      {
         TStrings PK = ObjToRecord(dbConnection, obj, ObjToRecordActionType.OnlyPK);

         string Where = PK.Text;
         Where = Where.Substring(0, Where.Length - 1).Replace(",", " and ");

         string SQL = String.Format("select count(*) from {0} where {1}", TableName, Where);

         return QuickQuery(dbConnection, SQL) == "1";
      }

      public static bool Delete(object Object)
      {
         return Delete(DB_SQL.Connection, Object);
      }

      public static bool Delete(DbConnection dbConnection, object Object)
      {
         string TableName = Object.GetType().ToString();

         return Delete(DB_SQL.Connection, TableName, Object);
      }

      public static bool Delete(DbConnection dbConnection, string TableName, object Object)
      {
         // TStrings Record = ObjToRecord(Object, ObjToRecordActionType.ExcludePK, Hmm: false);
         TStrings PK = ObjToRecord(dbConnection, Object, ObjToRecordActionType.OnlyPK);
         string Where = PK.Text;
         Where = Where.Substring(0, Where.Length - 1).Replace(",", " and ");

         string SQL = String.Format("delete from {0} where {1}", TableName, Where);

         return QuickQuery(dbConnection, SQL) == "1";
      }

      [Obsolete("Not used anymore", false)]
      public static bool QuickDelete(DbConnection dbConnection, string TableName, Object Object)
      {
         return Delete(dbConnection, TableName, Object);
      }

      // - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - - 

      public static bool QuickInsert(string TableName, Object obj)
      {
         return QuickInsert(Connection, TableName, obj);
      }

      public static bool QuickInsert(DbConnection dbConnection, string TableName, Object obj, bool SetPK = false)
      {
         TStrings Record = null;

         if (SetPK)
         {
            Record = ObjToRecord(dbConnection, obj, ObjToRecordActionType.IncludePK, Hmm: true, ExcludeNull: true);
         }
         else
         {
            Record = ObjToRecord(dbConnection, obj, ObjToRecordActionType.ExcludePK, Hmm: true, ExcludeNull: true);
         };

         // Insert into DBLists ( List, Param, Value, ValueType ) Values( 'DBSchema', 'Version',   '0.98', 1 );

         string FieldList = "";
         string ValueList = "";
         string format = "{0}";

#if SqlServer
         if (dbConnection is SqlConnection)
         {
            format = "[{0}]";
         };
#endif

#if SQLite
         if (dbConnection is SQLiteConnection)
         {
            format = "[{0}]";
         };
#endif

#if MySQL
         if (dbConnection is MySqlConnection)
         {
            format = "`{0}`";
         };
#endif

         for (int i = 0; i < Record.Count; i++)
         {
            FieldList += string.Format(format, Record.Names(i));

            string Value = Record.ValueFromIndex(i);
            ValueList += Value.TrimEnd(new char[] { ',' });

            if (i < Record.Count - 1)
            {
               FieldList += ", ";
               ValueList += ", ";
            };
         };

         string SQL = String.Format("Insert into {0} ({1}) Values( {2} )", TableName, FieldList, ValueList);
         DB_SQL.LastQuery = SQL;

         return QuickQuery(dbConnection, SQL) == "1";
      }


#endif

      // - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - - 

#if System_Drawing
      public static System.Drawing.Image QuickQueryImage(DbConnection dbConnection, string SQL)
      {
         System.Drawing.Image Result = null;
         LastError = "";

         DbCommand Command = NewCommand(SQL, dbConnection);

         if (SQL.TrimStart(new char[] { ' ', '\n', '\r' }).ToUpper().StartsWith("SELECT"))
         {
            DbDataReader Reader = NewReader(Command);

            if (Reader == null)
            {
               return null;
            };

            if (Reader.Read())
            {
               if (Reader[0].GetType().Name != "DBNull")
               {
                  //Store binary data read from the database in a byte array
                  byte[] blob = (byte[])Reader[0];
                  System.IO.MemoryStream stream = new System.IO.MemoryStream();
                  stream.Write(blob, 0, blob.Length);
                  stream.Position = 0;

                  try
                  {
                     Result = System.Drawing.Image.FromStream(stream);
                  }
                  catch (Exception ex)
                  {
                     LastError = "QuickQueryImage " + ex.Message;
                     Reader.Close();
                     return null;
                  };
               }
               else
               {
                  Reader.Close();
                  return null;
               };
            };

            Reader.Close();
         }
         else
         {
            return null;
         };

         return Result;
      }

      public static bool SetBlob(DbConnection dbConnection, string SQL, byte[] blobData)
      {
         // Read Image Bytes into a byte array
         // byte[] imageData = vmeID.ConvertToBytes(vmeID.Current_eID.Picture);

         // Set insert query
         // string SQL = string.Format("update HOS_Attachment set Picture=@blobData where PK='{0}'", PK);

         // Initialize SqlCommand object for insert.
         DbCommand cmd = DB_SQL.NewCommand(SQL, DB_SQL.Connection);

         // We are passing the image byte data as SQL parameters.
#if SqlServer
         if (dbConnection is SqlConnection)
         {
            cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("@BlobData", (object)blobData));
         };
#endif

#if MySQL
         if (dbConnection is MySqlConnection)
         {
            cmd.Parameters.Add(new MySql.Data.MySqlClient.MySqlParameter("@BlobData", (object)blobData));
         };
#endif

         // Open connection and execute insert query.
         return (cmd.ExecuteNonQuery() == 1);
      }
#endif

      // - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - - 
   }

   public static class DB_SQL_Extensions
   {
      // - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - - 
      public static DataTable ToDataTable<T>(this IEnumerable<T> collection)
      {
         DataTable dt = new DataTable("DataTable");
         Type t = typeof(T);
         PropertyInfo[] pia = t.GetProperties();

         // Inspect the properties and create the columns in the DataTable
         foreach (PropertyInfo pi in pia)
         {
            Type ColumnType = pi.PropertyType;
            if ((ColumnType.IsGenericType))
            {
               ColumnType = ColumnType.GetGenericArguments()[0];
            }
            dt.Columns.Add(pi.Name, ColumnType);
         }

         // Populate the data table
         foreach (T item in collection)
         {
            DataRow dr = dt.NewRow();

            dr.BeginEdit();
            foreach (PropertyInfo pi in pia)
            {
               if (pi.GetValue(item, null) != null)
               {
                  dr[pi.Name] = pi.GetValue(item, null);
               }
            }
            dr.EndEdit();

            dt.Rows.Add(dr);
         }

         return dt;
      }

      // - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - - 
   }
}

