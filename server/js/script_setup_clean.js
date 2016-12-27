
WebVRConfig = {
	MOUSE_KEYBOARD_CONTROLS_DISABLED: true, // Default: false.
	BUFFER_SCALE: 0.5, // Default: 1.0.
	// PREVENT_DISTORTION: true,
	predistorted: true,
	DIRTY_SUBMIT_FRAME_BINDINGS: true, // Polyfill optimizations
	// isUndistorted: false
	TOUCH_PANNER_DISABLED: true,
};

// PointerLockControls
// http://www.html5rocks.com/en/tutorials/pointerlock/intro/
	var element = document.body;
	var blocker, instructions;

	var havePointerLock = 
				'pointerLockElement' in document || 
				'mozPointerLockElement' in document || 
				'webkitPointerLockElement' in document;

	if( !isMobile ) {
		if ( havePointerLock ) {
			blocker = document.getElementById('blocker');
			instructions = document.getElementById('instructions');
		
			var pointerlockchange = function ( event ) {

				if ( document.pointerLockElement === element || document.mozPointerLockElement === element || document.webkitPointerLockElement === element ) {
					// console.log("enable pointerControls");

					controls.enabled = true;
					// console.log("controls.enabled = true");
					blocker.style.display = 'none';

				} else {
					controls.enabled = false;
					// console.log("controls.enabled = false");
					blocker.style.display = '-webkit-box';
					blocker.style.display = '-moz-box';
					blocker.style.display = 'box';

					instructions.style.display = '';
				}
			}

			var pointerlockerror = function(event){
				instructions.style.display = '';
			}

			// Hook pointer lock state change events
			document.addEventListener( 'pointerlockchange', pointerlockchange, false );
			document.addEventListener( 'mozpointerlockchange', pointerlockchange, false );
			document.addEventListener( 'webkitpointerlockchange', pointerlockchange, false );

			document.addEventListener( 'pointerlockerror', pointerlockerror, false );
			document.addEventListener( 'mozpointerlockerror', pointerlockerror, false );
			document.addEventListener( 'webkitpointerlockerror', pointerlockerror, false );

			instructions.addEventListener( 'click', funToCall, false );
		} else {
				instructions.innerHTML = 'Your browser doesn\'t seem to support Pointer Lock API';
		}
	}

	function funToCall(event){
		// console.log("click or touch!");
		instructions.style.display = 'none';

		// Ask the browser to lock the pointer
		element.requestPointerLock = element.requestPointerLock || element.mozRequestPointerLock || element.webkitRequestPointerLock;

		controls.enabled = true;

		element.requestPointerLock();
	}