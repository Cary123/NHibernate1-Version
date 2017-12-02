using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entry.model
{
    public class User
    {
        private string username;
        private LoginState state;

        public virtual Guid UserId { get; set; }

        public virtual string Username
        {
            get
            {
                return username;
            }
            set
            {
                username = value;
            }
        }      

        public virtual string Password { get; set; }

        public virtual char Gender { get; set; }

        public virtual int Age { get; set; }

        public virtual string Phone { get; set; }

        public virtual DateTime LastUpdateTime { get; set; }

        public virtual string Email { get; set; }

        public virtual LoginState State
        {
            get
            {
                return state;
            }
            set
            {
                state = value;
            }
        }

    }
}
