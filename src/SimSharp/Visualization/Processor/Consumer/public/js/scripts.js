var svgDocument;
var svgns;
var frames = [];
var framesCount;
var timePerStep;
var timePerFrame;
var it;
var timeWhenLastUpdate;
var timeFromLastUpdate;
var requestId;
var textInput;

const socket = new WebSocket('ws://localhost:8080');

socket.addEventListener('message', function (event) {
  var json = JSON.parse(event.data);
  if (json.hasOwnProperty("start"))
    init(json)
  else 
    frames.push(json);
});

function init(config) {
  framesCount = 0;
  svgDocument = document.getElementById('canvas');
  textInput = document.getElementById('textInput');
  svgns = "http://www.w3.org/2000/svg";
  timePerStep = 1000 / config["fps"];
  timePerFrame = timePerStep;
  speedStep = 100;
  it = makeFrameIterator();
  requestId = undefined;

  if (config.hasOwnProperty('width')) {
    if (config.hasOwnProperty('height')) {
      if (config.hasOwnProperty('startX')) {
        if (config.hasOwnProperty('startY'))
          svgDocument.setAttribute('viewBox', config['startX'].toString().concat(" ", config['startY'].toString(), " ", config['width'].toString(), " ", config['height'].toString()));
      }
    }
  }

  requestId = window.requestAnimationFrame(increment);
}

function increment(timestamp) {
  if (!timeWhenLastUpdate)
    timeFromLastUpdate = timePerFrame + 1;
  else
    timeFromLastUpdate = timestamp - timeWhenLastUpdate;

  if (timeFromLastUpdate > timePerFrame) {
    textInput.value = msToTime(framesCount * timePerStep);
    animate();
    timeWhenLastUpdate = timestamp;
  }
  
  requestId = window.requestAnimationFrame(increment);
}

function animate() {
  let result = it.next();
  if (!result.done) {
    framesCount++;
    Object.keys(result.value).forEach(e => updateShape(e, result.value[e], svgDocument));
  }
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
    shape.setAttribute(key, value);
  }
}

function makeFrameIterator() {
  const frameIterator = {
    next: function () {
      let result;
      if (frames.length > 0) {
        var frame = frames.shift();
        if (!frame.hasOwnProperty("stop")) {
          result = { value: frame, done: false }
          return result;
        } else {
          window.cancelAnimationFrame(requestId);
        }
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
