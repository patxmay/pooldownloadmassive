/*
==============================================================================
Nom              : DownloadTask.cs
Version          : 1.0
Auteur           : f.milhau@wanao.com
Date de creation : 19/12/2016
Description      : Cette classe decrit les fonctions pour réaliser le 
                   téléchargement de fichiers.
------------------------------------------------------------------------------
Modification    : redmine#yyy - xx/xx/xxxx - xxx@wanao.com :
==============================================================================
*/using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net; // pour downloading
using System.ComponentModel; // pour AsyncCompletedEventHandler

namespace Wanao
{
    #region Rafraichir UIElement
    /// Les Delegate anonymes et les méthodes d'extension : 
    /// La méthode Refresh est l'extension qui prend un élément UI et qui appelle son Invoke du Dispatcher. 
    /// L'astuce consiste à appeler la méthode Invoke avec DispatcherPriority. Comme nous ne voulons rien faire, on créé un Delegate vide 
    /// Lorsque le DispatcherPriority est défini, le code exécutera alors toutes les opérations.
    public static class ExtensionMethods
    {
        private static int p ;
        private static Action EmptyDelegate = delegate () { };
        public static void Refresh(this System.Windows.UIElement uiElement)
        {
            p++;
            System.Console.WriteLine("DownloadTask.cs::Refresh : Refresh : {0}" , p.ToString());
            try
            {
                uiElement.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Render, EmptyDelegate);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine("DownloadTask.cs::Refresh : Probleme Refresh : Exception = {0}",ex.Message);
            }  
        }
    }
    #endregion

    class DownloadTask ////: System.Windows.Forms.Control
    {
        const Boolean BREFRESH = false;

        #region Attributs
        private int mcounter;
        private int mnbcontrol;
        private System.Windows.Controls.Grid mfileinfo;
        private WebClient mwebClient ;

        ///Description des champs de la table de la base de données
        ///
        private Uri mUri;          // FI_URL
        private Int64 mfilesize;   // FI_SIZE
        private string mfilepath;  // FI_PATH
        private string mfilename;  // FI_NAME 
        private string mhash;      // FI_HASH
        private string mdatedeb;   // FI_DATEBEGIN
        private string mdatefin;   // FI_DATEEND
        private string mstate;     // FI_STATE

        ///Description des informations liées au fichier téléchargé
        ///
        private class FileInfo
        {
            private string name;
            private Int64 size;
            public FileInfo(string pname, Int64 psize)
            {
                name = pname;
                size = psize;
            }
            public string Name
            {
                get { return name; }
                set { name = value; }
            }
            public Int64 Size
            {
                get { return size; }
                set { size = value; }
            }
        }

        ////private System.Diagnostics.Stopwatch _sw;
        #endregion

        #region Constructeur
        public DownloadTask(string pUri, int counter, System.Windows.Controls.Grid fileinfo, string filepath, WebClient wc)
        ///Description : Constructeur
        ///Nom              :
        ///Parametre Entree : pUri (string) : adresse wew
        ///                   counter (int) :  index du controle xaml courant
        ///                   fileinfo (grid) : xaml grid 
        ///                   filepath (string ) : chemin de destination des fichiers téléchargés
        ///                   wc (WebClient) : object crée par le programme principal permettant de gérer le downloading
        ///Parametre Sortie : N/A
        ///Parametre Retour : N/A
        ///Auteur           : f.milhau@wanao.com
        ///Date de création : 19/12/2016
        ///--------------------------------------------------------------------------------------------------------------
        ///Modification    : redmine#yyy - xx/xx/xxxx - @wanao.com :
        {
            mcounter = counter;
            mfileinfo = fileinfo;
            mUri = new Uri(pUri);
            mnbcontrol = 5;
            mfilepath = filepath;

            // on stocke l'objet webclient pour pouvoir gerer evenement cancel
            mwebClient = wc;
        }
        #endregion

        #region Méthodes
        public void DownloadRun()
        ///Description : 
        ///Nom              :
        ///Parametre Entree : 
        ///Parametre Sortie : N/A
        ///Parametre Retour : N/A
        ///Auteur           : f.milhau@wanao.com
        ///Date de création : 19/12/2016
        ///--------------------------------------------------------------------------------------------------------------
        ///Modification    : redmine#yyy - xx/xx/xxxx - @wanao.com :
        {
            ////System.Threading.Thread bgThread = new System.Threading.Thread(() =>
            ////{
                ////_sw = new System.Diagnostics.Stopwatch();
                ////_sw.Start();

                // initialisation du proxy
                //WebProxy wp = new WebProxy("127.0.0.1", 9666);
                try
                {

                    /*if (mUri.AbsoluteUri == "https://www.achatpublic.com/sdm/ent/pub/affichageAvis.do?&docs=53396644&formatParam=pdf")
                    {
                        string t = "eee";
                    }*/

                    // Affichage uri à l'écran (index 0)
                    if (mfileinfo.Children[mcounter * mnbcontrol] is System.Windows.Controls.TextBox)
                    {
                        System.Windows.Controls.TextBox myTextBox = new System.Windows.Controls.TextBox();
                        myTextBox = (System.Windows.Controls.TextBox)mfileinfo.Children[mcounter * mnbcontrol];
                        myTextBox.Text = mUri.AbsoluteUri;
                        if (BREFRESH)
                        {
                            myTextBox.Refresh();
                            //System.Threading.Thread.Sleep(500);
                        }
                    }

                    //mwebClient.Credentials = (ICredentials)netCred;
                    //mwebClient.UseDefaultCredentials = true;

                    // Connexion via proxy
                    //mwebClient.Proxy = wp;

                    mwebClient.DownloadFileCompleted += new AsyncCompletedEventHandler(mwebClient_Completed);
                    mwebClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(mwebClient_ProgressChanged);

                    // Mise à jour des attriobuts avant le downloading
                    mdatedeb = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
                    FileInfo fi = new FileInfo("", 0);
                    fi = GetFileInfo();
                    mfilename = fi.Name;
                    mfilesize = fi.Size;
                    //mfilename = "fff" + mcounter.ToString();

                    // Affichage console
                    System.Console.WriteLine("DownloadTask.cs::DownloadRun : Fichier = {0}", mfilepath + mfilename);

                    // Downloading
                    mwebClient.DownloadFileAsync(mUri, mfilepath + mfilename, mcounter);//, evtCompleted

                }
                catch (Exception ex)
                {
                    string message = "Une exception est survenue sur la commande de téléchargement du fichier " + mfilepath + mfilename;

                    // Affichage état à l'écran
                    if (mfileinfo.Children[mcounter * mnbcontrol + 4] is System.Windows.Controls.TextBox)
                    {
                        System.Windows.Controls.TextBox myTextBox = new System.Windows.Controls.TextBox();
                        myTextBox = (System.Windows.Controls.TextBox)mfileinfo.Children[mcounter * mnbcontrol + 4];
                        myTextBox.Text = message;
                    }
                    // Stockage état en base de données
                    mstate = message;

                    // Affichage console
                    System.Console.WriteLine("DownloadTask.cs::DownloadRun : {0}", message);
                    System.Console.WriteLine("DownloadTask.cs::DownloadRun : Exception = {0}", ex.Message);
                }

                // Destruction de l'objet mwebClient
                mwebClient.Dispose();

            ////});
            ////bgThread.Start();
        }

        #region évènements
        private void mwebClient_ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        ///Description : 
        ///Nom              :
        ///Parametre Entree : 
        ///Parametre Sortie : N/A
        ///Parametre Retour : N/A
        ///Auteur           : f.milhau@wanao.com
        ///Date de création : 19/12/2016
        ///--------------------------------------------------------------------------------------------------------------
        ///Modification    : redmine#yyy - xx/xx/xxxx - @wanao.com :
        {
            ////this.BeginInvoke((System.Windows.Forms.MethodInvoker)delegate
            ////{

                int pgbv = 0;
                string message = "??????????????";

                double bytesIn = double.Parse(e.BytesReceived.ToString());
                double totalBytes = double.Parse(e.TotalBytesToReceive.ToString());
                double percentage = bytesIn / totalBytes * 100;

                //ProgressBar myProgressBar = (ProgressBar)sender;
                System.Windows.Controls.ProgressBar myProgressBar = null;
                if (mfileinfo.Children[(mcounter * mnbcontrol) + 1] is System.Windows.Controls.ProgressBar)
                {
                    myProgressBar = (System.Windows.Controls.ProgressBar)mfileinfo.Children[(mcounter * mnbcontrol) + 1];
                    pgbv = int.Parse(Math.Truncate(percentage).ToString());
                    myProgressBar.Value = pgbv;
                    message = myProgressBar.Name;
                }
                // Mise à jour de l'attribut état
                mstate = "En cours...";

                // Affichage console
                //System.Console.WriteLine("DownloadTask.cs::mwebClient_ProgressChanged : {0} = {1} %", message, pgbv);
            ////});


            }

        private void mwebClient_Completed(object sender, AsyncCompletedEventArgs e)
        ///Description : 
        ///Nom              :
        ///Parametre Entree : 
        ///Parametre Sortie : N/A
        ///Parametre Retour : N/A
        ///Auteur           : f.milhau@wanao.com
        ///Date de création : 19/12/2016
        ///--------------------------------------------------------------------------------------------------------------
        ///Modification    : redmine#yyy - xx/xx/xxxx - @wanao.com :
        {
            // ArgumentNullException : Le parametre address a la valeur null ou Le parametre fileName a la valeur null.
            // WebException : URI forme en combinant BaseAddress et address n’est pas valide ou Une erreur s’est produite lors du telechargement de la ressource.
            // InvalidOperationException : Le fichier local spécifié par fileName est en cours d’utilisation par un autre thread.
            
            ////this.BeginInvoke((System.Windows.Forms.MethodInvoker)delegate
            ////{
                string message = "";
                if (e.Error == null)
                {
                    message = "Le téléchargement s'est terminé correctement.";
                }
                else if (e.Cancelled == false && e.Error != null)
                {
                    message = "Une erreur est survenue lors du téléchargement.";
                }
                if (e.Cancelled == true)
                {
                    message = "L'opération de téléchargement a été annulée.";
                }

                // Affichage de l'état à l'écran (index 4)
                if (mfileinfo.Children[mcounter * mnbcontrol + 4] is System.Windows.Controls.TextBox)
                {
                    System.Windows.Controls.TextBox myTextBox = new System.Windows.Controls.TextBox();
                    myTextBox = (System.Windows.Controls.TextBox)mfileinfo.Children[mcounter * mnbcontrol + 4];
                    myTextBox.Text = myTextBox.Text + message + "\n";
                    if (BREFRESH)
                    {
                        myTextBox.Refresh();
                        //System.Threading.Thread.Sleep(500);
                    }
                }

                // Mise à jour des attributs de classe avant l'écriture en base
                mstate = message; // état du téléchargement
                mhash = GetFileHash(mfilepath + mfilename); // hash du fichier télécharger
                mdatefin = DateTime.Now.ToString("yyyy-MM-dd HH:mm");

                // Mise à jour en base
                UpdateDatabase();

                // Dispose mwebClient 
                ((IDisposable)sender).Dispose();

                // Affichage console
                System.Console.WriteLine("DownloadTask.cs::mwebClient_Completed : Completed : {0}", message);

            ////});

        }
        #endregion évènements

        private FileInfo GetFileInfo()
        ///Description : Retourne des informations sur le fichier à télécharger : taille, nom
        ///Nom              :
        ///Parametre Entree : 
        ///Parametre Sortie : N/A
        ///Parametre Retour : N/A
        ///Auteur           : f.milhau@wanao.com
        ///Date de création : 19/12/2016
        ///--------------------------------------------------------------------------------------------------------------
        ///Modification    : redmine#yyy - xx/xx/xxxx - @wanao.com
        {
            string header;
            FileInfo fi = new FileInfo("",0);
            try
            {
                mwebClient.OpenRead(mUri.AbsoluteUri);
                // recuperation de la taille
                fi.Size = Convert.ToInt64(mwebClient.ResponseHeaders["Content-Length"]);
                // recuperation du nom
                header = mwebClient.ResponseHeaders["Content-Disposition"] ?? string.Empty;
                if (!String.IsNullOrEmpty(header))
                {
                    fi.Name = header.Substring(header.IndexOf("filename=", StringComparison.OrdinalIgnoreCase) + 9);
                    fi.Name = fi.Name.Replace("\"", "");
                }
                else
                {
                    // extraction du nom depuis l'url
                    int position = mUri.AbsoluteUri.LastIndexOf('/');
                    if (position == -1)
                        fi.Name = "file_" + mcounter.ToString();
                    else
                        fi.Name = mUri.AbsoluteUri.Substring(position + 1);
                }
                // on teste si le fichier n'existe pas déjà dans le répertoire
                if (System.IO.File.Exists(mfilepath+ fi.Name))
                {
                    string extension = System.IO.Path.GetExtension(mfilepath + fi.Name);
                    string name = System.IO.Path.GetFileName(mfilepath + fi.Name);
                    fi.Name = name + "_" + mcounter + extension;
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
                mstate = mstate + "\nImpossible de récupérer les informations sur le fichier (nom, taille) car l'url ne contient pas de fichier";
            }
            mwebClient.Dispose();
            return fi;
        }

        private string GetFileHash(string cheminFichier)
        ///Description : Calcule à partir d’un fichier numérique son empreinte numérique  
        ///Nom              :
        ///Parametre Entree : 
        ///Parametre Sortie : N/A
        ///Parametre Retour : N/A
        ///Auteur           : f.milhau@wanao.com
        ///Date de création : 19/12/2016
        ///--------------------------------------------------------------------------------------------------------------
        ///Modification    : redmine#yyy - xx/xx/xxxx - @wanao.com
        {
            string strhash = "";
            try
            {
                System.Security.Cryptography.SHA256 encrypt = System.Security.Cryptography.SHA256.Create();
                System.IO.FileStream flux = new System.IO.FileStream(cheminFichier, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite);
                byte[] hachage = encrypt.ComputeHash(flux);
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hachage.Length; i++)
                {
                    sb.Append(hachage[i].ToString("X2"));
                }
                System.Console.WriteLine("DownloadTask.cs::GetFileHash : Hash = {0}", sb);
                strhash = sb.ToString();
                
            }
            catch (Exception ex)
            {
                mstate = "Impossible de récupérer le hash du fichier";
                System.Console.WriteLine("DownloadTask.cs::GetFileHash : Impossible de récupérer le hash du fichier");
                System.Console.WriteLine("DownloadTask.cs::GetFileHash : Exception = {0}", ex.Message);
            }
            return strhash;          
        }

        private void UpdateDatabase()
        ///Description : Met à jour les informations téléchargées dans la base de donnée
        ///Nom              :
        ///Parametre Entree : 
        ///Parametre Sortie : N/A
        ///Parametre Retour : N/A
        ///Auteur           : f.milhau@wanao.com
        ///Date de création : 19/12/2016
        ///--------------------------------------------------------------------------------------------------------------
        ///Modification    : redmine#yyy - xx/xx/xxxx - @wanao.com
        {
            string sqlfield = "";
            string sqlvalue = "";
            string sql = "";

            string fi_url = "\"FI_URL\"";
            string fi_size = "\"FI_SIZE\"";
            string fi_name = "\"FI_NAME\"";
            string fi_hash = "\"FI_HASH256\"";
            string fi_datedeb = "\"FI_DATEBEGIN\"";
            string fi_dateend = "\"FI_DATEEND\"";
            string fi_path = "\"FI_PATH\"";
            string fi_state = "\"FI_STATE\"";

            sqlfield = "(" + fi_url + "," + fi_datedeb + "," + fi_dateend + "," + fi_size + "," + fi_path + "," + fi_name + "," + fi_state +  "," + fi_hash + ")";
            sqlvalue = "'" + mUri.AbsoluteUri + "'," +
                       "'" + mdatedeb  + "'," +
                       "'" + mdatefin  + "'," + 
                       mfilesize + "," + 
                       "'" + mfilepath + "'," + 
                       "'" + mfilename + "'," + 
                       "'" + mstate.Replace("'", "''") + "'," + 
                       "'" + mhash + "'";
            sql = "INSERT INTO \"TB_FILEINFO\" " + sqlfield + " VALUES (" + sqlvalue + ")";
            DownloadDB bd = DownloadDB.Instance;
            bd.ExecuteNonQuery(sql);
        }
        #endregion
    }
}
