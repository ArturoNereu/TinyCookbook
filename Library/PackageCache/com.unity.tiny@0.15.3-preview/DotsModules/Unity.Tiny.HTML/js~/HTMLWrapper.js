mergeInto(LibraryManager.library, {
  // helper function for strings 
  $utf16_to_js_string: function(ptr) {
    var str = '';
    ptr >>= 1;
    while (1) {
      var codeUnit = HEAP16[ptr++];
      if (!codeUnit) return str;
      str += String.fromCharCode(codeUnit);
    }
  },

  // Create a variable 'ut' in global scope so that Closure sees it.
  js_html_init__postset : 'var ut;',
  js_html_init__proxy : 'async',
  js_html_init : function() {
    ut = ut || {};
    ut._HTML = ut._HTML || {};

    var html = ut._HTML;
    html.visible = true;
    html.focused = true;
  },

  js_html_getDPIScale__proxy : 'sync',
  js_html_getDPIScale : function () {
    return window.devicePixelRatio;
  },

  js_html_getScreenSize__proxy : 'sync',
  js_html_getScreenSize : function (wPtr, hPtr) {
    HEAP32[wPtr>>2] = screen.width | 0;
    HEAP32[hPtr>>2] = screen.height | 0;
  },
  
  js_html_getFrameSize__proxy : 'sync',
  js_html_getFrameSize : function (wPtr, hPtr) {
    HEAP32[wPtr>>2] = window.innerWidth | 0;
    HEAP32[hPtr>>2] = window.innerHeight | 0;
  },  
  
  js_html_getCanvasSize__proxy : 'sync',
  js_html_getCanvasSize : function (wPtr, hPtr) {
    var html = ut._HTML;
    HEAP32[wPtr>>2] = html.canvasElement.width | 0;
    HEAP32[hPtr>>2] = html.canvasElement.height | 0;
  },

  testBrowserCannotHandleOffsetsInUniformArrayViews__proxy : 'sync',
  testBrowserCannotHandleOffsetsInUniformArrayViews : function (g) {
    function b(c, t) {
      var s = g.createShader(t);
      g.shaderSource(s, c);
      g.compileShader(s);
      return s;
    }
    try {
      var p = g.createProgram();
      var sv = b("attribute vec4 p;void main(){gl_Position=p;}", g.VERTEX_SHADER);
      var sf = b("precision lowp float;uniform vec4 u;void main(){gl_FragColor=u;}", g.FRAGMENT_SHADER);
      g.attachShader(p, sv);
      g.attachShader(p, sf);
      g.linkProgram(p);
      var h = new Float32Array(8);
      h[4] = 1;
      g.useProgram(p);
      var l = g.getUniformLocation(p, "u");
      g.uniform4fv(l, h.subarray(4, 8)); // Uploading a 4-vector GL uniform from last four elements of array [0,0,0,0,1,0,0,0], i.e. uploading vec4=(1,0,0,0)
      var r = !g.getUniform(p, l)[0]; // in proper WebGL we expect to read back the vector we just uploaded: (1,0,0,0). On buggy WeChat browser would instead have uploaded offset=0 of above array, i.e. vec4=(0,0,0,0)
      g.useProgram(null);
      g.deleteShader(sv);
      g.deleteShader(sf);
      g.deleteProgram(p);
      return r;
    } catch (e) {
      return false; // On failure, we assume we failed on something completely different, so behave as if the workaround is not needed.
    }
  },

  js_html_setCanvasSize__deps : ['testBrowserCannotHandleOffsetsInUniformArrayViews'],
  js_html_setCanvasSize__proxy : 'sync',
  js_html_setCanvasSize : function(width, height, webgl) {
    console.log('setCanvasSize', width, height, webgl ? 'gl' : '2d');
    if (!width>0 || !height>0)
        throw "Bad canvas size at init.";
    var canvas = ut._HTML.canvasElement;
    if (!canvas) {
      // take possible user element
      canvas = document.getElementById("UT_CANVAS");
      if (canvas)
        console.log('Using user UT_CANVAS element.');
    } else {
      // destroy old canvas if renderer changed
      var waswebgl =
          ut._HTML.canvasMode == 'webgl2' || ut._HTML.canvasMode == 'webgl';
      if (webgl != waswebgl) {
        if (ut._HTML.freeAllGL)
          ut._HTML.freeAllGL();
        console.log('Rebuilding canvas for renderer change.');
        canvas.parentNode.removeChild(canvas);
        canvas = 0;
      }
    }

    if (!canvas) {
      canvas = document.createElement("canvas");
      canvas.setAttribute("id", "UT_CANVAS");
      canvas.setAttribute("style", "touch-action: none;");
      canvas.setAttribute("tabindex", "1");
      if (document.body) {
        document.body.style.margin = "0px";
        document.body.style.border = "0";
        document.body.style.overflow = "hidden"; // disable scrollbars
        document.body.style.display = "block";   // no floating content on sides
        document.body.insertBefore(canvas, document.body.firstChild);
      } else {
        document.documentElement.appendChild(canvas);
      }
    }

    ut._HTML.canvasElement = canvas;
    
    canvas.width = width;
    canvas.height = height;
    if (webgl) {
      ut._HTML.canvasContext = canvas.getContext('webgl2'); // = null to force webgl1
      if (!ut._HTML.canvasContext) {
        ut._HTML.canvasContext = canvas.getContext('webgl');
        if (!ut._HTML.canvasContext) {
          ut._HTML.canvasContext = canvas.getContext('experimental-webgl');
          if (!ut._HTML.canvasContext) {
            console.log('WebGL context failed, falling back to canvas.');
            webgl = false;
          } else {
            console.log('WebGL context ok, but experimental.');
            ut._HTML.canvasMode = 'webgl';
          }
        } else {
          ut._HTML.canvasMode = 'webgl';
          console.log('WebGL context is webgl1.');
        }
        if (ut._HTML.canvasContext) {
          ut._HTML.browserCannotHandleOffsetsInUniformArrayViews = _testBrowserCannotHandleOffsetsInUniformArrayViews(ut._HTML.canvasContext);
        }
      } else {
        console.log('WebGL context is webgl2.');
        ut._HTML.canvasMode = 'webgl2';
      }
    }
    if (!webgl) {
      ut._HTML.canvasContext = canvas.getContext('2d');
      ut._HTML.canvasMode = 'canvas';
    } else {
      canvas.addEventListener("webglcontextlost", function(event) { event.preventDefault(); }, false);
    }
            
    window.addEventListener("focus", function(event) { ut._HTML.focus = true; } );
    window.addEventListener("blur", function(event) { ut._HTML.focus = false; } );
    
    canvas.focus();
    return webgl;
  },

  js_html_debugReadback__proxy : 'sync',
  js_html_debugReadback : function(w, h, pixels) {
    if (!ut._HTML.canvasContext || ut._HTML.canvasElement.width<w || ut._HTML.canvasElement.height<h)
      return;
    var imd;
    if (ut._HTML.canvasMode == 'webgl' || ut._HTML.canvasMode == 'webgl2') {
      var gl = ut._HTML.canvasContext;
      imd = new Uint8Array(w*h*4);
      gl.readPixels(0, 0, w, h, gl.RGBA, gl.UNSIGNED_BYTE, imd); 
    } else {
      imd = ut._HTML.canvasContext.getImageData(0, 0, w, h).data;
    }
    for (var i=0; i<w*h*4; i++)
      HEAPU8[pixels+i] = imd[i];
  },

  js_html_promptText__proxy : 'sync',
  js_html_promptText : function(message, defaultText) {
    var res =
        prompt(UTF8ToString(message), UTF8ToString(defaultText));
    var bufferSize = lengthBytesUTF8(res) + 1;
    var buffer = _malloc(bufferSize);
    stringToUTF8(res, buffer, bufferSize);
    return buffer;
  },
});
