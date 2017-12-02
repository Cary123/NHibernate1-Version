using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entry.model;
using DAL.util;
using NHibernate;

namespace DAL
{
    public class ProductDal
    {
        public Product GetProduct(Guid productId)
        {
            try
            {
                Product product = null;
                if (productId != Guid.Empty)
                {
                    ISession session = NHibernateHelper.GetSession();
                    product = (Product)session.Get(typeof(Product), productId);
                }
                return product;
            }
            catch (Exception)
            {

                throw;
            }
            finally
            {
                NHibernateHelper.CloseSession();
            }
        }
        public static void InsertProduct(Product product)
        {
            ISession session = null;
            ITransaction transaction = null;
            try
            {
                if (product != null)
                {
                    session = NHibernateHelper.GetSession();
                    transaction = session.BeginTransaction();
                    session.BeginTransaction();
                    session.Save(product);
                    session.Flush();
                    transaction.Commit();
                }
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;
            }
            finally
            {
                NHibernateHelper.CloseSession();
            }
        }

        public static void UpdateProduct(Product product)
        {
            ISession session = null;
            ITransaction transaction = null;
            try
            {
                if (product != null)
                {
                    session = NHibernateHelper.GetSession();
                    transaction = session.BeginTransaction();
                    session.BeginTransaction();
                    session.Update(product);
                    session.Flush();
                    transaction.Commit();
                }
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;
            }
            finally
            {
                NHibernateHelper.CloseSession();
            }
        }

        public static void DeleteProduct(Product product)
        {
            ISession session = null;
            ITransaction transaction = null;
            try
            {
                if (product != null)
                {
                    session = NHibernateHelper.GetSession();
                    transaction = session.BeginTransaction();
                    session.BeginTransaction();
                    session.Delete(product);
                    session.Flush();
                    transaction.Commit();
                }
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;
            }
            finally
            {
                NHibernateHelper.CloseSession();
            }
        }
    }
}
