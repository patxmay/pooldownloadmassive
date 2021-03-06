#if __PC__
#define Desktop
using System.Threading.Tasks;
#endif

#if WINDOWS_PHONE_APP
#define WINDOWS_PHONE
#define WP8
#endif

#if SILVERLIGHT || WINDOWS_PHONE || NETFX_CORE || PCL
#define INNER_LIST
#endif

using System;
using System.Collections;
using System.IO;
using System.Diagnostics;

#if NETFX_CORE || WP8
using Windows.Storage;
using Windows.Foundation;
using System.Threading.Tasks;
#endif

#if PCL
using System.Threading.Tasks;
#endif

#if WINDOWS_PHONE
using System.IO.IsolatedStorage;
#endif

#if PCL
using PCLStorage;
#endif

namespace ZPF
{
   /// <summary>
   /// TStrings is the base class for objects that represent a list of strings.
   /// 
   /// 20/04/06 - ME  - Add: public string this[ int Index ] ... set
   /// 01/08/06 - ME  - Share rights with Tiki Labs
   /// 15/08/06 - ME  - Bugfix: public string Names( int Index );
   /// 16/08/06 - ME  - Bugfix: LoadFromFile
   /// 25/08/06 - ME  - Bugfix: public string this[ string Name ]
   /// 06/10/06 - ME  - Code review 
   /// 31/01/07 - ME  - Bugfix: Names: if Count=0 
   /// 12/02/07 - ME  - Bugfix: IndexOfName( string Name ) "\R\N"
   ///                          this[ string Name ]
   /// 29/05/07 - ME  - Add: _IgnoreSPC
   /// 17/09/09 - ME  - Add: HTML
   /// 12/12/11 - ME  - Add: Add basic support for Windows Phone
   /// 12/12/10 - ME  - Add: Constructor to class TStringsItem
   /// 01/01/11 - ME  - Add: Add file support for Windows Phone ( IsolatedStorage ) LoadFromFile
   /// 24/03/11 - ME  - Bugfix: Insert(int Index, string st) - if( Index == 0 && Count == 0 )
   /// 18/02/12 - ME  - Add: Add file support for Windows Phone ( IsolatedStorage ) SaveToFile
   /// 11/11/12 - ME  - Add: NETFX_CORE --> Windows 8 RT: LoadFromFile, SaveToFile, Clear, TStrings()
   /// 19/12/12 - ME  - Update: public TStrings(), public void Clear() --> WP8
   /// 10/01/12 - ME  - Bugfix: IndexOfName if there is no value (--> no = )
   /// 29/03/13 - ME  - Bugfix: WP: Add(string st)
   /// 21/12/13 - ME  - Add: Push / Pop object
   /// 03/11/14 - ME  - Add: Stream SaveToStream()
   /// 12/11/14 - ME  - Add: public void Sort()
   /// 11/01/15 - ME  - Add: LoadFromFile: MaxSize
   /// 06/02/15 - ME  - Add: Reengeniering: Conditional compilation: Load / SaveSteam
   /// 10/09/15 - ME  - Error(string Msg, int Data) if (Debugger.IsAttached) Break();
   /// 18/04/16 - ME  - Add: bool Rename(string OldName, string NewName);
   /// 
   /// 2005..2016 ZePocketForge.com, SAS ZPF
   /// </summary>

#if INNER_LIST
   public class TStrings 
#else
   public class TStrings : CollectionBase, IEnumerable
#endif
   {
      class TStringsItem
      {
         public string FString;
         public Object FObject = null;

         public TStringsItem()
         {
         }

         public TStringsItem(string st)
         {
            FString = st;
         }

         public override string ToString()
         {
            return FString;
         }
      };

      //string SSortedListError    = "Operation not allowed on sorted string list";
      //string SDuplicateString    = "String list does not allow duplicates";
      string SListIndexError = "List index out of bounds ({0})";
      //string SListCapacityError  = "List capacity out of bounds ({0})";
      //string SListCountError     = "List count out of bounds ({0})";

#if INNER_LIST
      const int NEW_SIZE = 16;
      Array InnerList = null;
#else
#endif

      public TStrings()
      {
         _IgnoreSPC = false;

#if INNER_LIST
#if NETFX_CORE || WINDOWS_PHONE || PCL
         InnerList = Array.CreateInstance(Type.GetType(typeof(TStringsItem).AssemblyQualifiedName), NEW_SIZE);
#else
         InnerList = Array.CreateInstance(Type.GetType("TStringsItem"), NEW_SIZE);
#endif
#else
#endif
      }

      ~TStrings()
      {
         Clear();
      }

#if INNER_LIST
      public void Clear()
      {
#if NETFX_CORE || WINDOWS_PHONE || PCL
         InnerList = Array.CreateInstance(Type.GetType(typeof(TStringsItem).AssemblyQualifiedName), NEW_SIZE);
#else
         InnerList = Array.CreateInstance(Type.GetType("TStringsItem"), NEW_SIZE);
#endif
         _Count = 0;

         // throw new NotImplementedException();
      }

      int _Count = 0;

      public int Count
      {
         get
         {
            return _Count;
         }
      }

#else
#endif
      /******************************************************************************/

      public void Sort()
      {
         bool flag = true;
         string sTemp;
         object oTemp;

         // sorting an array
         for (int i = 1; (i <= (this.Count - 1)) && flag; i++)
         {
            flag = false;

            for (int j = 0; j < (this.Count - 1); j++)
            {
               if (this[j + 1].CompareTo(this[j]) < 0)
               {
                  sTemp = this[j];
                  oTemp = this.GetObject(j);

                  this[j] = this[j + 1];
                  this.SetObject(j, this.GetObject(j + 1));

                  this[j + 1] = sTemp;
                  this.SetObject(j + 1, oTemp);

                  flag = true;
               }
            }
         }
      }

      /******************************************************************************/

      private void Check(int Index)
      {
#if INNER_LIST
         if ((Index < 0) || (Index >= _Count))
#else
         if ((Index < 0) || (Index >= InnerList.Count))
#endif
         {
            Error(SListIndexError, Index);
         }
      }

      /******************************************************************************/

      string Get(int Index)
      {
         Check(Index);

         // return ((InnerList[ Index ] as TStringsItem).FString);

#if INNER_LIST
         return (InnerList.GetValue(Index) as TStringsItem).ToString();
#else
         return (InnerList[Index] as TStringsItem).ToString();
#endif
      }


      void Put(int Index, string S)
      {
         Check(Index);

#if INNER_LIST
         TStringsItem si = InnerList.GetValue(Index) as TStringsItem;
         si.FString = S;
         InnerList.SetValue(si, Index);
#else
         (InnerList[Index] as TStringsItem).FString = S;
#endif
      }

      /******************************************************************************/

      /// <summary>
      /// Returns the object associated with the string at a specified index. 
      /// </summary>
      /// <param name="Index">Index is the index of the string with which the object is associated.</param>
      /// <returns></returns>
      public Object GetObject(int Index)
      {
         Check(Index);

#if INNER_LIST
         return (InnerList.GetValue(Index) as TStringsItem).FObject;
#else
         return ((InnerList[Index] as TStringsItem).FObject);
#endif
      }

      public bool Rename(string OldName, string NewName)
      {
         if (string.IsNullOrEmpty(OldName)) return false;
         if (string.IsNullOrEmpty(NewName)) return false;

         int Ind = IndexOfName(OldName);

         if (Ind > -1)
         {
            this[Ind] = NewName + "=" + this.ValueFromIndex(Ind);
            return true;
         };

         return false;
      }

      public Object GetObject(string Name)
      {
         Object Result;
         int i;

         i = IndexOfName(Name);

         if (i >= 0)
         {
            Result = GetObject(i);
         }
         else
         {
            Result = null;
         }
         ;

         return (Result);
      }

      /// <summary>
      /// Changes the object associated with the string at a specified index.
      /// </summary>
      /// <param name="Index"></param>
      /// <param name="Obj"></param>
      public void SetObject(int Index, Object Obj)
      {
         Check(Index);

#if INNER_LIST
         TStringsItem si = InnerList.GetValue(Index) as TStringsItem;
         si.FObject = Obj;
         InnerList.SetValue(si, Index);
#else
         (InnerList[Index] as TStringsItem).FObject = Obj;
#endif
      }

      /// <summary>
      /// Adds a string to the list, and associates an object with the string. 
      /// </summary>
      /// <param name="Value"></param>
      /// <param name="Obj"></param>
      /// <returns></returns>

      public int AddObject(string Value, Object Obj)
      {
         int Result;

         Result = Add(Value);
         // PutObject( Result, Obj );

#if INNER_LIST
         TStringsItem si = InnerList.GetValue(Result) as TStringsItem;
         si.FObject = Obj;
         InnerList.SetValue(si, Result);
#else
         (InnerList[Result] as TStringsItem).FObject = Obj;
#endif

         return (Result);
      }

      /******************************************************************************/

      /// <summary>
      /// Adds a string at the end of the list.
      /// </summary>
      /// <param name="st"></param>
      /// <returns>Returns the index of the new string.</returns>

      public int Add(string st)
      {
         TStringsItem StringItem = new TStringsItem();
         StringItem.FString = st;

#if INNER_LIST
         int Result = -1;

         if (_Count + 1 > InnerList.Length)
         {
#if INNER_LIST
            Array tmp = Array.CreateInstance(Type.GetType(typeof(TStringsItem).AssemblyQualifiedName), InnerList.Length + NEW_SIZE);
#else
            Array tmp = Array.CreateInstance(Type.GetType("TStringsItem"), InnerList.Length + NEW_SIZE);
#endif
            InnerList.CopyTo(tmp, 0);
            InnerList = tmp;
         }

         InnerList.SetValue(new TStringsItem(st), _Count);
         Result = _Count;

         _Count += 1;
#else
         int Result = InnerList.Add(StringItem);
#endif

         return (Result);
      }

      /// <summary>
      /// Inserts a string at a specified position. If Index is 0, the string is inserted 
      /// at the beginning of the list. If Index is 1, the string is put in the second 
      /// position of the list, and so on.
      /// </summary>
      /// <param name="Index"></param>
      /// <param name="st"></param>

#if INNER_LIST
#else
      public void Insert(int Index, string st)
      {
         if (Index == 0 && Count == 0)
         {
            Add(st);
         }
         else
         {
            Check(Index);

            TStringsItem StringItem = new TStringsItem();
            StringItem.FString = st;

            InnerList.Insert(Index, StringItem);
         };
      }
#endif

      /******************************************************************************/
      /// <summary>
      /// Returns the position of a string in the list.
      /// </summary>
      /// <param name="st"></param>
      /// <returns>IndexOf returns the 0-based index of the string. Thus, if S 
      /// matches the first string in the list, IndexOf returns 0, if S is the 
      /// second string, IndexOf returns 1, and so on. If the string is not in 
      /// the string list, IndexOf returns -1.</returns>

      public int IndexOf(string st)
      {
         int Result;

         for (Result = 0; Result < this.Count; Result++)
         {
            if (Get(Result).ToUpper() == st.ToUpper())
            {
               return (Result);
            }
         }

         return (-1);
      }

      /// <summary>
      /// Returns the position of the first name-value pair with the specified name.
      /// </summary>
      /// <param name="Name"></param>
      /// <returns>Locates the first occurrence of a name-value pair where the name 
      /// part is equal to the Name parameter or differs only in case. IndexOfName 
      /// returns the 0-based index of the string. If no string in the list has the 
      /// indicated name, IndexOfName returns -1.</returns>

      public int IndexOfName(string Name)
      {
         int pos, Result;
         string st;

         for (Result = 0; Result < this.Count; Result++)
         {
            st = Get(Result);
            pos = st.IndexOf("=");

            if ((pos != -1) && (st.Substring(0, pos).ToUpper().Replace(@"\R\N", "\r\n") == Name.ToUpper()))
            {
               return (Result);
            }
            else
            {
               if (_IgnoreSPC)
               {
                  st = st.Trim();
                  pos = st.IndexOf("=");

                  if ((pos != -1) && (st.Substring(0, pos).Trim().ToUpper().Replace(@"\R\N", "\r\n") == Name.ToUpper()))
                  {
                     return (Result);
                  }
                  ;
               }
               else
               {
                  pos = st.IndexOf("=");

                  if (pos == -1)
                  {
                     if (Name.ToUpper() == st.ToUpper())
                     {
                        return (Result);
                     }
                     ;
                  }
                  else
                  {
                     if ((pos != -1) && (st.Substring(0, pos).ToUpper().Replace(@"\R\N", "\r\n") == Name.ToUpper()))
                     {
                        return (Result);
                     }
                     ;
                  }
                  ;
               }
               ;
            }
            ;
         }
         ;

         Result = -1;

         return (Result);
      }

      /******************************************************************************/
      /// <summary>
      /// Deletes a specified string from the list.
      /// </summary>
      /// <param name="Index"></param>

      public void Delete(int Index)
      {
         Check(Index);

         if (Index < this.Count)
         {
#if INNER_LIST
            for (int i = Index; i < _Count - 1; i++)
            {
               InnerList.SetValue(InnerList.GetValue(i + 1), i);
            }
            ;

            _Count -= 1;
#else
            InnerList.RemoveAt(Index);
#endif
         }
         ;
      }

      /******************************************************************************/
      /// <summary>
      /// Fills the list with the lines of text in a specified file.
      /// </summary>
      /// <param name="IniFileName"></param>

#if WINDOWS_PHONE
      static IsolatedStorageFile root = null;
      public void LoadFromFile(string FileName)
      {
         LoadFromFile(FileName, System.Text.Encoding.Unicode);
      }
      public void LoadFromFile(string FileName, System.Text.Encoding Encoding)
      {
         Clear();

         try
         {
            StreamReader sr;

            if (root == null)
            {
               root = IsolatedStorageFile.GetUserStoreForApplication();
            };

            if (root.FileExists(FileName))
            {
               IsolatedStorageFileStream fs = root.OpenFile(FileName, System.IO.FileMode.Open);

               if (Encoding == null)
               {
                  sr = new StreamReader(fs);
               }
               else
               {
                  sr = new StreamReader(fs, (System.Text.Encoding)(Encoding));
               };

               string line;

               while ((line = sr.ReadLine()) != null)
               {
                  Add(line);
               };

               sr.Close();
               fs.Close();
            };
         }
         catch (Exception ex)
         {
            Debug.WriteLine(String.Format("{0}: {1}", "TStrings.LoadFromFile( ... )", ex.Message));
         };
      }
#endif


#if PCL
      public async Task<bool> LoadFromFile(string FileName)
      {
         return await LoadFromFile(FileName, System.Text.Encoding.Unicode);
      }

      public async Task<bool> LoadFromFile(string FileName, System.Text.Encoding Encoding, int MaxSize = -1)
      {
         Clear();

         try
         {
            IFolder rootFolder = FileSystem.Current.LocalStorage;

            var Exists = await rootFolder.CheckExistsAsync(FileName);

            if (Exists == PCLStorage.ExistenceCheckResult.NotFound)
            {
               return false;
            }

            IFile iFile = await rootFolder.GetFileAsync(FileName);

            string st = await iFile.ReadAllTextAsync();

            if (MaxSize > -1)
            {
               // Used by AuditTrail

               if (st.Length > MaxSize)
               {
                  st = st.Substring(st.Length - MaxSize);
               }
            }

            this.Text = st;

            for (int i = 0; i < this.Count; i++)
            {
               if (this[i].EndsWith("\r"))
               {
                  this[i] = this[i].Substring(0, this[i].Length - 1);
               }
            }

            return true;
         }
         catch (Exception ex)
         {
            Debug.WriteLine(String.Format("{0}: {1}", "TStrings.LoadFromFile( ... )", ex.Message));
            return false;
         }
      }
#endif

#if NETFX_CORE
      public Task<bool> LoadFromFile(string FileName)
      {
         return LoadFromFile(FileName, System.Text.Encoding.UTF8);
      }

      public async Task<bool> LoadFromFile(string FileName, System.Text.Encoding Encoding, int MaxSize = -1)
      {
         Clear();

         try
         {
            StreamReader sr;
            StorageFolder myFolder;
            if (Path.GetDirectoryName(FileName) == "")
            {
               myFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            }
            else
            {
               myFolder = await StorageFolder.GetFolderFromPathAsync(Path.GetDirectoryName(FileName));
            };

            using (Stream s = await myFolder.OpenStreamForReadAsync(Path.GetFileName(FileName)))
            {

               using (sr = new StreamReader(s, Encoding))
               {
                  string Line;

                  while ((Line = sr.ReadLine()) != null)
                  {
                     Add(Line);
                  };

               };
            };

            return true;
         }
         catch (Exception ex)
         {
            Debug.WriteLine(String.Format("{0}: {1}", "TStrings.LoadFromFile( ... )", ex.Message));
            return false;
         }
      }
#endif


#if !WINDOWS_PHONE && !PCL && !NETFX_CORE
#if PCL || NETFX_CORE
#else
      public void LoadFromFile(string FileName, System.Text.Encoding Encoding)
#endif
      {
         Clear();

         try
         {
#if PCL
#else
            StreamReader sr;

#if WINDOWS_PHONE
#else

#if NETFX_CORE
#else
            if (Encoding == null)
            {
               sr = new StreamReader(FileName);
            }
            else
            {
               sr = new StreamReader(FileName, (System.Text.Encoding)(Encoding));
            };

            string astringLine;

            while ((astringLine = sr.ReadLine()) != null)
            {
               Add(astringLine);
            };

            sr.Close();
#endif

#endif

#endif

         }
         catch (Exception ex)
         {
            System.Diagnostics.Debug.WriteLine(String.Format("{0}: {1}", "TStrings.LoadFromFile( ... )", ex.Message));
         }
      }

#if WINCE
      public bool LoadFromFile(string FileName)
      {
         LoadFromFile(FileName, System.Text.Encoding.Default);
         return true;
      }
#endif

#if WPF || WebService || WPF || Desktop
      public void LoadFromFile(string FileName)
      {
         LoadFromFile(FileName, System.Text.Encoding.Default);
      }
#endif


#if (SILVERLIGHT && !WINDOWS_PHONE)
      public async Task<bool> LoadFromFile(string FileName)
      {
         return await LoadFromFile(FileName, System.Text.Encoding.Unicode);
      }
#endif
#endif

      /******************************************************************************/
      /// <summary>
      /// Saves the strings in the list to the specified file.
      /// </summary>
      /// <param name="IniFileName"></param>
      /// 
#if WINDOWS_PHONE
      public void SaveToFile(string FileName)
      {
         SaveToFile(FileName, System.Text.Encoding.Unicode);
      }

      public void SaveToFile(string FileName, System.Text.Encoding Encoding)
      {
         if (FileName == "") return;

         if (root == null)
         {
            root = IsolatedStorageFile.GetUserStoreForApplication();
         };

         IsolatedStorageFileStream fs = root.OpenFile(FileName, System.IO.FileMode.Create);

         StreamWriter sw = null;
         if (Encoding == null)
         {
            sw = new StreamWriter(fs);
         }
         else
         {
            sw = new StreamWriter(fs, (System.Text.Encoding)(Encoding));
         };

         for (int i = 0; i < this.Count; i++)
         {
            sw.WriteLine(Get(i));
         };

         sw.Close();
         fs.Close();
      }
#endif

#if PCL
      public async Task<bool> SaveToFile(string FileName)
      {
         return await SaveToFile(FileName, System.Text.Encoding.Unicode);
      }

      public async Task<bool> SaveToFile(string FileName, System.Text.Encoding Encoding)
      {
         //IFolder rootFolder = FileSystem.Current.LocalStorage;
         IFolder rootFolder = PCLStorage.FileSystem.Current.LocalStorage;
         IFile file = await rootFolder.CreateFileAsync(FileName, CreationCollisionOption.ReplaceExisting);

         string Result = "";

         for (int i = 0; i < this.Count; i++)
         {
            if (i == 0)
            {
               Result = this[i];
            }
            else
            {
               Result = Result + "\r\n" + this[i];
            }
         }

         await file.WriteAllTextAsync(Result);

         //await DisplayAlert("FileContent", userInfoFileContent, "OK", "Cancel");

         return true;
      }
#endif

#if NETFX_CORE
      public Task<AsyncStatus> SaveToFile(string FileName)
      {
         return SaveToFile(FileName, System.Text.Encoding.Unicode);
      }

      public async Task<AsyncStatus> SaveToFile(string FileName, System.Text.Encoding Encoding)
      {
         if (FileName == "") return AsyncStatus.Canceled;

         StorageFolder myFolder;
         if (Path.GetDirectoryName(FileName) == "")
         {
            myFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
         }
         else
         {
            myFolder = await StorageFolder.GetFolderFromPathAsync(Path.GetDirectoryName(FileName));
         };

         using (Stream s = await myFolder.OpenStreamForWriteAsync(Path.GetFileName(FileName), CreationCollisionOption.ReplaceExisting))
         {

            using (StreamWriter sw = new StreamWriter(s))
            {
               for (int i = 0; i < this.Count; i++)
               {
                  sw.WriteLine(Get(i));
               }
            };
         };

         return AsyncStatus.Completed;
      }
#endif

#if Desktop
      public void SaveToFile(string FileName)
      {
         SaveToFile(FileName, System.Text.Encoding.Default);
      }

      public void SaveToFile(string FileName, System.Text.Encoding Encoding)
      {
         if (FileName == "") return;

         StreamWriter asw = new StreamWriter(FileName, false, Encoding);

         for (int i = 0; i < this.Count; i++)
         {
            asw.WriteLine(Get(i));
         }

         asw.Close();
      }
#endif

#if WPF
      public void SaveToFile(string FileName)
      {
         SaveToFile(FileName, System.Text.Encoding.Default);
      }

      public void SaveToFile(string FileName, System.Text.Encoding Encoding)
      {
         if (FileName == "") return;

         using (StreamWriter asw = new StreamWriter(FileName, false, Encoding))
         {
            for (int i = 0; i < this.Count; i++)
            {
               asw.WriteLine(Get(i));
            }

            asw.Close();
         };
      }
#endif


#if !WINDOWS_PHONE && !NETFX_CORE && !PCL && !Desktop && !WPF

      public void SaveToFile(string FileName, System.Text.Encoding Encoding)
      {
         if (FileName == "") return;

         using (StreamWriter asw = new StreamWriter(FileName, false, Encoding))
         {
               for (int i = 0; i < this.Count; i++)
               {
                  asw.WriteLine(Get(i));
               }

               asw.Close();
         };
      }

      //async public Task<bool> SaveToFile(string FileName)
      //{
      //   return await SaveToFile(FileName, System.Text.Encoding.Unicode);
      //}

      public void SaveToFile(string FileName)
      {
         SaveToFile(FileName, System.Text.Encoding.Default);
      }
#endif

      /******************************************************************************/

      public Stream SaveToStream()
      {
         return new MemoryStream(System.Text.Encoding.UTF8.GetBytes(this.Text));
      }

      /******************************************************************************/

      public void Push(string Value)
      {
         Add(Value);
      }

      public void PushObject(string Value, Object Obj)
      {
         Push(Value);
         SetObject(Count - 1, Obj);
      }

      public string Pop()
      {
         string Result;

         Result = Get(this.Count - 1);
         Delete(this.Count - 1);

         return (Result);
      }

      public Object PopObject()
      {
         Object Result = GetObject(this.Count - 1);
         Delete(this.Count - 1);

         return Result;
      }

      /******************************************************************************/

      void Error(string Msg, int Data)
      {
         if (Debugger.IsAttached)
         {
            Debugger.Break();
         }

         throw new Exception(string.Format(Msg, Data));
      }

      /******************************************************************************/

      public string this[int Index]
      {
         get
         {
            return Get(Index);
         }
         set
         {
            Put(Index, value);
         }
      }

      public string this[string Name]
      {
         get
         {
            string Result;
            int i;

            i = IndexOfName(Name);

            if (i >= 0)
            {
               Result = Get(i).Replace("\\r\\n", "\r\n");
               Result = Result.Substring(Result.IndexOf("=") + 1);
            }
            else
            {
               Result = "";
            }
            ;

            return (Result);
         }
         set
         {
            int i;

            i = IndexOfName(Name);

            if ((value != null) && (value.Length != 0))
            {
               if (i < 0)
               {
                  i = Add(Name + "=" + value);
               }
               else
               {
                  Put(i, Name + "=" + value);
               }
               ;
            }
            else
            {
               if (i >= 0)
                  Delete(i);
            }
            ;
         }
      }

      /******************************************************************************/
      /// <summary>
      /// Indicates the name part of strings that are name-value pairs.
      /// </summary>
      /// <param name="Index"></param>
      /// <returns></returns>

      public string Names(int Index)
      {
         string Result;

         Check(Index);

         if (this.Count == 0)
         {
            Result = "";
         }
         else
         {
            if (Index >= 0)
            {
               Result = Get(Index);

               int Ind = Result.IndexOf("=");

               if (Ind != -1)
               {
                  Result = Result.Substring(0, Ind);
               }
               ;
            }
            else
            {
               Result = "";
            }
            ;
         }
         ;

         return (Result);
      }

      /******************************************************************************/
      /// <summary>
      /// Lists the strings in the TStrings object as a single string with 
      /// the individual strings delimited by carriage returns and line feeds.
      /// </summary>

      public string Text
      {
         get
         {
            string Result = "";

            for (int i = 0; i < this.Count; i++)
            {
               if (i == 0)
               {
                  Result = this[i];
               }
               else
               {
                  Result = Result + "\n" + this[i];
               };
            };

            return (Result);
         }
         set
         {
            int Ind = 0;

            this.Clear();

            Ind = value.IndexOf("\n");

            while (Ind != -1)
            {
               this.Add(value.Substring(0, Ind));
               value = value.Substring(Ind + 1, value.Length - Ind - 1);
               Ind = value.IndexOf("\n");
            };

            if (value.Length > 0)
            {
               this.Add(value);
            }
         }
      }

      /******************************************************************************/
      /// <summary>
      /// Lists the strings in the TStrings object as a single string with 
      /// the individual strings delimited by <br />.
      /// </summary>

      public string HTML
      {
         get
         {
            string Result = "";

            for (int i = 0; i < this.Count; i++)
            {
               if (i == 0)
               {
                  Result = this[i];
               }
               else
               {
                  Result = Result + "<br />" + this[i];
               }
               ;
            }
            ;

            return (Result);
         }
      }

      /******************************************************************************/
      public string HTMLTable(string ClassName = "")
      {
         string Result = (string.IsNullOrEmpty(ClassName) ? "<table>" : string.Format("<table class='{0}'>", ClassName));

         for (int i = 0; i < this.Count; i++)
         {
            if (string.IsNullOrEmpty(ClassName))
            { Result += string.Format("<tr><th>{0}</th><td>{1}</td></tr>", Names(i), ValueFromIndex(i)); }
            else
            { Result += string.Format("<tr class='##'><th class='##'>{0}</th><td class='##'>{1}</td></tr>", Names(i), ValueFromIndex(i)).Replace("##", ClassName); }
         };

         Result += "</table>";

         return (Result);
      }

      /******************************************************************************/
      /// <summary>
      /// Represents the value part of a string with a given index, on strings 
      /// that are name-value pairs.
      /// </summary>

      public string ValueFromIndex(int Index)
      {
         string Result = Get(Index);
         int pos = Result.IndexOf("=");

         if (pos != -1)
         {
            Result = Result.Substring(pos + 1);
         }
         ;

         return Result;
      }

      /******************************************************************************/

      public int Find(string Pattern)
      {
         int Result = -1;

         for (int i = 0; i < this.Count; i++)
         {
            string st = Get(i);

            if (st.StartsWith(Pattern))
            {
               return (i);
            }
         }
         ;

         return Result;
      }

      /******************************************************************************/

      private bool _IgnoreSPC;

      public bool IgnoreSPC
      {
         get { return _IgnoreSPC; }
         set { _IgnoreSPC = value; }
      }

      /******************************************************************************/

      public static TStrings FromJSon(string Text)
      {
         TStrings Result = new TStrings();

         Text = Text.Replace(",\"", "\n");
         Text = Text.Replace("\":", "=");
         Text = Text.Replace("={\"", "={");
         Text = Text.Replace("=[{\"", "=[{");

         Result.Text = Text;

         if (Result.Count > 0)
         {
            if ((Text[0] == '[') && (Text[Text.Length - 1] == ']'))
            {
               Text = Text.Substring(1, Text.Length - 2);
               Result.Text = Text + "}";
            }
            else
            {
               Result[0] = Result[0].Substring(2);
            }
            ;

            string st = Result[Result.Count - 1];

            st = st.TrimEnd(new char[] { '\r', '\n', ' ' });
            st = st.Substring(0, st.Length - 1);
            //if( !st.EndsWith( "]" ) ) st = st.Substring( 0, st.Length - 1 );

            Result[Result.Count - 1] = st;
         }
         ;

         Result.Text = Result.Text.Replace("\":", "=");

         for (int i = Result.Count - 1; i > 0; i--)
         {
            if ((CountOf(Result[i], "}") > 0) && (CountOf(Result[i], "{") != (CountOf(Result[i], "}"))))
            {
               Result[i - 1] = Result[i - 1] + "," + Result[i];
               Result.Delete(i);
            }
            ;
         }
         ;

         return Result;
      }

      private static int CountOf(string Text, string Pattern)
      {
         // Loop through all instances of the string 'text'.
         int Result = 0;
         int i = 0;

         while ((i = Text.IndexOf(Pattern, i)) != -1)
         {
            i += Pattern.Length;
            Result++;
         }
         ;

         return Result;
      }

      /******************************************************************************/

      public static TStrings FromJSonValue(string Text)
      {
         TStrings Result = new TStrings();

         if (Text != "" && Text != "[]")
         {
            if ((Text[0] == '[') && (Text[Text.Length - 1] == ']'))
            {
               Text = Text.Substring(1, Text.Length - 2);
            }
            ;

            if ((Text[0] == '{') && (Text[Text.Length - 1] == '}'))
            {
               Text = Text.Substring(1, Text.Length - 2);
            }
            ;

            Text = Text.Replace("\",", "\"\n");

            Result.Text = Text;

            for (int i = 0; i < Result.Count; i++)
            {
               if (Result.ValueFromIndex(i).StartsWith("\"") && Result.ValueFromIndex(i).EndsWith("\""))
               {
                  Result[i] = Result.Names(i) + "=" + Result.ValueFromIndex(i).Substring(1, Result.ValueFromIndex(i).Length - 2);
               }
               ;
            }
            ;
         };

         return Result;
      }
   }
}
