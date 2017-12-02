using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL;

namespace NHibernate1Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            UserDal.GetUserByName("Joseph");
            Console.ReadLine();
        }
    }
}
