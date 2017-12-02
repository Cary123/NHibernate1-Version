using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHibernate;
using DAL.util;
using System.Data;
using System.Data.SqlClient;
using System.Threading;

namespace DAL
{
    public class EnterpriseDal
    {
        public static void DeleteOrder(Guid orderId)
        {
            SqlTransaction tran = null;
            SqlConnection conn = null;
            SqlCommand cmd = null;
            try
            {
                conn =NHibernateHelper.GetSqlConnection();               
                if(conn != null)
                {
                    conn.Open();
                    Thread.Sleep(5 * 1000);
                    tran = conn.BeginTransaction();
                    cmd = conn.CreateCommand();
                    cmd.Transaction = tran;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "ProcDeleteOrder";
                    cmd.Parameters.Add(new SqlParameter("@OrderId",orderId.ToString()));
                    cmd.Parameters.Add(new SqlParameter("@LastUpdateTime", DateTime.Now));
                    cmd.ExecuteNonQuery();
                    tran.Commit();
                }
            }
            catch (Exception)
            {
                tran.Rollback();
                throw;
            }
            finally
            {
                NHibernateHelper.CloseSession();
            }
        }
    }
}
