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
        ** TODO: Then per chat sends messages from database
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
        ** NewGroupChat([usernames]) - TODO Implement NewGroupChat
        - Create new chat saved into database
        - Returns Chat id (saved to localstorage)
        */

        /*
        * Using the SignalR goups per chat to send messages to the chats
        * Uses in-memory Lists to store active users (TODO:Future: Should this be in cache instead?)
        * 
        * 
        */



        readonly IUserDAL userData;
        readonly IChatDAL chatData;
        private Dictionary<string, string> usersList = new Dictionary<string, string>(); // Store sessions indexed by Username [UserName, SignalR context]
        private Dictionary<string, string> usersContext = new Dictionary<string, string>(); // Store sessions indexed by Context [ Context, Username]

        public ChatHub(IUserDAL userDAL, IChatDAL chatDAL)
        {
            userData = userDAL;
            chatData = chatDAL;
        }

        public async Task Send(string nick, int chatid, string message)
        {
            // TODO: verify user is part of chat
            // Save Message to DB
            chatData.SaveMessage(chatid, nick, message);
            // Send to everyone in Chat Group
            await Clients.Group("Chat-"+chatid).SendAsync("Send", nick, chatid, message);
            userData.UpdateLastActive();

        }

        // Registers a user to use the system. 
        public async Task Register(string nick, string deviceid, string devicename)
        {
            // TODO: If new Device form exisitng user send messages from last X time
            var DTO = await userData.UserRegistration(nick, deviceid, devicename);

            var serializerSettings = new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.Objects };
            var json = JsonConvert.SerializeObject(DTO, Formatting.Indented, serializerSettings);

            await Clients.Caller.SendAsync("Registered", json);
            usersList.Add("User-" + DTO.UserID, Context.ConnectionId);
            usersContext.Add(Context.ConnectionId,"User-" + DTO.UserID);

            await Chats(DTO.UserID);
        }

        // Search User List
        public async Task GetActive()
        {
            var DTO = usersList;

            var serializerSettings = new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.Objects };
            var json = JsonConvert.SerializeObject(DTO, Formatting.Indented, serializerSettings);

            await Clients.Caller.SendAsync("ActiveUsers", json);
        }

        // Search User List
        public async Task Users(string search)
        {
            var DTO = userData.GetChatUsers(search);

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
                DTO = chatData.StartChat(userid, userid2);
                // Add users to SignalR Group
                // Done Verify users are connected before adding to group
                if (!usersList.ContainsKey("User-" + userid))
                {
                    await Groups.AddToGroupAsync(usersList["User-" + userid], "Chat-" + Convert.ToInt32(DTO));
                }
                if (!usersList.ContainsKey("User-" + userid2))
                {
                    await Groups.AddToGroupAsync(usersList["User-" + userid2], "Chat-" + Convert.ToInt32(DTO));
                }
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
        // Called internally when users connect
        private async Task Chats(int userid)
        {
            var DTO = chatData.GetChats(userid);
            foreach(var chat in DTO) // Add user to Group for each Chat - ensure immediate message sending
            {
                await Groups.AddToGroupAsync(usersList["User-" + userid], "Chat-" + Convert.ToInt32(chat.ChatID));
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
            await Groups.AddToGroupAsync(Context.ConnectionId, "SignalR Users"); // List of all connected users - for real time broadcasts (TODO Implement RealTime broadcasts)
            await base.OnConnectedAsync();
            await SendMessageToCaller("Connected","Connected");
        }
        #endregion

        #region OnDisconnectedAsync
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "SignalR Users");
            string userstr = usersContext[Context.ConnectionId];
            int userid = Convert.ToInt32(userstr);

            // Remove User from Group for each Chat 
            var DTO = chatData.GetChats(userid);
            foreach (var chat in DTO) 
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Chat-" + Convert.ToInt32(chat.ChatID));
            }
            // Clear user from Active User Maps
            usersList.Remove(userstr);
            usersContext.Remove(Context.ConnectionId);
            // Done: Remove from Sessions
            userData.RemoveActive(userid);
            await base.OnDisconnectedAsync(exception);
        }
        #endregion
    }
}

