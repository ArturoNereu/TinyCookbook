#include "zeroplayer.h"

#include <emscripten.h>
#include <emscripten/html5.h>
#include <stdio.h>

#include "il2cpp-config.h"
#include "gc/GarbageCollector.h"

extern "C" {
    void js_html_init();
    bool js_html_setCanvasSize(int width, int height, bool webgl);
    void js_html_debugReadback(int w, int h, void *pixels);
}

static bool (*raf)() = 0; // c# delegate

ZEROPLAYER_EXPORT
bool init_html() {
    js_html_init();
    return true;
}

ZEROPLAYER_EXPORT
void shutdown_html(int exitCode) {
    raf = 0;
}

ZEROPLAYER_EXPORT
double time_html() {
    // TODO: If we want to target such old mobile browsers that they do not have performance.now() API,
    //       we should change the following call to emscripten_get_now() instead of emscripten_performance_now().
    //       They are the same otherwise, except that emscripten_performance_now() is much smaller code-size wise,
    //       since it does not emulate performance.now() via Date.now() (which emscripten_get_now() does)
    return emscripten_performance_now()*0.001;
}

// from liballocators
extern "C" void unsafeutility_freetemp();

static EM_BOOL tick(double /*wallclock_time_in_msecs*/, void */*userData*/)
{
    using namespace il2cpp::gc;
    bool disabled = GarbageCollector::IsDisabled();
    if (disabled)
        GarbageCollector::Enable();
    GarbageCollector::Collect(0);
    unsafeutility_freetemp();
    if (disabled)
        GarbageCollector::Disable();
    if (!raf())
    {
        shutdown_html(2);
        return EM_FALSE; // return back to Emscripten runtime saying that animation loop should stop here
    }
    return EM_TRUE; // return back to Emscripten runtime saying that animation loop should keep going
}

ZEROPLAYER_EXPORT
bool rafcallbackinit_html(bool (*func)()) {
    if (raf)
        return false;
    raf = func;
#if __EMSCRIPTEN_PTHREADS__
    // When running in a web worker, which does not have requestAnimationFrame(), instead run a manual loop
    // with setTimeout()s. TODO: With OffscreenCanvas rAF() will likely become available in Workers, so in
    // future will probably want to feature test rAF() first, and only if not available, fall back to
    // setTimeout() loop.
    emscripten_set_timeout_loop(tick, 1000.0/60.0, 0);
#else
    // In singlethreaded build on main thread, use rAF().
    emscripten_request_animation_frame_loop(tick, 0);
#endif
    // Unwind back to browser, skipping executing anything after this function.
    emscripten_throw_string("unwind");
    // This line is never reached, the throw above throws a JS statement that skips over rest of the code.
    return true;
}
