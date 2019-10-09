using ChatApp.Models;
using Microsoft.Extensions.DependencyInjection;
using ChatApp.Data;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.DataAccess
{
    public interface IUserDAL
    {
        Task<ValidRegistration> UserRegistration(string Username, string DeviceID, string Device);
        Task<List<User>> GetChatUsers(string name);
        Task<bool> UpdateLastActive();
        Task<bool> RemoveActive(int UserID);
    }
    public class UserDAL : IUserDAL
    {
        ApplicationDbContext context;

        public UserDAL(ApplicationDbContext DBContext)
        {
            context = DBContext;
        }

        public async Task<List<User>> GetChatUsers(string name)
        {
            // If blank name sent return all users else search
            if (name == null) { name = ""; }
            if (name.Length > 0)
            {
                return await context.ChatUsers.Where(a => a.UserName.Contains(name) || a.FirstName.Contains(name) || a.Surname.Contains(name)).ToListAsync();
            }
            else
            {
                return await context.ChatUsers.ToListAsync();
            }
        }

        public async Task<bool> UpdateLastActive()
        {
            // Done: Update All Users with Record in Session to Now
            var ActiveUsers = await context.UserSessions.Join(context.UserDevice, US => new { US.DeviceID, US.UserID }, U => new { U.DeviceID, U.UserID }, (US, U) => new { UserDevice = U, UserSession = US }).Select(q => q.UserDevice).ToListAsync();
            foreach (UserDevice US in ActiveUsers)
            {
                US.LastSeen = DateTime.Now;
                context.Entry(US).State = EntityState.Modified;
            }
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveActive(int UserID)
        {
            List<UserSession> items = await context.UserSessions.Where(U => U.UserID == UserID).ToListAsync();
            foreach(UserSession item in items)
            {
                context.UserSessions.Remove(item);
            }
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<ValidRegistration> UserRegistration(string Username, string DeviceID, string Device)
        {
            ValidRegistration reg = new ValidRegistration();

            // Check if user Exists // If Not Create
            User chatuser = context.ChatUsers.FirstOrDefault(e => e.UserName == Username);
            if (chatuser == null)            
            {
                chatuser = new User();
                chatuser.UserName = Username;
                context.ChatUsers.Add(chatuser);                
            }

            // Check if Device/User is registered, else create
            UserDevice UD = context.UserDevice.FirstOrDefault(e => e.UserName == Username && e.DeviceID == DeviceID);
            if (UD == null)
            {
                // Check if Device was used previously, register to new user
                UD = context.UserDevice.FirstOrDefault(e => e.DeviceID == DeviceID);
                UserDevice UD2 = new UserDevice();
                UD2.UserName = Username;
                if (UD == null)
                {
                    UD2.DeviceID = Guid.NewGuid().ToString();
                }
                else
                {
                    UD2.DeviceID = DeviceID;
                }
                UD2.DeviceName = UD2.DeviceID;
                context.UserDevice.Add(UD2);
                await context.SaveChangesAsync();
                UD = UD2;
            }

            // Create User Session
            UserSession US = new UserSession();
            US.UserName = Username;
            US.DeviceID = UD.DeviceID;
            US.SessionID = Guid.NewGuid().ToString();
            context.UserSessions.Add(US);
            await context.SaveChangesAsync();

            // Return Details
            reg.UserName = Username;
            reg.DeviceID = UD.DeviceID;
            reg.Token = US.SessionID;
            reg.UserID = chatuser.ID;
            reg.LastSeen = UD.LastSeen;
            return reg;
        }
    }

    public static class DALExtensions
    {
        public static void AddUserDAL(this IServiceCollection services)
        {
            services.AddSingleton(typeof(IUserDAL), typeof(IUserDAL));
        }
    }
}
