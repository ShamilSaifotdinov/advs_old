using Newtonsoft.Json;
using System.Text;
using System.Text.RegularExpressions;
using advs_backend.JSON;

namespace advs_backend
{
    struct HTTPHeaders
    {
        public string Method;
        //public string RealPath;
        public string URN;
        public string Body;
    }
    internal class HTTP
    {
        readonly string HTTPNotFound =
            "HTTP/1.1 404 Not Found\n"
            + "Access-Control-Allow-Origin: *\n";

        public string Responce;
        public HTTP(string Request)
        {
            HTTPHeaders header = Parse(Request);

            Console.WriteLine("URI: " + header.URN);
            string[] parsedURI = header.URN.TrimStart('/').Split('/');
            foreach (string uri in parsedURI) Console.WriteLine(uri);

            Console.WriteLine("\tРеестр: " + parsedURI[0]);

            string result;
            switch (parsedURI[0])
            {
                case "advs":
                    if (parsedURI[1] == "new")
                    {
                        Console.WriteLine("Данные для добавления: " + header.Body);
                        NewAdvJSON adv = JsonConvert.DeserializeObject<NewAdvJSON>(header.Body);
                        result = Database.AddAdv(adv);
                        Console.WriteLine("Данные для отправки: " + result);

                        Responce = HTTPresponce(result);
                        break;
                    }
                    else
                    {
                        try
                        {
                            Console.WriteLine("\tID: " + parsedURI[1]);
                            if (parsedURI.Length > 2) throw new Exception("Такой страницы нет!");
                            int ID = Int32.Parse(parsedURI[1]);
                            result = Database.GetAdv(ID);

                            Responce = HTTPresponce(result);
                            break;
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                            Responce = HTTPNotFound;
                            break;
                        }
                    }
                
                case "login":
                    try
                    {
                        Console.WriteLine("Данные для проверки: " + header.Body);
                        result = Database.Login(header.Body);
                        Console.WriteLine("Данные для отправки: " + result);

                        Responce = HTTPresponce(result);
                        break;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        Responce = HTTPNotFound;
                        break;
                    }

                default:
                    result = Database.Connection();
                    Console.WriteLine("Данные для отправки: " + result);

                    Responce = HTTPresponce(result);
                    break;
            }
        }
        static HTTPHeaders Parse(string headers)
        {
            HTTPHeaders result = new HTTPHeaders();
            result.Method = Regex.Match(headers, @"\A\w[a-zA-Z]+", RegexOptions.Multiline).Value;
            result.URN = Regex.Match(headers, @"(?<=\w\s)([\Wa-zA-Z0-9]+)(?=\sHTTP)", RegexOptions.Multiline).Value;
            //result.RealPath = $"{AppDomain.CurrentDomain.BaseDirectory}{result.File}";
            result.Body = headers.Split("\r\n\r\n")[1];

            //Console.WriteLine(result.Method);
            //Console.WriteLine(result.URI);
            //Console.WriteLine(result.RealPath);
            Console.WriteLine(result.Body);

            return result;
        }

        //static void HTTPParse(string Request)
        //{
        //    // Парсим строку запроса с использованием регулярных выражений
        //    // При этом отсекаем все переменные GET-запроса
        //    Match ReqMatch = Regex.Match(Request, @"^\w+\s+([^\s\?]+)[^\s]*\s+HTTP/.*|");

        //    //// Если запрос не удался
        //    //if (ReqMatch == Match.Empty)
        //    //{
        //    //    // Передаем клиенту ошибку 400 - неверный запрос
        //    //    SendError(Client, 400);
        //    //    return;
        //    //}

        //    // Получаем строку запроса
        //    string RequestUri = ReqMatch.Groups[1].Value;
        //    Console.WriteLine(RequestUri);
        //    // Приводим ее к изначальному виду, преобразуя экранированные символы
        //    // Например, "%20" -> " "
        //    RequestUri = Uri.UnescapeDataString(RequestUri);

        //    //// Если в строке содержится двоеточие, передадим ошибку 400
        //    //// Это нужно для защиты от URL типа http://example.com/../../file.txt
        //    //if (RequestUri.IndexOf("..") >= 0)
        //    //{
        //    //    SendError(Client, 400);
        //    //    return;
        //    //}

        //    // Если строка запроса оканчивается на "/", то добавим к ней index.html
        //    if (RequestUri.EndsWith("/"))
        //    {
        //        RequestUri += "index.html";
        //    }
        //}

        static string HTTPresponce(string data)
        {
            return "HTTP/1.1 200 OK\n"
                    + "Access-Control-Allow-Origin: *\n"
                    + "Content-type: application/json\n"
                    + "Content-Length:" + Encoding.UTF8.GetByteCount(data)
                    + "\n\n"
                    + data;
        }        
    }
}
