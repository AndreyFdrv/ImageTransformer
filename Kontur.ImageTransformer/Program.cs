﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kontur.ImageTransformer
{
    class Program
    {
        static void Main(string[] args)
        {
            var server = new HttpServer();
            server.Start("http://+:8080/");
            Console.ReadKey(true);
        }
    }
}