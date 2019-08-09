#include "zeroplayer.h"

#include "include/glew.h"
#include <GLFW/glfw3.h>
#include <stdio.h>
#include <math.h>
#include <vector>
#include <stdlib.h>

// TODO: could keep array of windows, window class etc.
// for now one static window is perfectly fine
static GLFWwindow* mainWindow = 0;
static bool shouldClose = false;

// input
static std::vector<int> mouse_pos_stream;
static std::vector<int> mouse_button_stream;
static std::vector<int> key_stream;

static int windowW = 0;
static int windowH = 0;

// callbacks
static void
window_size_callback(GLFWwindow* window, int width, int height)
{
    //printf ( "GLFW resize %i, %i\n", width, height);
    windowW = width;
    windowH = height;
}

static void
window_close_callback(GLFWwindow* window)
{
    shouldClose = true;
}

static void
cursor_position_callback(GLFWwindow* window, double xpos, double ypos)
{
    //printf ( "GLFW C mouse pos %f, %f\n", (float)xpos, (float)ypos);
    mouse_pos_stream.push_back((int)xpos);
    mouse_pos_stream.push_back(windowH - 1 - (int)ypos);
}

static void
mouse_button_callback(GLFWwindow* window, int button, int action, int mods)
{
    //printf ( "GLFW C mouse button %i, %i, %i\n", button, action, mods);
    mouse_button_stream.push_back(button);
    mouse_button_stream.push_back(action);
    mouse_button_stream.push_back(mods);
}

static void
key_callback(GLFWwindow* window, int key, int scancode, int action, int mods)
{
    //printf ( "GLFW C key %i, %i, %i, %i\n", key, scancode, action, mods);
    key_stream.push_back(key);
    key_stream.push_back(scancode);
    key_stream.push_back(action);
    key_stream.push_back(mods);
}

static void
error_callback(int error, const char* description)
{
    printf("GLFW error %d : %s\n", error, description);
}

// exports to c#
ZEROPLAYER_EXPORT
bool ZEROPLAYER_CALL init_glfw(int width, int height) {
    glfwSetErrorCallback(error_callback);

    if (mainWindow)
        return false;
    printf ( "GLFW C Init %i, %i\n", width, height);
    if (!glfwInit()) {
        printf ( "GLFW init failed.\n" );
        return false;
    }
    glfwWindowHint(GLFW_CLIENT_API, GLFW_OPENGL_API);
    glfwWindowHint (GLFW_CONTEXT_VERSION_MAJOR, 3);
    glfwWindowHint (GLFW_CONTEXT_VERSION_MINOR, 3);
    glfwWindowHint (GLFW_OPENGL_FORWARD_COMPAT, GL_TRUE);
    glfwWindowHint (GLFW_OPENGL_PROFILE, GLFW_OPENGL_CORE_PROFILE);
    windowW = width;
    windowH = height;
    mainWindow = glfwCreateWindow(width, height, "Unity - DOTS Project", NULL, NULL);
    if (!mainWindow) {
        printf ( "GLFW window creation failed.\n" );
        return false;
    }
    glfwMakeContextCurrent(mainWindow);

    int fbw, fbh;
    glfwGetFramebufferSize(mainWindow, &fbw, &fbh);
    printf ( "GLFW framebuffer size %i, %i\n", fbw, fbh);

    if (glewInit() != GL_NO_ERROR) {
        printf ( "GLEW init failed.\n" );
        return false;
    }
    printf("OpenGL: %s, %s, %s\n", glGetString(GL_VENDOR), glGetString(GL_RENDERER), glGetString(GL_VERSION));

    glfwSetWindowCloseCallback(mainWindow, window_close_callback);
    //glfwSetWindowUserPointer(mainWindow, this);
    glfwSetWindowSizeCallback(mainWindow, window_size_callback);

    return true;
}

ZEROPLAYER_EXPORT
GLFWwindow * ZEROPLAYER_CALL getwindow_glfw() {
    return mainWindow;
}

ZEROPLAYER_EXPORT
void ZEROPLAYER_CALL getWindowSize_glfw(int *width, int *height) {
    if (!mainWindow)
    {
        *width = 0;
        *height = 0;
        return;
    }
    glfwGetWindowSize(mainWindow, width, height);
    windowW = *width;
    windowH = *height;
}

ZEROPLAYER_EXPORT
void ZEROPLAYER_CALL getWindowFrameSize_glfw(int *left, int *top, int *right, int *bottom) {
    if (!mainWindow)
    {
        *left = 0;
        *top = 0;
        *right = 0;
        *bottom = 0;
        return;
    }
    glfwGetWindowFrameSize(mainWindow, left, top, right, bottom);
}

ZEROPLAYER_EXPORT
void ZEROPLAYER_CALL getScreenSize_glfw(int *width, int *height) {
    if (!mainWindow)
    {
        *width = 0;
        *height = 0;
        return;
    }
    GLFWmonitor* monitor = glfwGetWindowMonitor (mainWindow);
    if (!monitor)
        monitor = glfwGetPrimaryMonitor();
    const GLFWvidmode* mode = glfwGetVideoMode(monitor);
    *width = mode->width;
    *height = mode->height;
}

ZEROPLAYER_EXPORT
void ZEROPLAYER_CALL getFramebufferSize_glfw(int *width, int *height) {
    if (!mainWindow)
    {
        *width = 0;
        *height = 0;
        return;
    }
    glfwGetFramebufferSize(mainWindow, width, height);
}

ZEROPLAYER_EXPORT
void ZEROPLAYER_CALL shutdown_glfw(int exitCode) {
    if (mainWindow) {
        glfwDestroyWindow(mainWindow);
        mainWindow = 0;
    }
    glfwTerminate();
    #ifndef WIN32
    exit(0); // no cleanup, needed for mac via mono?
    #endif
}

ZEROPLAYER_EXPORT
void ZEROPLAYER_CALL resize_glfw(int width, int height) {
    glfwSetWindowSize(mainWindow, width, height);
    windowW = width;
    windowH = height;
}

ZEROPLAYER_EXPORT
bool ZEROPLAYER_CALL messagePump_glfw() {
    if (!mainWindow || shouldClose)
        return false;
    glfwMakeContextCurrent(mainWindow);
    glfwPollEvents();
    return !shouldClose;
}

ZEROPLAYER_EXPORT
void ZEROPLAYER_CALL swapBuffers_glfw() {
    if (!mainWindow || shouldClose)
        return;
    glfwMakeContextCurrent(mainWindow);
    glfwSwapBuffers(mainWindow);
}

ZEROPLAYER_EXPORT
void ZEROPLAYER_CALL debugClear_glfw() {
    if (!mainWindow)
        return;
    glfwMakeContextCurrent(mainWindow);
    glClearColor ( 1, (float)fabs(sin(glfwGetTime())),0,1 );
    glClear(GL_COLOR_BUFFER_BIT);
}

ZEROPLAYER_EXPORT
void ZEROPLAYER_CALL debugReadback_glfw(int w, int h, void *pixels) {
    if (!mainWindow || w>windowW || h>windowH)
        return;
    glfwMakeContextCurrent(mainWindow);
    glReadPixels(0,0,w,h,GL_RGBA,GL_UNSIGNED_BYTE,pixels);
}

ZEROPLAYER_EXPORT
double ZEROPLAYER_CALL time_glfw() {
    return glfwGetTime();
}

// exports to c#
ZEROPLAYER_EXPORT
bool ZEROPLAYER_CALL init_glfw_input(GLFWwindow *window) {
    printf ( "GLFW C input Init\n");
    if (!window) {
        printf ( "GLFW C input init failed.\n" );
        return false;
    }
    glfwSetKeyCallback(window, key_callback);
    glfwSetCursorPosCallback(window, cursor_position_callback);
    glfwSetMouseButtonCallback(window, mouse_button_callback);
    return true;
}

ZEROPLAYER_EXPORT
void ZEROPLAYER_CALL reset_glfw_input()
{
    mouse_pos_stream.clear();
    mouse_button_stream.clear();
    key_stream.clear();
}

ZEROPLAYER_EXPORT
const int * ZEROPLAYER_CALL get_mouse_pos_stream_glfw_input(int *len) {
    *len = (int)mouse_pos_stream.size();
    return mouse_pos_stream.data();
}

ZEROPLAYER_EXPORT
const int * ZEROPLAYER_CALL get_mouse_button_stream_glfw_input(int *len) {
    *len = (int)mouse_button_stream.size();
    return mouse_button_stream.data();
}

ZEROPLAYER_EXPORT
const int * ZEROPLAYER_CALL get_key_stream_glfw_input(int *len) {
    *len = (int)key_stream.size();
    return key_stream.data();
}

