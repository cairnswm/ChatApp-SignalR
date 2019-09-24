using ChatApp.DataAccess;
using Microsoft.AspNetCore.SignalR;
using ChatApp.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ChatApp.SignalRHub
{
    public class ChatHub : Hub
    {
        /*         
        Register(username, password)
        - If username already exists gets id 
        - Returns OK and registers used in Database
        - returns userid
        Connect(username, chat)
        - Returns Chat list, messages for current chat from database (Register, both empty) 
        ** Then per chat sends messages from database
        StartChat(user1,user2,chatid)
        - Create new chat saved into database if no chat between these users
        - Returns Chat id 
        Send(chatid,message)
        - All members of chat loggedin Returns Chatid, username, message
        - All members of chat NOT loggedin saves Chatid, username, message into database
        GetActive()
        - Returns all currently connected users
        Users(Search)
        - Returns all users containing the search string
        Chats(userid)
        - Returns all chats user is currently connected to
        **NewGroupChat([usernames]) - TODO Implement NewGroupChat
        - Create new chat saved into database
        - Returns Chat id (saved to localstorage)
        */



        readonly IUserDAL _User;
        readonly IChatDAL _Chat;
        readonly ApplicationDbContext _context;
        private Dictionary<string, string> _Users = new Dictionary<string, string>();
        private Dictionary<string, string> _Contexts = new Dictionary<string, string>();

        public ChatHub(ApplicationDbContext Context, IUserDAL userDAL, IChatDAL chatDAL)
        {
            _context = Context;
            _User = userDAL;
            _Chat = chatDAL;
        }

        public async Task Send(string nick, int chatid, string message)
        {
            // TODO: verify user is part of chat
            // Save Message to DB
            _Chat.SaveMessage(chatid, nick, message);
            // Send to everyone in Chat Group
            await Clients.Group("Chat-"+chatid).SendAsync("Send", nick, chatid, message);
            _User.UpdateLastActive();

        }

        // Registers a user to use the system. 
        public async Task Register(string nick, string deviceid, string devicename)
        {
            var DTO = await _User.UserRegistration(nick, deviceid, devicename);

            var serializerSettings = new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.Objects };
            var json = JsonConvert.SerializeObject(DTO, Formatting.Indented, serializerSettings);

            await Clients.Caller.SendAsync("Registered", json);
            _Users.Add("User-" + DTO.UserID, Context.ConnectionId);
            _Contexts.Add(Context.ConnectionId,"User-" + DTO.UserID);

            await Chats(DTO.UserID);
        }

        // Search User List
        public async Task GetActive()
        {
            var DTO = _Users;

            var serializerSettings = new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.Objects };
            var json = JsonConvert.SerializeObject(DTO, Formatting.Indented, serializerSettings);

            await Clients.Caller.SendAsync("ActiveUsers", json);
        }

        // Search User List
        public async Task Users(string search)
        {
            var DTO = _User.GetChatUsers(search);

            var serializerSettings = new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.Objects };
            var json = JsonConvert.SerializeObject(DTO, Formatting.Indented, serializerSettings);

            await Clients.Caller.SendAsync("Users", json);
        }

        // Start a chat with a user - Return valid ChatID
        public async Task StartChat(int userid, int userid2, int chatid)
        {
            int DTO;
            if (chatid == 0) // Starting a new chat
            {
                DTO = _Chat.StartChat(userid, userid2);
                // Add users to Group
                // TODO Verify users are connected before adding to group
                await Groups.AddToGroupAsync(_Users["User-" + userid], "Chat-" + Convert.ToInt32(DTO));
                await Groups.AddToGroupAsync(_Users["User-" + userid2], "Chat-" + Convert.ToInt32(DTO));
            }
            else // Chat exists
            {
                DTO = chatid;
            }
            var serializerSettings = new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.Objects };
            var json = JsonConvert.SerializeObject(DTO, Formatting.Indented, serializerSettings);

            await Clients.Caller.SendAsync("Chat", json);
        }

        // Get List of Chats
        public async Task Chats(int userid)
        {
            var DTO = _Chat.GetChats(userid);
            foreach(var chat in DTO) // Add user to Group for each Chat - ensure immediate message sending
            {
                await Groups.AddToGroupAsync(_Users["User-" + userid], "Chat-" + Convert.ToInt32(chat.ChatID));
            }

            var serializerSettings = new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.Objects };
            var json = JsonConvert.SerializeObject(DTO, Formatting.Indented, serializerSettings);

            await Clients.Caller.SendAsync("Chats", json);
        }


        #region HubMethods
        private Task SendMessage(string user, string message)
        {
            return Clients.All.SendAsync("ReceiveMessage", user, message);
        }

        private Task SendMessageToCaller(string message, string messageType = "ReceiveMessage")
        {
            return Clients.Caller.SendAsync(messageType, message);
        }

        private Task SendMessageToGroup(string message)
        {
            return Clients.Group("SignalR Users").SendAsync("ReceiveMessage", message);
        }
        #endregion

        #region HubMethodName
        [HubMethodName("SendMessageToUser")]
        private Task DirectMessage(string user, string message)
        {
            return Clients.User(user).SendAsync("ReceiveMessage", message);
        }
        #endregion

        #region ThrowHubException
        public Task ThrowException()
        {
            throw new HubException("This error will be sent to the client!");
        }
        #endregion

        #region OnConnectedAsync
        public override async Task OnConnectedAsync()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "SignalR Users");
            await base.OnConnectedAsync();
            await SendMessageToCaller("Connected","Connected");
        }
        #endregion

        #region OnDisconnectedAsync
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "SignalR Users");
            string userstr = _Contexts[Context.ConnectionId];
            int userid = Convert.ToInt32(userstr);

            // Remove User from Group for each Chat 
            var DTO = _Chat.GetChats(userid);
            foreach (var chat in DTO) 
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Chat-" + Convert.ToInt32(chat.ChatID));
            }
            // Clear user from Active User Maps
            _Users.Remove(userstr);
            _Contexts.Remove(Context.ConnectionId);
            // Done: Remove from Sessions
            _User.RemoveActive(userid);
            await base.OnDisconnectedAsync(exception);
        }
        #endregion
    }
}

