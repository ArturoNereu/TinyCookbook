
mergeInto(LibraryManager.library, {

    //Initializes caches
    js_initialize__proxy: 'async',
    js_initialize: function () {
      ut = ut || {};
      ut._HTML = ut._HTML || {};
      ut._HTML.videoSourceLoading = {};
      ut._HTML.videoSourcePlaying = {};
    },

    // Creates a new video element
    js_create_video_element__proxy: 'async',
    js_create_video_element: function (videoFileName, controls, loop, left, top, width, height) {

        //Get first free index
        var index = 0;
        for (var i = 0; i <= ut._HTML.videoSourceLoading.length; i++) {
            if (!ut._HTML.videoSourceLoading[i]) {
                index = i;
                break;
            }
        }

      var videoElement = document.getElementById("video-entity-" + index);
      //Adds new video elements
        if (!videoElement)
        {
            videoElement = window.document.createElement("video");
            videoElement.setAttribute("id", "video-entity-" + index);
            videoElement.autoplay = true;
            var defaultText = document.createTextNode("Your browser doesn't support this media format.");
            videoElement.appendChild(defaultText);
            document.body.appendChild(videoElement);
        }

        var url = UTF8ToString(videoFileName);
        if (url.substring(0, 9) === "ut-asset:")
          url = UT_ASSETS[url.substring(9)];
          
        videoElement.setAttribute("src", url);
        videoElement.setAttribute("preload", "metadata");
        
        //Adds new video source
        ut._HTML.videoSourceLoading[index] = {};
        ut._HTML.videoSourcePlaying[index] = {};
        ut._HTML.videoSourceLoading[index].status = 'loading';
        ut._HTML.videoSourcePlaying[index].status = 'not_playing';
        
        //OnLoadedData callback
        videoElement.onloadeddata = function () {
            if(ut._HTML.videoSourceLoading[index] != null)
              ut._HTML.videoSourceLoading[index].status = 'loaded';
        
            if (ut._HTML.videoSourcePlaying[index] != null) {
              ut._HTML.videoSourcePlaying[index].status = 'playing';
              ut._HTML.videoSourcePlaying[index].currentTime = 0;
            }
        }

        //Onerror callback
        videoElement.onerror = function () {
          console.log("[Video] Failed to load error:" + videoElement.error.code + " message: " + videoElement.error.message);
          ut._HTML.videoSourceLoading[index].status = 'error';
        };

        videoElement.width = width;
        videoElement.height = height;
        videoElement.style.position = "absolute";
        videoElement.style.top = left + "px";
        videoElement.style.left = top + "px";
        videoElement.style.zIndex = 100;
        videoElement.style.backgroundColor = "black";
        videoElement.controls = controls;
        videoElement.loop = loop;

        // Chrome browser doesnt do autoplay if sound not muted
        videoElement.muted = true;

        //Onended callback
        videoElement.onended = function () {
          ut._HTML.videoSourcePlaying[index].status = 'done_playing';
        };

        // Update the current Time of the player
        videoElement.ontimeupdate = function () {
            ut._HTML.videoSourcePlaying[index].currentTime = videoElement.currentTime;
        };

        return index;
    },
    
    //Checks video loading status
    js_check_loading__proxy: 'sync',
    js_check_loading: function (index) {
      if (!ut._HTML.videoSourceLoading || index < 0 || ut._HTML.videoSourceLoading[index] == null)
        return -1;
      if (ut._HTML.videoSourceLoading[index].status === 'error')
        return 0;
      if (ut._HTML.videoSourceLoading[index].status === 'loading')
        return 1; 
      if (ut._HTML.videoSourceLoading[index].status === 'loaded')
        return 2;
      return -1;
    },
      
   //Checks if a video is playing 
   js_check_isPlaying__proxy: 'sync',
   js_check_isPlaying: function (index) {
      if(!ut._HTML.videoSourcePlaying || index < 0 || ut._HTML.videoSourcePlaying[index] == null)
        return 0;
      if (ut._HTML.videoSourcePlaying[index].status === 'not_playing')
          return 0;
      if (ut._HTML.videoSourcePlaying[index].status === 'playing')
          return 1;
      if (ut._HTML.videoSourcePlaying[index].status === 'done_playing')
          return 2;
      return 0;
   },
      
   //Retrieves the current playback time of a video
   js_getCurrentTime__proxy: 'sync',
   js_getCurrentTime: function(index) {
     if (!ut._HTML.videoSourcePlaying || index < 0 || ut._HTML.videoSourcePlaying[index] == null)
       return 0;
     if(ut._HTML.videoSourcePlaying[index].status === 1)
       return ut._HTML.videoSourcePlaying[index].currentTime;
    return 0;
   },
    
    // Remove a video element from its id
    js_remove_video_element__proxy: 'async',
    js_remove_video_element: function (index) {
      var videoElement = document.getElementById("video-entity-" + index);
      if (videoElement !== 'undefined') {
        document.body.removeChild(videoElement);
        ut._HTML.videoSourceLoading[index] = null;
        ut._HTML.videoSourcePlaying[index] = null;
      }
    }
});

