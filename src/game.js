const STACK_SIZE = 36;
const HAND_SIZE = 6;

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
    this.room = 'game' + this.id;
    this.match = {};
    console.log('Game ' + this.id + ' created');

    this.waitForPlayers();
};

function function1() {
    // stuff you want to happen right away
    console.log('Welcome to My Console,');
}

function function2() {
    // all the stuff you want to happen after that pause
    console.log('Blah blah blah blah extra-blah');
}

Game.prototype.waitForPlayers = function () {
    var game = this;
    var ioPlayers = new IOPlayer(this.io, this.room, this.playerList);
    var names = '';

    /** PLAYER READY */
    // we don't prevent players when another player is ready'
    ioPlayers.receiveMsg(Messages.PLAYER_READY, function (data) {
        console.log('player ' + data + ' is ready');
        game.playerList.find(function (player, index, array) {
            if (player.playerId === data) {
                player.ready = true;
                names = names + data + ' ';
            };
        });
        if (allPlayerReady(game.playerList)) {
            console.log('Everyone is ready');

            ioPlayers.sendToAll("INFO_PLAYERS", { names: names });
            game.playGame(ioPlayers);
        }
    });
    //ioPlayers.on('disconnect', function(socket){
    //TODO
    // });
}

Game.prototype.playGame = function (ioPlayers) {

    this.match = createMatch(this.playerList);
    this.newTurn(ioPlayers);
}

Game.prototype.newTurn = function (ioPlayers) {
    this.match.nbTurn++;
    this.match.turn = new Turn();
    distributeCards(this.match);
    electNarrator(this.match);
    // Send new turn infos
    sendStartTurn(this, ioPlayers);

    var game = this;

    // Receive the theme from the narrator
    ioPlayers.receiveMsgFrom(this.match.turn.narrator, Messages.THEME, function (msg) {
        var theme = msg.theme;
        console.log('theme: ' + theme);
        game.match.turn.theme = theme;
        ioPlayers.sendToAll(Messages.THEME, { theme: theme });
    });

    // Receive the card pick from each player
    ioPlayers.receiveMsg(Messages.PLAY_CARD, function (playerID, msg) {
        var cardID = msg.cardID;
        var match = game.match;
        var trick = match.turn.trick;
        console.log('player ' + playerID + ' played the card ' + cardID);
        match.players.find(function (player, index, array) {
            if (player.id === playerID) {
                var j = 0;
                // remove card from player's hand
                player.hand = player.hand.filter(function (card) {
                    if (card == cardID) {
                        return false;
                    }
                    return true;
                });
                var playerTrick = { playerID: playerID, cardPlayed: cardID, score: 0 };
                trick.push(playerTrick);
                ioPlayers.sendToAll(Messages.CARD_PLAYED, { playerID: playerID });
            };
        });
        /** REVEAL CARD */
        if (match.players.length === trick.length) {
            var cards = [];
            trick.forEach(function (element) {
                cards.push(element.cardPlayed);
            });
            cards = cards.sort(function () { return 0.5 - Math.random() });
            var i = 0;
            var reveal = '';
            for(i = 0; i < cards.length; ++i ) {
                reveal = reveal + cards[i] + ',';
            }
            if(i === cards.length) {
                ioPlayers.sendToAll(Messages.REVEAL_CARDS, { cards: reveal });
            }
        }
    });

    ioPlayers.receiveMsg(Messages.PICK_CARD, function (playerID, msg) {
        if (playerID != game.match.turn.narrator) {
            var voteID = msg.voteID;
            var match = game.match;
            var trick = match.turn.trick;

            //TODO : check if the card picked by the player isn't his own card

            console.log('player ' + playerID + ' picked the card ' + voteID);
            match.players.find(function (player) {
                if (player.id === playerID) {
                    var idx = 0;
                    trick.find(function (playerTrick, index, array) {
                        if (playerTrick.playerID == player.id) {
                            playerTrick.cardPicked = voteID;
                        };
                    });
                    ioPlayers.sendToAll(Messages.CARD_PICKED, { playerID: playerID });
                };
            });

            if (allPlayersHaveVoted(trick, match.turn.narrator)) {
                calculteScores(match.turn);
                console.log("scores of the turn calculated");
                /**TRICK */

                match.turn.trick.forEach(function(element) {
                    ioPlayers.sendToAll(Messages.TRICK, element);
                });
             //   ioPlayers.sendToAll(Messages.TRICK, { trick: match.turn.trick });
                updatePlayersScores(match);
                console.log("players' scores updated");
                var scores = getScores(match.players);
                var aux = 0;
                if (match.stack.length < match.players.length && match.players[0].hand.length === 0) {
                    /**GAME_OVER */
                    scores.forEach(function(Element) {
                        ioPlayers.sendToAll(Messages.GAME_OVER,  Element );
                    });
                    
                } else {
                    /**NEW_TURN */
                     scores.forEach(function(Element) {
                        ioPlayers.sendToAll(Messages.NEW_TURN,  Element );
                    });
                    ioPlayers.receiveMsg(Messages.READY_FOR_NEXT, function() {
                        aux++;
                        console.log(aux + " " + match.players.length);
                        if(aux == match.players.length) {
                            game.newTurn(ioPlayers);
                        }
                    });
                 
                }

            }
        }
    });
}

/**
 * Update players scores
 * @param {Object} match : Object representing the state of the match
 */
function updatePlayersScores(match) {
    var players = match.players;
    var trick = match.turn.trick;
    players.forEach(function (player) {
        trick.find(function (playerTrick, index, array) {
            if (playerTrick.playerID == player.id) {
                player.score += playerTrick.score;
            };
        });
    });
    match.players = players;
}

/**
 * Get players score
 * @return {array} scores
 */
function getScores(players) {
    var scores = [];
    players.forEach(function (player) {
        var score = {};
        score.player = player.id;
        score.value = player.score;
        scores.push(score);
    });
    return scores;
}

/**
 * Checks if all the players have voted
 * @return {boolean} haveVoted
 */
function allPlayersHaveVoted(trick, narratorID) {
    var haveVoted = true;
    trick.forEach(function (playerTrick) {
        if (playerTrick.playerID != narratorID && playerTrick.cardPicked === undefined) {
            haveVoted = false;
        }
    });
    return haveVoted;
}

/**
 * Update the trick with scores updated
 * @param {object} turn : Object representing the turn of the match
 */
function calculteScores(turn) {
    var narratorID = turn.narrator;
    var trick = turn.trick;
    var narratorCard = getNarratorCard(trick, narratorID);
    var nbNarrVote = getNbNarratorVote(trick, narratorCard);

    if (nbNarrVote === (trick.length - 1 || 0)) {
        // everyone or nobody find the narrator's card
        // +2 for all players except the narrator (too easy/too hard)
        trick.forEach(function (playerTrick, index, array) {
            if (playerTrick.playerID !== narratorID) {
                playerTrick.score += 2;
            };
        });
        console.log("+2 for everyone");
    } else {
        // some players find narrator's card
        trick.forEach(function (playerTrick, index, array) {
            if (playerTrick.playerID == narratorID) {
                // narrator succed
                playerTrick.score += 3;
            } else {
                // if the player choosed the narrator'card : +3
                if (playerTrick.score.cardPicked == narratorCard) {
                    playerTrick.score += 3;
                }
                // if others players choosed the player's card, he gets +1
                var nbPlayerCardVote = 0; // nb of votes for the player card
                trick.forEach(function (pTrick) {
                    if (pTrick.playerID !== playerTrick.playerID && pTrick.cardPicked == playerTrick.cardPlayed) {
                        nbPlayerCardVote++;
                    };
                });
                playerTrick.score += nbPlayerCardVote;
            };
        });
    }
    turn.trick = trick;
}

/**
 * Gives the card picked by the narrator
 * @return {number} cardID
 */
function getNarratorCard(trick, narratorID) {
    var cardID = -1;
    trick.find(function (playerTrick, index, array) {
        if (playerTrick.playerID === narratorID) {
            cardID = playerTrick.cardPlayed;
        };
    });
    return cardID;
}

/**
 * Gives the number of votes for the narrator's card
 * @return {number} nbVoteNarr
 */
function getNbNarratorVote(trick, narratorCard) {
    var nbVoteNarr = 0;
    trick.forEach(function (playerTrick, index, array) {
        if (playerTrick.cardPicked == narratorCard) {
            nbVoteNarr++;
        };
    });
    return nbVoteNarr;
}

/**
 * Checks if all the players are ready.
 * @return {boolean} ready
 */
function allPlayerReady(playerList) {
    var ready = true;
    playerList.forEach(function (playerInfo) {
        if (playerInfo.ready !== true) {
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
function createMatch(playerList) {
    //init nbTurn
    var nbTurn = 1;
    // init the stack
    var stack = initStack(STACK_SIZE);
    console.log(stack.toString());
    // init players
    var players = [];
    playerList.forEach(function (playerInfo) {
        players.push(new Player(playerInfo.playerId, 0));
    }, this)
    var match = new Match(numGame, players, stack, new Turn(), nbTurn);
    return match;
}

/**
 * Generates a random array of numbers representing the cards's id.
 * @param {number} stackSize : Number of cards to be generated
 * @return {array} stack : The generated cards ids 
 */
function initStack(stackSize) {
    var allCards = [];
    for (var i = 0; i < STACK_SIZE; i++) {
        allCards[i] = i + 1;
    }
    var stack = allCards.sort(function () { return 0.5 - Math.random() }).slice(0, stackSize);
    return stack;
}

/**
 * Gives each player their cards by picking from the stack
 * @param {Object} match : Object representing the state of the match
 */
function distributeCards(match) {
    // init the hand of players -> use it at the start of a match
    if (match.stack.length >= match.players.length) {
        match.players.forEach(function (player) {
            console.log("hand's player " + player.id + " :");
            var cards = [];
            var playerHand = player.hand || [];
            var handSize = playerHand.length;
            for (var i = handSize; i < HAND_SIZE; i++) {
                cards.push(match.stack.pop());
            }
            player.hand = playerHand.concat(cards);
            for (var i = 0; i < HAND_SIZE; i++) {
                console.log(player.hand[i]);
            }
        });
    }

}

/**
 * Elect the narrator beginning from the first player to the last.
 * @param {Object} match : Object representing the state of the match 
 */
function electNarrator(match) {
    match.turn.narrator = match.players[(match.nbTurn - 1) % (match.players.length)].id;
}

/**
 * Informations sent :
 * - player's infos : score, hand, id
 * - narrator id
 */
function sendStartTurn(game, ioPlayers) {
    game.playerList.forEach(function (playerInfo) {
        var newTurn = game.match.players.find(function (player, index, array) {
            return playerInfo.playerId === player.id;
        });
        newTurn.narrator = game.match.turn.narrator;
        var hand = Object.assign([], newTurn.hand);
        newTurn.hand = newTurn.hand.toString();
        console.log(newTurn);
        ioPlayers.sendToPlayer(playerInfo.playerId, Messages.START_TURN, newTurn);
        newTurn.hand = hand;
    })
    console.log('start turn information sent');
}

module.exports = Game;