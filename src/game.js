const NB_CARD = 20;
const HAND_SIZE = 3;

var Match = require('./models/Match.js');
var Turn = require('./models/Turn.js');
var Player = require('./models/Player.js');
var IOPlayer = require('./ioPlayers.js');
var Messages = require('./messageType');

var numGame = 0;

function Game(io, playerList) {
    this.io = io;
    this.playerList = playerList;
    this.id = ++numGame;
    this.room = io.of('/game/'+ this.id);
    this.match = {};
    console.log('Game ' + this.id + ' created');
    this.waitForPlayers();
};

Game.prototype.waitForPlayers = function(){
    var game = this;
    
    this.room.on('connection', function(socket){
        socket.on(Messages.PLAYER_READY, function(playerID){
            console.log('player ' + playerID + ' is ready');
            game.playerList.find(function(element, index, array) {
                if(element.playerId === playerID){
                    element.socket = socket;
                    element.ready = true;
                };
            });
            if(allPlayerReady(game.playerList)){
                console.log('Everyone is ready');
                game.playGame();                
            }
        });
        socket.on('disconnect', function(socket){
            //TODO
        });
    });
}

Game.prototype.playGame = function(){
    ioPlayers = new IOPlayer(this.room, this.playerList);
    this.match = createMatch(this.playerList);
    initHands(this.match);
    electNarrator(this.match);
    console.log('Init match done : init hand + elect narrator');
    var game = this;
    sendStartTurn(this, ioPlayers);

    ioPlayers.receiveMsgFrom(this.match.turn.narrator, Messages.THEME, function(theme){
        console.log('narrator theme received');
        match.turn.theme = theme;
        ioPlayers.sendToAll(Messages.THEME, theme);
        console.log('Narrator theme broadcasted');
    });

    ioPlayers.receiveMsg(Messages.PLAY_CARD, function(playerID, payload){
        var playerID = playerID;
        var cardID = payload.cardID;
        var match = game.match;
        var trick = match.turn.trick;
        console.log('player ' + playerID + ' played the card ' + cardID);
        match.players.find(function(element, index, array) {
            if(element.id === playerID){
                var j = 0;
                while(trick[0][j] !== undefined){
                    j++;
                }
                trick[0][j] = playerID;
                trick[1][j] = cardID;
                ioPlayers.sendToAll(Messages.CARD_PLAYED, element.id);
            };
        });
        if(match.players.length === trick[0].length){
            //if all played, send revealed cards
            var cards = [];
            for(var i = 0; i < trick[0].length; i++){
                cards.push(trick[1][i]);
            }
            //TODO : mélanger cartes
            ioPlayers.sendToAll(Messages.REVEAL_CARD, cards);
        }
    });

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

function sendStartTurn(game, ioPlayers){
    // Sending players' infos (score, hand, id = username) and the narrator id
    game.playerList.forEach(function(playerInfo){
        var newTurn = game.match.players.find(function(element, index, array) {
            return playerInfo.playerId === element.id;
        });
        newTurn.narrator = game.match.turn.narrator;
        ioPlayers.sendToPlayer(playerInfo.playerId, Messages.START_TURN, newTurn);

    })
    console.log('start turn information sent');
}

module.exports = Game;