const sessionSize = 5;

// FIFO that represents the players that are waiting for a match
var queue = [];

module.exports = function Matchmaking(io) {
  connect(io);
}

function connect(io) {
  var matchmaking = io.of('/matchmaking')
  matchmaking.on('connection', function (socket) {

    socket.on('matchmaking request', function (playerId) {

      var player = { playerId: playerId, socketId: socket.id };

      // Add the user to the queue
      queue.unshift(player);

      console.log("Added player " + player.playerId + " to matchmaking");

      var playerList = attemptMatch();
      if (playerList.length == sessionSize) {
        console.log("Created match with players : " + playerList.toString());

        // TODO notify the relevant players
        socket.broadcast.to(playerList[0].socketId).emit('queueSize', 234);
        socket.broadcast.to(playerList[1].socketId).emit('queueSize', 234);
        socket.broadcast.to(playerList[2].socketId).emit('queueSize', 234);
        socket.broadcast.to(playerList[3].socketId).emit('queueSize', 234);
        socket.broadcast.to(playerList[4].socketId).emit('queueSize', 234);
      }

      // Send the new queue size to the users
      matchmaking.emit('queueSize', queue.length);

    });

    socket.on('disconnect', function () {

    });

  });
}

// Checks if there are enough players to create a match
function attemptMatch() {
  if (queue.length >= sessionSize) {
    var playerList = [];
    for (var i = 0; i < sessionSize; i++) {
      playerList.push(queue.pop());
    }

    return playerList;
  }
  else {
    return [];
  }
}
