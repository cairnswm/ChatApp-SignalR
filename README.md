# ChatApp-SignalR
Chat Application using C# and SignalR (Javascript client)

An attempt to create a Whatsapp chat clone for use in businesses.

Data is stored in SQL Server database.


(** means not yet implemented)
ChatHub methods
        Register(username, device)
        - If username already exists gets id 
        - Returns OK and registers used in Database
        - returns userid
		** Add password
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

Outstanding General
- Profiles (including Images)

Outstanding on ChatHub
- Group Chats
- Image loading
- Return all messages since last chat on connect
-- (Modify Register to return chats AND to return all messages)
- Remove Error Handling

Outstanding on Client
- Save Chats/Messages Locally (SQLLite?)
- Error Handling
- Reconnect when connection lost (Maybe display connection status)
- Change Header based on Tab (and/or active chat)
