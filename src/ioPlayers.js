/**
 * Interface for communicating with the players via socket objects.
 * 
 * @constructor
 * @param {Object} room : IO object representing the connection to the game's' namespace
 * @param {Array} playerList : Array of the connected players with their socket
 */
function IOPlayer(room, playerList) {
    this.room = room;
    this.playerList = playerList;
};


/**
 * Send a message to a player and listens for his response.
 *
 * @param {any} playerId : Id of the player to send the message
 * @param {string} type : Type of the message
 * @param {any} content : Content of the message
 * @param {string} resType : Type of the expected response message
 * @param {resCallback} callback : Callback function called when a response is sent by the receiver
 */
IOPlayer.prototype.sendToPlayer = function (playerId, type, content, resType, callback) {
    var socket = getSocketFromPlayerId(playerId, this.playerList);
    if (socket != undefined) {
        if (resType !== undefined) {
            socket.once(resType, callback); //callback executed
        }
        socket.emit(type, content);
    } else {
        //TODO : handle error
    }
};

/**
 * Send a message to all players and listens for their response.
 *
 * @param {string} Type : Type of the message
 * @param {any} content : Content of the message
 * @param {string} resType : Type of the expected response message
 * @param {resCallback} callback : Callback function called when a response is sent by the receiver
 */
IOPlayer.prototype.sendToAll = function (type, content, resType, callback) {
    if (resType != undefined) {
        this.playerList.forEach(function (element) {
            element.socket.once(resType, msg => callback(element.playerId, msg));
        });
    }
    this.room.emit(type, content);
};

/**
 * Listen for a specific message (only once) sent by a specific player.
 *
 * @param {any} playerId : Id of the player to listen
 * @param {string} type : Type of the expected message
 * @param {resCallback} callback : Callback function called when the message arrives
 */
IOPlayer.prototype.receiveMsgFrom = function (playerId, type, callback) {
    var socket = getSocketFromPlayerId(playerId, this.playerList);
    if (socket !== undefined) {
        socket.once(type, callback);
    } else {
        //TODO : handle error
    }
};

/**
 * Listen for a specific message (only once) sent by each player.
 *
 * @param {string} type : Type of the expected message
 * @param {resCallback} callback : Callback function called when the message arrives
 */
IOPlayer.prototype.receiveMsg = function (type, callback) {
    this.playerList.forEach(function (element) {
        element.socket.once(type, msg => callback(element.playerId, msg));
    });
};

function getSocketFromPlayerId(playerId, playerList) {
    var socket = {};
    playerList.find(function (element, index, array) {
        if (element.playerId === playerId) {
            socket = element.socket;
        };
    });
    return socket;
};

/**
 * @callback resCallback
 * @param {any} playerId : Player Id of the receiver
 * @param {any} message : Content of the response's message
 */

module.exports = IOPlayer;