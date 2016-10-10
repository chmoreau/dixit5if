function IOPlayer(room, playerList) {
    this.room = room;
    this.playerList = playerList;
};

IOPlayer.prototype.sendToPlayer = function(playerId, type, content, resType, callback){
    var socket = getSocketPlayer(playerId, this.playerList);
    if(socket != undefined){
        if(resType !== undefined){
            socket.on(resType, function(res){
                callback(res);
            }); //callback executed
        }
        socket.emit(type, content);
    } else {
        //TODO : handle error
    }
};

function getSocketPlayer(playerId, playerList){
    var socket = {};
    playerList.find(function(element, index, array) {
        if(element.playerId === playerId){
            socket = element.socket;
        };
    });
    return socket;
};

module.exports = IOPlayer;