var Player = require("./Player.js");
var Turn = require("./Turn.js");

var Match = function Match(id, players, stack, turn) {
    this.id = id;
    this.players = players;
    this.stack = stack;
    this.turn = turn;
};

module.exports = Match;
