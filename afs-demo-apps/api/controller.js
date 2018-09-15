'use strict';

var mongoose = require('mongoose');
var user = mongoose.model('user', new mongoose.Schema({
  username: {
    type: String,
    required: 'username is required'
  },
  name: {
    type: String,
    required: 'name is required'
  },
  roles: { 
      type: [{ type: String, enum: [ 'reader', 'contributor', 'admin' ] }],
      default: 'contributor'
  },
  password_hash: {
    type: String,
    required: 'SHA256 password hash is required',
    select: false
  },
}))

var jwt = require('jsonwebtoken');

exports.getUser = function(req, res) {
  var header = req.headers['authorization'] || '',
      token = header.split(/\s+/).pop() || '';
  
  console.log("token: " + token);

  var reqUsername = req.params.username;
  console.log("requesting user details for: " + reqUsername);
  
  // only the public key is supported atm
  var jwtVerifyOptions = {
    algorithms: ['RS256'],
    audience: 'example.com',
    issuer: 'example.com'
  };

  // verify a token asymmetric 
  const fs = require('fs');
  var path = require('path').basename(__dirname);
  var cert = fs.readFileSync(path + '/app_data/jwt_1530633205_public.pem');

  var authorized;

  // we belive the claims in a signed token.
  //No database query must be required to authenticate the user and authorize against the claims in tokenb payload.
  jwt.verify(token, cert, jwtVerifyOptions, function(err, decoded) {
    console.log("decoded: " + decoded)
    console.log("err: " + err)

    if (err)
      res.send(err);

    var claimedUsername = JSON.parse(decoded.sub).Username;
    console.log("claimedUsername: " + claimedUsername);

    var roles = JSON.parse(decoded.roles);
    console.log("roles: " + roles);    

    // Validate, if username in token claims matches requested username or is 'admin' user
    // If valid, then return user details, otherwise return forbidden error
    authorized = (claimedUsername == reqUsername && roles.indexOf("reader") > -1) || (roles.indexOf("admin") > -1);
  });

  if(!authorized)
  {
    res.send("Not authorized");
    return null;
  }

  user.findOne({username : reqUsername }, function(err, obj){
    console.log('obj: ' + obj);
    console.log('err: ' + err);
    if (err)
      res.send(err);
    res.json(obj);
  });
};