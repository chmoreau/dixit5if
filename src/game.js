const STACK_SIZE = 20;
const HAND_SIZE = 3;

var Match = require('./models/Match.js');
var Turn = require('./models/Turn.js');
var Player = require('./models/Player.js');
var IOPlayer = require('./ioPlayers.js');
var Messages = require('./messageType');

// global counter to identify each game from their id
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
        /** PLAYER READY */
        // we doesn't prevent players when another is ready'
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
    distributeCards(this.match);
    electNarrator(this.match);
    console.log('Init match done : init hand + elect narrator');
    var game = this;
    
    /** START TURN*/
    sendStartTurn(this, ioPlayers);

    /** THEME RECEIVED */
    ioPlayers.receiveMsgFrom(this.match.turn.narrator, Messages.THEME, function(theme){
        console.log('narrator theme received');
        game.match.turn.theme = theme;
        ioPlayers.sendToAll(Messages.THEME, theme);
        console.log('Narrator theme broadcasted');
    });

    /** PLAY CARD */
    ioPlayers.receiveMsg(Messages.PLAY_CARD, function(playerID, payload){
        var playerID = playerID;
        var cardID = payload.cardID;
        var match = game.match;
        var trick = match.turn.trick;
        console.log('player ' + playerID + ' played the card ' + cardID);
        match.players.find(function(element, index, array) {
            if(element.id === playerID){
                var j = 0;
                // remove card from player's hand
                element.hand = element.hand.filter(function(element){
                    if(element == cardID){
                        return false;
                    }
                    return true;
                });
                while(trick[0][j] !== undefined){
                    j++;
                }
                trick[0][j] = playerID;
                trick[1][j] = cardID;
                ioPlayers.sendToAll(Messages.CARD_PLAYED, playerID);
            };
        });
        /** REVEAL CARD */
        if(match.players.length === trick[0].length){
            //when everyone has played, send revealed cards
            var cards = [];
            for(var i = 0; i < trick[0].length; i++){
                cards.push(trick[1][i]);
            }
            //TODO : shuffle the cards
            ioPlayers.sendToAll(Messages.REVEAL_CARD, cards);
        }
    });

    ioPlayers.receiveMsg(Messages.CARD_PICKED, function(playerId, payload){
        var playerID = playerID;
        var voteID = payload.voteID;
        var match = game.match;
        var trick = match.turn.trick;
        console.log('player ' + playerID + ' picked the card ' + voteID);
        match.players.find(function(element, index, array) {
            if(element.id === playerID){
                var j = 0;
                while(trick[0][j] !== playerID){
                    j++;
                }
                trick[2][j] = voteID;
                ioPlayers.sendToAll(Messages.CARD_PICKED, playerID);
            };
        });
        // TODO : Everyone has voted : calculate score and send informations
        if(match.players.length === trick[2].length){

        }
    });

}

/**
 * Checks if all the players are ready.
 * @return {boolean} ready
 */
function allPlayerReady(playerList){
    var ready = true;
    playerList.forEach(function(element) {
        if(element.ready !== true){
            ready = false;
        };
    });
    return ready;
}

/**
 * Init all the variables of the match
 * @param {array} playerList : List of the participating players
 * @return {object} match : Object representing the state of the match
 */
function createMatch(playerList){
    //init nbTurn
    var nbTurn = 1;
    // init the stack
    var stack = initStack(STACK_SIZE);
    console.log(stack.toString());
    // init players
    var players = [];
    playerList.forEach(function(element){
        players.push(new Player(element.playerId, 0));
    }, this)
    var match = new Match(numGame, players, stack, new Turn(), nbTurn);
    return match;
}

/**
 * Generates a random array of numbers representing the cards's id.
 * @param {number} stackSize : Number of cards to be generated
 * @return {array} stack : The generated cards ids 
 */
function initStack(stackSize){
    var allCards = [];
    for(var i = 0; i < STACK_SIZE; i++){
        allCards[i] = i+1;
    }
    var stack = allCards.sort(function(){ return 0.5 - Math.random()}).slice(0, stackSize);
    return stack;
}

/**
 * Gives each player their cards by picking from the stack
 * @param {Object} match : Object representing the state of the match
 */
function distributeCards(match){
    // init the hand of players -> use it at the start of a match
    match.players.forEach(function(player){
        console.log("hand's player "+player.id + " :");
        var cards = [];
        var playerHand = player.hand || [];
        var handSize = playerHand.length;
        for(var i = handSize; i < HAND_SIZE; i++){
            cards.push(match.stack.pop());
            console.log(cards[i]);
        }
        player.hand = cards;
    });
}

/**
 * Elect the narrator beginning from the first player to the last.
 * @param {Object} match : Object representing the state of the match 
 */
function electNarrator(match){
    match.turn.narrator = match.players[(match.nbTurn-1)%(match.players.length)].id;
}

/**
 * Informations sent :
 * - player's infos : score, hand, id
 * - narrator id
 */
function sendStartTurn(game, ioPlayers){
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