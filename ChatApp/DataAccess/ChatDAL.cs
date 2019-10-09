using ChatApp.Data;
using ChatApp.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatApp.DataAccess
{
    public interface IChatDAL
    {
        Task<List<UserChats>> GetChats(int UserID);
        Task<int> StartChat(int user1, int user2);
        Task<int> SaveMessage(int ChatID, string userName, string Text);
        Task<List<Message>> GetChatMessagesSince(int ChatId, DateTime LastSeen);
    }
    public class ChatDAL : IChatDAL
    {
        ApplicationDbContext context;

        public ChatDAL(ApplicationDbContext DBContext)
        {
            context = DBContext;
        }

        public async Task<List<UserChats>> GetChats(int UserID)
        {
            if (UserID > 0)
            {
                List<UserChats> data = await context.ChatUsers
                   .Join(context.UserPerChat.Where(x => x.UserID == UserID), c => c.ID, c2 => c2.UserID, (c, c2) => new { c, c2 })
                   .Join(context.UserPerChat.Where(x => x.UserID != UserID), c3 => c3.c2.ChatID, c4 => c4.ChatID, (c3, c4) => new { c3, c4 })
                   .Join(context.ChatUsers, c5 => c5.c4.UserID, c6 => c6.ID, (c5, c6) => new { c5, c6 })
                   .Select(z => new UserChats() { UserID = z.c6.ID, UserName = z.c6.UserName, ChatID = z.c5.c4.ChatID }).ToListAsync();                
                return data;
            }
            else
            {
                return null;
            }
        }

        public async Task<List<Message>> GetChatMessagesSince(int ChatId, DateTime LastSeen)
        {
            List<Message> messages = new List<Message>();
            messages = await context.Messages.Where(x => x.ChatID == ChatId && x.Sent > LastSeen).OrderBy(x => x.Sent).ToListAsync();
            return messages;
        }

        public async Task<int> SaveMessage(int ChatID, string userName, string Text)
        {
            Message msg = new Message();
            msg.ChatID = ChatID;
            msg.FromUserName = userName;
            msg.Text = Text;
            await context.Messages.AddAsync(msg);
            await context.SaveChangesAsync();
            return msg.ID;
        }

        public async Task<int> StartChat(int user1, int user2)
        {
            // Check if chat with this user Exists - return chat ID
            var data = context.UserPerChat
                .Where(u => u.UserID == user1)
                .Join(context.UserPerChat
                              .Where(u3 => u3.UserID == user2), c => c.ChatID, c2 => c2.UserID, (c, c2) => new { Chat = c, User = c2 })
                .Select(c => c.Chat);
            if (data.Count() > 0)
            {
                UserPerChat userchat = await data.FirstAsync();
                return userchat.ChatID;
            }

            // Else create new chat and return ID
            Chat chat = new Chat() { Name = "Chat" };
            await context.Chat.AddAsync(chat);
            await context.SaveChangesAsync(); // Need to save to get the ChatID for FK relationships
            UserPerChat u1 = new UserPerChat() { ChatID = chat.ID, UserID = user1 };
            await context.UserPerChat.AddAsync(u1);
            UserPerChat u2 = new UserPerChat() { ChatID = chat.ID, UserID = user2 };
            await context.UserPerChat.AddAsync(u2);
            await context.SaveChangesAsync();
            return chat.ID;
        }

    }
}
