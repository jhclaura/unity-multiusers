
// THE SIT PERSON OBJECT
function SimplePerson( _pos, _color, _id, _name ) {

	var scope = this;

	this.color = _color;
	this.whoIam = _id;
	this.ahhRotation;
	this.nname = _name;

	// for interpolation
	this.realPosition = new THREE.Vector3();
	this.realRotation = new THREE.Quaternion();
	this.realBillboardRotation = new THREE.Quaternion();
	this.yaxisInQ = new THREE.Quaternion(0,Math.PI/2,0,0);

	// construct physical existence
	this.player = new THREE.Object3D();
	this.pMat = new THREE.MeshLambertMaterial( { color: this.color, side: THREE.DoubleSide } );

	// 1-body
	this.playerBody = new THREE.Mesh( personBody, this.pMat);
	this.playerBody.name = _id + " body";

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
		
		// create eye cube
		var pEye = new THREE.Mesh( personHead, this.pMat );
		pEye.scale.set(0.8, 0.2, 0.2);
		pEye.position.set(0, 0.5, 0.5);
		this.playerHead.add( pEye );
	}
	this.player.add( this.playerHead );

	// 3-toilet
	// this.playerToilet = new THREE.Mesh( personToilet, toiletMat );
	// this.playerToilet.name = "toilet";
	// this.player.add( this.playerToilet );

	this.player.position.copy( _pos );

	// 3-macaronPoop
	// this.poopMacaron = poopHeart.clone();	//poopM
	// this.poopMacaron.scale.set(0.5,0.5,0.5);
	// this.poopMacaron.visible = false;
	// this.player.add( this.poopMacaron );

	//
	scene.add( this.player );
}

SimplePerson.prototype.update = function( _playerLocX, _playerLocY, _playerLocZ, _playerQ ) {

	var newQ = _playerQ.clone();
	
	this.player.position.x = _playerLocX;
	this.player.position.y = _playerLocY;
	this.player.position.z = _playerLocZ;

	// head
	if(this.player.children[1]) {
		this.player.children[1].rotation.setFromQuaternion( newQ );
		// console.log(this.player.children[1].rotation);
	}
	
	// body
	newQ._x = 0;
	newQ._z = 0;
	newQ.normalize();
	this.ahhRotation = new THREE.Euler().setFromQuaternion( newQ, 'YXZ');

	if(this.player.children[0]){
		this.player.children[0].rotation.y = this.ahhRotation.y;
	}
	// if(this.player.children[2]){
	// 	this.player.children[2].rotation.y = this.ahhRotation.y;
	// }
}

SimplePerson.prototype.updateU = function( _playerLocX, _playerLocY, _playerLocZ, _playerQ ) {

	this.player.position.x = _playerLocX;
	this.player.position.y = _playerLocY;
	this.player.position.z = -_playerLocZ;

	var newQ = _playerQ.clone();
	newQ._y *= -1;
	newQ._z *= -1;	

	var v = new THREE.Euler();  
	v.setFromQuaternion( newQ, 'YXZ' );
	v.y += Math.PI; // Y is 180 degrees off
	//v.z *= -1; // flip Z

	// head
	if(this.player.children[1]) {
		// this.player.children[1].rotation.setFromQuaternion( newQ );
		this.player.children[1].rotation.copy( v );
	}
	
	// body
	newQ._x = 0;
	newQ._z = 0;
	newQ.normalize();
	this.ahhRotation = new THREE.Euler().setFromQuaternion( newQ, 'YXZ');

	if(this.player.children[0]){
		this.player.children[0].rotation.y = this.ahhRotation.y + Math.PI;
	}
}

SimplePerson.prototype.updateReal = function( _playerLocX, _playerLocY, _playerLocZ, _playerQ ) {

	this.realPosition.set(_playerLocX, _playerLocY, _playerLocZ);
	this.realRotation.copy( _playerQ );

	var newQ = _playerQ.clone();
	newQ._x = 0;
	newQ._z = 0;
	newQ.normalize();
	this.realBillboardRotation.copy(newQ);
}

SimplePerson.prototype.updateRealU = function( _playerLocX, _playerLocY, _playerLocZ, _playerQ ) {

	this.realPosition.set(_playerLocX, _playerLocY, _playerLocZ * -1);

	var newQ = _playerQ.clone();
	newQ._y *= -1;
	newQ._z *= -1;
	newQ.normalize();
	var v = new THREE.Euler();  
	v.setFromQuaternion( newQ, 'YXZ' );
	v.y += Math.PI;
	this.realRotation.setFromEuler( v );

	newQ._x = 0;
	newQ._z = 0;
	newQ.normalize();
	v.setFromQuaternion( newQ, 'YXZ' );
	v.y += Math.PI;
	this.realBillboardRotation.setFromEuler( v );
}

SimplePerson.prototype.transUpdate = function() {

	this.player.position.lerp(this.realPosition, 0.1);
	
	// head
	if(this.player.children[1]) {
		this.player.children[1].quaternion.slerp( this.realRotation, 0.2 );
	}
	
	// body
	if(this.player.children[0]){
		this.player.children[0].quaternion.slerp( this.realBillboardRotation, 0.2 );
	}
}