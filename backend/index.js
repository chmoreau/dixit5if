var app = require('express')();
var http = require('http').Server(app);
var io = require('socket.io')(http);

var matchmaking = require('./matchmaking.js')(io);


app.get('/', function(req, res){
  res.sendFile(__dirname + '/index.html');
});

var server = http.listen(3000, function(){
  console.log('listening on *:3000');
});

module.exports.startServer = function(port){
  server = http.listen(port, function(){
    console.log('listening on *:'+port);
  });
}

module.exports.closeServer = function(){
  server.close();
};
