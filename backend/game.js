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
    this.match = {};
    console.log('Game ' + this.id + ' created');
    this.listenPlayer();
};

Game.prototype.joinRoom = function(){
    this.playerList.forEach(function(element) {
        element.socket.join('/game/'+numGame);
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
                console.log('Init match done');
                sendStartTurn(game);
            }
        });
        socket.on('submit theme', function(theme){
            console.log('narrator theme received');
            match.turn.theme = theme;
            sendNarratorTheme(theme);
        });
        socket.on('play card', function(data){ //TODO : remove card from player's hand
            var playerID = data.playerID;
            var cardID = data.cardID;
            var match = game.match;
            console.log('player ' + playerID + ' played the card ' + cardID);
            match.players.find(function(element, index, array) {
                if(element.playerId === playerID){
                    var trick = match.turn.trick;
                    for(var i = 0; i < trick.length; i++){
                        if(trick[i][0] !== undefined){
                            trick[i][0] = playerID;
                            trick[i][1] = cardID;
                        }
                    }
                };
            });
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
    });
    return ready;
}

function createMatch(playerList){
    //init nbTurn
    var nbTurn = 1;
    // init the stack
    var stack = initStack(NB_CARD);
    console.log(stack.toString());
    // init players
    var players = [];
    playerList.forEach(function(element){
        players.push(new Player(element.playerId, 0));
    }, this)
    var match = new Match(numGame, players, stack, new Turn(), nbTurn);
    return match;
}

function initStack(stackSize){
    var allCards = [];
    for(var i = 0; i < NB_CARD; i++){
        allCards[i] = i+1;
    }
    var stack = allCards.sort(function(){ return 0.5 - Math.random()}).slice(0, stackSize);
    return stack;
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

function sendStartTurn(game){
    // Sending players' infos (score, hand, id = username) and the narrator id
    game.playerList.forEach(function(playerInfo){
        var socket = playerInfo.socket;
        var newTurn = game.match.players.find(function(element, index, array) {
            return playerInfo.playerId === element.id;
        });
        newTurn.narrator = game.match.turn.narrator;
        socket.emit('new turn', newTurn); //TODO adapt with the protocol

    })
    console.log('start turn information sent');
}

function sendNarratorTheme(theme){
    game.playerList.forEach(function(playerInfo){
        var socket = playerInfo.socket;
        socket.emit('narrator theme', theme); //TODO adapt with the protocol

    })
    console.log('Narrator theme broadcasted');
}

module.exports = Game;