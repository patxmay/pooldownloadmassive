using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using ZPF;
using Wanao_Core; // pour appeler import
using System.Data; // bd

namespace Wanao
{
   /// <summary>
   /// Interaction logic for MainWindow.xaml
   /// </summary>
   public partial class MainWindow : Window
   {
        #region attributs
        private DownloadMain mfileimp;
        #endregion

        public MainWindow()
        {
            InitializeComponent();

            try
            {
                MainViewModel.Instance.ReadFormPos(this, false, false);
                mfileimp = new DownloadMain(this.fileinfo, this.cmbchoix.SelectedIndex);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace);
            };
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            MainViewModel.Instance.Load();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MainViewModel.Instance.WriteFormPos(this, false);
        }

        private void run_Click(object sender, RoutedEventArgs e)
        ///Description : Lancement du téléchargement massif
        ///Nom              :
        ///Parametre Entree : 
        ///Parametre Sortie : N/A
        ///Parametre Retour : N/A
        ///Auteur           : f.milhau@wanao.com
        ///Date de création : 19/12/2016
        ///--------------------------------------------------------------------------------------------------------------
        ///Modification    : redmine#yyy - xx/xx/xxxx - @wanao.com :
        {
            brddatagrid.Visibility = System.Windows.Visibility.Hidden;
            brdgrid.Visibility = System.Windows.Visibility.Visible;

            try
            {
                // version avec modele
                //VMLocator.Import.RunImport();

                // version sans modele
                mfileimp.RunImport();
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
            }
            
        }
        
        private void dashboard_Click(object sender, RoutedEventArgs e)
        {

        }

        private void bdview_Click(object sender, RoutedEventArgs e)
        ///Description : Afficher le résultat du téléchargement (bd)
        ///Nom              :
        ///Parametre Entree : 
        ///Parametre Sortie : N/A
        ///Parametre Retour : N/A
        ///Auteur           : f.milhau@wanao.com
        ///Date de création : 19/12/2016
        ///--------------------------------------------------------------------------------------------------------------
        ///Modification    : redmine#yyy - xx/xx/xxxx - @wanao.com :
        {
            try
            {

                brddatagrid.Visibility = System.Windows.Visibility.Visible;
                brdgrid.Visibility = System.Windows.Visibility.Hidden;

                // remplir un datagrid
                DownloadDB bd = DownloadDB.Instance;
                string sql = "SELECT \"FI_URL\" AS url, \"FI_SIZE\", \"FI_NAME\", \"FI_HASH256\", \"FI_DATEBEGIN\", \"FI_DATEEND\", \"FI_STATE\" , \"FI_PATH\" FROM \"TB_FILEINFO\"";
                Npgsql.NpgsqlCommand cmd = bd.Command(sql);
                int nbrows = cmd.ExecuteNonQuery();

                Npgsql.NpgsqlDataAdapter dataApp = new Npgsql.NpgsqlDataAdapter(cmd);
                DataTable dt = new DataTable("TB_FILEINFO");
                dataApp.Fill(dt);
                dataGrid.ItemsSource = dt.DefaultView;
                dataApp.Update(dt);

            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
            }
        }

        private void cmbchoix_SelectionChanged(object sender, SelectionChangedEventArgs e)
        ///Description : Sélection des urls (bd ou fichier), création des contrôles XAML et reset bd/rep
        ///Nom              :
        ///Parametre Entree : 
        ///Parametre Sortie : N/A
        ///Parametre Retour : N/A
        ///Auteur           : f.milhau@wanao.com
        ///Date de création : 19/12/2016
        ///--------------------------------------------------------------------------------------------------------------
        ///Modification    : redmine#yyy - xx/xx/xxxx - @wanao.com :
        {
            try
            {
                //--------------------------------------
                // Création de l'IHM
                //--------------------------------------
                ComboBoxItem cbitem = ((sender as ComboBox).SelectedItem as ComboBoxItem);
                // slection de element du combobox
                if (cbitem.Content != null)
                {
                    int index = (sender as ComboBox).SelectedIndex;
                    // suppression des controles existant dans la grid
                    foreach (System.Windows.UIElement child in this.fileinfo.Children)
                    {
                        this.fileinfo.Children.Remove(child);
                    }
                    // execution du telechargement
                    mfileimp.minputdata = this.cmbchoix.SelectedIndex;
                    mfileimp.NBRECORD = Int32.Parse(this.txtNbRec.Text);
                    mfileimp.GenerateIhm();
                }

                //--------------------------------------
                // Reset de la base de données et du répertoire destination
                //--------------------------------------
                // Vide la table TB_FILEINFO
                DownloadDB bd = DownloadDB.Instance;
                bd.ExecuteNonQuery("DELETE FROM \"TB_FILEINFO\"");
                this.txtstatus.Text = "Base de données initalisée.";
                Console.WriteLine("La table TB_FILEINFO a été vidée.");

                // Vide le repertoire de sortie
                string exePath = Environment.GetCommandLineArgs()[0];
                TIniFile ini = new TIniFile("Import_Connaissance.ini");
                string exeDir = ini.ReadString("General", "DirDest", "");
                string[] filenames = System.IO.Directory.GetFiles(exeDir, "*.*", System.IO.SearchOption.TopDirectoryOnly);
                foreach (string fName in filenames)
                {
                    System.IO.File.Delete(fName);
                }
                Console.WriteLine("Le répertoire [{0}] a été vidé.", exeDir);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
            }

        }


    }
}

