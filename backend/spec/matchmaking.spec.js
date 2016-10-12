var server = require('../index.js');

var request = require("request");
var base_url = "http://localhost:3000/"

describe("All Tests", function(){

    beforeAll(function(){
        server.startServer(3000);
    });
    afterAll(function(){
        console.log('closing server');
        server.closeServer();
    });
    describe("GET /", function() {
        it("returns status code 200", function(done) {
            request.get(base_url, function(error, response, body) {
                expect(response.statusCode).toBe(200);
                done();
            });
        });
    });

    describe("Get socket :", function(){
        var socket = require('socket.io-client')('http://localhost:3000/matchmaking');
        it("connection to matchmaking", function(done){
            socket.on('connect', function(){
                console.log()
                done();
            });
        });
        it("matchmaking request", function(done){
            socket.emit('matchmaking request', "playerIdTest");
            socket.on('queueSize', function(msg){
                console.log(msg);
                done();
            });
        });


    });

});
