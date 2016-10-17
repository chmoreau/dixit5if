var server = require('../index.js');
var Messages = require('../messageType');

const playerIds = [1, 2];

var request = require("request");
var base_url = "http://localhost:3000"

describe("All Tests: ", function () {

    beforeAll(function () {
        server.startServer(3000);
        jasmine.DEFAULT_TIMEOUT_INTERVAL = 500; // 500ms timeout 
    });
    afterAll(function () {
        console.log('closing server');
        server.closeServer();
    });

    describe("GET /", function () {
        it("Should return status code 200", function (done) {
            request.get(base_url, function (error, response, body) {
                expect(response.statusCode).toEqual(200);
                done();
            });
        });
    });

    describe("Matchmaking: ", function () {
        var socket = require('socket.io-client')(base_url+'/matchmaking');
        var gameId;

        it("Should connect to matchmaking", function (done) {
            socket.on('connect', function () {
                done();
            });
        });

        it("Should send matchmaking request and receive a response", function (done) {
            socket.emit(Messages.JOIN_MATCHMAKING, playerIds[0]);
            socket.on(Messages.QUEUE_SIZE, function (msg) {
                expect(msg => 0).toBeTruthy();
                done();
            });
        });

        it("Should find a game when enough players are there", function(done) {
            // Add more players to start a game
            for(var i=1; i<playerIds.length; i++) {
                socket.emit(Messages.JOIN_MATCHMAKING, playerIds[i]);
            }
            socket.on(Messages.GAME_CREATED, function(msg) {
                gameId = msg;
                expect(msg).toEqual(jasmine.any(Number));
                done();
            });
        })

        it("Should start the game when everyone is ready", function() {
            socket = require('socket.io-client')(base_url+'/game/'+gameId);
            for(var i=1; i<playerIds.length; i++) {
                socket.emit(Messages.PLAYER_READY, playerIds[i]);
            }
            //TODO
        });

        

    });

});
