////////////////////////////////////////////////////////////	
// SET_UP_VARIABLES
////////////////////////////////////////////////////////////

var scene, camera, container, renderer, effect, stats;
var vrmanager;
var hemiLight, controls;
var screenWidth = window.innerWidth;
var screenHeight = window.innerHeight;
var time, clock;

var loadedCount = 0;

var myStartX = 0, myStartZ = -10, myStartY = 0; //2
var myPosition, myStartRotY, worldBubble, pplCount, pplCountTex, pplCountMat;

var model, texture;
var dummy;
var perlin = new ImprovedNoise(), noiseQuality = 1;

var textureLoader, loadingManger, br_mat_loadingManager, starLoadingManager;
var poop_TLM, poopHeart_TLM, graffiti_TLM, floor_TLM, door_TLM, intestine_TLM;
var poster_TLM, waterwave_TLM, glow_TLM, person_TLM, skin_TLM, particle_TLM;
var keyIsPressed;

// WAVE
	var timeWs = [0, Math.PI/2, Math.PI, -Math.PI/2, Math.PI+0.3, -Math.PI/5, Math.PI/1.1];
	var frequencyWs = [0.02, 0.01];
	var frequencyW = 0.02, amplitudeW = 0.1, offsetW = 0;
	var sinWave, sinWaves = [], cosWaves = [], tanWaves = [], spin;
	var sinWRun = [], cosWRun = [], tanWRun = [];

// RAYCAST
	var objects = [];
	var ray;
	var projector, eyerayCaster, eyeIntersects;
	var lookDummy, lookVector;

// PLAYERS
	var skinTexture;
	var guyBodyGeo, guyLAGeo, guyRAGeo, guyHeadGeo;
	var personTex;
	var player, playerBody, playerHead;
	var firstPlayer, secondPlayer;
	var firstGuy, firstGuyBody, firstGuyHead, secondGuy, secondGuyBody, secondGuyHead;
	var QforBodyRotation;
	var fGuyHandHigh = false, sGuyHandHigh = false;
	var bodyGeo;
	var dailyLifeME, colorME, dailyLifePlayers = [];
	var dailyLifePlayerDict = {};

	var person, personGeo, personMat, toiletTex, toiletMat;
	var persons = [], personIsWalking = [], personCircle, personAmount = 3;
	var personsAppeared = false, personsAnied = false, personsWalked = false;
	var personWalkTimeoutID, personAniInterval, personAniIntervalCounter=0, personAniSequence = [1,3,5,6];
	var poop, poopGeo, poopTex, poopMat, poopHat, poopHeartTex;
	var poopM, poopMGeo, poopMTex, poopMMat, poopHeartGeo, poopHeartMat, poopHeart;
	var personBody, personHead, personToilet;
	var keyframe, lastKeyframe, currentKeyframe;
	var animOffset = 1, keyduration = 28;
	var aniStep = 0, aniTime = 0, slowAni = 0.4;
	var personKeyframeSet =   [ 28, 15,  1,  8,  1, 12, 10, 1 ];
	var personAniOffsetSet = [  1, 30, 48, 50, 58, 60, 72, 82 ];	//2: sit freeze; 4: push freeze; 7: stand freeze
	var personFreeze = false;

//
	var planet;

////////////////////////////////////////////////////////////

// init();				// Init after CONNECTION
superInit();			// init automatically
// connectSocket();		// Init after superInit

///////////////////////////////////////////////////////////
// FUNCTIONS 
///////////////////////////////////////////////////////////
function superInit(){
	myColor = new THREE.Color();

	//Prevent scrolling for Mobile
	noScrolling = function(event){
		event.preventDefault();
	};

	// HOWLER
		// sound_forest = new Howl({
		// 	urls: ['../audios/duet/nightForest.mp3'],
		// 	loop: true,
		// 	volume: 0.2
		// });


	time = Date.now();

	// THREE.JS -------------------------------------------
		clock = new THREE.Clock();

	// RENDERER
		container = document.getElementById('render-canvas');
		renderer = new THREE.WebGLRenderer({antialias: true, alpha: true});

		renderer.setPixelRatio(window.devicePixelRatio);
		// renderer.setSize(window.innerWidth, window.innerHeight);

		renderer.setClearColor(0xffff00, 1);
		container.appendChild(renderer.domElement);

	// VR_EFFECT
		effect = new THREE.VREffect(renderer);
		effect.setSize(window.innerWidth, window.innerHeight);

	// Create a VR manager helper to enter and exit VR mode.
		var params = {
		  hideButton: false, // Default: false.
		  isUndistorted: false // Default: false.
		};
		vrmanager = new WebVRManager(renderer, effect, params);

	// SCENE
		scene = new THREE.Scene();
		// scene = new Physijs.Scene();
		// scene.setGravity(new THREE.Vector3( 0, -30, 0 ));

	// LIGHT
		hemiLight = new THREE.HemisphereLight( 0xf9ff91, 0x3ac5b9, 1);
		hemiLight.intensity = 1;
		scene.add(hemiLight);

	// CAMERA
		camera = new THREE.PerspectiveCamera(40, window.innerWidth / window.innerHeight, 0.1, 10000);
		camera.position.z -= 0.6;

	// RAYCASTER!
		eyerayCaster = new THREE.Raycaster();	

	// Sinwave
		sinWave = new SinWave(timeWs[0], frequencyW, amplitudeW, offsetW);

	var sphereGeo = new THREE.SphereGeometry(0.3);
	planet = new THREE.Mesh( sphereGeo, new THREE.MeshLambertMaterial() );
	planet.position.y = -2;
	scene.add( planet );

	var positiveX = new THREE.Mesh( sphereGeo, new THREE.MeshLambertMaterial({color: 0xff0000}) );
	positiveX.position.set(1,-2,0);
	scene.add( positiveX );

	var positiveZ = new THREE.Mesh( sphereGeo, new THREE.MeshLambertMaterial({color: 0x0000ff}) );
	positiveZ.position.set(0,-2,1);
	scene.add( positiveZ );
	
	//////////////////////////////////////////////////////////////////////////////////////////
	//////////////////////////////////////////////////////////////////////////////////////////
	//////////////////////////////////////////////////////////////////////////////////////////
	/*
		START LOADING	                                                          
	*/
	//////////////////////////////////////////////////////////////////////////////////////////

	loadingManger = new THREE.LoadingManager();
		loadingManger.onProgress = function ( item, loaded, total ) {
		    console.log( item, loaded, total );
		    var loadingPercentage = Math.floor(loaded/total*100);
		    // loadingTxt.innerHTML = "loading " + loadingPercentage +"%";
		    console.log("loading " + loadingPercentage +"%");
		};

		loadingManger.onError = function(err) {
			console.log(err);
		};

		loadingManger.onLoad = function () {
		    console.log( "All loaded!" );
		};

	personBody = new THREE.BoxGeometry(.5,2,.5);
	transY(personBody,-1.5);
	personHead = new THREE.BoxGeometry(1,1,1);

	stats = new Stats();
	stats.domElement.style.position = 'absolute';
	stats.domElement.style.bottom = '5px';
	stats.domElement.style.zIndex = 100;
	stats.domElement.children[ 0 ].style.background = "transparent";
	stats.domElement.children[ 0 ].children[1].style.display = "none";
	container.appendChild( stats.domElement );
	
	// EVENTS
	window.addEventListener('resize', onWindowResize, false);

	// After trigger the loading functions
	// Connect to WebSocket!
		// connectSocket();

	//
	//lateInit();
}

function AssignIndex() {
	myPosition = new THREE.Vector3( myStartX, myStartY, myStartZ );
	// console.log("Me in world: " + meInWorld + ", table: " + meInBGroup + ", seat: " + meInSGroup);
}

// lateInit() happens after click "Start"
function lateInit() 
{	
	// console.log("late init!");
	document.body.addEventListener('touchmove', noScrolling, false);
	// window.addEventListener('keydown', myKeyPressed, false);
	// window.addEventListener('keyup', myKeyUp, false);

	clock.start();

	myWorldCenter = new THREE.Vector3();

	// create controls
	controls = new THREE.DeviceControls(camera, myWorldCenter, true);
	scene.add( controls.getObject() );

	// build SELF
	myPosition = new THREE.Vector3( myStartX, myStartY, myStartZ-5 );
	firstGuy = new SimplePerson( myPosition, myColor, whoIamInLife, username );
	dailyLifePlayerDict[ whoIamInLife ] = firstGuy;

	/////////////////////////////////////////////////////
	// WebSocket ////////////////////////////////////////
	/////////////////////////////////////////////////////
	// if( !addNewPlayerYet ){
	var msg = {
		'index': whoIamInLife,
		'startX': myStartX,
		'startY': myStartY,
		'startZ': myStartZ,
		'color': myColor,
		'username': username,
		'worldId': -1
	};
	// sendMessage( JSON.stringify(msg) );
	socket.emit('create new player', msg);

	addNewPlayerYet = true;
	/////////////////////////////////////////////////////
	/////////////////////////////////////////////////////
	/////////////////////////////////////////////////////

	// TEST
	// var testGuy = new SimplePerson( myPosition, new THREE.Color(), 1, "andy" );
	// testGuy.player.position.x = 5;

	// start to animate()!
	animate(performance ? performance.now() : Date.now());

	trulyFullyStart = true;
}


// v.2
// Request animation frame loop function
var lastRender = 0;

function animate(timestamp) {
	// if(!isAllOver){
		var delta = Math.min(timestamp - lastRender, 500);
		lastRender = timestamp;

		update();
		
		// Render the scene through the manager.
		vrmanager.render(scene, camera, timestamp);
		stats.update();
	// }
	requestAnimationFrame(animate);
}


function update()
{	
	// TWEEN.update();
	controls.update( Date.now() - time );

	var dt = clock.getDelta();

	// eyeRay!
		var directionCam = controls.getDirection(1).clone();
		eyerayCaster.set( controls.position().clone(), directionCam );
		eyeIntersects = eyerayCaster.intersectObjects( scene.children, true );
		//console.log(intersects);

		if( eyeIntersects.length > 0 ){
			// var iName = eyeIntersects[ 0 ].object.name;
			// iName = iName.split(" ");
			// console.log(eyeIntersects[ 0 ].object);

			// if ( eyeIntersects[ 0 ].object == flushHandler ){
			// 	// ...
			// }

			// if ( eyeIntersects.length > 1 ) {
			// 	// ...
			// }
		} else {
			// ...
		}

	//
	time = Date.now();
}

function render() 
{	
	effect.render(scene, camera);
}

function removePlayer(playerID){
	if(dailyLifePlayerDict[playerID]){
		scene.remove( dailyLifePlayerDict[playerID].player );
		//
		delete dailyLifePlayerDict[playerID];
	}
}

function fullscreen() {
	if (container.requestFullscreen) {
		container.requestFullscreen();
	} else if (container.msRequestFullscreen) {
		container.msRequestFullscreen();
	} else if (container.mozRequestFullScreen) {
		container.mozRequestFullScreen();
	} else if (container.webkitRequestFullscreen) {
		container.webkitRequestFullscreen();
	}
}

function onWindowResize() {
	effect.setSize( window.innerWidth, window.innerHeight );
	camera.aspect = window.innerWidth / window.innerHeight;
	camera.updateProjectionMatrix();
}

function isTouchDevice() { 
	return 'ontouchstart' in window || !!(navigator.msMaxTouchPoints);
}

function loadingCount() {
	loadedCount ++;

	if(loadedCount>=8) {
		// hide the loading gif and display start link
		startLink.style.display = "";
		loadingImg.style.display = "none";
		loadingTxt.style.display = "none";
		readyToStart = true;
	}
}

function loadingCountText( item ) {
	console.log( "loaded " + item );
	loadedCount ++;

	if(loadedCount>=7) {
		// hide the loading gif and display start link
		startLink.style.display = "";
		loadingImg.style.display = "none";
		loadingTxt.style.display = "none";
		readyToStart = true;
	}
}
