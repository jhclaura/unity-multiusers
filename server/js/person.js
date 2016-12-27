
// THE SIT PERSON OBJECT
function Person( _pos, _color, _id, _name ) {

	var scope = this;

	this.color = _color;
	this.whoIam = _id;
	this.ahhRotation;
	this.nname = _name;

	// construct physical existence
	this.player = new THREE.Object3D();
	this.pMat = new THREE.MeshLambertMaterial( { map: personTex, color: this.color, side: THREE.DoubleSide } );

	// 1-body
	this.playerBody = new THREE.Mesh( personBody, this.pMat);
	this.playerBody.name = _id + " body";

	// if it's ME, create inner poop
	if( this.whoIam == whoIamInLife ){
		this.poopMini = poop.clone();
		this.poopMini.name = "miniPoop";
		this.poopMini.scale.set(0.1,0.1,0.1);
		this.poopMini.rotation.x += Math.PI/2;
		this.poopMini.position.y -= 1.3;
		this.poopMini.position.z -= 0.1;
		this.playerBody.add( this.poopMini );

		// message_name!
		scope.wordTexture = new THREEx.DynamicTexture(1024,128);	//512,512; 1000,128
		scope.wordTexture.context.font = "bolder 70px StupidFont";
		// scope.wordTexture.clear('#dc5e64').drawText("You got a poop heart from --- <3", undefined, 96, 'white');
		scope.wordTexture.clear();
		scope.wordMaterial = new THREE.MeshBasicMaterial({map: this.wordTexture.texture, side: THREE.DoubleSide, transparent: true});
		scope.wordBubble = new THREE.Mesh(new THREE.PlaneGeometry( this.wordTexture.canvas.width, this.wordTexture.canvas.height), this.wordMaterial);
		scope.wordBubble.scale.set(0.002,0.002,0.002);
		scope.wordBubble.position.z = 3;
		scope.wordBubble.position.y = -1;
		scope.wordBubble.rotation.y = Math.PI;
		scope.wordBubble.name = "wordBubble";
		scope.playerBody.add( scope.wordBubble );
	}
	else {
		// attach toilet to body
		this.playerToilet = new THREE.Mesh( personToilet, toiletMat );
		this.playerToilet.name = _id + " toilet";
		this.playerBody.add( this.playerToilet );
	}

	// message_name!
	this.nameTexture = new THREEx.DynamicTexture(512,128);	//512,512
	this.nameTexture.context.font = "bolder 100px StupidFont";
	this.nameTexture.clear('cyan').drawText(this.nname, undefined, 96, 'red');
	this.nameMaterial = new THREE.MeshBasicMaterial({map: this.nameTexture.texture, side: THREE.DoubleSide});
	this.nameMaterial.transparent = true;
	this.nameBubble = new THREE.Mesh(new THREE.PlaneGeometry( this.nameTexture.canvas.width, this.nameTexture.canvas.height), this.nameMaterial);
	this.nameBubble.scale.set(0.005,0.005,0.005);
	this.nameBubble.position.z = -0.4;
	this.nameBubble.position.y = 2;
	this.nameBubble.name = "nameBubble";
	this.playerBody.add( this.nameBubble );

	this.player.add( this.playerBody );

	// 2-head
	// if it's ME, create empty head
	if( this.whoIam == whoIamInLife ){
		this.playerHead = new THREE.Object3D();
		this.playerHead.name = _id + " head";
	} else {
		this.playerHead = new THREE.Mesh( personHead, this.pMat );
		this.playerHead.name = _id + " head";
		
		// create poop hat
		var pHat = poopHat.clone();
		pHat.rotation.y += Math.PI;
		pHat.position.y += 0.3;
		this.playerHead.add( pHat );
	}
	this.player.add( this.playerHead );

	// 3-toilet
	// this.playerToilet = new THREE.Mesh( personToilet, toiletMat );
	// this.playerToilet.name = "toilet";
	// this.player.add( this.playerToilet );

	this.player.position.copy( _pos );

	// 3-macaronPoop
	this.poopMacaron = poopHeart.clone();	//poopM
	this.poopMacaron.scale.set(0.5,0.5,0.5);
	this.poopMacaron.visible = false;
	this.player.add( this.poopMacaron );

	//
	scene.add( this.player );
}

Person.prototype.update = function( _playerLocX, _playerLocY, _playerLocZ, _playerRotY, _playerQ ) {

	this.player.position.x = _playerLocX;
	this.player.position.y = _playerLocY;
	this.player.position.z = _playerLocZ;

	// head
	if(this.player.children[1]) {
		this.player.children[1].rotation.setFromQuaternion( _playerQ );
		// console.log(this.player.children[1].rotation);
	}
	
	// body
	_playerQ._x = 0;
	_playerQ._z = 0;
	_playerQ.normalize();
	this.ahhRotation = new THREE.Euler().setFromQuaternion( _playerQ, 'YXZ');

	if(this.player.children[0]){
		this.player.children[0].rotation.y = this.ahhRotation.y;
	}
	// if(this.player.children[2]){
	// 	this.player.children[2].rotation.y = this.ahhRotation.y;
	// }
}