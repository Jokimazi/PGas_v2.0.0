using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PGas_v2._0._0
{
    public static class GlobalVariables
    {
        static public string REST_API_URL = "https://pgas.jokimazi.site/rest-api/token-obtain-pair/";

        static public string ACCESS_TOKEN { get; set; }

        static public string REFRESH_TOKEN { get; set; }

    }
}
