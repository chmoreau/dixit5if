const SESSION_SIZE = 2;

var Messages = require('./messageType');
var Game = require('./game.js');

// FIFO that represents the players that are waiting for a match
var queue = [];

module.exports = function Matchmaking(io) {
    connect(io);
}

function connect(io) {
    //var matchmaking = io.of('/matchmaking');
    io.on('connection', function(socket) {

        socket.on(Messages.JOIN_MATCHMAKING, function(msg) {

            var player = { playerId: msg.playerID, socket: socket };

            // Add the user to the queue
            queue.unshift(player);

            console.log("Added player " + player.playerId + " to matchmaking");

            var playerList = attemptMatch();
            if (playerList.length === SESSION_SIZE) {
                console.log("Created match with players : " + playerList.toString());

                // TODO notify the relevant players
                var game = new Game(io, playerList);
                playerList.forEach(function(element) {
                    element.socket.join(game.room);
                }, this);

                io.in(game.room).emit(Messages.GAME_CREATED, {gameID: game.id});
            }

            // Send the new queue size to the users
            io.emit(Messages.QUEUE_SIZE, {queueLength:queue.length});
        });

        socket.on('Disconnect', function() {
            // Find the disconnected player from his socket id
            var discPlayer = queue.find(function(element, index, array) {
                return socket.id === element.socket.id;
            });

            if (discPlayer !== undefined) {
                // Remove the player from the queue
                queue.splice(queue.indexOf(discPlayer, 0), 1);
                console.log(discPlayer.playerId + ' has left matchmaking');
            }
        });

    });
}

// Checks if there are enough players to create a match
function attemptMatch() {
    if (queue.length >= SESSION_SIZE) {
        var playerList = [];
        for (var i = 0; i < SESSION_SIZE; i++) {
            playerList.push(queue.pop());
        }
        return playerList;
    }
    else {
        return [];
    }
}
