var Player = require("./Player.js");
var Turn = require("./Turn.js");

var Match = function Match(id, players, stack, turn, nbTurn) {
    this.id = id;
    this.players = players;
    this.stack = stack;
    this.turn = turn;
    this.nbTurn = nbTurn;
};

module.exports = Match;
