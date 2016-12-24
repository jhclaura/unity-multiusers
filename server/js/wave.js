
//--------------------------------------------
//WAVE CLASS
function Wave(_time, _frequency, _amplitude, _offset){
	
	this.t = _time;
	this.frequency = _frequency;
	this.amplitude = _amplitude;
	this.offset = _offset;
}

Wave.prototype.run = function(){
	this.t += this.frequency;
	return (Math.sin(this.t)*this.amplitude+this.offset);
};

Wave.prototype.setAmp = function(_amp){
	this.amplitude = _amp;
};

Wave.prototype.setTime = function(_time){
	this.t = _time;
};

//--------------------------------------------
//SIN WAVE
function SinWave(_time, _frequency, _amplitude, _offset){
	Wave.call(this, _time, _frequency, _amplitude, _offset);
}

SinWave.prototype = Object.create(Wave.prototype);
SinWave.prototype.constructor = SinWave;

SinWave.prototype.run = function(){
	this.t += this.frequency;
	return (Math.sin(this.t)*this.amplitude+this.offset);
};

//--------------------------------------------
//COS WAVE
function CosWave(_time, _frequency, _amplitude, _offset){
	Wave.call(this, _time, _frequency, _amplitude, _offset);
}

CosWave.prototype = Object.create(Wave.prototype);
CosWave.prototype.constructor = CosWave;

CosWave.prototype.run = function(){
	this.t += this.frequency;
	return (Math.cos(this.t)*this.amplitude+this.offset);
};

//--------------------------------------------
//TAN WAVE
function TanWave(_time, _frequency, _amplitude, _offset){
	Wave.call(this, _time, _frequency, _amplitude, _offset);
}

TanWave.prototype = Object.create(Wave.prototype);
TanWave.prototype.constructor = TanWave;

TanWave.prototype.run = function(){
	this.t += this.frequency;
	return (Math.tan(this.t)*this.amplitude+this.offset);
};

//--------------------------------------------
//SAW_TOOTH WAVE
function SawWave(_time, _frequency, _amplitude, _offset){
	Wave.call(this, _time, _frequency, _amplitude, _offset);
}

SawWave.prototype = Object.create(Wave.prototype);
SawWave.prototype.constructor = SawWave;

SawWave.prototype.run = function(){
	this.t += this.frequency;
	return (((this.t)%(Math.PI*2)) * (this.amplitude+this.offset));
};

