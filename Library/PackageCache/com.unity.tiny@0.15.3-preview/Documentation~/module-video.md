# Video Module

The **Video** and **VideoHTML** Modules allow you to add auto-play videos in your apps by using **VideoPlayer** and **VideoSource** components on an Entity.

Tiny Mode supports the video formats commonly supported on the web by most browsers (WebM, MP4, Ogg).

To use Video:

- Drag and drop a Video Clip Asset from the Project window to the Hierarchy. Unity will automatically create an Entity, attach the VideoPlayer components, and assign the Video Clip.

Alternatively you can perform this process manually:

- Create an Entity
- Attach a VideoPlayer Component
- Add a Video Clip Asset by dragging it from the Project window into the **clip** field of the VideoPlayer Component.

Or, you can do this from Script:

Create an entity clip, attach a VideoClip component, a VideoClipLoadFromFile component and VideoClipLoadFromFileName for the name (file name or Url) of your video.
Create an entity player, attach a VideoPlayer component and set your entity clip to the clip entity field.

Use cases: 

* Add videos to your game (introduction, final videos in the game, non-interactive cut scenes)
* Add video playable ads.

The **VideoPlayer** component has 4 attributes:

* **Controls**. Set it to true if you want to show the video controls like (play, pause, seeking, volume, fullscreen toggle)

* **Loop**. Set it to true if you want the video to restart automatically when reaching the end

* **currentTime**. A readonly attribute to follow the current playback time of a playing video. It can be useful in cases where you want some actions to happen (like displaying a skip button) after a period of time.

* **Clip**. A link to the video clip entity.

The **VideoPlayerAutoDeleteOnEnd** component allows you to automatically delete the video once it reaches the end.

At runtime, when a video entity is added with a video clip, it will automatically play it but the video will be muted by default (this is a requirement from most browsers to avoid noise pollution). 

# How to use it:

##From the Editor:
  
  - Create an entity, attach a VideoPlayer component, and add a video clip asset to the field clip.

  - Or drag and drop a video clip asset to the hierarchy. It will automatically create the entity and attach the right components to it.

##From Script:

  - Create an entity clip, attach a **VideoClip** component, a **VideoClipLoadFromFile** component and **VideoClipLoadFromFileName** for the name (file name or Url) of your video.

  - Create an entity player, attach a **VideoPlayer** component and set your entity clip to the clip entity field.

Optional

- Attach a **VideoPlayerAutoDeleteOnEnd** component to the Video player entity if you want the video to be automatically deleted after it reaches the end.

- Attach a **RectTransform** component to position the video on the UI. If no RectTransform is attached the video will be played full screen.

(See this module's API documentation for more information)