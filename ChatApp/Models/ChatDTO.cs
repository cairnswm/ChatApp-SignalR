using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatApp.Models
{ 
    [Serializable]
    public class ValidRegistration
    {
        public string Status { get; set; }
        public string UserName { get; set; }
        public int UserID { get; set; }
        public string DeviceID { get; set; }
        public string Token { get; set; }
        public DateTime LastSeen { get; set; }
        public ValidRegistration()
        {
            Status = "New";
        }
    }

    [Serializable]
    public class UserChats
    {
        public int UserID { get; set; }
        public string UserName { get; set; }
        public int ChatID { get; set; }
    }
}
