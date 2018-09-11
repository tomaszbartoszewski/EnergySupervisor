"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/pageUpdateHub").build();

connection.on("LedStatusUpdate", function (deviceId, isLedOn) {
    var deviceDiv = document.getElementById(deviceId);
    
    if (deviceDiv != null)
        deviceDiv.getElementsByClassName("ledStatus")[0].src = isLedOnToImgSrc(isLedOn);
    else {
        var newDevice = document.createElement("div");
        newDevice.id = deviceId;
        var idLabel = document.createElement("label");
        idLabel.className = "deviceId";
        idLabel.innerHTML = deviceId;
        
        var ledStatusLabel = document.createElement("img");
        ledStatusLabel.className = "ledStatus";
        ledStatusLabel.src = isLedOnToImgSrc(isLedOn);
        
        newDevice.appendChild(idLabel);
        newDevice.appendChild(ledStatusLabel);
        
        var listOfActiveDevices = document.getElementById("listOfActiveDevices");
        listOfActiveDevices.appendChild(newDevice);
    }
});

function isLedOnToImgSrc(isLedOn) {
    if (isLedOn) {
        return "images/LedOn.png";
    } 
    else {
        return "images/LedOff.png";
    }
}

connection.on("PowerGeneratorUpdate", function (generatedPower) {
    var labelWithPowerValue = document.getElementById("currentPowerGeneration");
    if (generatedPower <= 0) {
        labelWithPowerValue.className = "redLabel";
    } 
    else if (generatedPower < 3) {
        labelWithPowerValue.className = "yellowLabel";
    } 
    else {
        labelWithPowerValue.className = "greenLabel"
    }
    
    labelWithPowerValue.innerHTML = generatedPower.toFixed(2);
});

connection.start().catch(function (err) {
    return console.error(err.toString());
});
