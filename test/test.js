var assert = require('assert');

var server = require('../src/index.js');
var Messages = require('../src/messageType');

const playerIds = [1, 2];
var gameId;

var request = require("../src/node_modules/request");
var base_url = "http://localhost:3000"

var code;

describe('Playing a simple game:', function () {
    this.timeout(500);

    after(function () {
        server.closeServer();
    });

    describe('matchmaking phase', function () {
        var socket;
        before(function () {
            socket = require('../src/node_modules/socket.io-client')(base_url + '/matchmaking');
        });

        after(function () {
            socket.disconnect();
        });

        it("Should connect to matchmaking", function (done) {
            socket.once('connect', function () {
                done();
            });
        });

        it("Should send matchmaking request and receive the queue size", function (done) {
            socket.emit(Messages.JOIN_MATCHMAKING, playerIds[0]);
            socket.once(Messages.QUEUE_SIZE, function (msg) {
                // expect(msg => 0).toBeTruthy();
                done();
            });
        });

        it("Should find a game when enough players are there", function (done) {
            // Add more players to start a game
            for (var i = 1; i < playerIds.length; i++) {
                socket.emit(Messages.JOIN_MATCHMAKING, playerIds[i]);
            }
            socket.on(Messages.GAME_CREATED, function (msg) {
                gameId = msg;
                assert.equal(gameId, 1);
                done();
            });
        })
    });

    describe('game phase', function () {
        var socket;
        before(function () {
            assert.equal(gameId, 1);
            socket = require('../src/node_modules/socket.io-client')(base_url + '/game/' + gameId);
        });

        after(function () {
            socket.disconnect();
        });

        // Might not work
        playerIds.forEach(function (playerId) {
            it("Should notify everybody when a player is ready", function () {
                socket.emit(Messages.PLAYER_READY, playerId);
              /*  var responseCount = 0; // All players should be notified when a player is ready
                socket.on(Messages.PLAYER_READY, function () {
                    if (++responseCount === playerIds.length) {
                    
                    }
                });*/
            });
        })

        it("Should receive cards", function (done) {
            socket.on(Messages.CARDS, function (cards) {
                done();
            })
        });

        it("Should receive narrator ID");

        it("Should send theme");

        it("Should receive theme");

        it("Should be notified when a player has played a card");

        it("Should receive revealed cards if everyone has played");

        it("Should be notified when a player has voted for a card");

        it("Should receive the trick when everyone has voted");

        it("Should receive scores if a new turn start");

        it("Should receive scores when the game is over");
    });
});


