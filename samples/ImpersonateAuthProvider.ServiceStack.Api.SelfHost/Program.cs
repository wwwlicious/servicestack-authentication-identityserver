// // This Source Code Form is subject to the terms of the Mozilla Public
// // License, v. 2.0. If a copy of the MPL was not distributed with this
// // file, You can obtain one at http://mozilla.org/MPL/2.0/.
namespace ImpersonateAuthProvider.ServiceStack.Api.SelfHost
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using global::ServiceStack.Text;

    class Program
    {
        static void Main(string[] args)
        {
            Thread.Sleep(5000);

            new AppHost().Init().Start("http://*:5003/");
            "ServiceStack SelfHost listening at http://localhost:5003/".Print();
            Process.Start("http://localhost:5003/");

            Console.ReadLine();
        }
    }
}
