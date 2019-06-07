// A message is structured as follow:
//   -- 1 message name --
//   4 bytes for name length (int32)
//   N bytes for name data (bytes)
//   -- N message data --
//   4 bytes for data length (int32)
//   N bytes for data (bytes)
//   ...
//   -- last message data --
//   4 bytes for data length (int32, must be equal to 0)
//   0 bytes for data
//
// The message name is an ASCII string (8-bits per character, no support for
// UTF-8 necessary)
//
// The message data is raw bytes, can store anything.
//
// So a message must always have a name, this is used as an identifier for
// commands and tagging what the data contains. However, it is perfectly fine
// for a message to have no data attached, which means the data length bytes
// should be present, but set to zero. It is also perfectly fine to have more
// than one data block attached.
ut._wsclient = ut._wsclient || {};
(function (WSClient) {
    var ServerURL = Module.WSServerURL;
    var LastConnectionState = WebSocket.CLOSED;
    var Socket = null;
    var Buffers = [];

    function Log(msg) {
        console.log("[LiveLink] " + msg);
    }

    function LogError(msg) {
        console.error("[LiveLink] error: " + msg);
    }

    function BufferToString(buffer) {
        return String.fromCharCode.apply(null, buffer);
    }

    function StringToBuffer(str) {
        var buffer = new Uint8Array(str.length);
        for (var i = 0, length = str.length; i < length; ++i)
            buffer[i] = str.charCodeAt(i);
        return buffer;
    }

    function BrowserInfo() {
        var ua = navigator.userAgent, tem, M = ua.match(/(opera|chrome|safari|firefox|msie|trident(?=\/))\/?\s*(\d+)/i) || [];
        if (/trident/i.test(M[1])) {
            tem = /\brv[ :]+(\d+)/g.exec(ua) || [];
            return 'IE ' + (tem[1] || '');
        }
        if (M[1] === 'Chrome') {
            tem = ua.match(/\b(OPR|Edge)\/(\d+)/);
            if (tem != null)
                return tem.slice(1).join(' ').replace('OPR', 'Opera');
        }
        M = M[2] ? [M[1], M[2]] : [navigator.appName, navigator.appVersion, '-?'];
        if ((tem = ua.match(/version\/(\d+)/i)) != null)
            M.splice(1, 1, tem[1]);
        return M.join(' ');
    }

    function EncodeSize(buffer) {
        var arraybuf = new ArrayBuffer(buffer.byteLength + 4);
        var result = new Uint8Array(arraybuf);
        new DataView(arraybuf, 0, 4).setInt32(0, buffer.byteLength);
        result.set(buffer, 4);
        return result;
    }

    function EncodeMessage(name, buffer) {
        var nameBuffer = EncodeSize(StringToBuffer(name));
        var dataBuffer = EncodeSize(buffer);
        var result = new Uint8Array(nameBuffer.byteLength + dataBuffer.byteLength + 4);
        result.set(nameBuffer, 0);
        result.set(dataBuffer, nameBuffer.byteLength);
        result.set(new Uint8Array(4).fill(0), nameBuffer.byteLength + dataBuffer.byteLength);
        return result;
    }

    function Decode(arraybuf) {
        var bytesRead = 0;
        var bytes = new Uint8Array(arraybuf);
        while (bytesRead < bytes.byteLength) {
            // Check if we have an incomplete buffer
            var buffer = Buffers[Buffers.length - 1];
            if (buffer == null || buffer.index == buffer.bytes.byteLength) {
                // Check if we have enough bytes to read data size
                if (bytes.byteLength - bytesRead < 4)
                    break;

                // Get data size
                var size = new DataView(arraybuf, bytesRead, 4).getInt32(0);
                if (size > 1024 * 1024 * 16) {
                    LogError("Message data size (" + size + ") is too large (max " + (1024 * 1024 * 16) + " bytes).");
                    return;
                }
                if (size < 0) {
                    LogError("Message data size (" + size + ") is negative.");
                    return;
                }
                bytesRead += 4;

                // Check if its a new buffer or the end of a message
                if (size > 0) {
                    // We have data, add a new buffer
                    buffer = {
                        index: 0,
                        bytes: new Uint8Array(size)
                    };
                    Buffers.push(buffer);
                } else {
                    // No data means end of message
                    var buffers = [];
                    for (var i = 0; i < Buffers.length; ++i)
                        buffers.push(Buffers[i].bytes);
                    Buffers = [];
                    return {
                        buffers: buffers
                    };
                }
            }

            // Add data to existing buffer
            var length = Math.min(bytes.byteLength - bytesRead, buffer.bytes.byteLength - buffer.index);
            var data = bytes.slice(bytesRead, bytesRead + length);
            buffer.bytes.set(data, buffer.index);
            buffer.index += length;
            bytesRead += length;
        }
        return {
            buffers: []
        };
    }

    function KeepAlive() {
        if (Socket != null && Socket.readyState == WebSocket.OPEN) {
            Socket.send(new Uint8Array([255]).buffer);
            setTimeout(function () {
                KeepAlive();
            }, 30 * 1000);
        }
    }

    function Send(message) {
        var byteIndex = 0;
        var bytes = message.byteLength;
        while (byteIndex < bytes) {
            var length = Math.min(bytes - byteIndex, 1024);
            Socket.send(message.slice(byteIndex, byteIndex + length));
            byteIndex += 1024;
        }
    }

    function SendMessage(name, data) {
        //Log("sending message of " + data.byteLength + " bytes");
        Send(EncodeMessage(name, data));
    }

    function ProcessCommand(command, data) {
        //Log("received command '" + command + "'");
        var world = WSClient.world ? WSClient.world : null;
        var scheduler = world ? world.scheduler() : null;
        if (command === "reload") {
            if (location.hostname === "localhost" || location.hostname === "127.0.0.1" || window.location.protocol === 'file:') {
                location.href = BufferToString(data.buffers[1]);
            } else {
                location.reload(true);
            }
        } else if (command === "getConnectionInfo") {
            SendMessage("connectionInfo", StringToBuffer(BrowserInfo()));
        } else if (command === "getWorldState") {
            if (world) {
                var jsonWorldState = world.toJSON();
                SendMessage("worldState", StringToBuffer(jsonWorldState));
            }
        } else if (command === "setWorldState") {
            if (world) {
                world.fromJSON(BufferToString(data.buffers[1]));
                SendMessage("worldStateLoaded");
            }
        } else if (command === "pause") {
            if (scheduler) {
                scheduler.pause();
                var waitForPause = function () {
                    if (!scheduler.isPaused()) {
                        setTimeout(waitForPause, 1);
                        return;
                    }
                    SendMessage("pauseState", new Uint8Array([1]));
                };
                waitForPause();
            }
        } else if (command === "isPaused") {
            if (scheduler) {
                var paused = scheduler.isPaused();
                SendMessage("pauseState", new Uint8Array([paused]));
            }
        } else if (command === "step") {
            if (scheduler) {
                scheduler.step();
            }
        } else if (command === "resume") {
            if (scheduler) {
                scheduler.resume();
            }
        } else {
            LogError("received unknown command '" + command + "'");
        }
    }

    WSClient.Connect = function () {
        if (LastConnectionState == WebSocket.CLOSED) {
            Log("connecting...");
            LastConnectionState = WebSocket.CONNECTING;
        }
        Socket = new WebSocket(ServerURL, ['unity.tiny']);
        Socket.binaryType = 'arraybuffer';

        // On connected event
        Socket.onopen = function () {
            if (LastConnectionState != WebSocket.OPEN) {
                Log("connected");
                LastConnectionState = WebSocket.OPEN;
            }

            // On data received event
            Socket.onmessage = function (message) {
                //Log("received message of " + message.data.byteLength + " bytes");
                var data = Decode(message.data);
                if (data.buffers.length == 0)
                    return;

                var command = BufferToString(data.buffers[0]);
                ProcessCommand(command, data);
            };

            // Start keep-alive ping to prevent connection timeout
            KeepAlive();
        };

        // On connection closed event
        Socket.onclose = function () {
            if (LastConnectionState == WebSocket.OPEN) {
                Log("connection closed");
                LastConnectionState = WebSocket.CLOSED;
            }
            // Try to reconnect every second
            setTimeout(function () {
                WSClient.Connect();
            }, 1000);
        };
    };
})(ut._wsclient);

// Immediately try to connect
ut._wsclient.Connect();
