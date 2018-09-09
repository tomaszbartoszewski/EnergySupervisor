"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/pageUpdateHub").build();

connection.on("LedStatusUpdate", function (deviceId, isLedOn) {
    var encodedMsg = deviceId + " is LED on " + isLedOn;
    var li = document.createElement("li");
    li.textContent = encodedMsg;
    document.getElementById("messagesList").appendChild(li);
});

connection.on("PowerGeneratorUpdate", function (generatedPower) {
    var encodedMsg = "Current generated power: " + generatedPower;
    var li = document.createElement("li");
    li.textContent = encodedMsg;
    document.getElementById("messagesList").appendChild(li);
});
//
// connection.on("ReceiveMessage", function (user, message) {
//     var msg = message.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
//     var encodedMsg = user + " says " + msg;
//     var li = document.createElement("li");
//     li.textContent = encodedMsg;
//     document.getElementById("messagesList").appendChild(li);
// });

connection.start().catch(function (err) {
    return console.error(err.toString());
});

// document.getElementById("sendButton").addEventListener("click", function (event) {
//     var user = document.getElementById("userInput").value;
//     var message = document.getElementById("messageInput").value;
//     connection.invoke("SendMessage", user, message).catch(function (err) {
//         return console.error(err.toString());
//     });
//     event.preventDefault();
// });