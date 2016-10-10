function IOPlayer(room, playerList) {
    this.room = room;
    this.playerList = playerList;
};

IOPlayer.prototype.sendToPlayer = function (playerId, type, content, resType, callback) {
    var socket = getSocketPlayer(playerId, this.playerList);
    if (socket != undefined) {
        if (resType !== undefined) {
            socket.once(resType, callback); //callback executed
        }
        socket.emit(type, content);
    } else {
        //TODO : handle error
    }
};

IOPlayer.prototype.sendToAll = function (type, content, resType, callback) {
    if (resType != undefined) {
        this.playerList.forEach(function (element) {
            element.socket.once(resType, msg => callback(element.playerId, msg));
        });
    }
    this.room.emit(type, content);
};

IOPlayer.prototype.receiveMsgFrom = function (playerId, type, callback) {
    var socket = getSocketPlayer(playerId, this.playerList);
    if (socket !== undefined) {
        socket.once(type, callback);
    } else {
        //TODO : handle error
    }
};

IOPlayer.prototype.receiveMsg = function (type, callback) {
    this.playerList.forEach(function (element) {
        element.socket.once(resType, msg => callback(element.playerId, msg));
    });
};

function getSocketPlayer(playerId, playerList) {
    var socket = {};
    playerList.find(function (element, index, array) {
        if (element.playerId === playerId) {
            socket = element.socket;
        };
    });
    return socket;
};

module.exports = IOPlayer;