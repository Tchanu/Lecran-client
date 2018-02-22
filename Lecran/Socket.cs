using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Quobject.EngineIoClientDotNet.ComponentEmitter;
using Quobject.SocketIoClientDotNet.Client;
using Quobject.SocketIoClientDotNet.Parser;

namespace Lecran
{
    class Socket
    {
        public bool isConnected = false;
        private dynamic machine = new JObject();
        private Quobject.SocketIoClientDotNet.Client.Socket socket;
        private ScreenCapture screenshot = new ScreenCapture();
        private Int64 quality = 20L;
        private MainWindow window;

        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;
        private const int MOUSEEVENTF_RIGHTDOWN = 0x08;
        private const int MOUSEEVENTF_RIGHTUP = 0x10; //
        public const int KEYEVENTF_EXTENDEDKEY = 0x0001; //Key down flag
        public const int KEYEVENTF_KEYUP = 0x0002; //Key up flag
        public const int VK_LCONTROL = 0xA2; //Left Control key code
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo);
        [DllImport("user32.dll", SetLastError = true)]
        static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);



        public Socket(MainWindow window)
        {
            this.window = window;
            socket = IO.Socket("http://localhost:3003");
            socket.Connect();
        }

        public void Init()
        {
            machine.client = "Machine";
            machine.ScreenHeight = "0";
            machine.ScreenWidth = "0";

            socket.On("connect", () =>
            {
                socket.Emit("init", machine);
                socket.On("init", ServerInit);
            });
            socket.On("disconnect", () =>
            {
                window.Dispatcher.BeginInvoke((Action)(() =>
                {
                    window.Disconnect();
                }));
            });
        }

        //Client Connected
        public void ServerInit(object data)
        {
            isConnected = true;
            window.Dispatcher.BeginInvoke((Action)(() =>
            {
                window.Connect(((JObject)data).GetValue("machine_id").ToString());
            }));

            socket.On("clientconnect", (res) =>
            {
                window.Dispatcher.BeginInvoke((Action)(() =>
                {
                    window.ClientConnect();
                }));
                Thread t = new Thread(this.Loop);
                t.Start();
            });
        }

        public void Send()
        {
            var capture = screenshot.Capture(quality);
            if (capture != null)
            {
                socket.Emit("frame", capture.ToArray());
            }
        }
        public void Loop()
        {
            initListeners();
            while (true)
            {
                try
                {
                    System.Threading.Thread.Sleep(200);
                    Send();
                }
                catch (ThreadAbortException)
                {
                    Console.WriteLine("Thread Abort Exception");
                }
            }
        }

        public void initListeners()
        {
            socket.On("mousemove", (positionData) =>
            {
                MoveMouseTo(getMousePosition((JObject)positionData));
            });

            socket.On("mousedown", (positionData) =>
            {
                Tuple<Int16, Int16> positoin = getMousePosition((JObject)positionData);
                MoveMouseTo(positoin);
                mouse_event(MOUSEEVENTF_LEFTDOWN, (uint)positoin.Item1, (uint)positoin.Item2, 0, 0);
            });

            socket.On("mouseup", (positionData) =>
            {
                Tuple<Int16, Int16> positoin = getMousePosition((JObject)positionData);
                MoveMouseTo(positoin);
                mouse_event(MOUSEEVENTF_LEFTUP, (uint)positoin.Item1, (uint)positoin.Item2, 0, 0);
            });

            socket.On("keypress", (keypressData) =>
            {
                byte data = byte.Parse((string)((JObject)keypressData).GetValue("key"));
                PressKey(data);
            });
            socket.On("settings", (settingsData) =>
            {
                quality = Int64.Parse((string)((JObject)settingsData).GetValue("quality"));
                Console.WriteLine(quality);
            });


        }

        public Tuple<Int16, Int16> getMousePosition(JObject coordinates)
        {
            Int16 x = Convert.ToInt16(Screen.PrimaryScreen.Bounds.Width * Double.Parse(coordinates.GetValue("x").ToString()));
            Int16 y = Convert.ToInt16(Screen.PrimaryScreen.Bounds.Height * Double.Parse(coordinates.GetValue("y").ToString()));
            return Tuple.Create(x, y);
        }
        public void MoveMouseTo(Tuple<Int16, Int16> position)
        {
            Win32.POINT p = new Win32.POINT();
            p.x = position.Item1;
            p.y = position.Item2;

            Win32.ClientToScreen(this.Handle, ref p);
            Win32.SetCursorPos(p.x, p.y);
        }

        public void PressKey(byte key)
        {
            Console.WriteLine(key);
            keybd_event(key, 0, KEYEVENTF_EXTENDEDKEY, 0);
            keybd_event(key, 0, KEYEVENTF_KEYUP, 0);
        }

        public IntPtr Handle { get; set; }
    }
}
