// var urlRace = "/race";
//var urlRace = "https://ellist.cn/bulletmessage/race";
let isRacing = false;
let trackTop = 0;
let trackBottom = 0;
let finishLinePos, startLinePos;
let reportSend = false;
let racerList = [];

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

    $('.finishLine').css("left", finishLinePos + 'px');
    console.log(finishLinePos);

    $('#btnSubmit').click(e => {
        let userId = $('#userId').val();
        let msg = $('#msg').val();

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

connection.on("SetRacer", (userId, avatorUrl, message, englishName) => {
    let distant = parseInt(message, 10)
    if (listRunner.has(userId)) {
        console.log("moveRacer", userId, message);
        moveRunner(userId, distant)
    } else {
        listRunner.add(userId);
        console.log("SetRacer", userId, avatorUrl, message, englishName);
        setRacer(userId, avatorUrl, distant, englishName);
    }
})
connection.on("readyRacer", (userId, avatorUrl, message) => {
    let distant = parseInt(message, 10)
    if (listRunner.has(userId)) {
        console.log("readyRacer moveRacer", userId, message);
        moveRunner(userId, distant)
    } else {
        listRunner.add(userId);
        console.log("readyRacer SetRacer", userId, avatorUrl, message, englishName);
        setRacer(userId, avatorUrl, distant, englishName);
    }
})

function parseName(name) {
    return name.replace(' ', '_');
}

const css_direction = 'left';


function getRacerTop() {
    let trackHeight = ($('.track')[0].clientHeight - 90);
    let top = (Math.random() * 1000) % trackHeight;
    return top + trackTop;
}

function setRacer(name, avatorUrl, position, englishName) {
    //if (listRunner.has(userId)) {
    //    return;
    //}
    //listRunner.add(userId);
    // avatorUrl = 'https://www.pymnts.com/wp-content/uploads/2014/12/green-dot-logo.jpg'
    position = startLinePos - 40;
    let track = $('div.track');
    let top = getRacerTop();
    let strRunner = ` <div id="${parseName(name)}" class="runner">
            <div class="nickName">${englishName}</div>
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
    if (!distant) distant = 10;
    if (!isRacing) return; // if not racing then do not move the player
    if (listFinish.has(name)) return;
    let runner = $('#' + parseName(name));
    //let position = runner.css(css_direction);
    let position = runner.data('currentLocation');
    //position = position.replace('px', '');
    console.log(position);
    if (position > finishLinePos) {
        console.log('listFinish', listFinish.size);
        listFinish.add(name);
        if (listFinish.size >= 10 || listFinish.size == listRunner.size) {
            $('.finishGame').show();
            if (listFinish.size >= 10) {
                runner.css("display", "none");
            }
            if (!reportSend) {
                let resultUrl = apiRace + "/result";
                let listData = Array.from(listFinish)
                let resultStr = JSON.stringify(listData);
                // resultStr = resultStr.replace('"', '\"');
                data = {
                    Value: resultStr
                };
                console.log('send report', data);
                reportSend = true;

                $.ajax({
                    url: resultUrl,
                    type: "POST",
                    data: JSON.stringify(data),
                    contentType: "application/json",
                    dataType: "json",
                    success: resp => {
                        console.log(resp);
                    }
                });
            }
            // return;
        }
        runner.removeClass("stage3");
        runner.addClass('finishRun');
        runner.css("top", (listFinish.size) * 50 + 'px')
        runner.css("left", '100px');
        runner.css('transform', `translateX(0) translateZ(0)`);
        console.log(`${name} finish run`);
        connection.invoke("FinishRun", name, 'finish at ' + listFinish.size)
            .catch(err => console.error(err));
    } else {
        position = position + parseInt(distant);
        let transformText = `translateX(${position}px) translateZ(${position}px)`;
        if (position > finishLinePos - 300 * 2) {
            runner.addClass("stage1");
            transformText += ' scale(1.2)';
        }
        if (position > finishLinePos - 200 * 2) {
            runner.removeClass("stage1");
            runner.addClass("stage2");
            transformText += ' scale(1.3)';
        }
        if (position > finishLinePos - 100 * 2) {
            runner.removeClass("stage2");
            runner.addClass("stage3");
            transformText += ' scale(1.4)';
        }
        runner.data('currentLocation', position);
        runner.css('transform', transformText);
        //runner.css(css_direction, (parseInt(position) + parseInt(distant)) + 'px');
    }
}