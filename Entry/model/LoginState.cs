using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entry.model
{
    public enum LoginState : byte
    {
        Offline = 0x00,
        Online = 0x01,
        Disconn = 0x02
    }
}
