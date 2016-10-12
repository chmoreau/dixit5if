var server = require('../index.js');

var request = require("request");
var base_url = "http://localhost:3000/"

describe("Hello World Server", function() {
  describe("GET /", function() {
    it("returns status code 200", function(done) {
      request.get(base_url, function(error, response, body) {
          expect(response.statusCode).toBe(200);
          done();
      });
    });
  });
});

describe("Test débile comme Charles :", function() {

  it("contains spec with an expectation", function() {
    expect(true).toBe(true);
  });
});