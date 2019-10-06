using ChatApp.Data;
using ChatApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatApp.DataAccess
{
    public interface IChatDAL
    {
        List<UserChats> GetChats(int UserID);
        int StartChat(int user1, int user2);
        int SaveMessage(int ChatID, string userName, string Text);
    }
    public class ChatDAL : IChatDAL
    {
        ApplicationDbContext context;

        public ChatDAL(ApplicationDbContext DBContext)
        {
            context = DBContext;
        }

        public List<UserChats> GetChats(int UserID)
        {
            if (UserID > 0)
            {
                List<UserChats> data = context.ChatUsers
                   .Join(context.UserPerChat.Where(x => x.UserID == UserID), c => c.ID, c2 => c2.UserID, (c, c2) => new { c, c2 })
                   .Join(context.UserPerChat.Where(x => x.UserID != UserID), c3 => c3.c2.ChatID, c4 => c4.ChatID, (c3, c4) => new { c3, c4 })
                   .Join(context.ChatUsers, c5 => c5.c4.UserID, c6 => c6.ID, (c5, c6) => new { c5, c6 })
                   .Select(z => new UserChats() { UserID = z.c6.ID, UserName = z.c6.UserName, ChatID = z.c5.c4.ChatID }).ToList();                
                return data;
            }
            else
            {
                return null;
            }
        }

        public int SaveMessage(int ChatID, string userName, string Text)
        {
            Message msg = new Message();
            msg.ChatID = ChatID;
            msg.FromUserName = userName;
            msg.Text = Text;
            context.Messages.Add(msg);
            context.SaveChanges();
            return msg.ID;
        }

        public int StartChat(int user1, int user2)
        {
            // Check if chat with this user Exists - return chat ID
            var data = context.UserPerChat
                .Where(u => u.UserID == user1)
                .Join(context.UserPerChat
                              .Where(u3 => u3.UserID == user2), c => c.ChatID, c2 => c2.UserID, (c, c2) => new { Chat = c, User = c2 })
                .Select(c => c.Chat);
            if (data.Count() > 0)
            {
                return data.First().ChatID;
            }

            // Else create new chat and return ID
            Chat chat = new Chat() { Name = "Chat" };
            context.Chat.Add(chat);
            context.SaveChanges(); // Need to save to get the ChatID for FK relationships
            UserPerChat u1 = new UserPerChat() { ChatID = chat.ID, UserID = user1 };
            context.UserPerChat.Add(u1);
            UserPerChat u2 = new UserPerChat() { ChatID = chat.ID, UserID = user2 };
            context.UserPerChat.Add(u2);
            context.SaveChanges();
            return chat.ID;
        }

    }
}
