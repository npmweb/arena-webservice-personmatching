using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Web;
using Arena.Core;

using Arena.Custom.NPM.DataLayer.Data;
using Arena.Custom.NPM.DataLayer.Services;

namespace Arena.Custom.NPM.WebServiceMatching
{
    /// <summary>
    /// SessionAPI handles all methods and functions dealing with the Arena Session and related objects.
    /// </summary>
    class SessionAPI
    {
        public static int GetPersonIdBySessionId(string sessionId)
        {
            Guid sessionIdGuid = new Guid(sessionId);

            ApiSession apiSession = new ApiSession(sessionIdGuid);
            if (apiSession.Found)
            {
                Person person = new Person(apiSession.LoginID);
                return person.PersonID;
            }
            else
            {
                throw new Exception("Session was not found.");
            }
        }

        public static string GetPersonNameBySessionId(string sessionId)
        {
            if (!String.IsNullOrEmpty(sessionId))
            {
                Guid sessionIdGuid = new Guid(sessionId);
                ApiSession apiSession = new ApiSession(sessionIdGuid);

                ApiApplication aa = new ApiApplication(apiSession.ApplicationID);

                Person person = new Person(apiSession.LoginID);
                return aa.Name + "(" + person.FirstName + " " + person.LastName + ", " + person.PersonID + ")";
            }
            else
            {
                return "Session was not specified.";
            }
        }

        public static string GetPersonNameByApiKey(string apiKey)
        {
            if (!String.IsNullOrEmpty(apiKey))
            {
                Guid apiKeyGuid = new Guid(apiKey);
                ApiApplication aa = new ApiApplication(apiKeyGuid);
                return aa.Name + "(Anonymous)";
            }
            else
            {
                return "Api key was not specified.";
            }
        }        
    }
}
