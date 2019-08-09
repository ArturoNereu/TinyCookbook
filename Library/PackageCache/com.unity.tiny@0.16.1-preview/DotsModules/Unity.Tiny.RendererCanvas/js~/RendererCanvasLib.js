mergeInto(LibraryManager.library, {
    js_renderTextTo2DCanvas__deps: ['$utf16_to_js_string'],
    js_renderTextTo2DCanvas__proxy: 'sync',
    js_renderTextTo2DCanvas: function(text, family, size, weight, italic, r, g, b, a, width, height) {
        text = utf16_to_js_string(text);
        var font = size + 'pt' + ' ' + utf16_to_js_string(family);
        
        var context = ut._HTML.canvasContext;
        context.font = weight + ' ' + (italic ? 'italic ' : '') + font;
        context.fillStyle = 'rgb(' + (r | 0) + ',' + (g | 0) + ',' + (b | 0) + ')';
        context.globalAlpha = a / 255;
        context.textAlign = "center";
        context.textBaseline = "middle";
        context.fillText(text, width/2, height/2);
    },

    js_canvasBlendingAndSmoothing__proxy : 'async',
    js_canvasBlendingAndSmoothing : function (blend, smooth) {
      var context = ut._HTML.canvasContext;
      context.globalCompositeOperation = ([ 'source-over', 'lighter', 'multiply', 'destination-in' ])[blend];
      context.imageSmoothingEnabled = smooth;
    },

    js_canvasRenderNormalSpriteWhite__proxy : 'async',
    js_canvasRenderNormalSpriteWhite : function (txa, txb, txc, txd, txe, txf, alpha,
      imageIndex, sx, sy, sw, sh, x, y, w, h) {
      var context = ut._HTML.canvasContext;
      context.setTransform(txa, txb, txc, txd, txe, txf);
      context.globalAlpha = alpha;
      context.drawImage(ut._HTML.images[imageIndex].image, sx, sy, sw, sh, x, y, w, h);
    },

    js_canvasRenderNormalSpriteTinted__proxy : 'async',
    js_canvasRenderNormalSpriteTinted : function (txa, txb, txc, txd, txe, txf, alpha,
      tintedIndex, x, y, w, h) {
      var context = ut._HTML.canvasContext;
      context.setTransform(txa, txb, txc, txd, txe, txf);
      context.globalAlpha = alpha;
      context.drawImage(ut._HTML.tintedSprites[tintedIndex].image, x, y, w, h);
    },

    js_canvasFreeImage__proxy : 'async',
    js_canvasFreeImage : function (idx) {
      ut._HTML.freeImage(idx);
    },

    js_canvasPushImageAsContext__proxy : 'async',
    js_canvasPushImageAsContext : function (imageIndex) {
      if (!ut._HTML.canvasStack)
        ut._HTML.canvasStack = [];
      ut._HTML.canvasStack.push(ut._HTML.canvasContext);
      ut._HTML.canvasContext = ut._HTML.images[imageIndex].context;
    },

    js_canvasPopContext__proxy : 'async',
    js_canvasPopContext : function () {
      ut._HTML.canvasContext = ut._HTML.canvasStack.pop();
    },

    js_canvasResizeRenderTarget__proxy : 'async',
    js_canvasResizeRenderTarget : function (imageIndex, w, h) {
      ut._HTML.images[imageIndex].image.width = w;
      ut._HTML.images[imageIndex].image.height = h;
      ut._HTML.images[imageIndex].width = w;
      ut._HTML.images[imageIndex].height = h;
    },

    js_canvasFreeRenderTarget__proxy : 'async',
    js_canvasFreeRenderTarget : function (imageIndex) {
      ut._HTML.images[imageIndex] = null; 
    },

    js_canvasMakeRenderTarget__proxy : 'sync',
    js_canvasMakeRenderTarget : function (w, h) {
      // return new index
      var canvas = document.createElement('canvas');
      canvas.width = w;
      canvas.height = h;
      var cx = canvas.getContext('2d');
      // grab first free index
      var idx;
      for (var i = 1; i <= ut._HTML.images.length; i++) {
        if (!ut._HTML.images[i]) {
          idx = i;
          break;
        }
      }
      // set image
      ut._HTML.initImage(idx);
      ut._HTML.images[idx].image = canvas;
      ut._HTML.images[idx].context = cx;
      ut._HTML.images[idx].width = w;
      ut._HTML.images[idx].height = h;
      ut._HTML.images[idx].isrt = true;
      return idx;
    },

    js_canvasMakeTintedSprite__proxy : 'sync',
    js_canvasMakeTintedSprite : function (imageIndex, sx, sy, sw, sh, r, g, b) {
      var context = ut._HTML.canvasContext;
      // make a temp canvas
      var canvas = document.createElement('canvas');
      canvas.width = sw;
      canvas.height = sh;
      var cx = canvas.getContext('2d');
      var srcimg = ut._HTML.images[imageIndex].image;
      // initialize temp with with image
      cx.globalCompositeOperation = 'copy';
      cx.drawImage(srcimg, sx, sy, sw, sh, 0, 0, sw, sh);
      // check case for r==g==b==255, which can happen with non-pattern tiling as we reuse the tint cache there 
      if ((r&g&b)!==255) {
        if (!ut._HTML.supportMultiply) {
          // fall back to software if context does not support multiply like for example the wechat platform
          var imd = cx.getImageData(0,0,sw,sh);
          var s = sw*sh*4;
          var da = imd.data;
          var scaleR = ((r / 255.0)*256.0)|0;
          var scaleG = ((g / 255.0)*256.0)|0;
          var scaleB = ((b / 255.0)*256.0)|0;
          for (var i=0; i<s; i+=4) {
            da[i] = (da[i]*scaleR)>>8;
            da[i+1] = (da[i+1]*scaleG)>>8;
            da[i+2] = (da[i+2]*scaleB)>>8;
          }
          cx.putImageData(imd,0,0);
        } else {
          // multiply with color (unfortunately sets alpha=1)
          cx.globalCompositeOperation = 'multiply';
          cx.fillStyle = 'rgb(' + (r | 0) + ',' + (g | 0) + ',' + (b | 0) + ')';
          cx.fillRect(0, 0, sw, sh);
          // take alpha channel from image again
          cx.globalCompositeOperation = 'destination-in';
          cx.drawImage(srcimg, sx, sy, sw, sh, 0, 0, sw, sh);
        }
      }
      // grab first free index
      var idx;
      if ( ut._HTML.tintedSpritesFreeList.length===0 )
        idx = ut._HTML.tintedSprites.length;
      else
        idx = ut._HTML.tintedSpritesFreeList.pop();
      // put the canvas into tinted
      ut._HTML.tintedSprites[idx] = { image : canvas, pattern : null };
      return idx;
    },

    js_canvasReleaseTintedSprite__proxy : 'async',
    js_canvasReleaseTintedSprite : function (tintedIndex) {
      ut._HTML.tintedSprites[tintedIndex] = null;
      ut._HTML.tintedSpritesFreeList.push(tintedIndex);
    },

    js_canvasMakePattern__proxy : 'async',
    js_canvasMakePattern : function (tintedIndex) {
      // tinted sprite has to be made first!
      var context = ut._HTML.canvasContext;
      var img = ut._HTML.tintedSprites[tintedIndex].image;
      ut._HTML.tintedSprites[tintedIndex].pattern = context.createPattern ( img, 'repeat');
    },

    js_canvasSetTransformOnly__proxy: 'async',
    js_canvasSetTransformOnly: function (txa, txb, txc, txd, txe, txf) {
      var context = ut._HTML.canvasContext;
      context.setTransform(txa, txb, txc, txd, txe, txf);
    },

    js_canvasRenderNormalSpriteWhiteNoTransform__proxy: 'async',
    js_canvasRenderNormalSpriteWhiteNoTransform: function (alpha, imageIndex, sx, sy, sw, sh, x, y, w, h) {
      var context = ut._HTML.canvasContext;
      context.globalAlpha = alpha;
      context.drawImage(ut._HTML.images[imageIndex].image, sx, sy, sw, sh, x, y, w, h);
    },

    js_canvasRenderNormalSpriteTintedNoTransform__proxy: 'async',
    js_canvasRenderNormalSpriteTintedNoTransform: function (alpha, tintedIndex, x, y, w, h) {
      var context = ut._HTML.canvasContext;
      context.globalAlpha = alpha;
      context.drawImage(ut._HTML.tintedSprites[tintedIndex].image, x, y, w, h);
    },

    js_canvasRenderShape__proxy: 'sync',
    js_canvasRenderShape: function (vertices, nv, indices, ni, r, g, b, a) {
      var cx = ut._HTML.canvasContext;
      cx.fillStyle = 'rgb(' + (r | 0) + ', ' + (g | 0) + ', ' + (b | 0) + ')';
      cx.globalAlpha = a;
      var verts = HEAPF32.subarray(vertices >> 2, (vertices >> 2) + (nv << 1));
      if (ni <= 0) {
          cx.beginPath();
          cx.moveTo(verts[0], -verts[1]);
          for (var i = 2; i < (nv << 1); i += 2)
              cx.lineTo(verts[i], -verts[i + 1]);
          cx.fill();
      } else {
          var inds = HEAPU16.subarray(indices >> 1, (indices >> 1) + ni);
          for (var i = 0; i < ni; i += 3) {
              cx.beginPath();
              cx.moveTo(verts[inds[i] << 1], -verts[(inds[i] << 1) + 1]);
              cx.lineTo(verts[inds[i + 1] << 1], -verts[(inds[i + 1] << 1) + 1]);
              cx.lineTo(verts[inds[i + 2] << 1], -verts[(inds[i + 2] << 1) + 1]);
              cx.fill();
          }
      }
    },

    js_canvasInit__proxy: 'sync',
    js_canvasInit: function(){
        var cx = ut._HTML.canvasContext;
        if (!cx || !cx.save)
          return false;
        cx.save();
        cx.globalCompositeOperation = 'multiply';
        ut._HTML.supportMultiply = cx.globalCompositeOperation == 'multiply';
        cx.restore();
        return true;
    },

    js_canvasSupportsMultiply__proxy: 'sync',
    js_canvasSupportsMultiply: function () {
        return ut._HTML.supportMultiply;
    },

    js_canvasClear__proxy: 'async',
    js_canvasClear: function (r,g,b,a,w,h) {
        var cx = ut._HTML.canvasContext;
        cx.globalCompositeOperation = 'copy';
        cx.globalAlpha = 1.0;
        cx.fillStyle = 'rgba(' + (r | 0) + ', ' + (g | 0) + ', ' + (b | 0) + ', ' + a + ')';
        cx.fillRect(0, 0, w, h);
    },

    js_canvasRenderPatternSprite__proxy: 'async',
    js_canvasRenderPatternSprite: function(patternIdx, x, y, w, h, txa, txb, txc, txd, txe, txf, alpha) {
      // draw clipping path (note: base tx must be set!)
      var cx = ut._HTML.canvasContext;
      cx.globalAlpha = alpha;
      cx.save();
      cx.beginPath();
      cx.rect(x, y, w, h);
      cx.clip(); // TODO: test if this works with camera clip rectangles!
      // set a transform for the pattern!
      cx.setTransform(txa, txb, txc, txd, txe, txf);
      // draw a huge filled rect
      cx.fillStyle = ut._HTML.tintedSprites[patternIdx].pattern;
      cx.fillRect(0, -10000, 10000, 10000);
      // reset clipping
      cx.restore();
    },

    js_canvasRenderMultipleSliced__proxy: 'sync',
    js_canvasRenderMultipleSliced: function (tintIndex, imageIndex, v, n, alpha) {
      var cx = ut._HTML.canvasContext;
      cx.globalAlpha = alpha;
      var img = tintIndex > 0 ? ut._HTML.tintedSprites[tintIndex].image : ut._HTML.images[imageIndex].image;
      // draw all images
      var i8 = v >> 2;
      for (var i = 0; i < n; i++) {
        if ( HEAPF32[i8 + 2] > 0 && HEAPF32[i8 + 3] > 0 ) // have to check zero source rect for firefox
          cx.drawImage(img, HEAPF32[i8], HEAPF32[i8 + 1], HEAPF32[i8 + 2], HEAPF32[i8 + 3],
            HEAPF32[i8 + 4], -HEAPF32[i8 + 7] - HEAPF32[i8 + 5], HEAPF32[i8 + 6], HEAPF32[i8 + 7]);
        i8 += 8;
      }
    },
});
