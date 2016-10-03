const NB_CARD = 20;
const HAND_SIZE = 3;

var numGame = 0;
var Match = require('./models/Match.js');
var Turn = require('./models/Turn.js');
var Player = require('./models/Player.js');

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
                game.match = createMatch(game.playerList);
                initHands(game.match);
                electNarrator(game.match);
            }
        });

        socket.on('disconnect', function(socket){
            //TODO
        });
    });
    
};

function createMatch(playerList){
    // init the stack, players and nbTurn
    var stack = initStack(NB_CARD);
    console.log(stack.toString());
    var players = [];
    playerList.forEach(function(element){
        players.push(new Player(element.playerId));
    }, this)
    var match = new Match(numGame, players, stack, new Turn(), 1);
    return match;
}

function initHands(match){
    // init the hand of players -> use it at the start of a match
    match.players.forEach(function(player){
        var cards = [];
        for(var i = 0; i < HAND_SIZE; i++){
            cards.push(match.stack.pop());
        }
        player.hand = cards;
    });
    
}

function electNarrator(match){
    // choose a narrator among the players
    match.turn.narrator = match.players[(match.nbTurn-1)%(match.players.length)].id;
}

function initStack(stackSize){
    var allCards = [];
    for(var i = 0; i < NB_CARD; i++){
        allCards[i] = i+1;
    }
    var stack = allCards.sort(function(){ return 0.5 - Math.random()}).slice(0, stackSize);
    return stack;
}

function allPlayerReady(playerList){
    var ready = true;
    playerList.forEach(function(element) {
        if(element.ready !== true){
            ready = false;
        };
    });
    return ready;
}

module.exports = Game;