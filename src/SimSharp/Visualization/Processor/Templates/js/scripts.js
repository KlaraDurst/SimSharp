var svgDocument;
var svgns;
var frames;
var timePerStep;
var timePerFrame;
var it;
var timeWhenLastUpdate;
var timeFromLastUpdate;
var requestId;
var playBtn;
var slider;
var textInput;
var numberInput;
var prevSliderVal;
var wasPlaying;
var totalFrameNumber;

function init() {
  let json = JSON.parse(data);

  frames = json["frames"];
  svgDocument = document.getElementById('canvas');
  playBtn = document.getElementById('playBtn');
  slider = document.getElementById('slider');
  textInput = document.getElementById('textInput');
  numberInput = document.getElementById('numberInput');
  prevSliderVal = 0;
  svgns = "http://www.w3.org/2000/svg";
  timePerStep = 1000 / json["fps"];
  timePerFrame = timePerStep;
  speedStep = 100;
  it = makeFrameIterator();
  requestId = undefined;
  totalFrameNumber = frames.length;

  if (json.hasOwnProperty('width')) {
    if (json.hasOwnProperty('height')) {
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
    if (wasPlaying && parseInt(slider.value) < totalFrameNumber)
      start();
  })
  slider.addEventListener('change', () => {
    let sliderInt = parseInt(slider.value);
    let sliderDiff = sliderInt - prevSliderVal;
    textInput.value = msToTime(sliderInt * timePerStep);

    if (prevSliderVal < sliderInt) {
      animate();
      slider.stepUp();
      prevSliderVal++;
      for (let i = 0; i < sliderDiff; i++)
        animate();
    }

    if (prevSliderVal > sliderInt) {
      reset();
      for (let i = 0; i < sliderInt; i++)
        animate();
    }

    prevSliderVal = parseInt(slider.value);
  })

  numberInput.max = timePerFrame / speedStep;
  numberInput.addEventListener('input', () => {
    let numberInputInt = parseInt(numberInput.value);
    if (numberInputInt >= 0)
      timePerFrame = timePerStep - speedStep * numberInputInt;
    else if (0 > numberInputInt)
      timePerFrame = timePerStep + speedStep * numberInputInt * -1;
  }) 

  animate();
  slider.stepUp();
  prevSliderVal++;
}

function start() {
  if (parseInt(slider.value) >= totalFrameNumber) {
    reset();
    slider.value = 1;
    prevSliderVal = 1;
  }
  playBtn.innerHTML = "&#10074;&#10074;";
  requestId = window.requestAnimationFrame(increment);
}

function stop() {
  playBtn.innerHTML = "&#9658;";
  window.cancelAnimationFrame(requestId);
  requestId = undefined;
}

function reset() {
  svgDocument.textContent = '';
  it = makeFrameIterator();
  animate();
  slider.stepUp();
  prevSliderVal++;
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
    textInput.value = msToTime(parseInt(slider.value) * timePerStep);
    animate();
    if (parseInt(slider.value) >= totalFrameNumber) {
      stop();
      timeWhenLastUpdate = timestamp;
      return;
    }
    slider.stepUp();
    prevSliderVal++;
    timeWhenLastUpdate = timestamp;
  }
  
  requestId = window.requestAnimationFrame(increment);
}

function animate() {
  let result = it.next();
  if (!result.done)
    Object.keys(result.value).forEach(e => updateShape(e, result.value[e], svgDocument));
}

function updateShape(key, value, parent) {
  let obj = document.getElementById(key);

  if (obj == null) {
    let type = value['type'];
    if (type == 'group') 
      obj = document.createElementNS(svgns, "svg");
    else 
      obj = document.createElementNS(svgns, type);
    obj.setAttribute("id", key);
    parent.appendChild(obj);
  } 

  Object.keys(value).forEach(e => updateAttr(obj, e, value[e]))
}

function updateAttr(shape, key, value) {
  if (key == 'shapes') {
    Object.keys(value).forEach(e => updateShape(e, value[e], shape))
  } else if (key == 'text') {
    shape.innerHTML = value;
  } else if (key == 'remove') {
    if (value)
      shape.remove();
  } else {
    if (key == 'visibility') {
      if (value)
        value = 'visible';
      else
        value = 'hidden';
    }
    if (value != "" && value != "-1")
      shape.setAttribute(key, value);
  }
}

function makeFrameIterator() {
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
