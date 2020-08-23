using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonParser
{
    class Program
    {
        static void Main(string[] args)
        {
            String JsonStr = "{\"sites\": [{ \"name\":\"菜鸟教程\" , \"url\":\"www.runoob.com\" }, { \"name\":\"google\" , \"url\":\"www.google.com\" }, { \"name\":\"微博\" , \"url\":\"www.weibo.com\" },\"name\":null]}";
            string result = Json.Format(JsonStr);
            Console.WriteLine(result);
            Console.WriteLine(Json.Compress(result));
            Console.ReadKey();
        }
    }
}
