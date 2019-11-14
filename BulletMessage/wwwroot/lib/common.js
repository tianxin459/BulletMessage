const key_nameList = '##$nameList';

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

function setNameSelected(userID,flag) {
    let nameList = getNameList();
    let selectedName = nameList
        .filter(x => x.UserID == userID)
        .forEach(x => x.selected = flag);
    saveNameList(nameList);
}


function getLuckyDraw() {
    let nameList = getNameList();
    let nameListFree = nameList.filter(x => !x.selected);
    if (nameListFree.length == 0) return {
        "UserID": "0",
        "Name": "",
        "Department": ""
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
    return winner.Name;//`${winner.Name} + ${winner.Department}`;
}

function getWinner() {
    return getLuckyDraw();
}