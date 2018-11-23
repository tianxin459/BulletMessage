var urlRace = "/race";
//var urlRace = "https://ellist.cn/bulletmessage/race";
let isRacing = false;
let trackTop = 0;
let trackBottom = 0;

$('document').ready(() => {
    connection
        .start()
        .catch(err => {
            console.error(err);
        })

    // track size
    trackTop = $('.track').position().top
    trackBottom = $('.track').position().top + $('.track').height()
    let finishLinePos = window.innerWidth - 100;
    console.log(finishLinePos);
    $('.finishLine').css("left", finishLinePos + 'px');

    $('#btnSubmit').click(e => {
        let userId = $('#userId').val();
        let msg = $('#msg').val();
        setRacer(userId, 'ddd', 10);

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
    isRacing = true;
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

function setRacer(name, avatorUrl, position) {
    //if (listRunner.has(userId)) {
    //    return;
    //}
    //listRunner.add(userId);
    // avatorUrl = 'https://www.pymnts.com/wp-content/uploads/2014/12/green-dot-logo.jpg'
    let track = $('div.track');
    let top = (Math.random() * 1000) % window.innerHeight;
    let strRunner = ` <div id="${parseName(name)}" class="runner">
            <div>${name}</div>
            <image class="running-body" src="static/img/running1.gif" title="this slowpoke moves" />
            <image class="running-head" src="${avatorUrl}"></image>
        </div>`;
    let runner = $(strRunner);
    runner.css('top', top + 'px');
    runner.css(css_direction, position + 'px');
    track.append(runner);
}

function moveRunner(name, distant) {
    if (!isRacing) return; // if not racing then do not move the player
    let runner = $('#' + parseName(name));
    let position = runner.css(css_direction);
    position = position.replace('px', '');
    console.log(position);
    runner.css(css_direction, (parseInt(position) + parseInt(distant)) + 'px');
}