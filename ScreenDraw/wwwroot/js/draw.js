"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/drawHub").build();
const canvas = document.querySelector('#canvas');
const ctx = canvas.getContext('2d');
const roomName = document.getElementById("roomName").value;

let engage = false;
let previousX = 0;
let previousY = 0;
let drawHitFirstTime = false;
let startX = 0;
let startY = 0;
let startingImage = new Image();
let startingImageData = null;

connection.on("ReceiveXYData", function (X, Y, colour, shape) {
    
    draw(X, Y, colour, shape);
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

connection.on("RecieveLastImageAfterUndoRedo", function (image) {
    
    setImageOnCanvas(image);

});


connection.on("ReceiveStartXAndYData", function (x, y) {

    //Make sure everyone has the same start coordinates so a
    //draw happend in the same place on everyones canvas

    startX = x;
    startY = y;

    //Also set the starting image data here. This is used when drawing a shape. Each move
    //of the mouse or from touch causes a draw and swaping this image in and out
    //of the canvas prevents the shape from turning solid

    startingImageData = ctx.getImageData(0, 0, canvas.clientWidth, canvas.clientHeight);
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

    connection.invoke("ChangeColour", roomName, colour).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
});



//***  MOUSE EVENTS ***
document.getElementById("canvas").addEventListener("mousedown", function (event) {

    startDrawing(event, event.clientX.toString(), event.clientY.toString());
});

document.getElementById("canvas").addEventListener("mousemove", function (event) {

    move(event, event.clientX.toString(), event.clientY.toString());
});

document.getElementById("canvas").addEventListener("mouseup", function (event) {

    stopDrawing(event);
});

document.getElementById("canvas").addEventListener("mouseleave", function (event) {

    Reset();
    event.preventDefault();
});
//******************


//*** TOUCH EVENTS ***

document.getElementById("canvas").addEventListener("touchstart", function (event) {

    startDrawing(event, event.changedTouches[0].clientX.toString(), event.changedTouches[0].clientY.toString());
});

document.getElementById("canvas").addEventListener("touchmove", function (event) {

    move(event, event.changedTouches[0].clientX.toString(), event.changedTouches[0].clientY.toString());
});

document.getElementById("canvas").addEventListener("touchend", function (event) {

    stopDrawing(event);
});

//*************

function Reset() {
    previousX = 0;
    previousY = 0;
    engage = false;
}


function setImageOnCanvas(image) {
    const context = canvas.getContext('2d');

    //Set the canvas to the image passed. This is a string dataUrl
    //representation of the image

    var img = document.getElementById("currentImage");
    img.onload = function () {
        clearCanvas();
        context.drawImage(img, 0, 0);
    };
    if (image !== "") {
        img.setAttribute("src", image);
    }
    else {
        clearCanvas();
    }
}


function startDrawing(event, X, Y) {

    Reset();
    engage = true;
    //When the first move happens, take a snapshot of the canvas to undo

    drawHitFirstTime = true;

    //Set the shape so we know what to draw
    //Send startX and Y coordinates

    connection.invoke("SendStartXAndYData", roomName, X.toString(), Y.toString())
        .catch(function (err) {
            return console.error(err.toString());
        });
    
    event.preventDefault();
}

function stopDrawing(event) {

    drawHitFirstTime = false;
    //startingImageData = null;

    //Push out a reset data command to all clients so the next time
    //a draw happens the previous X and Y are set to 0 in their
    //instance as well as this one
    Reset();

    connection.invoke("ResetDataCommand", roomName).catch(function (err) {
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

        var shape = document.getElementById("shapes").options[document.getElementById("shapes").selectedIndex].value;
        connection.invoke("SendXAndYData", roomName, X, Y, colour, shape)
            .catch(function (err) {
                return console.error(err.toString());
            });

        event.preventDefault();
    }
}


function passCanvasBack()
{
    //Pass image data back to the server. This is used as a starting point
    //when new people enter the room, it also adds it to the undo stack

    var image = document.getElementById("canvas").toDataURL("image/png");

    connection.invoke("SetCurrentImage", image, roomName).catch(function (err) {
        return console.error(err.toString());
    });
}

function addToUndoStack() {
    //Pass image data back to the server. This is added to the undo stack
    //and is initiated at the start of a draw event

    var image = document.getElementById("canvas").toDataURL("image/png");

    connection.invoke("AddToUndoStack", image, roomName).catch(function (err) {
        return console.error(err.toString());
    });
}

function clearCanvas() {
    const context = canvas.getContext('2d');
    context.clearRect(0, 0, canvas.clientWidth, canvas.clientHeight);

    document.getElementById("currentImage").style.display = 'none';

}

function undo() {
    var image = document.getElementById("canvas").toDataURL("image/png");

    //Undo the last edit. This will return the previous image and set it
    //in the canvas

    connection.invoke("DoUndoAndReturnImage", image, roomName)
        .catch(function (err) {
            return console.error(err.toString());
        });

    event.preventDefault();
}

function redo() {
    var image = document.getElementById("canvas").toDataURL("image/png");

    //Undo the last edit. This will return the previous image and set it
    //in the canvas

    connection.invoke("DoRedoAndReturnImage", image, roomName)
        .catch(function (err) {
            return console.error(err.toString());
        });

    event.preventDefault();
}


function draw(X, Y, colour, shape) {

    //If this is the first time the draw function has been called for this
    //draw event, take a snapshot of the canvas so an undo can happen after
    //the draw has finished

    let lineWidth = 5;

    if (drawHitFirstTime == true) {
        addToUndoStack();
        drawHitFirstTime = false;
    }
    
    switch (shape) {
        case "free":
            drawFreeLine(X, Y, colour, lineWidth);
            break;
        case "square":
            drawSquare(startX, startY, X, Y, colour, lineWidth);
            break;
        case "circle":
            drawCircle(startX, startY, X, Y, colour, lineWidth);
            break;
        case "ellipse":
            drawEllipse(startX, startY, X, Y, colour, lineWidth);
            break;
        case "line":
            drawStraightLine(startX, startY, X, Y, colour, lineWidth);
            break;
    }
}

function drawFreeLine(X, Y, colour, lineWidth) {

    //Get the canvas so we can offset the x and y relative to its position

    var rect = document.getElementById("canvas").getBoundingClientRect();

    if (!canvas.getContext) {
        return;
    }

    if (previousX == 0) {
        previousX = X - rect.x;
    }

    if (previousY == 0) {
        previousY = Y - rect.y;
    }

    // set line stroke and line width
    ctx.strokeStyle = colour;
    ctx.lineWidth = lineWidth;

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

function drawSquare(startX, startY, X, Y, colour, lineWidth) {

    var rect = document.getElementById("canvas").getBoundingClientRect();

    if (!canvas.getContext) {
        return;
    }

    ctx.putImageData(startingImageData, 0, 0);

    // set line stroke and line width
    ctx.strokeStyle = colour;
    ctx.lineWidth = lineWidth;

    let width = X - startX;
    let height = Y - startY; 


    console.log(height);

    // draw a square

    ctx.beginPath();
    ctx.rect(startX - rect.x, startY - rect.y, width, height);
    ctx.stroke();

}

function drawCircle(startX, startY, X, Y, colour, lineWidth) {

    var rect = document.getElementById("canvas").getBoundingClientRect();

    if (!canvas.getContext) {
        return;
    }

    ctx.putImageData(startingImageData, 0, 0);

    // set line stroke and line width
    ctx.strokeStyle = colour;
    ctx.lineWidth = lineWidth;

    let radius = X - startX;
    //let height = Y - startY;

    if (radius < 0) {
        radius = radius * -1;
    }

    ctx.beginPath();
    ctx.arc(startX - rect.x, startY - rect.y, radius, 0, 2 * Math.PI);
    ctx.stroke();

}

function drawEllipse(startX, startY, X, Y, colour, lineWidth) {

    var rect = document.getElementById("canvas").getBoundingClientRect();

    if (!canvas.getContext) {
        return;
    }


    ctx.putImageData(startingImageData, 0, 0);

    startX = startX - rect.x;
    startY = startY - rect.y;

    // set line stroke and line width
    ctx.strokeStyle = colour;
    ctx.lineWidth = lineWidth;
    ctx.save();

    ctx.beginPath();
    var scalex = 1 * ((X - startX) / 2);
    var scaley = 1 * ((Y - startY) / 2);
    ctx.scale(scalex, scaley);

    var centerx = (startX / scalex) + 1;
    var centery = (startY / scaley) + 1;
    ctx.arc(centerx, centery, 1, 0, 2 * Math.PI);

    ctx.restore();
    ctx.stroke();

}

function drawStraightLine(startX, startY, X, Y, colour, lineWidth) {

    var rect = document.getElementById("canvas").getBoundingClientRect();

    if (!canvas.getContext) {
        return;
    }

    ctx.putImageData(startingImageData, 0, 0);

    startX = startX - rect.x;
    startY = startY - rect.y;

    // set line stroke and line width
    ctx.strokeStyle = colour;
    ctx.lineWidth = lineWidth;

    ctx.beginPath();
    ctx.moveTo(startX, startY);
    ctx.lineTo(X - rect.x, Y - rect.y);
    ctx.stroke();

}







