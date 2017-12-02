using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entry.model
{
    public class Order
    {
        public virtual Guid OrderId { get; set; }
        public virtual User User { get; set; }
        public virtual DateTime OrderDate { get; set; }
        public virtual ISet<OrderProduct> OrderProducts { get; set; }
    }
}
