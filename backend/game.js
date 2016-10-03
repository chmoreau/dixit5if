var numGame = 0;
var Match = require('./models/Match.js');
var Turn = require('./models/Turn.js');

function Game(io, playerList) {
    this.io = io;
    this.playerList = playerList;
    this.id = ++numGame;
    this.room = io.of('/game/'+ this.id);
    console.log('Game ' + this.id + ' created');
    this.listenPlayer();
};

Game.prototype.joinRoom = function(){
    this.playerList.forEach(function(element) {
        element.socket.join('/game/'+numGame, function(err) {
            console.log(err + " " + element.playerId);
        });
    }, this);
};

Game.prototype.listenPlayer = function(){
    var game = this;
    this.room.on('connection', function(socket) {
        socket.on('ready', function(playerID){
            console.log('player ' + playerID + ' is ready');
            game.playerList.find(function(element, index, array) {
                if(element.playerId === playerID){
                    element.ready = true;
                };
            });
            if(allPlayerReady(game.playerList)){
                console.log('Everyone is ready');
                game.match = new Match(numGame, game.playerList, [], new Turn());
            }
        });

        socket.on('disconnect', function(socket){
            //TODO
        });
    });
    
};

function allPlayerReady(playerList){
    var ready = true;
    playerList.forEach(function(element) {
        if(element.ready !== true){
            ready = false;
        };
    }, this);
    return ready;
}

module.exports = Game;

/*

*/