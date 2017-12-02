using DAL.util;
using Entry.model;
using NHibernate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DAL
{
    public class OrderDal
    {
        public Order GetOrder(Guid orderId)
        {
            ISession session = null;
            try
            {
                Order order = null;
                if (orderId != Guid.Empty)
                {
                    session = NHibernateHelper.GetSession();
                    order = (Order)session.Get(typeof(Order), orderId);
                }
                return order;
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

        public static void InsertOrder(Order order)
        {
            ISession session = null;
            ITransaction transaction = null;
            try
            {
                if (order != null)
                {
                    session = NHibernateHelper.GetSession();
                    transaction = session.BeginTransaction();
                    session.BeginTransaction();
                    session.Save(order);
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

        public static void UpdateOrder(Order order)
        {
            ISession session = null;
            ITransaction transaction = null;
            try
            {
                if (order != null)
                {
                    session = NHibernateHelper.GetSession();
                    transaction = session.BeginTransaction();
                    session.BeginTransaction();
                    session.Update(order);
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

        public static void DeleteOrder(Order order)
        {
            ISession session = null;
            ITransaction transaction = null;
            try
            {
                if (order != null)
                {                    
                    session = NHibernateHelper.GetSession();
                    order = (Order)session.Get(typeof(Order), order.OrderId);
                    User user = order.User;
                    Thread.Sleep(10 * 1000);
                    transaction = session.BeginTransaction();
                    session.BeginTransaction();                
                    user.LastUpdateTime = DateTime.Now;
                    session.Delete(order);                                   
                    session.Update(user);
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
