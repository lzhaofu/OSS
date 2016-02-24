using System;
using System.Collections.Generic;

namespace OSS.Data.Models
{
    public partial class T_User
    {
        public int ID { get; set; }
        public string LoginAccount { get; set; }
        public string LoginPassword { get; set; }
        public Nullable<int> IsEnabled { get; set; }
    }
}
