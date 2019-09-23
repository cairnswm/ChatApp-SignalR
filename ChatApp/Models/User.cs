using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace ChatApp.Models
{
    public class User
    {
        [Key]
        public int ID { get; set; }
        public string UserName { get; set; } // Indexed in ApplicationDbContext
        public string FirstName { get; set; }
        public string Surname { get; set; }
        public string Status { get; set; }
        public string ContactNumber { get; set; }
        public string EmailAddress { get; set; }
        public string ProfilePic { get; set; } // Load image to disk and store link
    }
    public class UserSession // Users currently active (i.e. in application)
    {
        [Key]
        public int ID { get; set; }
        public int UserID { get; set; } // Indexed in ApplicationDbContext
        public string UserName { get; set; } // Indexed in ApplicationDbContext
        public string DisplayName { get; set; }
        public string DeviceID { get; set; }
        public string SessionID { get; set; }
    }

    public class UserDevice
    {
        [Key]
        public int ID { get; set; }
        public int UserID { get; set; } // Indexed in ApplicationDbContext
        public string UserName { get; set; } // Indexed in ApplicationDbContext
        public string DeviceID { get; set; } // Indexed in ApplicationDbContext
        public string DeviceName { get; set; }
        public DateTime LastSeen { get; set; }
        // TODO: Last Seen, Active etc
    }
}
