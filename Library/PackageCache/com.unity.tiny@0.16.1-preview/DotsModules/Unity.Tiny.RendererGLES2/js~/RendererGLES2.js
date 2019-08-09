mergeInto(LibraryManager.library, {

  js_texImage2D_from_html_image__proxy: 'async',
  js_texImage2D_from_html_image: function(htmlImageId) {
    GLctx['texImage2D'](0x0DE1/*GL_TEXTURE_2D*/, 0, 0x1908/*GL_RGBA*/, 0x1908/*GL_RGBA*/, 0x1401/*GL_UNSIGNED_BYTE*/, ut._HTML.images[htmlImageId].image);
  },

  js_texImage2D_from_html_text__proxy: 'sync',
  js_texImage2D_from_html_text: function(text, family, fontSize, weight, italic, labelWidth, labelHeight) {

    var font = fontSize.toString() + 'px ' + utf16_to_js_string(family);
    var newFont = weight.toString() + ' ' + (italic ? 'italic ' : '') + font;

    // Update the canvas and texture
    var textCanvas = window.document.createElement('canvas');
    textCanvas.width = labelWidth;
    textCanvas.height = labelHeight;

    var context = textCanvas.getContext("2d");
    context.fillStyle = 'white';
    context.font = newFont;
    context.textAlign = "center";
    context.textBaseline = "middle";
    context.fillText(utf16_to_js_string(text), labelWidth / 2, labelHeight / 2);

    GLctx['texImage2D'](0x0DE1/*GL_TEXTURE_2D*/, 0, 0x1908/*GL_RGBA*/, 0x1908/*GL_RGBA*/, 0x1401/*GL_UNSIGNED_BYTE*/, textCanvas);
  }

});
