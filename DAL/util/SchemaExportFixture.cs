using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHibernate;
using NHibernate.Cfg;

namespace DAL.util
{
    /// <summary>
    /// Export tool
    /// </summary>
    public class SchemaExportFixture
    {
        private Configuration cfg;
        public SchemaExportFixture()
        {
            cfg = new Configuration();
            cfg.Configure();
        }
        public void DropTest()
        {
            var export = new NHibernate.Tool.hbm2ddl.SchemaExport(cfg);
            export.SetOutputFile(System.Environment.CurrentDirectory + "\\sql\\DropSql.sql");
            export.Drop(true, true);
        }
        public void CreateTest()
        {
            var export = new NHibernate.Tool.hbm2ddl.SchemaExport(cfg);
            export.SetOutputFile(System.Environment.CurrentDirectory + "\\sql\\CreateSql.sql");
            export.Create(true, true);            
        }
        public void ExecuteTest()
        {
            var export = new NHibernate.Tool.hbm2ddl.SchemaExport(cfg);
            export.SetOutputFile(System.Environment.CurrentDirectory + "\\sql\\ExecuteSql.sql"); 
           // export.Execute(true, true, false);
        }
    }
}
