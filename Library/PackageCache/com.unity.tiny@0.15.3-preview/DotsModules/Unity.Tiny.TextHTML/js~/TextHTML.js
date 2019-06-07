mergeInto(LibraryManager.library, {

  js_measureText__proxy: 'sync',
  js_measureText: function (text, family, size, weight, italic, outWidth, outHeight) {
      text = UTF8ToString(text);
      family = UTF8ToString(family);
      var useMeasureText = false;
      if (useMeasureText) {
          // measureText() gives worthless old DOMTextMetrics on all but the most recent browsers,
          // and even then not in some cases.  The worthless one has only a width, no height.
          if (!ut._HTML.canvasTextMeasureContext) {
              ut._HTML.canvasTextMeasureCanvas = document.createElement("canvas");
              ut._HTML.canvasTextMeasureContext = ut._HTML.canvasTextMeasureCanvas.getContext("2d");
          }

          var context = ut._HTML.canvasTextMeasureContext;
          context.font = weight + ' ' + (italic ? 'italic ' : '') + size + "pt" + ' ' + family;
          context.fillStyle = "black";
          context.textAlign = "left";
          context.textBaseline = "bottom";

          var metrics = context.measureText(text);
          HEAPF32[outWidth>>2] = metrics.actualBoundingBoxLeft + metrics.actualBoundingBoxRight;
          HEAPF32[outHeight>>2] = metrics.actualBoundingBoxAscent + metrics.actualBoundingBoxDescent;
      } else {
          // This works everywhere, but is not sufficient to figure out the black box origin
          // of the text.
          var div = document.createElement("div");
          div.style.position = "absolute";
          div.style.visibility = "hidden";
          div.style.fontFamily = family;
          div.style.fontWeight = weight;
          // UTINY-1723: Getting the text measurements for small font size (<5) is inaccurate (always same width (12px) for example in Firefox).
          // Let's compute it for a font 20 times bigger and get w/h 20 times smaller
          var mult = 1;
          if (size < 5)
            mult = 20;
          div.style.fontSize = size * mult + "pt";
          div.style.fontStyle = italic ? "italic" : "normal";
          div.style.textAlign = "left";
          div.style.verticalAlign = "bottom";
          div.style.color = "black";
          //Remove any white spaces when computing the bbox. We will consider them separately below
          var textWithWS = text.replace(/\s/g, "");
          div.innerText = textWithWS;
          document.body.appendChild(div);
          var rect = div.getBoundingClientRect();
          document.body.removeChild(div);

          //Previous bbox computed reduces consecutive white spaces to one white space. So we need here to compute the width of all white spaces separately
          var newCanvas = document.createElement("canvas");
          var ct = newCanvas.getContext("2d");
          ct.font = weight + ' ' + (italic ? 'italic ' : '') + size * mult + "pt" + ' ' + family;
          ct.textAlign = "left";
          ct.textBaseline = "bottom";
          var wsWidth = ct.measureText(" ").width;
          var wsCount = text.split(" ").length - 1;
          var tabWidth = ct.measureText("\t").width;
          var tabCount = text.split("\t").length - 1;

          var resW = (rect.width + wsWidth * wsCount + tabCount * tabWidth) / mult;
          var resH = rect.height / mult;
          
          HEAPF32[outWidth >> 2] = (rect.width + wsWidth * wsCount + tabCount * tabWidth) / mult;
          HEAPF32[outHeight >> 2] = rect.height / mult;
      }
    },
});
