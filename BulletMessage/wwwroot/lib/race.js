// var urlRace = "/race";
//var urlRace = "https://ellist.cn/bulletmessage/race";
let isRacing = false;
let trackTop = 0;
let trackBottom = 0;
let finishLinePos, startLinePos

$('document').ready(() => {
    connection
        .start()
        .catch(err => {
            console.error(err);
        })

    // track size
    trackTop = $('.track').position().top
    trackBottom = $('.track').position().top + $('.track').height()
    finishLinePos = window.innerWidth - 100;
    startLinePos = $('.startLine').position().left;

    console.log(finishLinePos);
    $('.finishLine').css("left", finishLinePos + 'px');

    $('#btnSubmit').click(e => {
        let userId = $('#userId').val();
        let msg = $('#msg').val();
        // let avatorUrl = "dddd";
        // setRacer(userId, 'ddd', 10);

        // if (listRunner.has(userId)) {
        //     console.log("readyRacer moveRacer", userId, msg);
        //     moveRunner(userId, distant)
        // } else {
        //     listRunner.add(userId);
        //     console.log("readyRacer SetRacer", userId, avatorUrl, msg);
        //     setRacer(userId, avatorUrl, distant);
        // }

        connection.invoke("SendMessage", userId, msg)
            .catch(err => console.error(err));
        e.preventDefault();
    })
});


const connection = new signalR.HubConnectionBuilder()
    .withUrl(urlRace, {
        skipNegotiation: true,
        transport: signalR.HttpTransportType.WebSockets
    })
    .build();

var listRunner = new Set();
var listFinish = new Set();

connection.on("ReceiveMessage", (userId, message) => {
    console.log("ReceiveMessage", userId, message);
    let distant = parseInt(message, 10)
    if (!!distant) {
        moveRunner(userId, distant);
        //setRacer(userId, 'ddd', 10);
    }
})


connection.on("beginRace", (message) => {
    console.log("beginRace", message);
    // isRacing = true;
    startRace();
})

connection.on("SetRacer", (userId, avatorUrl, message) => {
    let distant = parseInt(message, 10)
    if (listRunner.has(userId)) {
        console.log("moveRacer", userId, message);
        moveRunner(userId, distant)
    } else {
        listRunner.add(userId);
        console.log("SetRacer", userId, avatorUrl, message);
        setRacer(userId, avatorUrl, distant);
    }
})
connection.on("readyRacer", (userId, avatorUrl, message) => {
    let distant = parseInt(message, 10)
    if (listRunner.has(userId)) {
        console.log("readyRacer moveRacer", userId, message);
        moveRunner(userId, distant)
    } else {
        listRunner.add(userId);
        console.log("readyRacer SetRacer", userId, avatorUrl, message);
        setRacer(userId, avatorUrl, distant);
    }
})

function parseName(name) {
    return name.replace(' ', '_');
}

const css_direction = 'left';


function getRacerTop() {
    let trackHeight = $('.track')[0].clientHeight;
    let top = (Math.random() * 1000) % trackHeight;
    return top + trackTop;
}

function setRacer(name, avatorUrl, position) {
    //if (listRunner.has(userId)) {
    //    return;
    //}
    //listRunner.add(userId);
    // avatorUrl = 'https://www.pymnts.com/wp-content/uploads/2014/12/green-dot-logo.jpg'
    position = startLinePos - 40;
    let track = $('div.track');
    let top = getRacerTop();
    let strRunner = ` <div id="${parseName(name)}" class="runner">
            <div class="nickName">${name}</div>
            <image class="running-body" src="static/img/running1.gif" title="this slowpoke moves" />
            <image class="running-head" src="${avatorUrl}"></image>
            <div class="info"></div>
        </div>`;
    let runner = $(strRunner);
    runner.css('top', top + 'px');
    runner.data('currentLocation', 0);
    runner.css(css_direction, position + 'px');
    track.append(runner);
}

function moveRunner(name, distant) {
    if (!isRacing) return; // if not racing then do not move the player
    if (listFinish.has(name)) return;
    let runner = $('#' + parseName(name));
    //let position = runner.css(css_direction);
    let position = runner.data('currentLocation');
    //position = position.replace('px', '');
    console.log(position);
    if (position > finishLinePos) {
        listFinish.add(name);
        runner.addClass('finishRun');
        runner.css("top", (listFinish.size) * 50 + 'px')
        runner.css("left", '100px');
        runner.css('transform', `translateX(0)`);
        console.log(`${name} finish run`);
        connection.invoke("FinishRun", name, 'finish at ' + listFinish.size)
            .catch(err => console.error(err));
    } else {
        position = position + parseInt(distant);
        runner.data('currentLocation', position);
        runner.css('transform', `translateX(${position}px)`);
        //runner.css(css_direction, (parseInt(position) + parseInt(distant)) + 'px');
    }
}