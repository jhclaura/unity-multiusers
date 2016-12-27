// EXPRESS_SERVER
var port = process.env.PORT || 3000;
var express = require('express');
var http = require('http');
var app = express();
var server = http.createServer(app); // a http server
var io = require('socket.io')(server); // a new instance

app.get('*', function(req, res){
	res.sendFile(__dirname + req.url); // + '/index.html'
});

server.listen(port, function(){
	console.log('Server listening at port %d', port);
});

// WebSocket Setup
var numPlayers = 0;
var allSockets = []; // for passing history players
var allPlayers = [];
// object list- index: whether it's occupied or not
var occuList = {};
// Seat_Index, default is 1
var occuIndex = 1;

io.on('connection', function(socket){
	var addedPlayer = false;

	// GET_THE_SEAT_INDEX
		// loop through all the elements in occuList
		// if there's a vacancy, get the index of the seat
		// and change the status to be occupied
		var haveEmptySeat = false;
		for( prop in occuList ){
			// if it's an empty seat
			if( occuList[prop] != "occupied" ){
				// get the index of the seat 
				occuIndex = prop;
				haveEmptySeat = true;

				// change it to be unoccupied
				occuList[prop] = "occupied";
				break;
			}
		}

		// if all the seat are already occupied
		// create a new seat and get the index
		if(!haveEmptySeat){
			occuIndex = Object.keys(occuList).length + 1;
			occuList[occuIndex] = "occupied";
		}
	
	socket.index = occuIndex;
	console.log("a user connected! index: %d", occuIndex);

	socket.on('new player', function(username){
		// if already added, ignore
		if(addedPlayer) return;

		// store username in the socket session for this client
		socket.username = username;
		++numPlayers;
		addedPlayer = true;

		// send back total player count + history players
		socket.emit('login', {
			numPlayers: numPlayers,
			//allPlayers: allPlayers,
			index: socket.index
		});

		console.log("emit login, user index: %d", socket.index);

		/* move allPlayers (history stuff) to "add new player"
		// update allPlayers after sending back history
		// to avoid adding self
		var newPlayer = {};
		newPlayer.id = socket.id;
		newPlayer.username = socket.username;
		newPlayer.numPlayers = numPlayers;
		allPlayers.push(newPlayer);

		// broadcast globally that a new player has joined
		socket.broadcast.emit('player joined', {
			username: socket.username,
			numPlayers: numPlayers,
			index: occuIndex
		});
		*/
	});

	socket.on('create new player', function(data){

		socket.emit('update history', {
			allPlayers: allPlayers,
		});

		// update allPlayers after sending back history
		// to avoid re-creating SELF
		// var newPlayer = {};
		// newPlayer.index = socket.index;
		// newPlayer.username = socket.username;
		// newPlayer.numPlayers = numPlayers;

		// to pass on the transformation data
		allPlayers.push(data); // old: allPlayers.push(newPlayer);
		// console.log("update history player");
		// console.log('after update history player, allPlayers length: ' + allPlayers.length);

		// broadcast globally that a new player has joined
		socket.broadcast.emit('player joined', {
			index: socket.index,
			username: socket.username,
			numPlayers: numPlayers,
			transform: data,
			color: data.color
		});

		console.log("emit create new player, index: %d, allPlayers lenght: %d", occuIndex, allPlayers.length);
	});

	socket.on('new message', function(data){
		socket.broadcast.emit('new message', {
			username: socket.username,
			message: data
		});
	});

	socket.on('update position', function(data){
		// socket.broadcast.emit('update position', {
		// 	username: socket.username,
		// 	message: data
		// });

		socket.broadcast.emit('update position', data);
	});

	socket.on('disconnect', function(){
		console.log('a user disconnected!');
		occuList[socket.index] = "empty";

		if(addedPlayer){
			--numPlayers;

			socket.broadcast.emit('player left', {
				index: socket.index,
				username: socket.username,
				numPlayers: numPlayers
			});

			// for(var i=0; i<allSockets.length; i++){
			// 	if(allSockets[i]==socket){
			// 		allSockets.splice(i,1);
			// 		break;
			// 	}
			// }

			// occuList[socket.index] = "empty";

			for(var j=0; j<allPlayers.length; j++){
				if(allPlayers[j].index == socket.index){
					allPlayers.splice(j,1);
					break;
				}
			}

			console.log('after removing player#' + socket.index + ', allPlayers length: ' + allPlayers.length);
		}
	});
});