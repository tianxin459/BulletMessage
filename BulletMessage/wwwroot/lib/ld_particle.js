let textUp = false;
let initSwitch = false;
let keyword = "Test Text";
let canvas;
let context;
let fontStr = '100px sans-serif';

let bgCanvas;
let bgContext;

let denseness = 7;
let dotSize = 5;

//Each particle/icon
let parts = [];

let mouse = {
    x: -100,
    y: -100
};
let mouseOnScreen = false;

let itercount = 0;
let itertot = 40;

let intervalId;

var initialize = function(canvas_id) {
    itercount = 0;
    canvas = document.getElementById(canvas_id);
    // canvas.empty();
    context = canvas.getContext('2d');

    canvas.width = window.innerWidth;
    canvas.height = window.innerHeight * 0.8;

    bgCanvas = document.createElement('canvas');
    bgContext = bgCanvas.getContext('2d');
    bgContext.clearRect(0, 0, canvas.width, canvas.height);

    bgCanvas.width = window.innerWidth;
    bgCanvas.height = window.innerHeight;
    clear();
    parts.length = 0;

    //canvas.addEventListener('mousemove', MouseMove, false);
    //canvas.addEventListener('mouseout', MouseOut, false);

    //start(keyword);
    //intervalId = setInterval(startLoop,100);
    startLoop();
}

startLoop = () => {
    let txt = getRandomText();
    setText(keyword);
}

clearAll = () => {
    console.log('clear');
    clear();
}


getRandomText = () => {
    var text = "";
    var possible = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
    for (var i = 0; i < 5; i++)
        text += possible.charAt(Math.floor(Math.random() * possible.length));
    return text;
}
var setText = function(text) {
    bgContext.clearRect(0, 0, canvas.width, canvas.height);
    bgContext.fillStyle = "#222";
    bgContext.font = fontStr;
    bgContext.fillText(text, 85, 275);
    clear();
    getCoords(false);
}

var start = function(text) {
    parts.length = 0;
    itercount = 0;
    console.log('parts===>', parts);
    //bgContext.fillStyle = "#000000";
    bgContext.clearRect(0, 0, canvas.width, canvas.height);
    bgContext.fillStyle = "#222";
    bgContext.font = fontStr;
    let textInfo = bgContext.measureText(text);
    let textLeft = (canvas.width - textInfo.width) / 2;
    let textTop = (canvas.height);
    console.log(textLeft, textTop, textInfo);
    bgContext.fillText(text, textLeft, textTop);
    clear();
    getCoords(true);
}

var getCoords = function(drawText) {
    var imageData, pixel, height, width;

    imageData = bgContext.getImageData(0, 0, canvas.width, canvas.height);

    // quickly iterate over all pixels - leaving density gaps
    for (height = 0; height < bgCanvas.height; height += denseness) {
        for (width = 0; width < bgCanvas.width; width += denseness) {
            pixel = imageData.data[((width + (height * bgCanvas.width)) * 4) - 1];
            //Pixel is black from being drawn on.
            if (pixel == 255) {
                drawCircle(width, height);
            }
        }
    }
    if (drawText)
        intervalId = setInterval(update, 100);
    else
        intervalId = setInterval(updateFree, 100);
}

var stopCtx = function() {
    clearInterval(intervalId);
}

var redraw = function() {
    if (itercount > (itertot + 10)) return;
    if (textUp) {
        update();
    } else updateFree();
    updateFree();
}

var drawCircle = function(x, y) {
    var startx = (Math.random() * canvas.width);
    var starty = (Math.random() * canvas.height);

    var velx = (x - startx) / itertot;
    var vely = (y - starty) / itertot;

    parts.push({
        c: '#' + (Math.random() * 0x949494 + 0xaaaaaa | 0).toString(16),
        x: x, //goal position
        y: y,
        x2: startx, //start position
        y2: starty,
        r: true, //Released (to fly free!)
        v: {
            x: velx,
            y: vely
        }
    })
}

var update = function() {
    var i, dx, dy, sqrDist, scale;
    itercount++;
    clear();
    for (i = 0; i < parts.length; i++) {

        //If the dot has been released
        if (parts[i].r == true) {
            //Fly into infinity!!
            parts[i].x2 += parts[i].v.x;
            parts[i].y2 += parts[i].v.y;
            //check if they are out of screen... and kill
        }
        if (itercount == itertot) {
            parts[i].v = {
                x: (Math.random() * 6) * 2 - 6,
                y: (Math.random() * 6) * 2 - 6
            };
            parts[i].r = false;
            stopCtx();
        }


        //Look into using svg, so there is no mouse tracking.
        //Find distance from mouse/draw!
        dx = parts[i].x - mouse.x;
        dy = parts[i].y - mouse.y;
        sqrDist = Math.sqrt(dx * dx + dy * dy);

        if (sqrDist < 20) {
            parts[i].r = true;
        }

        //Draw the circle
        context.fillStyle = parts[i].c;
        context.beginPath();
        context.arc(parts[i].x2, parts[i].y2, dotSize, 0, Math.PI * 2, true);
        context.closePath();
        context.fill();
    }
}


var updateFree = function() {

    var i, dx, dy, sqrDist, scale;
    //itercount++;
    clear();
    for (i = 0; i < parts.length; i++) {

        let randomX = Math.floor((Math.random() - 0.5) * 10);
        let randomY = Math.floor((Math.random() - 0.5) * 10);

        //parts[i].v = { x: randomY, y: randomY };
        //parts[i].x2 += parts[i].v.x;
        //parts[i].y2 += parts[i].v.y;

        context.fillStyle = parts[i].c;
        context.beginPath();
        context.arc(parts[i].x2 + randomX, parts[i].y2 + randomY, dotSize, 0, Math.PI * 2, true);
        context.closePath();
        context.fill();

    }
}

//Clear the on screen canvas
var clear = function() {
    context.clearRect(0, 0, canvas.width, canvas.height);
    //context.fillStyle = 'silver';
    context.fillStyle = 'rgba(0, 0, 0, 0)';
    context.beginPath();
    context.rect(0, 0, canvas.width, canvas.height);
    context.closePath();
    context.fill();
}