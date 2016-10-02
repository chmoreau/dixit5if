var player = require("./Player.js");
var turn = require("./Turn.js");

var Match = function () {};

Match.prototype.id = {};
Match.prototype.players = [];
Match.prototype.stack = [];
Match.prototype.turn = turn;

module.exports = new Match();
