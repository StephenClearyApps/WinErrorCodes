using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Win32ErrorTable
{
    public static class Export
    {
        public static void Json(Results results)
        {
            File.WriteAllText("data.json", JsonConvert.SerializeObject(results));
        }
    }
}
