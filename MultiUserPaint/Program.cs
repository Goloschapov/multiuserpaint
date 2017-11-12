using Fleck;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MultiUserPaint
{
    class Program
    {
        public static List<string> Canvas = new List<string>();
        static Mutex mutexObj = new Mutex(); 

        static void Main(string[] args)
        {
            GenerateStartArray();
            var socket = new WebSocketServer("ws://127.0.0.1:8081/");
            
            IWebSocketConnection connection = null;

            socket.Start(conn =>
            {
                conn.OnOpen = () =>
                {
                    connection = conn;
                };

                conn.OnMessage = message =>
                {
                    if (message == "ping test")
                    {
                        conn.Send("pong test");
                    }

                    Refresh(message);
                    conn.Send(GetCanvas());
                };
                conn.OnClose = () =>
                {
                    // ...
                };
            });

            if (connection != null)
            {
                connection.Send("Messaggio di esempio");
            }

            Console.Read();
        }

        static void GenerateStartArray()
        {
            for (int i = 0; i < 100; i++)
                Canvas.Add("FFFFFF");
        }

        static void Refresh(string message)
        {
            mutexObj.WaitOne();
            string[] pixel = message.Split('|');
            for (int i = 0; i < pixel.Length; i++)
                Canvas[Convert.ToInt32(pixel[i].Split(':')[0])] = pixel[i].Split(':')[1];
            mutexObj.ReleaseMutex();
        }

        static string GetCanvas() 
        {
            string text = "";
            foreach (string pixel in Canvas)
                text += pixel + "|";
            return text.Trim('|');
        }
    }
}
