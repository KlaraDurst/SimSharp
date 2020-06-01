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
  totalFrameNumber = frames.length - 1;

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
    if (wasPlaying) 
      start();
  })
  slider.addEventListener('change', () => {
    let sliderInt = parseInt(slider.value);
    let sliderDiff = sliderInt - prevSliderVal;
    textInput.value = msToTime(sliderInt * timePerStep);
    for (let i = 0; i < sliderDiff; i++)
      animate();

    if (prevSliderVal > sliderInt) {
      reset();
      for (let i = 0; i < sliderInt; i++)
        animate();
    }

    prevSliderVal = sliderInt;
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
}

function start() {
  if (parseInt(slider.value) >= totalFrameNumber) {
    reset();
    slider.value = 0;
    prevSliderVal = 0;
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
  
  if (parseInt(slider.value) >= totalFrameNumber)
    stop();
  else 
    requestId = window.requestAnimationFrame(increment);
}

function animate() {
  let result = it.next();
  if (!result.done)
    Object.keys(result.value).forEach(e => updateObj(e, result.value[e], svgDocument));
}

function updateObj(key, value, parent) {
  let obj = document.getElementById(key);
  let type;

  if (obj == null) {
    type = value['type'];
    if (type != 'group') 
      obj = document.createElementNS(svgns, value['type']);
    else 
      obj = document.createElementNS(svgns, "svg");
    obj.setAttribute("id", key);
    parent.appendChild(obj);
  } else {
    type = obj.getAttribute('type');
  }

  if (type != 'group')
    Object.keys(value).forEach(e => updateShapeAttr(obj, e, value[e]))
  else 
    Object.keys(value).forEach(e => updateGroupAttr(obj, e, value[e]));
}

function updateGroupAttr(group, key, value) {
  if (key != "shapes")
    updateShapeAttr(group, key, value);
  else {
    Object.keys(value).forEach(e => updateObj(e, value[e], group))
  }
}

function updateShapeAttr(shape, key, value) {
  if (key == 'visibility') {
    if (value)
      value = 'visible';
    else
      value = 'hidden';
  } else if (key == 'remove') {
    if (value)
      shape.remove();
  }
  shape.setAttribute(key, value);
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
