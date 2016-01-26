using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using relayr_csharp_sdk;

namespace SDKUser
{
    class Program
    {
        static void Main(string[] args)
        {
            RelayrHttpManager manager = new RelayrHttpManager("TujmWC4HPGjMLQDfsQ9z_OxmHZ0h4-Fk");
            manager.GetUserInfo();
            Console.WriteLine("LOL");
            if (System.Diagnostics.Debugger.IsAttached) Console.ReadLine();
            //Console.
        }
    }
}
