function Turn() {
    this.theme = {};
    this.narrator = {};
    this.trick = [];
    for(var i = 0; i < 3; i++){
        this.trick[i] = [];
    }
};

module.exports = Turn;