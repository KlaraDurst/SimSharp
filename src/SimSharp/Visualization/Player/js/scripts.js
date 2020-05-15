var svgDocument;
var svgns;
var timePerStep;
var timePerFrame;
var it;
var timeWhenLastUpdate;
var timeFromLastUpdate;
var requestId;
var btnIcon;
var slider;
var textInput;
var numberInput;
var prevSliderVal;
var wasPlaying;
var totalFrameNumber;

function init() {
  let json = JSON.parse(data);
  let frames = json["frames"];

  svgDocument = document.getElementById('canvas');
  btnIcon = document.getElementById('btnIcon')
  slider = document.getElementById('slider');
  textInput = document.getElementById('textInput');
  numberInput = document.getElementById('numberInput');
  prevSliderVal = 0;
  svgns = "http://www.w3.org/2000/svg";
  timePerStep = json["timeStep"] * 1000;
  timePerFrame = timePerStep;
  speedStep = 100;
  it = makeFrameIterator(frames);
  requestId = undefined;
  totalFrameNumber = frames.length;

  if (json.hasOwnProperty('width')) {
    if (json.hasOwnProperty('height')) {
      svgDocument.setAttribute('width', json['width']);
      svgDocument.setAttribute('height', json['height']);
      if (json.hasOwnProperty('startX')) {
        if (json.hasOwnProperty('startY'))
          svgDocument.setAttribute('viewBox', json['startX'].toString().concat(" ", json['startY'].toString(), " ", json['width'].toString(), " ", json['height'].toString()));
      }
    }
  }

  slider.setAttribute('max', totalFrameNumber);
  slider.addEventListener('mousedown', () => {
    if (!requestId)
      wasPlaying = false;
    else 
      wasPlaying = true;
    stop();
  });
  slider.addEventListener('mouseup', () => {
    if (wasPlaying) 
      start();
  })
  slider.addEventListener('change', () => {
    let sliderDiff = slider.value - prevSliderVal;
    textInput.value = msToTime(slider.value * timePerStep);
    for (let i = 0; i < sliderDiff; i++) {
      animate();
    }

    if (prevSliderVal > slider.value) {
      svgDocument.textContent = '';
      it = makeFrameIterator(frames);
      for (let i = 0; i < slider.value; i++) {
        animate();
      }
    }

    prevSliderVal = slider.value;
  })

  numberInput.max = timePerFrame / speedStep;
  numberInput.addEventListener('input', () => {
    if (numberInput.value >= 0) {
      timePerFrame = timePerStep - speedStep * numberInput.value;
    }
    else if (0 > numberInput.value){
      timePerFrame = timePerStep + speedStep * numberInput.value * -1;
    }
  }) 
}

function start() {
  if (slider.value < totalFrameNumber) {
    btnIcon.setAttribute('class', 'fa fa-pause');
    requestId = window.requestAnimationFrame(increment);
  }
}

function stop() {
  btnIcon.setAttribute('class', 'fa fa-play');
  window.cancelAnimationFrame(requestId);
  requestId = undefined;
}

function toggle() {
  if (!requestId)
    start();
  else  
    stop();
}

function increment(timestamp) {
  if (!timeWhenLastUpdate)
    timeFromLastUpdate = timePerFrame + 1;
  else
    timeFromLastUpdate = timestamp - timeWhenLastUpdate;

  if (timeFromLastUpdate > timePerFrame) {
    slider.stepUp();
    prevSliderVal++;
    animate();
    textInput.value = msToTime(prevSliderVal * timePerStep);
    timeWhenLastUpdate = timestamp;
  }
  
  if (slider.value >= totalFrameNumber)
    stop();
  else 
    requestId = window.requestAnimationFrame(increment);
}

function animate() {
  let result = it.next();
  if (!result.done) {
    Object.keys(result.value).forEach(e => updateObj(e, result.value[e]));
  }
}

function updateObj(key, value) {
  let shape = document.getElementById(key);
  if (shape == null) {
    shape = document.createElementNS(svgns, value['type']);
    shape.setAttribute("id", key);
    svgDocument.appendChild(shape);
  }
  Object.keys(value).forEach(e => updateAttr(shape, e, value[e]));
}

function updateAttr(shape, key, value) {
  if (key == 'visibility') {
    if (value)
      value = 'visible';
    else
      value = 'hidden';
  }
  shape.setAttribute(key, value);
}

function makeFrameIterator(frames) {
  let nextIndex = 0;

  const frameIterator = {
    next: function () {
      let result;
      if (frames.length > nextIndex) {
        result = { value: frames[nextIndex], done: false }
        nextIndex += 1;
        return result;
      }
      return { value: frames.length, done: true }
    }
  };
  return frameIterator;
}

function msToTime(s) {
  function pad(n, z) {
    z = z || 2;
    return ('00' + n).slice(-z);
  }

  var ms = s % 1000;
  s = (s - ms) / 1000;
  var secs = s % 60;
  s = (s - secs) / 60;
  var mins = s % 60;
  var hrs = (s - mins) / 60;

  return pad(hrs) + ':' + pad(mins) + ':' + pad(secs) + '.' + pad(ms, 3);
}

data = '{"name":"Gas Station Refuelling","timeStep":1.0,"width":1000,"height":1000,"startX":200,"startY":200,"frames":[{"fuelPumpTank":{"type":"rect","fill":"black","stroke":"black","strokeWidth":1,"visibility":true,"x":400,"y":650,"width":250,"height":200},"fuelPump":{"type":"rect","fill":"none","stroke":"black","strokeWidth":1,"visibility":true,"x":400,"y":650,"width":250,"height":200},"gasStationRight":{"type":"rect","fill":"grey","stroke":"grey","strokeWidth":1,"visibility":true,"x":500,"y":400,"width":50,"height":100},"gasStationLeft":{"type":"rect","fill":"grey","stroke":"grey","strokeWidth":1,"visibility":true,"x":300,"y":400,"width":50,"height":100}},{},{},{},{},{},{"Car1Tank":{"type":"rect","fill":"yellow","stroke":"yellow","strokeWidth":1,"visibility":true,"x":275,"y":275,"width":0,"height":50},"Car1":{"type":"rect","fill":"none","stroke":"yellow","strokeWidth":1,"visibility":true,"x":293,"y":275,"width":36,"height":50},"fuelPumpTank":{"y":632,"height":164}},{"Car1Tank":{"x":276,"width":2}},{"Car1Tank":{"x":277,"width":4}},{"Car1Tank":{"x":278,"width":6}},{"Car1Tank":{"x":279,"width":8}},{"Car1Tank":{"x":280,"width":11}},{"Car1Tank":{"x":281,"width":13}},{"Car1Tank":{"x":282,"width":15}},{"Car1Tank":{"x":283,"width":17}},{"Car1Tank":{"x":285,"width":19}},{"Car1Tank":{"x":286,"width":21}},{"Car1Tank":{"x":287,"width":23}},{"Car1Tank":{"x":288,"width":25}},{"Car1Tank":{"x":289,"width":28}},{"Car1Tank":{"x":290,"width":30}},{"Car1Tank":{"x":291,"width":32}},{"Car1Tank":{"x":292,"width":34}},{"Car1Tank":{"x":293,"width":36}},{"Car1Tank":{"visibility":false},"Car1":{"visibility":false}},{},{},{},{},{},{"Car2Tank":{"type":"rect","fill":"yellow","stroke":"yellow","strokeWidth":1,"visibility":true,"x":275,"y":275,"width":0,"height":50},"Car2":{"type":"rect","fill":"none","stroke":"yellow","strokeWidth":1,"visibility":true,"x":287,"y":275,"width":25,"height":50},"fuelPumpTank":{"y":620,"height":140}},{"Car2Tank":{"x":276,"width":2}},{"Car2Tank":{"x":277,"width":4}},{"Car2Tank":{"x":278,"width":6}},{"Car2Tank":{"x":279,"width":8}},{"Car2Tank":{"x":280,"width":10}},{"Car2Tank":{"x":281,"width":12}},{"Car2Tank":{"x":282,"width":15}},{"Car2Tank":{"x":283,"width":17}},{"Car2Tank":{"x":284,"width":19}},{"Car2Tank":{"x":285,"width":21}},{"Car2Tank":{"x":286,"width":23}},{"Car2Tank":{"visibility":false},"Car2":{"visibility":false}},{},{},{},{},{},{"Car3Tank":{"type":"rect","fill":"yellow","stroke":"yellow","strokeWidth":1,"visibility":true,"x":275,"y":275,"width":0,"height":50},"Car3":{"type":"rect","fill":"none","stroke":"yellow","strokeWidth":1,"visibility":true,"x":289,"y":275,"width":28,"height":50},"fuelPumpTank":{"y":606,"height":111}},{"Car3Tank":{"x":276,"width":2}},{"Car3Tank":{"x":277,"width":4}},{"Car3Tank":{"x":278,"width":6}},{"Car3Tank":{"x":279,"width":8}},{"Car3Tank":{"x":280,"width":10}},{"Car3Tank":{"x":281,"width":12}},{"Car3Tank":{"x":282,"width":14}},{"Car3Tank":{"x":283,"width":16}},{"Car3Tank":{"x":284,"width":18}},{"Car3Tank":{"x":285,"width":20}},{"Car3Tank":{"x":286,"width":22}},{"Car3Tank":{"x":287,"width":24}},{"Car3Tank":{"x":288,"width":26}},{"Car3Tank":{"visibility":false},"Car3":{"visibility":false}},{},{},{},{},{},{"Car4Tank":{"type":"rect","fill":"yellow","stroke":"yellow","strokeWidth":1,"visibility":true,"x":275,"y":275,"width":0,"height":50},"Car4":{"type":"rect","fill":"none","stroke":"yellow","strokeWidth":1,"visibility":true,"x":292,"y":275,"width":35,"height":50},"fuelPumpTank":{"y":588,"height":76}},{"Car4Tank":{"x":276,"width":2}},{"Car4Tank":{"x":277,"width":4}},{"Car4Tank":{"x":278,"width":6}},{"Car4Tank":{"x":279,"width":8}},{"Car4Tank":{"x":280,"width":10}},{"Car4Tank":{"x":281,"width":12}},{"Car4Tank":{"x":282,"width":14}},{"Car4Tank":{"x":283,"width":16}},{"Car4Tank":{"x":284,"width":19}},{"Car4Tank":{"x":285,"width":21}},{"Car4Tank":{"x":286,"width":23}},{"Car4Tank":{"x":287,"width":25}},{"Car4Tank":{"x":288,"width":27}},{"Car4Tank":{"x":289,"width":29}},{"Car4Tank":{"x":290,"width":31}},{"Car4Tank":{"x":291,"width":33}},{"Car4Tank":{"x":292,"width":35}},{"Car4Tank":{"visibility":false},"Car4":{"visibility":false}},{},{},{},{},{},{"Car5Tank":{"type":"rect","fill":"yellow","stroke":"yellow","strokeWidth":1,"visibility":true,"x":275,"y":275,"width":0,"height":50},"Car5":{"type":"rect","fill":"none","stroke":"yellow","strokeWidth":1,"visibility":true,"x":290,"y":275,"width":30,"height":50},"fuelPumpTank":{"y":573,"height":46}},{"Car5Tank":{"x":276,"width":2}},{"Car5Tank":{"x":277,"width":4}},{"Car5Tank":{"x":278,"width":6}},{"Car5Tank":{"x":279,"width":8}},{"Car5Tank":{"x":280,"width":10}},{"Car5Tank":{"x":281,"width":12}},{"Car5Tank":{"x":282,"width":14}},{"Car5Tank":{"x":283,"width":16}},{"Car5Tank":{"x":284,"width":18}},{"Car5Tank":{"x":285,"width":20}},{"Car5Tank":{"x":286,"width":22}},{"Car5Tank":{"x":287,"width":24}},{"Car5Tank":{"x":288,"width":26}},{"Car5Tank":{"x":289,"width":28}},{"Car5Tank":{"visibility":false},"Car5":{"visibility":false}}]}'