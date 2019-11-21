const key_nameList = '##$nameList';
let prizeText;

function showTitle(text) {
    prizeText = text;
    console.log(prizeText);
    $(".prize-title").text(text);
    $(".prize-title").removeClass('show-prize-title');
    $(".prize-title").removeClass('hide-prize-title');
    $(".prize-title").addClass('show-prize-title');
    // setTimeout(command['hideprizetitle'](), 6000);
}

function hideTitle() {
    $(".prize-title").removeClass('show-prize-title');
    $(".prize-title").removeClass('hide-prize-title');
    $(".prize-title").addClass('hide-prize-title');
}

function makesureNameList(cb) {
    if (localStorage.getItem(key_nameList) == null) {
        loadNameList(cb);
    }
    console.log('name list ready');
}

function loadNameList(cb) {
    $.getJSON("./resource/list.json",
        result => {
            localStorage.setItem(key_nameList, JSON.stringify(result));
            console.log('name list loaded');
            cb(result);
        });
}

function saveNameList(nameList) {
    localStorage.setItem(key_nameList, JSON.stringify(nameList));
}

function setNameList(nameList) {
    localStorage.setItem(key_nameList, JSON.stringify(nameList));
}

function getNameList() {
    let resp = localStorage.getItem(key_nameList);
    return JSON.parse(resp);
}

function setNameSelected(userID, flag, prize) {
    let nameList = getNameList();
    let selectedName = nameList
        .filter(x => x.userid == userID)
        .forEach(x => {
            x.selected = flag;
            x.prize = prize;
        });
    saveNameList(nameList);
}

function saveWinner(winnerList, prize) {
    let saveWinnerUrl = URLBASE + "/api/DB/saveWinners";
    winnerList.forEach(w => w.prize = prize);
    winnerList.forEach(w => w.englishName = w.name);
    data = {
        winners: winnerList
    }
    $.ajax({
        url: saveWinnerUrl,
        type: "POST",
        data: JSON.stringify(data),
        contentType: "application/json",
        dataType: "json",
        success: resp => {
            console.log(resp);
        }
    });
}


function getLuckyDraw() {
    let nameList = getNameList();
    let nameListFree = nameList.filter(x => !x.selected);
    if (nameListFree.length == 0) return {
        "userid": "0",
        "name": "",
        "department": ""
    };
    let seed = Math.floor(Math.random() * 1000000);
    let allCount = nameListFree.length;
    let index = seed % allCount;
    let chosen = nameListFree[index];
    return chosen;
}


function getWinnerName() {
    let winner = getWinner();
    console.log(winner);
    return winner.Name; //`${winner.Name} + ${winner.Department}`;
}

function getWinner() {
    return getLuckyDraw();
}

function notifyUser(userid,message) {
    let debug=true;
    // debug = (location.search.indexOf('debug=true')>-1)
    if(debug)userid = 'e0t00nk';
    let notifyUrl = `https://extgst.walmart.com/WeChatSSO/wechat/sendmessage?corpId=ww2736b20d7a3be388&corpSecret=5qrZWBSWFW4CTpexor_NDkRkYBRvrqZ_iSvBg_Dj-ds&appid=1000141&userid=${userid}&message=${message}`
        $.get(notifyUrl, function(data, status){
            console.log('notify user',userid,message)
        });
}

//----------------------Command------------------------

// let command = {
//     refreshnamelist: () => loadNameList(() => console.log('refresh list')),
//     draw: () => draw(),
//     startstop: () => toggleCurtain(),
//     showprizetitle: (text) => {
//         showTitle(text);
//         // prizeText = text;
//         // $(".prize-title").text(text);
//         // $(".prize-title").removeClass('show-prize-title');
//         // $(".prize-title").removeClass('hide-prize-title');
//         // $(".prize-title").addClass('show-prize-title');
//         // setTimeout(command['hideprizetitle'](), 6000);
//     },
//     hideprizetitle: () => {
//         $(".prize-title").removeClass('show-prize-title');
//         $(".prize-title").removeClass('hide-prize-title');
//         $(".prize-title").addClass('hide-prize-title');
//     },
//     refreshprizeimg: (id) => {
//         $(".prize-img").attr("src", `./Uploads/prize/${id}.jpg`);
//     }
// }