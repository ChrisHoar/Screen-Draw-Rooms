"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/drawHub").build();

connection.on("ReceiveXYData", function (X, Y, colour) {
    
    draw(X, Y, colour);
});

connection.on("ReceiveColourData", function (colour) {
    //selectElement("colours", colour);

});

connection.on("ReceiveResetDataCommand", function () {
    Reset();
});

connection.on("ReceiveAddedToRoom", function (success) {
    //For future use

});


function selectElement(id, valueToSelect) {
    let element = document.getElementById(id);
    element.value = valueToSelect;
}

connection.start().then(function () {
    connection.invoke("AddUserToRoom", document.getElementById("roomName").value);
}).catch(function (err) {
    return console.error(err.toString());
});


document.getElementById("colours").addEventListener("change", function (event) {
    var colour = document.getElementById("colours").options[document.getElementById("colours").selectedIndex].value;
    var rn = document.getElementById("roomName").value;
    connection.invoke("ChangeColour", rn, colour).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
});

var engage = false;
var previousX = 0;
var previousY = 0;


document.getElementById("canvas").addEventListener("mousedown", function (event) {

    startDrawing(event);
});

document.getElementById("canvas").addEventListener("mouseup", function (event) {

    stopDrawing(event);
});

document.getElementById("canvas").addEventListener("mouseleave", function (event) {

    Reset();
    event.preventDefault();
});

function Reset()
{
    previousX = 0;
    previousY = 0;
    engage = false;
}

document.getElementById("canvas").addEventListener("mousemove", function (event) {

    move(event, event.clientX.toString(), event.clientY.toString());
});

document.getElementById("canvas").addEventListener("touchstart", function (event) {

    startDrawing(event);
});

document.getElementById("canvas").addEventListener("touchend", function (event) {

    stopDrawing(event);
});


document.getElementById("canvas").addEventListener("touchmove", function (event) {

    move(event, event.changedTouches[0].clientX.toString(), event.changedTouches[0].clientY.toString());
});

function startDrawing(event) {
    Reset();
    engage = true;
    event.preventDefault();
}

function stopDrawing(event) {
    Reset();
    //Push out a reset data command to all clients so the next time
    //a draw happens the previous X and Y are set to 0 in their
    //instance as well as this one

    var rn = document.getElementById("roomName").value;
    connection.invoke("ResetDataCommand", rn).catch(function (err) {
        return console.error(err.toString());
    });

    passCanvasBack();
    event.preventDefault();
}

function move(event, X, Y) {
    //Send draw data coordinates back to the hub, this is then broadcast to all
    //artists in the room (including this one) and is drawn to the canvas

    if (engage == true) {
        var colour = document.getElementById("colours").options[document.getElementById("colours").selectedIndex].value;
        var roomName = document.getElementById("roomName").value;
        connection.invoke("SendXAndYData", roomName, X, Y, colour)
            .catch(function (err) {
                return console.error(err.toString());
            });

        event.preventDefault();
    }
}

const canvas = document.querySelector('#canvas');

function passCanvasBack()
{
    //Pass image data back to the server. This is used as a starting point
    //when new people enter the room

    var rn = document.getElementById("roomName").value;
    var image = document.getElementById("canvas").toDataURL("image/png");

    connection.invoke("SetCurrentImage", image, rn).catch(function (err) {
        return console.error(err.toString());
    });
}

function clearCanvas() {
    const context = canvas.getContext('2d');
    context.clearRect(0, 0, canvas.clientWidth, canvas.clientHeight);

    document.getElementById("currentImage").style.display = 'none';

}

function draw(X, Y, colour) {

    
    //Get the canvas so we can offset the x and y relative to its position
    var rect = document.getElementById("canvas").getBoundingClientRect();

    if (!canvas.getContext) {
        return;
    }
    const ctx = canvas.getContext('2d');

    if (previousX == 0) {
        previousX = X - rect.x;
    }

    if (previousY == 0) {
        previousY = Y - rect.y;
    }

    // set line stroke and line width
    ctx.strokeStyle = colour;
    ctx.lineWidth = 5;

    X = X - rect.x;
    Y = Y - rect.y;

    // draw a red line

    ctx.beginPath();
    ctx.moveTo(previousX, previousY);
    ctx.lineTo(X, Y);
    ctx.stroke();
    
    //Store the X and Y to use in the next itteration. This
    //is so we know where to drow the line from and to

    previousX = X;
    previousY = Y;

}



