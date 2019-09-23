using ChatApp.Models;
using Microsoft.Extensions.DependencyInjection;
using ChatApp.Data;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace ChatApp.DataAccess
{
    public interface IUserDAL
    {
        Task<ValidRegistration> UserRegistration(string Username, string DeviceID, string Device);
        List<User> GetChatUsers(string name);
        bool UpdateLastActive();
        bool RemoveActive(int UserID);
    }
    public class UserDAL : IUserDAL
    {
        ApplicationDbContext _context;

        public UserDAL(ApplicationDbContext Context)
        {
            _context = Context;
        }

        public List<User> GetChatUsers(string name)
        {
            if (name == null) { name = ""; }
            if (name.Length > 0)
            {
                return _context.ChatUsers.Where(a => a.UserName.Contains(name) || a.FirstName.Contains(name) || a.Surname.Contains(name)).ToList();
            }
            else
            {
                return _context.ChatUsers.ToList();
            }
        }

        public bool UpdateLastActive()
        {
            // Done: Update All Users with Record in Session to Now
            var ActiveUsers = _context.UserSessions.Join(_context.UserDevice, US => new { US.DeviceID, US.UserID }, U => new { U.DeviceID, U.UserID }, (US, U) => new { UserDevice = U, UserSession = US }).Select(q => q.UserDevice).ToList();
            foreach (UserDevice US in ActiveUsers)
            {
                US.LastSeen = DateTime.Now;
            }
            _context.SaveChanges();
            return true;
        }

        public bool RemoveActive(int UserID)
        {
            List<UserSession> items = _context.UserSessions.Where(U => U.UserID == UserID).ToList();
            foreach(UserSession item in items)
            {
                _context.UserSessions.Remove(item);

            }
            _context.SaveChanges();
            return true;
        }

        public async Task<ValidRegistration> UserRegistration(string Username, string DeviceID, string Device)
        {
            ValidRegistration reg = new ValidRegistration();

            // Check if user Exists // If Not Create
            User chatuser = _context.ChatUsers.FirstOrDefault(e => e.UserName == Username);
            if (chatuser == null)            
            {
                chatuser = new User();
                chatuser.UserName = Username;
                _context.ChatUsers.Add(chatuser);                
            }

            // Check if Device/User is registered, else create
            UserDevice UD = _context.UserDevice.FirstOrDefault(e => e.UserName == Username && e.DeviceID == DeviceID);
            if (UD == null)
            {
                // Check if Device was used previously, register to new user
                UD = _context.UserDevice.FirstOrDefault(e => e.DeviceID == DeviceID);
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
                _context.UserDevice.Add(UD2);
                await _context.SaveChangesAsync();
                UD = UD2;
            }

            // Create User Session
            UserSession US = new UserSession();
            US.UserName = Username;
            US.DeviceID = UD.DeviceID;
            US.SessionID = Guid.NewGuid().ToString();
            _context.UserSessions.Add(US);
            await _context.SaveChangesAsync();

            // Return Details
            reg.UserName = Username;
            reg.DeviceID = UD.DeviceID;
            reg.Token = US.SessionID;
            reg.UserID = chatuser.ID;
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
