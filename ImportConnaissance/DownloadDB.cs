using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ZPF; // pour TIniFile
using Npgsql; // postgresql

namespace Wanao
{
    public sealed class DownloadDB
    {
        private NpgsqlConnection BdConnexion;

        private DownloadDB()
        {
            TIniFile ini = new TIniFile("Import_Connaissance.ini");

            string bdServer = ini.ReadString("BD", "SERVER", "");
            string bdDatabase = ini.ReadString("BD", "DATABASE", "");
            string bdLogin = ini.ReadString("BD", "LOGIN", "");
            string bdPassword = ini.ReadString("BD", "PASSWORD", "");
            string chaineconnexion = "Server=" + bdServer + ";User Id=" + bdLogin + ";" + "Password=" + bdPassword + ";Database=" + bdDatabase + ";";

            BdConnexion = new NpgsqlConnection(chaineconnexion);
            BdConnexion.Open();
        }

        public static DownloadDB Instance { get { return Nested.instance; } }

        private class Nested
        {
            static Nested()
            {
            }

            internal static readonly DownloadDB instance = new DownloadDB();
        }

        public void ExecuteNonQuery(string ssql)
        {
            NpgsqlTransaction transaction = null;

            System.Console.WriteLine("DownloadDB::ExecuteNonQuery : {0}", ssql);

            // si l'operation est deja en cours, on ferme puis re-ouvre la connexion
            if (BdConnexion.FullState == (System.Data.ConnectionState.Open | System.Data.ConnectionState.Fetching))
            {
                BdConnexion.Close();
                BdConnexion.Open();
                System.Console.WriteLine("DownloadDB::ExecuteNonQuery : Connexion fermée puis ré-ouverte");
            }
            try
            {
                // le try nous permet de verifier qu'il n'y ait pas une transaction en cours
                transaction = BdConnexion.BeginTransaction();
                
                NpgsqlCommand command = BdConnexion.CreateCommand();
                command.CommandText = ssql;
                command.ExecuteNonQuery();
                command.Dispose();
                transaction.Commit();
                transaction.Dispose();
                //BdConnexion.Close();
            }
            catch (Exception ex)
            {
                System.Console.WriteLine("DownloadDB::ExecuteNonQuery : Exception = {0}", ex.Message);
                System.Console.WriteLine("DownloadDB::ExecuteNonQuery : La requête ne s'est pas exécutée.");
            }
        }

        public NpgsqlDataReader DataReader(string ssql)
        {
            NpgsqlCommand command = new NpgsqlCommand(ssql, BdConnexion);
            NpgsqlDataReader dr = command.ExecuteReader();
            return dr;
        }

        public NpgsqlCommand Command(string ssql)
        {
            NpgsqlCommand command = new NpgsqlCommand(ssql, BdConnexion);
            return command;
        }
    }


}
