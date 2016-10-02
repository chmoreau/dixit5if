var Turn = function () {};

Turn.prototype.number = {};
Turn.prototype.theme = {};
Turn.prototype.narrator = {};

Turn.prototype.trick = new Array();
for(var i = 0; i <3; i++){
        Turn.prototype.trick[i] = new Array();
    }

module.exports = new Turn();