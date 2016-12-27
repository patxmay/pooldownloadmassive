using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Web; // pour parser url

using ZPF; // pour TIniFile
using System.Net; // pour downloading

namespace Wanao_Core
{
    public class ImportViewModel : BaseViewModel
    {
        // declaration pour acceder au controle textbox 'NomURL'p par la methode binding
        private string _NomURl;
        public string NomURl
        {
            get { return _NomURl; }
            set { SetField(ref _NomURl, value); }
        }
        private string _myStackPanel;
        public string myStackPanel
        {
            get { return _myStackPanel; }
            set { SetField(ref _myStackPanel, value); }
        }

        public void RunImport()
        {
            int counter = 0;
            string line;
            // initiailisation du proxy
            WebProxy wp = new WebProxy("127.0.0.1", 9666);

            // -----------------------------
            // Lecture du fichier des urls
            // -----------------------------
            TIniFile ini = new TIniFile("Import_Connaissance.ini");
            string FileName = ini.ReadString("General", "FichierUrl", "");
            string DirDest = ini.ReadString("General", "DirDest", "");
            System.IO.StreamReader file = new System.IO.StreamReader(FileName);
            // -------------------------------
            // positions initiales des controles
            // -------------------------------
            int pos_top, pos_left;
            pos_top = 20;
            pos_left = 10;
            while ((line = file.ReadLine()) != null)
            {
                // -----------------------------
                // creation automatique de controles
                // -----------------------------
                // Create a stringBuilder
                StringBuilder sb = new StringBuilder();
                // declare a textbox as string containing xaml
                sb.Append(@"<TextBox x:Name='txtConsole' Grid.Column='1' TextWrapping='Wrap' Text='{Binding NomURl}' ");
                sb.Append(@"HorizontalAlignment='Left' Grid.Row='1' VerticalAlignment='Top'  Height='30' Width='300' ");
                sb.Append(@"Margin='"+pos_left+","+pos_top+",0,0' />");
                // Create a textbox using a XamlReader
                System.Windows.Controls.TextBox myTextBox = (System.Windows.Controls.TextBox)System.Windows.Markup.XamlReader.Parse(sb.ToString());
                // Add created textbox to previously created container.
                
                //myStackPanel .Children.Add(myTextBox);

                //System.Console.WriteLine(line);
                // parser url

                // -----------------------------
                // telechargement url
                //---------------------------------------
                Uri myUri = new Uri(line);
                System.Console.WriteLine(myUri.Query);
                
                NomURl = NomURl + line + "\n"; 
                try
                {
                    WebClient webClient = new WebClient();
                    webClient.Proxy = wp;
                    //webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(Completed);
                    //webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);
                    string filedest = DirDest + "file_" + counter.ToString();
                    webClient.DownloadFileAsync(new Uri(line), filedest);
                    // Destruction de l'objet WebClient
                    webClient.Dispose();
                    NomURl = NomURl + "----------> Le téléchargement est terminée\n";
                    //System.Console.WriteLine("Le téléchargement est terminé");
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine("Une erreur est survenue lors du téléchargement : {0}\n", ex.Message);
                    NomURl = NomURl + "----------> Une erreur est survenue lors du téléchargement\n";
                }

                counter++;
            }

            file.Close();
            // System.Console.WriteLine("There were {0} lines.", counter);
            // Suspend the screen.  
            System.Console.ReadLine();
        }

        private void download()
        {

        }/*
        private void ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            progressBar.Value = e.ProgressPercentage;
        }

        private void Completed(object sender, AsyncCompletedEventArgs e)
        {
            MessageBox.Show("Download completed!");
        }*/

    }
}
