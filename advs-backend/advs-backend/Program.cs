using advs_backend;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Text.RegularExpressions;
using static System.Net.WebRequestMethods;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            Server(8080);
        }
        static void Server(int Port)
        {
            TcpListener server = new TcpListener(IPAddress.Any, Port);
            Console.WriteLine(server.LocalEndpoint);

            try
            {
                server.Start();
                Console.WriteLine("Сервер запущен. Ожидание подключений... ");

                while (true)
                {
                    TcpClient Client = server.AcceptTcpClient();
                    // Создаем поток
                    Thread Thread = new Thread(new ParameterizedThreadStart(ClientThread));
                    // И запускаем этот поток, передавая ему принятого клиента
                    Thread.Start(Client);
                }
            }
            catch
            {
                Console.WriteLine("Ошибка сервер");
            }
            finally
            {
                server.Stop(); // останавливаем сервер
                Console.WriteLine("Сервер остановлен");
            }
        }

        //private static string Win1251ToUTF8(string source)
        //{
        //    try
        //    {
        //        Encoding utf8 = Encoding.GetEncoding("UTF-8");
        //        Encoding win1251 = Encoding.GetEncoding("windows-1251");

        //        byte[] utf8Bytes = win1251.GetBytes(source);
        //        byte[] win1251Bytes = Encoding.Convert(utf8, win1251, utf8Bytes);
        //        source = win1251.GetString(win1251Bytes);
        //        return source;

        //    }
        //    catch(Exception e)
        //    {
        //        Console.WriteLine(e);
        //        return null;
        //    }

        //}

        static void ClientThread(Object StateInfo)
        {
            Client((TcpClient)StateInfo);
        }

        static void Client(TcpClient client)
        {
            Console.WriteLine($"Входящее подключение: {client.Client.RemoteEndPoint}");

            // Объявим строку, в которой будет хранится запрос клиента
            string Request = "";
            // получаем объект NetworkStream для взаимодействия с клиентом
            var stream = client.GetStream();
            //stream.ReadTimeout = 10000;
            // определяем буфер для получения данных
            byte[] buffer = new byte[1024];
            int bytes = 0; // количество считанных байтов
            while ((bytes = stream.Read(buffer)) > 0)
            {                
                //Console.WriteLine(bytes);
                Request += Encoding.UTF8.GetString(buffer, 0, bytes);
                //Console.WriteLine(Request);
                if (Request.IndexOf("\r\n\r\n") != -1) break;
            }
            //Console.WriteLine($"Запрос {client.Client.RemoteEndPoint}:\n" + Request);
            
            // Если запрос пустой - соединение завершается
            if (Request == "")
            {
                client.Close();
                return;
            }

            // Обработка HTTP запроса и создание ответа
            string Response = new HTTP(Request).Responce;

            // определяем данные для отправки
            byte[] data = Encoding.UTF8.GetBytes(Response);
            // отправляем данные
            client.GetStream().Write(data, 0, data.Length);
            Console.WriteLine($"Клиенту {client.Client.RemoteEndPoint} отправлены данные\n");

            Console.WriteLine($"Соединение {client.Client.RemoteEndPoint} закрывается\n");
            client.Close();
        }
    }
}

/*
IPEndPoint ipPoint = new IPEndPoint(IPAddress.Any, 8888);
using Socket tcpListener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

try
{
    tcpListener.Bind(ipPoint);
    tcpListener.Listen();    // запускаем сервер
    Console.WriteLine("Сервер запущен. Ожидание подключений... ");

    while (true)
    {
        // получаем входящее подключение
        using var tcpClient = await tcpListener.AcceptAsync();
        // определяем данные для отправки - текущее время
        byte[] data = Encoding.UTF8.GetBytes(DateTime.Now.ToLongTimeString());
        // отправляем данные
        tcpClient.Send(data);
        Console.WriteLine($"Клиенту {tcpClient.RemoteEndPoint} отправлены данные");
    }
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}
*/
/*
namespace Server
{   class Program
    {
        static void Main(string[] args)
        {
            Server(8080);
        }
        async static void Server(int Port)
        {
            IPEndPoint ipPoint = new IPEndPoint(IPAddress.Any, Port);
            using Socket tcpListener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                tcpListener.Bind(ipPoint);
                tcpListener.Listen();    // запускаем сервер
                Console.WriteLine("Сервер запущен. Ожидание подключений... ");

                while (true)
                {
                    // получаем входящее подключение
                    using var tcpClient = await tcpListener.AcceptAsync();
                    // определяем данные для отправки - текущее время
                    byte[] data = Encoding.UTF8.GetBytes(DateTime.Now.ToLongTimeString());
                    // отправляем данные
                    await tcpClient.SendAsync(data);
                    Console.WriteLine($"Клиенту {tcpClient.RemoteEndPoint} отправлены данные");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        
        /*
        async static void Server(int Port)
        {
            TcpListener server = new TcpListener(IPAddress.Any, Port);
            Console.WriteLine(server.LocalEndpoint);

            try
            {
                server.Start();
                Console.WriteLine("Сервер запущен. Ожидание подключений... ");

                while (true)
                {
                    using TcpClient client = server.AcceptTcpClient();
                    Console.WriteLine($"Входящее подключение: {client.Client.RemoteEndPoint}");

                    // определяем данные для отправки - текущее время
                    byte[] data = Encoding.UTF8.GetBytes(DateTime.Now.ToLongTimeString());
                    Console.WriteLine("Данные для отправки: " + data);
                    // отправляем данные
                    await client.SendAsync(data);
                    Console.WriteLine($"Клиенту {client.Client.RemoteEndPoint} отправлены данные");
                }
            }
            finally
            {
                server.Stop(); // останавливаем сервер
            }
        }
        *//*
    }
}

*/

/*
TcpListener server = new TcpListener(IPAddress.Any, 8080);
Console.WriteLine(server.LocalEndpoint);

try
{
    server.Start();
    Console.WriteLine("Сервер запущен. Ожидание подключений... ");

    while (true)
    {
        using TcpClient client = await server.AcceptTcpClientAsync();
        Console.WriteLine($"Входящее подключение: {client.Client.RemoteEndPoint}");
    }
}
finally
{
    server.Stop(); // останавливаем сервер
}
*/

/*
IPEndPoint ipPoint = new IPEndPoint(IPAddress.Any, 8888);
using Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
socket.Bind(ipPoint);   // связываем с локальной точкой ipPoint

// получаем конечную точку, с которой связан сокет
Console.WriteLine(socket.LocalEndPoint); // 0.0.0.0:8888

socket.Listen(1000);
Console.WriteLine("Сервер запущен. Ожидание подключений...");
// получаем входящее подключение
using Socket client = await socket.AcceptAsync();
// получаем адрес клиента
Console.WriteLine($"Адрес подключенного клиента: {client.RemoteEndPoint}");
*/