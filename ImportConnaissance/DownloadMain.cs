using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Web; // pour parser url

using ZPF; // pour TIniFile

using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace Wanao 
{
    public class DownloadMain 
    {
        #region Attributs
        private System.Windows.Controls.Grid mfileinfo;
        public int minputdata; // cette varaible est public car la construction de l'ihm se fait avant le téléchargement
        private System.Collections.Generic.List<System.Net.WebClient> mlistwebclient;
        private int counter;
        public int NBRECORD;
        private class XamlControl
        {
            private int top;
            private int left;
            private int width;
            private int height;
            public XamlControl(int pleft, int ptop, int pWidth, int pheight)
            {
                left = pleft;
                top = ptop;
                width = pWidth;
                height = pheight;
            }
            public int Top
            {
                get { return top; }
                set { top = value; }
            }
            public int Left
            {
                get { return left; }
                set { left = value; }
            }
            public int Width
            {
                get { return width; }
                set { width = value; }
            }
            public int Height
            {
                get { return height; }
                set { height = value; }
            }
        }
        #endregion

        #region Constructeur
        public DownloadMain(System.Windows.Controls.Grid fileinfo, int inputdata)
        {
            mfileinfo = fileinfo;
            minputdata = inputdata;
            mlistwebclient = null;
        }
        #endregion

        #region Méthodes
        private int NbUrl(string filename)
        ///Description : Compte le nombre d'urls à analyser (fichier ou table)
        ///Nom              :
        ///Parametre Entree : 
        ///Parametre Sortie : N/A
        ///Parametre Retour : N/A
        ///Auteur           : f.milhau@wanao.com
        ///Date de création : 19/12/2016
        ///--------------------------------------------------------------------------------------------------------------
        ///Modification    : redmine#yyy - xx/xx/xxxx - @wanao.com
        {
            if (minputdata == 0)
            {
                // traitement du fichier
                string commandedos = System.IO.Directory.GetCurrentDirectory() + "\\nbline.bat";
                try
                {
                    if (System.IO.File.Exists(commandedos))
                    {
                        System.IO.File.Delete(commandedos);
                    }
                    using (System.IO.StreamWriter sw = new System.IO.StreamWriter(commandedos))
                    {
                        sw.WriteLine("C:\\Windows\\System32\\findstr.exe /r /n \"^\" %1 | find /c \":\"");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("The process failed: {0}", e.ToString());
                }

                System.Diagnostics.Process proc = new System.Diagnostics.Process();
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.Arguments = (char)34 + filename + (char)34;
                proc.StartInfo.FileName = commandedos;
                proc.StartInfo.CreateNoWindow = true;
                proc.Start();
                while (!proc.HasExited)
                {

                }
                string[] output = proc.StandardOutput.ReadToEnd().Split('\n');
                return Convert.ToInt32(output[2]);
            }

            else
            {
                Int32 nblines = 0;
                DownloadDB bd = DownloadDB.Instance;
                Npgsql.NpgsqlDataReader data;
                if (minputdata == 1)
                {
                    data = bd.DataReader("SELECT count(*) FROM \"TB_TEST\"");
                    while (data.Read())
                        nblines = Int32.Parse(data[0].ToString());
                    data.Close();
                }
                else if (minputdata == 2)
                {
                    nblines = NBRECORD;
                }
                else
                {
                    data = bd.DataReader("SELECT count(*) FROM \"TB_TEST_URL\"");
                    while (data.Read())
                        nblines = Int32.Parse(data[0].ToString());
                    data.Close();
                }
                return nblines;
            }


        }

        public void GenerateIhm()
        ///Description : Construction des contrôles xaml
        ///Nom              :
        ///Parametre Entree : 
        ///Parametre Sortie : N/A
        ///Parametre Retour : N/A
        ///Auteur           : f.milhau@wanao.com
        ///Date de création : 19/12/2016
        ///--------------------------------------------------------------------------------------------------------------
        ///Modification    : redmine#yyy - xx/xx/xxxx - @wanao.com
        {
            // -----------------------------
            // Lecture du fichier des urls
            // -----------------------------
            TIniFile ini = new TIniFile("Import_Connaissance.ini");
            string FileName = ini.ReadString("General", "FichierUrl", "");
            string DirDest = ini.ReadString("General", "DirDest", "");
            System.IO.StreamReader file = new System.IO.StreamReader(FileName);

            // -------------------------------
            // Nombre de lignes ou enregistrement
            // 15 sec 1 827 840 lignes
            // -------------------------------
            int nblines = this.NbUrl(FileName);

            // -------------------------------
            // positions initiales des controles
            // -------------------------------
            int progressbar_value = 0;
            XamlControl textbox_uri = new XamlControl(10, 20, 500, 40);
            XamlControl progressbar = new XamlControl(textbox_uri.Left + textbox_uri.Width + 5, textbox_uri.Top, 400, 40);
            XamlControl button_pause = new XamlControl(progressbar.Left + progressbar.Width + 5, textbox_uri.Top, 40, 40);
            XamlControl button_cancel = new XamlControl(button_pause.Left + button_pause.Width + 5, textbox_uri.Top, 40, 40);
            XamlControl textbox_status = new XamlControl(button_cancel.Left + button_cancel.Width + 5, textbox_uri.Top, 250, 40);

            // creation des controles
            TextBox myTextBox;
            Image myImage;
            ProgressBar myProgressBar;
            Button myButton;
            mlistwebclient = new List<System.Net.WebClient>(nblines);

            for (int counter = 0; counter <= nblines-1; counter++)
            {
                string xamlstring;
                ParserContext context = new ParserContext();
                context.XmlnsDictionary.Add("", "http://schemas.microsoft.com/winfx/2006/xaml/presentation");
                context.XmlnsDictionary.Add("x", "http://schemas.microsoft.com/winfx/2006/xaml");

                // -----------------------------
                // creation automatique du controle textbox
                // -----------------------------
                xamlstring = String.Format(@"<TextBox Text='{5}' x:Name='txtConsole{0}' TextWrapping='Wrap' HorizontalAlignment='Left'  VerticalAlignment='Top' Margin='{1},{2},0,0'  Width='{3}' Height='{4}' Grid.Column='1'  Grid.Row='1'/>", counter, textbox_uri.Left, textbox_uri.Top, textbox_uri.Width, textbox_uri.Height,counter);
                myTextBox = new TextBox();
                myTextBox = (TextBox)XamlReader.Parse(xamlstring, context);
                mfileinfo.Children.Add(myTextBox);

                // -----------------------------
                // creation automatique du controle progressbar
                // -----------------------------
                xamlstring = String.Format(@"<ProgressBar x:Name='prbConsole{0}' HorizontalAlignment='Left'  VerticalAlignment='Top' Margin='{1},{2},0,0'  Width='{3}' Height='{4}'  Grid.Column='1' Grid.Row='1' Value='{5}'/>", counter, progressbar.Left, progressbar.Top, progressbar.Width, progressbar.Height, progressbar_value);
                myProgressBar = new ProgressBar();
                myProgressBar = (ProgressBar)XamlReader.Parse(xamlstring, context);
                mfileinfo.Children.Add(myProgressBar);
                
                // -----------------------------
                // creation d'un boutton pose/reprise
                // -----------------------------
                xamlstring = String.Format(@"<Button x:Name='btConsole{0}' Content='Pause' HorizontalAlignment='Left'  VerticalAlignment='Top' Margin='{1},{2},0,0'   Width='{3}' Height='{4}' Grid.Column='1' Grid.Row='1' />", counter, button_pause.Left, button_pause.Top, button_pause.Width, button_pause.Height);
                myButton = new Button();
                myButton = (Button)XamlReader.Parse(xamlstring, context);
                mfileinfo.Children.Add(myButton);
                
                // -----------------------------
                // creation d'un boutton cancel
                // -----------------------------
                xamlstring = String.Format(@"<Button x:Name='btcancelConsole{0}' Content='Cancel' HorizontalAlignment='Left'  VerticalAlignment='Top' Margin='{1},{2},0,0'   Width='{3}' Height='{4}'   Grid.Column='1' Grid.Row='1' />", counter, button_cancel.Left, button_cancel.Top, button_cancel.Width, button_cancel.Height);
                myButton = new Button();
                myButton = (Button)XamlReader.Parse(xamlstring, context);
                // ajout de l'evenement click sur le bouton
                myButton.Click += new RoutedEventHandler(btcancelConsole_Click);
                //fileinfo.Children.Insert(fileinfo.Children.Count, myButton);
                // Ajout du controle au grid fileinfo
                mfileinfo.Children.Add(myButton);
                
                // -----------------------------
                // creation d'un textbox statut
                // -----------------------------
                xamlstring = String.Format(@"<TextBox x:Name='txtStatut{0}' TextWrapping='Wrap' HorizontalAlignment='Left'  VerticalAlignment='Top' Margin='{1},{2},0,0'   Width='{3}' Height='{4}'  Grid.Column='1'  Grid.Row='1'/>", counter, textbox_status.Left, textbox_status.Top, textbox_status.Width, textbox_status.Height);
                myTextBox = new TextBox();
                myTextBox = (TextBox)XamlReader.Parse(xamlstring, context);
                mfileinfo.Children.Add(myTextBox);

                // -----------------------------
                // Ajout d'un objet webclient
                // -----------------------------
                mlistwebclient.Add(new System.Net.WebClient());

                // -----------------------------
                // increment pour la position verticale
                //------------------------------
                textbox_uri.Top = textbox_uri.Top + textbox_uri.Height + 3;
                progressbar.Top = textbox_uri.Top;
                button_pause.Top = textbox_uri.Top;
                button_cancel.Top = textbox_uri.Top;
                textbox_status.Top = textbox_uri.Top;

                System.Console.WriteLine(counter);
            } // end for

        }

        void btcancelConsole_Click(object sender, RoutedEventArgs e)
        {
            const string NAMECONTROL = "btcancelConsole";
            string index="0";
            System.Windows.Controls.Button CancelButton = (System.Windows.Controls.Button)sender;
            string buttonname = CancelButton.Name;
            string buttoncontent = CancelButton.Content.ToString();
            int pos = buttonname.IndexOf(NAMECONTROL);
            
            if (buttoncontent == "Cancel" && pos >= 0)
            {
                index = buttonname.Substring(NAMECONTROL.Length);
                Console.WriteLine("Button cancel {0} : ", index);
            }

            System.Net.WebClient client = new System.Net.WebClient();
            // parcourir la liste
            int k = 0;
            foreach(System.Net.WebClient wc in mlistwebclient)
            {
                if (k== Int32.Parse(index))
                {
                    Console.WriteLine("Opération {0} annulée",index);
                    break;
                }
                k++;
            }

        }

        public void RunImport()
        ///Description : Lit les urls depuis un fichier ou une base, et télécharge le fichier
        ///Nom              :
        ///Parametre Entree : 
        ///Parametre Sortie : N/A
        ///Parametre Retour : N/A
        ///Auteur           : f.milhau@wanao.com
        ///Date de création : 19/12/2016
        ///--------------------------------------------------------------------------------------------------------------
        ///Modification    : redmine#yyy - xx/xx/xxxx - @wanao.com
        {
            string sturi = "";
            string line = "";
            
            TIniFile ini = new TIniFile("Import_Connaissance.ini");

            string FileName = ini.ReadString("General", "FichierUrl", "");
            string filepath = ini.ReadString("General", "DirDest", "");
            System.IO.StreamReader file = new System.IO.StreamReader(FileName);

            counter = 0;

            // -----------------------------
            // Lecture du fichier des urls
            // -----------------------------
            if (minputdata == 0)
            {
                while ((line = file.ReadLine()) != null)
                {
                    sturi = line;
                    System.Console.WriteLine(sturi);
                    DownloadTask downloadtask = new DownloadTask(sturi, counter, mfileinfo, filepath, mlistwebclient[counter]);
                    downloadtask.DownloadRun();
                    counter++;
                }
                file.Close();
            }
            else
            {
                // -----------------------------
                // Lecture depuis base
                // -----------------------------
                DownloadDB bd = DownloadDB.Instance;
                Npgsql.NpgsqlDataReader data;
                if (minputdata == 1)
                    data = bd.DataReader("SELECT \"BI_DL_FROM_URL\" FROM \"TB_TEST\" ");
                else if (minputdata == 2)
                    data = bd.DataReader("SELECT \"BI_DL_FROM_URL\" FROM \"TB_TEST\" LIMIT " + NBRECORD);
                else
                    data = bd.DataReader("SELECT URL FROM \"TB_TEST_URL\" ");
                while (data.Read())
                {
                    sturi = data[0].ToString();
                    System.Console.WriteLine(sturi);
                    DownloadTask downloadtask = new DownloadTask(sturi, counter, mfileinfo, filepath, mlistwebclient[counter]);
                    downloadtask.DownloadRun();
                    counter++;
                }
                data.Close();
            }
        }
        #endregion

    }
}
