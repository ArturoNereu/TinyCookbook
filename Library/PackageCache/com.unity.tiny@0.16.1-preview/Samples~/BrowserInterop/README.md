# BrowserInterop 
BrowserInterop is a sample DOTS project to demonstrate how C# code can interact with JS and back. This is useful when integrating with javascript libraries.

### Version and Date
v1 5/31/2019

### Related API documentation  
* [EntityQueryBuilding.WithAll](https://docs.unity3d.com/Packages/com.unity.entities@0.0/api/Unity.Entities.EntityQueryBuilder.html#Unity_Entities_EntityQueryBuilder_WithAll__1)
* [EntityQueryBuilding.ForEach](https://docs.unity3d.com/Packages/com.unity.entities@0.0/api/Unity.Entities.EntityQueryBuilder.html#Unity_Entities_EntityQueryBuilder_ForEach_Unity_Entities_EntityQueryBuilder_F_E_)

### Prerequisite samples
None

### Samples extending this sample
None

### Directories structure
- prejs~/ 
  - all `.js` files in this directory are included in the emscripten build as _prejs_ files, meaning that these files will be included _first_. This directory typically contains third party libraries or SDK. 
  - In this project it contains fake SDK named `RandomUserSdk.js` to demonstrate how user code can interact with a third party JS library. 
- js~/
  - all `.js` files in this directory are processed by emscripten as _libraries_. These files can use API exposed in `prejs~/*.js` files
  - Being treated as _libraries_ means: 
    - `prejs~/*.js` files are processed before, so the API exposed in these files are defined and can be used here.
    - the functions contained in these files are exposed by emscripten and invokable from C# using `[DllImport("__Internal")]`

### How to run the sample
Open the BrowserInterop.project, choose the `AsmJS` target in the play bar and finally click on the play icon.
Then you should see a browser opening.

### How does this demo work
1. `RandomUserDataSystem.OnUpdate` checks if an input has been actuated. Could be a click, a tap or the spacebar being pressed. If an input as been actuated then it call the `FetchRandomUserProfile` imported JS function.
2. This JS function calls the `RandomUserSdk.fetchRandomUserProfile` defined in JS in `prejs~/RandomUserSdk.js` passing a callback that will be invoked later. 
3. `RandomUserSdk.fetchRandomUserProfile` function creates an async http request, attach a callback and trigger it.
4. The attached callback gets called when we get the response of the request, we then parse the response and call back the C# side by calling `SendMessage` with the random user's profile fetched from the fake SDK.
5. `SendMessage` is then handled by the runtime and will create a new `NativeMessage` entity containing the data passed to the `SendMessage` JS function.
6. This entity is finally processed in `RandomUserDataSystem.OnUpdate` that will update the UI with its data.

### Feedback
We are always looking to understand how we can improve our samples. If you are able we would love to get your feedback: https://unitysoftware.co1.qualtrics.com/jfe/form/SV_3BOKADHkJsMdJrL

