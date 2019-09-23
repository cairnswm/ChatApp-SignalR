using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace ChatApp.Models
{
    public class Chat
    {
        [Key]
        public int ID { get; set; }
        public string Name { get; set; }
        public string ProfilePic { get; set; } // Load image to disk and store link
    }

    public class UserPerChat
    {
        [Key]
        public int ID { get; set; }
        [Required]
        public int ChatID { get; set; } // Indexed in ApplicationDbContext
        [Required]
        public int UserID { get; set; }  // Indexed in ApplicationDbContext
        public string Role { get; set; } // Admin/User/Owner // Owners are also Admins
        public UserPerChat()
        {
            Role = "User";
        }
    }

    public class Message
    {
        [Key]
        public int ID { get; set; }
        public int ChatID { get; set; } // Composite Index {userid, sent} in ApplicationDbContext
        public string FromUserName { get; set; }
        public string Text { get; set; }
        public DateTime Sent { get; set; } // Composite Index {userid, sent} in ApplicationDbContext
        public Message()
        {
            Sent = DateTime.Now;
        }

    }

}
