const SESSION_SIZE = 5;
const MATCHMAKING_REQUEST = "matchmaking request";
const QUEUE_SIZE = "queueSize"

// FIFO that represents the players that are waiting for a match
var queue = [];

module.exports = function Matchmaking(io) {
    connect(io);
}

function connect(io) {
    var matchmaking = io.of('/matchmaking');
    matchmaking.on('connection', function(socket) {

        socket.on(MATCHMAKING_REQUEST, function(playerId) {

            var player = { playerId: playerId, socket: socket };

            // Add the user to the queue
            queue.unshift(player);

            console.log("Added player " + player.playerId + " to matchmaking");

            var playerList = attemptMatch();
            if (playerList.length === SESSION_SIZE) {
                console.log("Created match with players : " + playerList.toString());

                // TODO notify the relevant players
                playerList.forEach(function(element) {
                    socket.join("test", function(err) {
                        console.log(err + " " + element.playerId);
                    });
                }, this);

                io.in('test').emit('nique ta mère');
            }

            // Send the new queue size to the users
            matchmaking.emit(QUEUE_SIZE, queue.length);

        });
        socket.on('disconnect', function() {
            // Find the disconnected player from his socket id
            var discPlayer = queue.find(function(element, index, array) {
                return socket.id === element.socket.id;
            });

            if (discPlayer === undefined) {
                // Player is not is the queue, so he probably was assigned to a game
                console.log('A player has left before the game started!');
            }
            else {
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
