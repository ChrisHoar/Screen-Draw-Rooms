﻿"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/drawHub").build();

connection.on("ReceiveXYData", function (X, Y) {
    var colour = document.getElementById("colours").options[document.getElementById("colours").selectedIndex].value;

    draw(X, Y, colour);
});

connection.on("ReceiveColourData", function (colour) {
    selectElement("colours", colour);
});

connection.on("ReceiveResetDataCommand", function (colour) {
    Reset();
});

function selectElement(id, valueToSelect) {
    let element = document.getElementById(id);
    element.value = valueToSelect;
}

connection.start().then(function () {
    

}).catch(function (err) {
    return console.error(err.toString());
});


document.getElementById("colours").addEventListener("change", function (event) {
    var colour = document.getElementById("colours").options[document.getElementById("colours").selectedIndex].value;
    
    connection.invoke("ChangeColour", colour).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
});

var engage = false;
var previousX = 0;
var previousY = 0;


document.getElementById("canvas").addEventListener("mousedown", function (event) {

    Reset();
    engage = true;       
    event.preventDefault();
});

document.getElementById("canvas").addEventListener("mouseup", function (event) {

    Reset();
    //Push out a reset data command to all clients so the next time
    //a draw happens the previous X and Y are set to 0 in their
    //instance as well as this one

    connection.invoke("ResetDataCommand").catch(function (err) {
            return console.error(err.toString());
        });
    event.preventDefault();
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

    if (engage == true) {
        connection.invoke("SendXAndYData", event.clientX.toString(), event.clientY.toString())
            .catch(function (err) {
                return console.error(err.toString());
            });

        event.preventDefault();
    }
});

document.getElementById("canvas").addEventListener("touchstart", function (event) {

    Reset();
    engage = true;
    event.preventDefault();
});

document.getElementById("canvas").addEventListener("touchend", function (event) {

    Reset();
    //Push out a reset data command to all clients so the next time
    //a draw happens the previous X and Y are set to 0 in their
    //instance as well as this one

    connection.invoke("ResetDataCommand").catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
});



document.getElementById("canvas").addEventListener("touchmove", function (event) {
    if (engage == true) {
        connection.invoke("SendXAndYData", event.changedTouches[0].clientX.toString(), event.changedTouches[0].clientY.toString())
            .catch(function (err) {
                return console.error(err.toString());
            });

        event.preventDefault();
    }
});

const canvas = document.querySelector('#canvas');
const ctx = canvas.getContext('2d');

function clearCanvas() {
    ctx.clearRect(0, 0, canvas.clientWidth, canvas.clientHeight);
}

function draw(X, Y, colour) {

    
    //Get the canvas so we can offset the x and y relative to its position
    var rect = document.getElementById("canvas").getBoundingClientRect();

    if (!canvas.getContext) {
        return;
    }
    //const ctx = canvas.getContext('2d');

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


