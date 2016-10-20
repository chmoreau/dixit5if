var server = require('../src/index.js');
var Messages = require('../src/messageType');

const playerIds = [1, 2];
var gameId;

var request = require("../src/node_modules/request");
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
        var previousItDone = true;
        beforeEach(function(done){
            setTimeout(function() {
                done();
            }, 500);
        });

        var socket = require('../src/node_modules/socket.io-client')(base_url+'/matchmaking');

        it("Should connect to matchmaking", function (done) {
            socket.once('connect', function () {
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

        

    });

    describe("Game: ", function(){
        var socket = require('../src/node_modules/socket.io-client')(base_url+'/game/'+gameId);

        it("Should start the game when everyone is ready", function() {
            var responseCount = 0;
            for(var i=1; i<playerIds.length; i++) {
                socket.emit(Messages.PLAYER_READY, playerIds[i]);
                socket.on(Messages.PLAYER_READY, function(){
                    responseCount++;
                    console.log(responseCount);
                })
            }
            

        });

        it("Should receive cards", function(){

        });

        it("Should receive narrator ID", function(){

        });

        it("Should send theme", function(){

        });

        it("Should receive theme", function(){

        });

        it("Should be notified when a player has played a card", function(){

        });

        it("Should receive revealed cards if everyone has played", function(){

        });

        it("Should be notified when a player has voted for a card", function(){

        });

        it("Should receive the trick when everyone has voted", function(){

        });

        it("Should receive scores if a new turn start", function(){

        });

        it("Should receive scores when the game is over", function(){

        });

    });
});
