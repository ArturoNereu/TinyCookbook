using System;
using System.Runtime.InteropServices;
using Unity.Tiny.Input;
using Unity.Entities;
using Unity.Tiny.Core2D;
using Unity.Tiny.HTML;

namespace Unity.Tiny.HTML
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [UpdateAfter(typeof(HTMLWindowSystem))]
    public class HTMLInputSystem : InputSystem
    {
        private bool initialized = false;
        protected override void OnStartRunning()
        {
            base.OnStartRunning();
            if (initialized)
                return;

            // must init after window
            InputHTMLNativeCalls.JSInitInput();
            firstTouch = -1;
            initialized = true;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }

        private const int maxStreamLen = 1024;
        private int[] streamBuf = new int[maxStreamLen];
        private int firstTouch = -1;

        protected override void OnUpdate()
        {
            // handle lost/reset
            if (InputHTMLNativeCalls.JSGetCanvasLost())
            {
                m_inputState.Clear();
                InputHTMLNativeCalls.JSResetStreams();
                InputHTMLNativeCalls.JSInitInput();
                firstTouch = -1;
                return;
            }

            if (InputHTMLNativeCalls.JSGetFocusLost())
            {
                m_inputState.Clear();
                InputHTMLNativeCalls.JSResetStreams();
                firstTouch = -1;
                return;
            }

            // advance input state
            base.OnUpdate();

            // keys
            int keyStreamLen;
            unsafe { fixed (int* ptr = streamBuf) { keyStreamLen = InputHTMLNativeCalls.JSGetKeyStream(streamBuf.Length, ptr); }}

            for (int i = 0; i < keyStreamLen; i += 2)
            {
                int action = streamBuf[i];
                int key = streamBuf[i + 1];
                KeyCode translatedKey = TranslateKey(key);
                if (translatedKey == KeyCode.None)
                    continue;
                if (action == 0)
                    m_inputState.KeyUp(translatedKey);
                else if (action == 1)
                    m_inputState.KeyDown(translatedKey);
            }

            // mouse (move + up/down)
            int mouseStreamLen;
            unsafe { fixed (int* ptr = streamBuf) { mouseStreamLen = InputHTMLNativeCalls.JSGetMouseStream(streamBuf.Length, ptr); }}

            for (int i = 0; i < mouseStreamLen; i += 4)
            {
                int ev = streamBuf[i];
                int button = streamBuf[i + 1];
                int x = streamBuf[i + 2];
                int y = streamBuf[i + 3];
                if (ev == 0)
                    m_inputState.MouseUp(button);
                else if (ev == 1)
                    m_inputState.MouseDown(button);
                m_inputState.mouseX = x;
                m_inputState.mouseY = y;
            }

            if (mouseStreamLen != 0)
                m_inputState.hasMouse = true;

            // touches
            int touchStreamLen;
            unsafe { fixed (int* ptr = streamBuf) { touchStreamLen = InputHTMLNativeCalls.JSGetTouchStream(streamBuf.Length, ptr); }}
            for (int i = 0; i < touchStreamLen; i += 4)
            {
                int ev = streamBuf[i];
                int id = streamBuf[i + 1];
                int x = streamBuf[i + 2];
                int y = streamBuf[i + 3];
                TouchState phase;
                switch (ev)
                {
                    case 0:
                        phase = TouchState.Ended;
                        break;
                    case 1:
                        phase = TouchState.Began;
                        break;
                    case 2:
                        phase = TouchState.Moved;
                        break;
                    case 3:
                        phase = TouchState.Canceled;
                        break;
                    default:
                        continue;
                }
                m_inputState.TouchEvent(id, phase, x, y);

                // simulate mouse from touch as well
                if (!m_inputState.hasMouse)
                {
                    if (ev == 0 || ev == 3)
                    {
                        if (firstTouch == id)
                        {
                            m_inputState.MouseUp(0);
                            firstTouch = -1;
                        }
                    }
                    else if (ev == 1)
                    {
                        if (firstTouch == -1)
                        {
                            firstTouch = id;
                            m_inputState.MouseDown(0);
                        }
                    }

                    if (firstTouch == id)
                    {
                        m_inputState.mouseX = x;
                        m_inputState.mouseY = y;
                    }
                }
            }
            InputHTMLNativeCalls.JSResetStreams();
        }

        private const int DOM_VK_CANCEL = 0x03;
        private const int DOM_VK_HELP = 0x06;
        private const int DOM_VK_BACK_SPACE = 0x08;
        private const int DOM_VK_TAB = 0x09;
        private const int DOM_VK_CLEAR = 0x0C;
        private const int DOM_VK_RETURN = 0x0D;
        private const int DOM_VK_ENTER = 0x0E;
        private const int DOM_VK_SHIFT = 0x10;
        private const int DOM_VK_CONTROL = 0x11;
        private const int DOM_VK_ALT = 0x12;
        private const int DOM_VK_PAUSE = 0x13;
        private const int DOM_VK_CAPS_LOCK = 0x14;
        private const int DOM_VK_KANA = 0x15;
        private const int DOM_VK_HANGUL = 0x15;
        private const int DOM_VK_EISU = 0x16;
        private const int DOM_VK_JUNJA = 0x17;
        private const int DOM_VK_FINAL = 0x18;
        private const int DOM_VK_HANJA = 0x19;
        private const int DOM_VK_KANJI = 0x19;
        private const int DOM_VK_ESCAPE = 0x1B;
        private const int DOM_VK_CONVERT = 0x1C;
        private const int DOM_VK_NONCONVERT = 0x1D;
        private const int DOM_VK_ACCEPT = 0x1E;
        private const int DOM_VK_MODECHANGE = 0x1F;
        private const int DOM_VK_SPACE = 0x20;
        private const int DOM_VK_PAGE_UP = 0x21;
        private const int DOM_VK_PAGE_DOWN = 0x22;
        private const int DOM_VK_END = 0x23;
        private const int DOM_VK_HOME = 0x24;
        private const int DOM_VK_LEFT = 0x25;
        private const int DOM_VK_UP = 0x26;
        private const int DOM_VK_RIGHT = 0x27;
        private const int DOM_VK_DOWN = 0x28;
        private const int DOM_VK_SELECT = 0x29;
        private const int DOM_VK_PRINT = 0x2A;
        private const int DOM_VK_EXECUTE = 0x2B;
        private const int DOM_VK_PRINTSCREEN = 0x2C;
        private const int DOM_VK_INSERT = 0x2D;
        private const int DOM_VK_DELETE = 0x2E;
        private const int DOM_VK_0 = 0x30;
        private const int DOM_VK_1 = 0x31;
        private const int DOM_VK_2 = 0x32;
        private const int DOM_VK_3 = 0x33;
        private const int DOM_VK_4 = 0x34;
        private const int DOM_VK_5 = 0x35;
        private const int DOM_VK_6 = 0x36;
        private const int DOM_VK_7 = 0x37;
        private const int DOM_VK_8 = 0x38;
        private const int DOM_VK_9 = 0x39;
        private const int DOM_VK_COLON = 0x3A;
        private const int DOM_VK_SEMICOLON = 0x3B;
        private const int DOM_VK_LESS_THAN = 0x3C;
        private const int DOM_VK_EQUALS = 0x3D;
        private const int DOM_VK_GREATER_THAN = 0x3E;
        private const int DOM_VK_QUESTION_MARK = 0x3F;
        private const int DOM_VK_AT = 0x40;
        private const int DOM_VK_A = 0x41;
        private const int DOM_VK_B = 0x42;
        private const int DOM_VK_C = 0x43;
        private const int DOM_VK_D = 0x44;
        private const int DOM_VK_E = 0x45;
        private const int DOM_VK_F = 0x46;
        private const int DOM_VK_G = 0x47;
        private const int DOM_VK_H = 0x48;
        private const int DOM_VK_I = 0x49;
        private const int DOM_VK_J = 0x4A;
        private const int DOM_VK_K = 0x4B;
        private const int DOM_VK_L = 0x4C;
        private const int DOM_VK_M = 0x4D;
        private const int DOM_VK_N = 0x4E;
        private const int DOM_VK_O = 0x4F;
        private const int DOM_VK_P = 0x50;
        private const int DOM_VK_Q = 0x51;
        private const int DOM_VK_R = 0x52;
        private const int DOM_VK_S = 0x53;
        private const int DOM_VK_T = 0x54;
        private const int DOM_VK_U = 0x55;
        private const int DOM_VK_V = 0x56;
        private const int DOM_VK_W = 0x57;
        private const int DOM_VK_X = 0x58;
        private const int DOM_VK_Y = 0x59;
        private const int DOM_VK_Z = 0x5A;
        private const int DOM_VK_WIN = 0x5B;
        private const int DOM_VK_CONTEXT_MENU = 0x5D;
        private const int DOM_VK_SLEEP = 0x5F;
        private const int DOM_VK_NUMPAD0 = 0x60;
        private const int DOM_VK_NUMPAD1 = 0x61;
        private const int DOM_VK_NUMPAD2 = 0x62;
        private const int DOM_VK_NUMPAD3 = 0x63;
        private const int DOM_VK_NUMPAD4 = 0x64;
        private const int DOM_VK_NUMPAD5 = 0x65;
        private const int DOM_VK_NUMPAD6 = 0x66;
        private const int DOM_VK_NUMPAD7 = 0x67;
        private const int DOM_VK_NUMPAD8 = 0x68;
        private const int DOM_VK_NUMPAD9 = 0x69;
        private const int DOM_VK_MULTIPLY = 0x6A;
        private const int DOM_VK_ADD = 0x6B;
        private const int DOM_VK_SEPARATOR = 0x6C;
        private const int DOM_VK_SUBTRACT = 0x6D;
        private const int DOM_VK_DECIMAL = 0x6E;
        private const int DOM_VK_DIVIDE = 0x6F;
        private const int DOM_VK_F1 = 0x70;
        private const int DOM_VK_F2 = 0x71;
        private const int DOM_VK_F3 = 0x72;
        private const int DOM_VK_F4 = 0x73;
        private const int DOM_VK_F5 = 0x74;
        private const int DOM_VK_F6 = 0x75;
        private const int DOM_VK_F7 = 0x76;
        private const int DOM_VK_F8 = 0x77;
        private const int DOM_VK_F9 = 0x78;
        private const int DOM_VK_F10 = 0x79;
        private const int DOM_VK_F11 = 0x7A;
        private const int DOM_VK_F12 = 0x7B;
        private const int DOM_VK_F13 = 0x7C;
        private const int DOM_VK_F14 = 0x7D;
        private const int DOM_VK_F15 = 0x7E;
        private const int DOM_VK_F16 = 0x7F;
        private const int DOM_VK_F17 = 0x80;
        private const int DOM_VK_F18 = 0x81;
        private const int DOM_VK_F19 = 0x82;
        private const int DOM_VK_F20 = 0x83;
        private const int DOM_VK_F21 = 0x84;
        private const int DOM_VK_F22 = 0x85;
        private const int DOM_VK_F23 = 0x86;
        private const int DOM_VK_F24 = 0x87;
        private const int DOM_VK_NUM_LOCK = 0x90;
        private const int DOM_VK_SCROLL_LOCK = 0x91;
        private const int DOM_VK_WIN_OEM_FJ_JISHO = 0x92;
        private const int DOM_VK_WIN_OEM_FJ_MASSHOU = 0x93;
        private const int DOM_VK_WIN_OEM_FJ_TOUROKU = 0x94;
        private const int DOM_VK_WIN_OEM_FJ_LOYA = 0x95;
        private const int DOM_VK_WIN_OEM_FJ_ROYA = 0x96;
        private const int DOM_VK_CIRCUMFLEX = 0xA0;
        private const int DOM_VK_EXCLAMATION = 0xA1;
        private const int DOM_VK_DOUBLE_QUOTE = 0xA3;
        private const int DOM_VK_HASH = 0xA3;
        private const int DOM_VK_DOLLAR = 0xA4;
        private const int DOM_VK_PERCENT = 0xA5;
        private const int DOM_VK_AMPERSAND = 0xA6;
        private const int DOM_VK_UNDERSCORE = 0xA7;
        private const int DOM_VK_OPEN_PAREN = 0xA8;
        private const int DOM_VK_CLOSE_PAREN = 0xA9;
        private const int DOM_VK_ASTERISK = 0xAA;
        private const int DOM_VK_PLUS = 0xAB;
        private const int DOM_VK_PIPE = 0xAC;
        private const int DOM_VK_HYPHEN_MINUS = 0xAD;
        private const int DOM_VK_OPEN_CURLY_BRACKET = 0xAE;
        private const int DOM_VK_CLOSE_CURLY_BRACKET = 0xAF;
        private const int DOM_VK_TILDE = 0xB0;
        private const int DOM_VK_VOLUME_MUTE = 0xB5;
        private const int DOM_VK_VOLUME_DOWN = 0xB6;
        private const int DOM_VK_VOLUME_UP = 0xB7;
        private const int DOM_VK_COMMA = 0xBC;
        private const int DOM_VK_PERIOD = 0xBE;
        private const int DOM_VK_SLASH = 0xBF;
        private const int DOM_VK_BACK_QUOTE = 0xC0;
        private const int DOM_VK_OPEN_BRACKET = 0xDB;
        private const int DOM_VK_BACK_SLASH = 0xDC;
        private const int DOM_VK_CLOSE_BRACKET = 0xDD;
        private const int DOM_VK_QUOTE = 0xDE;
        private const int DOM_VK_META = 0xE0;
        private const int DOM_VK_ALTGR = 0xE1;
        private const int DOM_VK_WIN_ICO_HELP = 0xE3;
        private const int DOM_VK_WIN_ICO_00 = 0xE4;
        private const int DOM_VK_WIN_ICO_CLEAR = 0xE6;
        private const int DOM_VK_WIN_OEM_RESET = 0xE9;
        private const int DOM_VK_WIN_OEM_JUMP = 0xEA;
        private const int DOM_VK_WIN_OEM_PA1 = 0xEB;
        private const int DOM_VK_WIN_OEM_PA2 = 0xEC;
        private const int DOM_VK_WIN_OEM_PA3 = 0xED;
        private const int DOM_VK_WIN_OEM_WSCTRL = 0xEE;
        private const int DOM_VK_WIN_OEM_CUSEL = 0xEF;
        private const int DOM_VK_WIN_OEM_ATTN = 0xF0;
        private const int DOM_VK_WIN_OEM_FINISH = 0xF1;
        private const int DOM_VK_WIN_OEM_COPY = 0xF2;
        private const int DOM_VK_WIN_OEM_AUTO = 0xF3;
        private const int DOM_VK_WIN_OEM_ENLW = 0xF4;
        private const int DOM_VK_WIN_OEM_BACKTAB = 0xF5;
        private const int DOM_VK_ATTN = 0xF6;
        private const int DOM_VK_CRSEL = 0xF7;
        private const int DOM_VK_EXSEL = 0xF8;
        private const int DOM_VK_EREOF = 0xF9;
        private const int DOM_VK_PLAY = 0xFA;
        private const int DOM_VK_ZOOM = 0xFB;
        private const int DOM_VK_PA1 = 0xFD;
        private const int DOM_VK_WIN_OEM_CLEAR = 0xFE;

        static KeyCode TranslateKey(int htmlKeyCode)
        {
            switch (htmlKeyCode)
            {
                case DOM_VK_UP:
                    return KeyCode.UpArrow;
                case DOM_VK_DOWN:
                    return KeyCode.DownArrow;
                case DOM_VK_LEFT:
                    return KeyCode.LeftArrow;
                case DOM_VK_RIGHT:
                    return KeyCode.RightArrow;
                case DOM_VK_RETURN:
                    return KeyCode.Return;
                case DOM_VK_SPACE:
                    return KeyCode.Space;
                case DOM_VK_0:
                    return KeyCode.Alpha0;
                case DOM_VK_1:
                    return KeyCode.Alpha1;
                case DOM_VK_2:
                    return KeyCode.Alpha2;
                case DOM_VK_3:
                    return KeyCode.Alpha3;
                case DOM_VK_4:
                    return KeyCode.Alpha4;
                case DOM_VK_5:
                    return KeyCode.Alpha5;
                case DOM_VK_6:
                    return KeyCode.Alpha6;
                case DOM_VK_7:
                    return KeyCode.Alpha7;
                case DOM_VK_8:
                    return KeyCode.Alpha8;
                case DOM_VK_9:
                    return KeyCode.Alpha9;
                case DOM_VK_A:
                    return KeyCode.A;
                case DOM_VK_B:
                    return KeyCode.B;
                case DOM_VK_C:
                    return KeyCode.C;
                case DOM_VK_D:
                    return KeyCode.D;
                case DOM_VK_E:
                    return KeyCode.E;
                case DOM_VK_F:
                    return KeyCode.F;
                case DOM_VK_G:
                    return KeyCode.G;
                case DOM_VK_H:
                    return KeyCode.H;
                case DOM_VK_I:
                    return KeyCode.I;
                case DOM_VK_J:
                    return KeyCode.J;
                case DOM_VK_K:
                    return KeyCode.K;
                case DOM_VK_L:
                    return KeyCode.L;
                case DOM_VK_M:
                    return KeyCode.M;
                case DOM_VK_N:
                    return KeyCode.N;
                case DOM_VK_O:
                    return KeyCode.O;
                case DOM_VK_P:
                    return KeyCode.P;
                case DOM_VK_Q:
                    return KeyCode.Q;
                case DOM_VK_R:
                    return KeyCode.R;
                case DOM_VK_S:
                    return KeyCode.S;
                case DOM_VK_T:
                    return KeyCode.T;
                case DOM_VK_U:
                    return KeyCode.U;
                case DOM_VK_V:
                    return KeyCode.V;
                case DOM_VK_W:
                    return KeyCode.W;
                case DOM_VK_X:
                    return KeyCode.X;
                case DOM_VK_Y:
                    return KeyCode.Y;
                case DOM_VK_Z:
                    return KeyCode.Z;
            }

            return KeyCode.None;
        }
    }

    static class InputHTMLNativeCalls
    {
        // directly calls out to JS!
        [DllImport("lib_unity_tiny_inputhtml", EntryPoint = "js_inputInit")]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool JSInitInput();

        [DllImport("lib_unity_tiny_inputhtml", EntryPoint = "js_inputResetStreams")]
        public static extern void JSResetStreams();

        [DllImport("lib_unity_tiny_inputhtml", EntryPoint = "js_inputGetKeyStream")]
        public static extern unsafe int JSGetKeyStream(int maxLen, int* dest);

        [DllImport("lib_unity_tiny_inputhtml", EntryPoint = "js_inputGetMouseStream")]
        public static extern unsafe int JSGetMouseStream(int maxLen, int* dest);

        [DllImport("lib_unity_tiny_inputhtml", EntryPoint = "js_inputGetTouchStream")]
        public static extern unsafe int JSGetTouchStream(int maxLen, int* dest);

        [DllImport("lib_unity_tiny_inputhtml", EntryPoint = "js_inputGetCanvasLost")]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool JSGetCanvasLost();

        [DllImport("lib_unity_tiny_inputhtml", EntryPoint = "js_inputGetFocusLost")]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool JSGetFocusLost();
    }
}
