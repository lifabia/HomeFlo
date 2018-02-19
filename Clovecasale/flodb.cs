using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Data.SqlClient;
using System.Data;

namespace Clovecasale
{
    public partial class SqlDataAccess : IDisposable
    {


        
        #region Variabili private
        private SqlConnection sqlConnection;
        private SqlTransaction oracleTx;

        #endregion

        #region Proprietà

        public SqlConnection SqlConnection
        {
            get { return this.sqlConnection; }
        }

        public object CommandStrings { get; private set; }

        #endregion

        #region Costruttori

        public SqlDataAccess(string connectionString)
        {
            try
            {
                
                sqlConnection = new SqlConnection();
                this.sqlConnection.ConnectionString = connectionString;
            }
            catch (Exception e)
            {
                //log.Error("DataAccess exception" + e);
            }
        }

        #endregion

        #region Connessioni e transazioni

        public void OpenConnection()
        {
            try
            {
                if (this.sqlConnection != null)
                    if (!(this.sqlConnection.State == System.Data.ConnectionState.Open))
                        this.sqlConnection.Open();
            }
            catch (Exception exc)
            {
               // log.Error("DataAccess.OpenConnection exception" + exc);
                throw exc;
            }
        }

        public void CloseConnection()
        {
            if (this.sqlConnection != null)
            {
                try
                {
                    this.sqlConnection.Close();
                }
                catch
                { }
            }
        }

        public void BeginTransaction()
        {
            try
            {
                this.oracleTx = this.sqlConnection.BeginTransaction();
            }
            catch (Exception exc)
            {
                throw exc;
            }
        }

        public void BeginTransaction(IsolationLevel isolationLevel)
        {
            try
            {
                this.oracleTx = this.sqlConnection.BeginTransaction(isolationLevel);
            }
            catch (Exception exc)
            {
                throw (exc);
            }
        }

        public bool IsTransactionActive()
        {
            return this.oracleTx != null;
        }

        public void CommitTransaction()
        {
            try
            {
                this.oracleTx.Commit();
            }
            catch (Exception exc)
            {
                throw (exc);
            }
        }

        public void RollBackTransaction()
        {
            try
            {
                if (this.oracleTx != null)
                    this.oracleTx.Rollback();
            }
            catch (Exception exc)
            {
                throw exc;
            }
        }

        #endregion

        #region metodi comuni
        public DataSet GetDataSetResult(string CommandText, string tablename)
        {
            object result = new object();
            DataSet ds = new DataSet();
            try
            {

                this.OpenConnection();
                SqlCommand command = this.sqlConnection.CreateCommand();
                command.CommandText = CommandText;
                SqlDataAdapter od = new SqlDataAdapter(command);
                od.Fill(ds, tablename);
            }
            catch (Exception exc)
            {
                //log.Error("DataAccess.GetDataSetResult exception" + exc);
                Exception myExc = new Exception(exc.Message + ", commandText: " + CommandText, exc);
                throw myExc;
            }
            finally
            {
                this.CloseConnection();
            }
            return ds;
        }
        #endregion metodi comuni

        public List<cZone> GetZoneUtente(int idUtente)
        {
            List<cZone> ListaZone = new List<cZone>();

            this.OpenConnection();

            string commandString = "";
            try
            {


                using (SqlCommand cmdGet = new SqlCommand(commandString, this.sqlConnection))
                {

                    using (SqlDataReader dr = cmdGet.ExecuteReader())
                    {
                        if (dr.HasRows)
                        {
                            while (dr.Read())
                            {
                                cZone zona = new cZone(idUtente);
                                zona.nomeZona = dr["Impianto"].ToString();
                                //zona.dettagliZona = dr["DescrizioneImpianto"].ToString();
                                //zona.immagini = Convert.ToDateTime(dr["DataInizioValiditaImpianto"]);
                               
                                ListaZone.Add(zona);
                            }
                        }
                        else
                        {
                            throw new Exception("Non trovati impianti attivi");
                        }
                    }
                }
             //   ListaZone = (from c in ListaZone select new cZone() { c.nomeZona = c.Impianto, DescrizioneImpianto = c.DescrizioneImpianto, DataInizioValidita = c.DataInizioValidita, DataFineValidita = c.DataFineValidita }).OrderBy(c => c.DataInizioValidita).GroupBy(c => c.Impianto).Select(g => g.Last()).ToList();

                return ListaZone;
            }
            catch (Exception exc)
            {

                throw exc;

            }
            finally
            {
               
                    this.CloseConnection();
            }


        }
      
       

        public DateTime GetDateTime()
        {
            DataSet dsdata = this.GetDataSetResult("STRINGA QUERY SQL", "DbDate");

            DateTime dt = dsdata.Tables["DbDate"].Rows[0].Field<DateTime>(0);
            return dt;
        }

      


        public int SQLExecute(string Query)
        {
            this.OpenConnection();
            this.BeginTransaction(IsolationLevel.ReadCommitted);

            try
            {
                SqlCommand SqlCmd = new SqlCommand("", this.sqlConnection);

                SqlCmd.Transaction = this.oracleTx;
                SqlCmd.CommandText = Query;

                int res = SqlCmd.ExecuteNonQuery();

                this.CommitTransaction();

                return res;

            }
            catch (Exception exc)
            {
                try
                {

                    this.RollBackTransaction();

                }
                catch (Exception)
                {
                    //nothing to do
                }

                Exception myExc = new Exception(exc.Message + Query, exc);
                //log.Debug(string.Format("Funzione:{0}", "SQLExecute " + Query));
                throw myExc;

                //   throw exc;

            }
            finally
            {

                this.CloseConnection();
            }
        }

        public DataSet SQLQuery(string Query)
        {

            DataSet Result = new DataSet("dsQueryResult");

            try
            {
                this.OpenConnection();

                using (SqlCommand cmdGet = new SqlCommand(Query, this.sqlConnection))
                {
                    using (SqlDataAdapter da = new SqlDataAdapter(cmdGet))
                    {
                        da.Fill(Result, "dtQueryResult");
                    }
                }

            }
            catch (Exception exc)
            {

                Exception myExc = new Exception(exc.Message + Query, exc);
                //log.Debug(string.Format("Funzione:{0}", "SQLExecute " + Query));
                throw;

            }
            finally
            {
                this.CloseConnection();
            }

            return Result;
        }

        //public List<ConfigurazioneLOG> SetConfigurazioneLog(bool isTransaction, List<ConfigurazioneLOG> itemconfigurazionelog)
        //{
        //    object result = new object();
        //    List<ConfigurazioneLOG> ListaElementiConfigurazione = new List<ConfigurazioneLOG>();
        //    this.OpenConnection();
        //    this.BeginTransaction(IsolationLevel.ReadCommitted);



        //    string insertstring = "";
        //    string commandstring = "";


        //    try
        //    {
        //        SqlCommand cmdInsert = new SqlCommand("", this.sqlConnection);
        //        cmdInsert.Transaction = this.oracleTx;

        //        foreach (ConfigurazioneLOG item in itemconfigurazionelog)
        //        {

        //            string insert = "";
        //            //delete record
        //            if (item.id != 0 && item.cancella)
        //            {
        //                insert += "delete from  ConfigurazioneLog where id=" + item.id;
        //            }
        //            //update
        //            if (item.id != 0 && (!item.cancella))
        //            {
        //                if (string.IsNullOrEmpty(item.OperationName))
        //                    item.OperationName = "";
        //                if (string.IsNullOrEmpty(item.EventTypes))
        //                    item.EventTypes = "";

        //                if (string.IsNullOrEmpty(item.logInfoFilter))
        //                    item.logInfoFilter = "";

        //                if (string.IsNullOrEmpty(item.ExternalOperationName))
        //                    item.ExternalOperationName = "";

        //                insert += "update ConfigurazioneLog set operationName = '" + item.OperationName + "' ,  eventTypes = '" + item.EventTypes + "' , logInfoFilter = '" + item.logInfoFilter + "' , externalOperationName  = '" + item.ExternalOperationName + "' ,  ordine = " + item.ordine + "  where id=" + item.id;
        //            }
        //            //inserisci
        //            if (item.id == 0)
        //            {
        //                insert += "insert into ConfigurazioneLog ( operationName, eventTypes, logInfoFilter, externalOperationName, ordine) values ('" + item.OperationName + "' ,   '" + item.EventTypes + "' ,  '" + item.logInfoFilter + "' , '" + item.ExternalOperationName + "' , " + item.ordine + ")";
        //            }
        //            insertstring = insert;
        //            cmdInsert.CommandText = insert;

        //            //  cmdInsert.Transaction = this.oracleTx;
        //            int res = cmdInsert.ExecuteNonQuery();
        //            if (isTransaction)
        //            {
        //                if (res != 1)
        //                {
        //                    //throw new Exception("Inserimento in errore: ");
        //                }
        //            }
        //        }
        //        this.CommitTransaction();











        //        commandstring = "select operationName, eventTypes, logInfoFilter, externalOperationName, ordine, id from ConfigurazioneLog order by ordine";
        //        using (SqlCommand cmdGet = new SqlCommand(commandstring, this.sqlConnection))
        //        {

        //            using (SqlDataReader dr = cmdGet.ExecuteReader())
        //            {
        //                if (dr.HasRows)
        //                {
        //                    while (dr.Read())
        //                    {
        //                        ConfigurazioneLOG item = new ConfigurazioneLOG();
        //                        if (dr["OperationName"] != DBNull.Value)
        //                            item.OperationName = dr["OperationName"].ToString();
        //                        if (dr["EventTypes"] != DBNull.Value)
        //                            item.EventTypes = dr["EventTypes"].ToString();

        //                        if (dr["ExternalOperationName"] != DBNull.Value)
        //                            item.ExternalOperationName = dr["OperationName"].ToString();

        //                        if (dr["logInfoFilter"] != DBNull.Value)
        //                            item.logInfoFilter = dr["logInfoFilter"].ToString();

        //                        item.id = Convert.ToInt32(dr["id"].ToString());
        //                        item.ordine = Convert.ToInt32(dr["ordine"].ToString());


        //                        ListaElementiConfigurazione.Add(item);
        //                    }
        //                }
        //                else
        //                {
        //                    //  throw new Exception("Non trovate configurazioni attive");
        //                }
        //            }
        //        }


        //        return ListaElementiConfigurazione;
        //    }
        //    catch (Exception exc)
        //    {
        //        try
        //        {

        //            this.RollBackTransaction();

        //        }
        //        catch (Exception)
        //        {
        //            //nothing to do
        //        }

        //        Exception myExc = new Exception(exc.Message + insertstring, exc);
        //        log.Debug(string.Format("Funzione:{0}", "SetConfigurazioneLOG " + insertstring));
        //        throw myExc;

        //        //   throw exc;

        //    }
        //    finally
        //    {

        //        this.CloseConnection();
        //    }


        //}

        //public List<ConfigurazioneLOG> GetConfigurazioneLog(bool isTransaction)
        //{
        //    object result = new object();
        //    List<ConfigurazioneLOG> ListaElementiConfigurazione = new List<ConfigurazioneLOG>();

        //    if (!isTransaction)
        //        this.OpenConnection();
        //    string commandstring = "";


        //    try
        //    {


        //        commandstring = "select operationName, eventTypes, logInfoFilter, externalOperationName, ordine, id from ConfigurazioneLog order by ordine";
        //        using (SqlCommand cmdGet = new SqlCommand(commandstring, this.sqlConnection))
        //        {

        //            using (SqlDataReader dr = cmdGet.ExecuteReader())
        //            {
        //                if (dr.HasRows)
        //                {
        //                    while (dr.Read())
        //                    {
        //                        ConfigurazioneLOG item = new ConfigurazioneLOG();
        //                        if (dr["OperationName"] != DBNull.Value)
        //                            item.OperationName = dr["OperationName"].ToString();
        //                        if (dr["EventTypes"] != DBNull.Value)
        //                            item.EventTypes = dr["EventTypes"].ToString();

        //                        if (dr["ExternalOperationName"] != DBNull.Value)
        //                            item.ExternalOperationName = dr["ExternalOperationName"].ToString();

        //                        if (dr["logInfoFilter"] != DBNull.Value)
        //                            item.logInfoFilter = dr["logInfoFilter"].ToString();

        //                        item.id = Convert.ToInt32(dr["id"].ToString());
        //                        item.ordine = Convert.ToInt32(dr["ordine"].ToString());


        //                        ListaElementiConfigurazione.Add(item);
        //                    }
        //                }
        //                else
        //                {
        //                    //  throw new Exception("Non trovate configurazioni attive");
        //                }
        //            }
        //        }


        //        return ListaElementiConfigurazione;
        //    }
        //    catch (Exception exc)
        //    {
        //        return new List<ConfigurazioneLOG>(); ;
        //        //   throw exc;

        //    }
        //    finally
        //    {
        //        if (!isTransaction)
        //            this.CloseConnection();
        //    }


        //}

      

        #region "Download file"
        //public NewsFile DownloadNewsFile(string id)
        //{
        //    NewsFile result = null;
        //    try
        //    {
        //        this.OpenConnection();
        //        using (SqlCommand cmdGet = new SqlCommand(CommandStrings.DownloadFile, this.sqlConnection))
        //        {
        //            SqlParameterCollection paramCollection = cmdGet.Parameters;
        //            SqlParameter parameter = new SqlParameter();
        //            parameter.ParameterName = "@id";
        //            parameter.Value = id;
        //            parameter.DbType = DbType.Int32;
        //            paramCollection.Add(parameter);
        //            using (SqlDataReader dr = cmdGet.ExecuteReader())
        //            {
        //                if (dr.HasRows)
        //                {
        //                    result = new NewsFile();
        //                    while (dr.Read())
        //                    {
        //                        result.titolo = dr["titolo"].ToString();
        //                        result.estensione = dr["estensione"].ToString();
        //                        result.fileBytes = (byte[])dr["bytesDocumento"];
        //                    }
        //                }
        //                else
        //                {
        //                    throw new Exception("File non trovato");
        //                }
        //            }
        //        }
        //        return result;
        //    }
        //    catch (Exception exc)
        //    {
        //        throw exc;
        //    }
        //    finally
        //    {
        //        this.CloseConnection();
        //    }

        //}
        //#endregion

        //#region "UploadFile"

        //public bool UploadFile(NewsFile file)
        //{
        //    try
        //    {
        //        this.OpenConnection();
        //        SqlParameterCollection paramCollection = new SqlCommand().Parameters;

        //        SqlCommand cmdInsert = new SqlCommand(CommandStrings.fileInsert, this.sqlConnection);
        //        paramCollection = cmdInsert.Parameters;

        //        SqlParameter parameter;

        //        parameter = new SqlParameter();
        //        parameter.ParameterName = "@UserID";
        //        parameter.Value = file.userId;
        //        parameter.DbType = DbType.String;
        //        paramCollection.Add(parameter);

        //        parameter = new SqlParameter();
        //        parameter.ParameterName = "@dataUpload";
        //        parameter.Value = DateTime.Now;
        //        parameter.DbType = DbType.DateTime;
        //        paramCollection.Add(parameter);

        //        parameter = new SqlParameter();
        //        parameter.ParameterName = "@dataInizioValidita";
        //        parameter.Value = file.dtInizioValidita;
        //        parameter.DbType = DbType.DateTime;
        //        paramCollection.Add(parameter);

        //        parameter = new SqlParameter();
        //        parameter.ParameterName = "@dataFineValidita";
        //        parameter.Value = file.dtFineValidita;
        //        parameter.DbType = DbType.DateTime;
        //        paramCollection.Add(parameter);

        //        parameter = new SqlParameter();
        //        parameter.ParameterName = "@titolo";
        //        parameter.Value = file.titolo;
        //        parameter.DbType = DbType.String;
        //        paramCollection.Add(parameter);

        //        parameter = new SqlParameter();
        //        parameter.ParameterName = "@estensione";
        //        parameter.Value = file.estensione.Replace(".", "");
        //        parameter.DbType = DbType.String;
        //        paramCollection.Add(parameter);

        //        string ordineValueCmd = @"SELECT ISNULL(max(ordine),0)+1
        //            FROM[SessionManager].[dbo].[News]";

        //        parameter = new SqlParameter();
        //        parameter.ParameterName = "@bytesDocumento";
        //        parameter.Value = file.fileBytes;
        //        parameter.DbType = DbType.Binary;
        //        paramCollection.Add(parameter);

        //        SqlCommand cmdOrdine = new SqlCommand(ordineValueCmd, this.sqlConnection);
        //        int ordine = 0;
        //        try
        //        {
        //            ordine = Convert.ToInt32(cmdOrdine.ExecuteScalar());
        //        }
        //        catch (Exception rr)
        //        {
        //            var a = rr;
        //        };


        //        parameter = new SqlParameter();
        //        parameter.ParameterName = "@ordine";
        //        parameter.Value = ordine;
        //        parameter.DbType = DbType.Int64;
        //        paramCollection.Add(parameter);

        //        cmdInsert.Transaction = this.oracleTx;
        //        int res = cmdInsert.ExecuteNonQuery();
        //        if (res != 1)
        //        {
        //            throw new Exception("Insert in errore: ");
        //        }

        //        return true;
        //    }

        //    catch (Exception exc)
        //    {
        //        Exception myExc = new Exception(exc.Message, exc);
        //        log.Debug(string.Format("Funzione:{0}", "UploadFile "));
        //        throw myExc;
        //    }
        //    finally
        //    {
        //        this.CloseConnection();
        //    }
        //}
        ///// <summary>
        ///// Lista delle news per il tablet
        ///// </summary>
        ///// <returns></returns>
        //public List<NewsFileItem> NewsFileList()
        //{
        //    List<NewsFileItem> result = new List<NewsFileItem>();
        //    try
        //    {
        //        this.OpenConnection();

        //        using (SqlCommand cmdSelect = new SqlCommand(CommandStrings.newsFileListBLT, this.sqlConnection))
        //        {
        //            using (SqlDataReader dr = cmdSelect.ExecuteReader())
        //            {
        //                if (dr.HasRows)
        //                {
        //                    while (dr.Read())
        //                    {
        //                        NewsFileItem item = new NewsFileItem();
        //                        item.guid = int.Parse(dr["id"].ToString());
        //                        item.titolo = dr["titolo"].ToString();
        //                        item.estensione = dr["estensione"].ToString();
        //                        item.order = int.Parse(dr["ordine"].ToString());

        //                        result.Add(item);
        //                    }
        //                }
        //            }
        //        }
        //        return result;
        //    }
        //    catch (Exception exc)
        //    {
        //        Exception myExc = new Exception(exc.Message, exc);
        //        log.Debug(string.Format("Funzione:{0}", "UploadFile "));
        //        throw myExc;
        //    }
        //    finally
        //    {
        //        this.CloseConnection();
        //    }
        //}

        ///// <summary>
        ///// Lista delle news per il backoffice: tutti i campi saranno elencati eccetto il flusso binario
        ///// </summary>
        ///// <returns></returns>
        //public List<NewsFile> NewsFileListBO()
        //{
        //    List<NewsFile> result = new List<NewsFile>();
        //    try
        //    {
        //        this.OpenConnection();

        //        using (SqlCommand cmdSelect = new SqlCommand(CommandStrings.newsFileList, this.sqlConnection))
        //        {
        //            using (SqlDataReader dr = cmdSelect.ExecuteReader())
        //            {
        //                if (dr.HasRows)
        //                {
        //                    while (dr.Read())
        //                    {
        //                        NewsFile item = new NewsFile();
        //                        item.guid = int.Parse(dr["id"].ToString());
        //                        item.titolo = dr["titolo"].ToString();
        //                        item.estensione = dr["estensione"].ToString();
        //                        item.order = int.Parse(dr["ordine"].ToString());
        //                        item.dataUpload = Convert.ToDateTime(dr["dataUpload"]);
        //                        item.dtFineValidita = Convert.ToDateTime(dr["dataFineValidita"]);
        //                        item.dtInizioValidita = Convert.ToDateTime(dr["dataInizioValidita"]);
        //                        item.userId = dr["userId"].ToString();

        //                        result.Add(item);
        //                    }
        //                }
        //            }
        //        }
        //        return result;
        //    }
        //    catch (Exception exc)
        //    {
        //        Exception myExc = new Exception(exc.Message, exc);
        //        log.Debug(string.Format("Funzione:{0}", "UploadFile "));
        //        throw myExc;
        //    }
        //    finally
        //    {
        //        this.CloseConnection();
        //    }
        //}

        //public string GetHashFromNewsTable()
        //{
        //    string result = "";
        //    try
        //    {
        //        this.OpenConnection();

        //        using (SqlCommand cmdSelect = new SqlCommand(CommandStrings.checksumNews, this.sqlConnection))
        //        {
        //            result = Convert.ToString(cmdSelect.ExecuteScalar());
        //        }
        //    }
        //    catch (Exception rr)
        //    {
        //        var a = rr;
        //    };
        //    return result;
        //}

        //public bool ChangeNewsOrder(Dictionary<string, string> feDictionary)
        //{
        //    try
        //    {
        //        this.OpenConnection();
        //        this.BeginTransaction(IsolationLevel.ReadCommitted);

        //        foreach (var entry in feDictionary)
        //        {
        //            using (SqlCommand cmdInsert1 = new SqlCommand(CommandStrings.updateOrderNews, this.sqlConnection))
        //            {

        //                SqlParameterCollection paramCollection = cmdInsert1.Parameters;

        //                SqlParameter parameter = new SqlParameter();
        //                parameter.ParameterName = "@ordine";
        //                parameter.Value = int.Parse(entry.Value);
        //                parameter.DbType = DbType.Int32;
        //                paramCollection.Add(parameter);

        //                parameter = new SqlParameter();
        //                parameter.ParameterName = "@id";
        //                parameter.Value = int.Parse(entry.Key);
        //                parameter.DbType = DbType.Int32;
        //                paramCollection.Add(parameter);


        //                cmdInsert1.Transaction = this.oracleTx;
        //                int res1 = cmdInsert1.ExecuteNonQuery();
        //                if (res1 < 0)
        //                {
        //                    throw new Exception("Cancellazione configurazione tablet in errore: ");
        //                }
        //            }
        //        }
        //        this.CommitTransaction();
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        RollBackTransaction();
        //        return false;
        //    }
        //    finally
        //    {
        //        CloseConnection();
        //    }
        //}

        //public bool UpsertNews(NewsFile request, bool mustBeDeleted)
        //{
        //    try
        //    {
        //        this.OpenConnection();
        //        //this.BeginTransaction(IsolationLevel.ReadCommitted);

        //        if (mustBeDeleted)
        //        {
        //            using (SqlCommand cmdInsert1 = new SqlCommand(CommandStrings.deleteNews, this.sqlConnection))
        //            //"Delete from [SessionManager].[dbo].[News] where id=" + request.guid, this.sqlConnection))
        //            {
        //                SqlParameterCollection paramCollection = cmdInsert1.Parameters;

        //                SqlParameter parameter = new SqlParameter();
        //                parameter.ParameterName = "@id";
        //                parameter.Value = request.guid;
        //                parameter.DbType = DbType.Int32;
        //                paramCollection.Add(parameter);

        //                cmdInsert1.Transaction = this.oracleTx;
        //                int res1 = cmdInsert1.ExecuteNonQuery();
        //                if (res1 < 0)
        //                {
        //                    throw new Exception("Delete news in errore: ");
        //                }
        //                return true;
        //            }
        //        }
        //        else
        //        {

        //            using (SqlCommand cmdInsert1 = new SqlCommand(CommandStrings.updateNews, this.sqlConnection))
        //            //"Update [SessionManager].[dbo].[News] set [titolo]='" + request.titolo + "', [dataFineValidita]=convert(datetime, '" + request.dtFineValidita.ToString("yyyy-MM-dd HH:mm:ss") + "',121) , [dataInizioValidita]=convert(datetime, '" + request.dtInizioValidita.ToString("yyyy-MM-dd HH:mm:ss") + "',121) where  [id]=" + request.guid, this.sqlConnection))
        //            {
        //                SqlParameterCollection paramCollection = cmdInsert1.Parameters;

        //                SqlParameter parameter = new SqlParameter();
        //                parameter.ParameterName = "@id";
        //                parameter.Value = request.guid;
        //                parameter.DbType = DbType.Int32;
        //                paramCollection.Add(parameter);

        //                parameter = new SqlParameter();
        //                parameter.ParameterName = "@dataInizioValidita";
        //                parameter.Value = request.dtInizioValidita;
        //                parameter.DbType = DbType.DateTime;
        //                paramCollection.Add(parameter);

        //                parameter = new SqlParameter();
        //                parameter.ParameterName = "@dataFineValidita";
        //                parameter.Value = request.dtFineValidita;
        //                parameter.DbType = DbType.DateTime;
        //                paramCollection.Add(parameter);

        //                parameter = new SqlParameter();
        //                parameter.ParameterName = "@titolo";
        //                parameter.Value = request.titolo;
        //                parameter.DbType = DbType.String;
        //                paramCollection.Add(parameter);

        //                cmdInsert1.Transaction = this.oracleTx;
        //                int res1 = cmdInsert1.ExecuteNonQuery();
        //                if (res1 < 0)
        //                {
        //                    throw new Exception("Update news in errore: ");
        //                }
        //                return true;
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        var a = ex;
        //        return false;
        //    }
        //    finally
        //    {
        //        CloseConnection();
        //    }
        //}

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            try
            {
                if (this.sqlConnection != null)
                {
                    if (this.sqlConnection.State == ConnectionState.Open)
                        sqlConnection.Close();
                    this.sqlConnection.Dispose();
                }
            }
            catch
            { }
            //GC.SuppressFinalize(this);
        }

        ~SqlDataAccess()
        {
            this.Dispose();
        }

        #endregion
    }
}
