using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.ServiceModel.Web;
using System.Text.RegularExpressions;
using System.Web;
using Arena.DataLayer;
using Arena.Core;
using Arena.Security;

using Arena.Custom.NPM.DataLayer.Data;
using Arena.Custom.NPM.DataLayer.Services;

namespace Arena.Custom.NPM.WebServiceMatching
{
    /// <summary>
    /// SecurityAPI handles all methods and functions dealing with the Arena Security and related objects.
    /// </summary>
    public class SecurityAPI
    {
        public static bool IsProduction()
        {
            bool isProduction = true;

            if (String.IsNullOrEmpty(ConfigurationSettings.AppSettings["Environments"]))
            {
                isProduction = false;
            }
            else
            {
                foreach (String url in ConfigurationSettings.AppSettings["Environments"].Split(','))
                {
                    if (HttpContext.Current.Request.Url.ToString().IndexOf(url) > -1)
                    {
                        isProduction = false;
                    }
                }
            }

            return isProduction;
        }
    }
}