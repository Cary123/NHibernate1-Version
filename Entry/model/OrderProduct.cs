using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entry.model
{
    public class OrderProduct
    {
        public virtual Order Order { get; set; }
        public virtual Product Product { get; set; }
        public virtual int ProductNum { get; set; }
        public virtual string Detail { get; set; }
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
