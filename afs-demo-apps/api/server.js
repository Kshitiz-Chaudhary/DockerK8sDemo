var express = require('express'),
  app = express(),
  port = process.env.PORT || 8080,
  mongoose = require('mongoose'),
  jwt = require('jsonwebtoken'),
  cors = require('cors'),
  bodyParser = require('body-parser');

mongoose.Promise = global.Promise;
mongoose.connect('mongodb://localhost:27017/users'); // Note! change this connection string for deployment

app.use(bodyParser.urlencoded({ extended: true }));
app.use(bodyParser.json());

app.use(cors());

var controller = require('./controller');
app.route('/user/:username')
  .get(controller.getUser)

app.use(function(req, res) {
  res.status(404).send({url: 'go to /user/:username URL'})
});

app.listen(port);
console.log('server started on: ' + port);