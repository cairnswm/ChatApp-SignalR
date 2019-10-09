var name = ""; // Currently active username
var userid = 0; // Logged in user
var chatid = 0; // Active Chatid

// TODO: Should this be in the onready handler - 
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/chat")
    .build();
connection.start().catch(err => console.error(err.toString()));

$(document).ready(function () {
    //TODO: Startup functions
});

function register(name, deviceid) {
    if (name == null || name == "null" || name === undefined) {
        $(".content").hide();
        $('#entrance').show();
        return;
    }
    // Register is also used for clients with active sessiosn (stored in indexexdb)
    connection.invoke('Register', name, deviceid, deviceid);
}

/*
 *         UI 
 */

$("#btnHome").on("click", function () {
    $(".content").hide();
    $("#Home").show();
});

$("#btnUsers").on("click", function () {
    $(".content").hide();
    $("#Users").show();
});

$("#btnChats").on("click", function () {
    $(".content").hide();
    $('#Chats').show();
});

$("#btnChat").on("click", function () {
    $(".content").hide();
    $('#Chat').show();
});

$("#btnProfile").on("click", function () {
    $(".content").hide();
    $("#Profile").show();
});
$("#btnSettings").on("click", function () {
    $(".content").hide();
    $('#Settings').show();

});

$("#btnLogout").on("click", function () {
    $(".content").hide();
    $('#entrance').show();
});

$("#btnUserSearch").on("click", function () {
    connection.invoke('Users', $("#UserSearch").val());
});

$("#UserList").on("click", ".names", function (event) {
    id = $(event.target).data("userid");
    console.log("Click on user " + id);
    // Done: Start chat with user
    connection.invoke("StartChat", userid, id, 0);
});

$("#ChatList").on("click", ".chats", function (event) {
    id = $(event.target).data("chatid");
    console.log("Click on chat " + id);
    // TODO: Get Messages for Chat
    connection.invoke("StartChat", userid, 0, id);
});


/*
 *         SignalR actions when recieving messages 
 */
// Display chat message recieved from server
connection.on('Send', (nick, forchatid, message) => {
    if (chatid == forchatid) { // chatid is the active chat
        appendLine(nick, message);
    }
    // TODO: Store Message in storage for when user opens chat, increment unread messages
});

// Display bulk chat messages recieved from server
connection.on('MessageList', (forchatid, messages) => {
    messages.forEach(function (message) {
        if (chatid == forchatid) { // chatid is the active chat
            appendLine(message.FromUserName, message.Text);
        }
    }
    // TODO: Store Message in storage for when user opens chat, increment unread messages
});


// Open chat
connection.on('Chat', (jsonstr) => {
    chatid = JSON.parse(jsonstr);
    $(".content").hide();
    $("#Chat").show();
    appendLine("System", "Connected to chat: " + chatid);
});

// Display list of Chats
// TODO: Change profile images
connection.on('Chats', (list) => {
    $("#ChatList").text("");
    Chats = JSON.parse(list);
    Chats.forEach(function (Chat) {
        $("#ChatList").append("<div><img src='http://mrg.bz/XduV5Q'><span class='chats' data-chatid='" + Chat.ChatID + "'>" + Chat.UserName + "</span></div> ");
    });
    if (chatid == 0) {
        if (Chats.length > 0) {
            chatid = Chats[0].ChatID;
        }
    }
});

// Display list of Users
connection.on('Users', (list) => {
    $("#UserList").text("");
    Users = JSON.parse(list);
    Users.forEach(function (User) {
        $("#UserList").append("<div><img src='http://mrg.bz/XduV5Q'><span class='names' data-userid='" + User.ID + "'>" + User.UserName + "</span></div> ");
    })
});

/*========== DEBUG =================*/
// Debug to see active users - should not be displayed to clients
// TODO Remove
connection.on('ActiveUsers', (list) => {
    $("#UserList").text("");
    Users = JSON.parse(list);
    Users.forEach(function (User) {
        $("#SettingsList").append("<div><img src='http://mrg.bz/XduV5Q'><span class='names' data-userid='" + User.ID + "'>" + User.UserName + "</span></div> ");
    })
});
/*========== END DEBUG ============= */

// Startup, either after registration of after app start for already registered
connection.on('Registered', (jsonstr) => {
    $(".content").hide();
    $('#Chat').show();
    json = JSON.parse(jsonstr);
    name = json.UserName;
    userid = json.UserID;
    $("#username").text(name);
    appendLine(name, "Registered as " + name + " (" + userid + ")");
    appendLine(name, "Server Debug: " + " U: " + name + " D:" + json.DeviceID + " S:" + json.Token);
    localStorage.setItem("DeviceID", json.DeviceID);
    localStorage.setItem("Token", json.Token);
    localStorage.setItem("Username", json.UserName);
});

// When connected to server, if pre registered send registration link
connection.on('Connected', (jsonstr) => {
    name = localStorage.getItem("Username"); // Get username from localstorage (preregistered)
    $(".content").hide();
    $("#Home").show();
    if (name !== undefined) {
        $('#nick').val(name);
        RegisterUser();
    }
});

// Send message to server
// TODO: Convert to jQuery
document.getElementById('frm-send-message').addEventListener('submit', event => {
    let message = $('#message').val();
    let nick = name;
    $('#message').val('');
    connection.invoke('Send', nick, chatid, message);
    event.preventDefault();
});

// Add line on chat screen for message recieved
// TODO: Convert to jQuery
function appendLine(nick, message, color) {
    let nameElement = document.createElement('strong');
    nameElement.innerText = `${nick}:`;

    let msgElement = document.createElement('div');
    msgElement.classList.add("message");
    msgElement.innerText = ` ${message}`;

    let imgElement = document.createElement('img');
    imgElement.src = "http://mrg.bz/XduV5Q";

    let li = document.createElement('li');
    if (nick == name) {
        li.classList.add("me");
    } else {
        li.classList.add("you");
    }

    li.appendChild(imgElement);
    li.appendChild(msgElement);

    $('#messages').append(li);
    var offset = $('#frm-send-message').offset(); // Contains .top and .left
    $('html, body').animate({
        scrollTop: offset.top,
        scrollLeft: offset.left
    });

};

// Register new user
function RegisterUser() {
    var DeviceID = localStorage.getItem("DeviceID");
    register($('#nick').val(), DeviceID);
}


// Creates a unique ID for device. Returns the same id each time, unless the browser config has changed with new plugins
var fingerprint = (function (window, screen, navigator) {

    // https://github.com/darkskyapp/string-hash
    function checksum(str) {
        var hash = 5381,
            i = str.length;

        while (i--) hash = (hash * 33) ^ str.charCodeAt(i);

        return hash >>> 0;
    }

    // http://stackoverflow.com/a/4167870/1250044
    function map(arr, fn) {
        var i = 0, len = arr.length, ret = [];
        while (i < len) {
            ret[i] = fn(arr[i++]);
        }
        return ret;
    }

    return checksum([
        navigator.userAgent,
        [screen.height, screen.width, screen.colorDepth].join('x'),
        new Date().getTimezoneOffset(),
        !!window.sessionStorage,
        !!window.localStorage,
        map(navigator.plugins, function (plugin) {
            return [
                plugin.name,
                plugin.description,
                map(plugin, function (mime) {
                    return [mime.type, mime.suffixes].join('~');
                }).join(',')
            ].join("::");
        }).join(';')
    ].join('###'));

}(this, screen, navigator));

var DeviceID = fingerprint;