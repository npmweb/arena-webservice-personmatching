using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Security.Cryptography;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Web;
using System.Text;
using System.Web;
using System.Xml;
using Arena.Core;
using Arena.Security;
using Arena.Services;
using Arena.Services.Behaviors.ErrorHandling;
using Arena.Services.Exceptions;

using Arena.Custom.NPM.DataLayer.Services;

namespace Arena.Custom.NPM.WebServiceMatching
{
    /// <summary>
    /// Provides all the information needed to find and call a method that
    /// has been registered in the API system.
    /// </summary>
    class RestMethodInfo
    {
        Object _instance;
        MethodInfo _methodInfo;
        UriTemplate _uriTemplate;
        String _method;

        public RestMethodInfo(Object instance, String httpMethod, String uri, MethodInfo mi)
        {
            _instance = instance;
            _methodInfo = mi;
            _method = httpMethod;
            _uriTemplate = new UriTemplate(uri);
        }

        public Object instance { get { return _instance; } }

        public MethodInfo methodInfo { get { return _methodInfo; } }

        public UriTemplate uriTemplate { get { return _uriTemplate; } }

        public String method { get { return _method; } }
    }

    /// <summary>
    /// When this attribute is applied to a WebGet or WebInvoke enabled
    /// it becomes an anonymous and does not require authentication to
    /// be called.
    /// </summary>
    public class RestApiAnonymous : System.Attribute
    {
    }

    /// <summary>
    /// When this attribute is applied to a WebInvoke
    /// it limits to custom authentication.
    /// </summary>
    public class RestApiCustom : System.Attribute
    {
    }

    [DataContract]
    public class Version
    {
        [DataMember]
        public string Number { get; set; }
    }

    public class RestServiceApi
    {
        public void RegisterHandlers(String baseUrl, RestApi api)
        {
        }
    }

    public class CustomServiceApi : RestServiceApi
    {
        [WebGet(UriTemplate = "/version")]
        [RestApiAnonymous]
        public Version Version()
        {
            Version v = new Version();
            v.Number = "1.0.2";
            return v;
        }
        [WebGet(UriTemplate = "/fault")]
        [RestApiAnonymous]
        public void Fault()
        {
            throw new Exception("This is an exception");
        }
    }

    /// <summary>
    /// The NoOp interface is a junk interface, it does nothing except
    /// provide a means for creating an OperationContract.
    /// </summary>
    [ServiceContract]
    interface NoOp
    {
        [OperationContract]
        void NoOp();
    }

    public class RestApi : IHttpHandler
    {
        ArrayList registeredHandlers = null;

        #region Handler registration code

        /// <summary>
        /// Register all handlers in the system both internal and
        /// external.
        /// </summary>
        void RegisterHandlers()
        {
            RegisterInternalHandlers();
            RegisterExternalHandlers();
        }


        /// <summary>
        /// Register all internal handlers that are a part of this
        /// assembly.
        /// </summary>
        void RegisterInternalHandlers()
        {
            RegisterObjectContractHandlers("/", this, this.GetType());

            Object api;
            //            api = new CoreRpc();
            //            RegisterObjectContractHandlers("/", api, api.GetType());
            //api = new AddressAPI();
            //RegisterObjectContractHandlers("/", api, api.GetType());
            //api = new ContributionAPI();
            //RegisterObjectContractHandlers("/", api, api.GetType());
            //api = new FamilyAPI();
            //RegisterObjectContractHandlers("/", api, api.GetType());
            //api = new PhoneAPI();
            //RegisterObjectContractHandlers("/", api, api.GetType());
            api = new PersonAPI();
            RegisterObjectContractHandlers("/", api, api.GetType());
            //api = new ProfileAPI();
            //RegisterObjectContractHandlers("/", api, api.GetType());
            //api = new SecurityAPI();
            //RegisterObjectContractHandlers("/", api, api.GetType());
            //api = new SessionAPI();
            //RegisterObjectContractHandlers("/", api, api.GetType());            
            //api = new SmallGroupAPI();
            //RegisterObjectContractHandlers("/", api, api.GetType());
            //api = new SystemAPI();
            //RegisterObjectContractHandlers("/", api, api.GetType());

        }

        /// <summary>
        /// Ths is a debug method that provides information about what is
        /// registered and the registration log.
        /// </summary>
        [WebGet(UriTemplate = "/info?showLog={showLog}")]
        [RestApiAnonymous()]
        public Stream Info(int showLog)
        {
            StringBuilder sb = new StringBuilder();

            HttpContext.Current.Response.ContentType = "text/plain";
            foreach (RestMethodInfo rmi in registeredHandlers)
            {
                sb.AppendLine(rmi.uriTemplate.ToString());
            }
            sb.AppendLine("");

            return new MemoryStream(ASCIIEncoding.Default.GetBytes(sb.ToString()));
        }


        /// <summary>
        /// Register all external handlers by calling the registration methods
        /// of each registered library in the lookup table.
        /// </summary>
        void RegisterExternalHandlers()
        {
            String assemblyName, namespaceName, className;


            assemblyName = "Arena.Custom.NPM.WebServiceMatching";
            namespaceName = "Arena.Custom.NPM.WebServiceMatching";
            className = "CustomServiceApi";

            RegisterExternalClass("/", assemblyName, namespaceName, className);

            RegisterExternalClass("/", "Arena.Services", "Arena.Services", "ArenaAPI");
        }


        /// <summary>
        /// Register the specified class given its assembly name (dll), namespace and
        /// class name. A new instance of that class is created and registered into the
        /// base url.
        /// </summary>
        /// <param name="baseUrl">The base url to use when registering this object.</param>
        /// <param name="assemblyName">The assembly (dll) name to load the class from.</param>
        /// <param name="namespaceName">The namespace that the class is a part of.</param>
        /// <param name="className">The name of the class to create an instance of.</param>
        void RegisterExternalClass(string baseUrl, String assemblyName, string namespaceName, string className)
        {
            Object instance;
            RestServiceApi service;
            Assembly asm;


            //
            // Try to load the assembly for the given class.
            //
            asm = Assembly.Load(assemblyName);
            if (asm == null)
                throw new Exception("Cannot load assembly");

            //
            // Try to load the class that will handle API service calls.
            //
            instance = asm.CreateInstance(namespaceName + "." + className);
            if (instance == null)
                throw new Exception("Cannot instantiate service");
            //
            // If this object is a subclass of the RestServiceApi then call
            // the standard registration handler method which allows a subclass
            // to do any custom registration it needs to.
            //
            if (typeof(RestServiceApi).IsAssignableFrom(instance.GetType()) == true)
            {
                service = (RestServiceApi)instance;

                //
                // Initialize the API service and have it register handlers.
                //
                service.RegisterHandlers(baseUrl, this);
            }

            RegisterObjectContractHandlers(baseUrl, instance, instance.GetType());
        }


        /// <summary>
        /// Register the given method with the specified url.
        /// </summary>
        /// <param name="url">The URL that will be used, relative to the service.api handler.</param>
        /// <param name="mi">The method to be invoked.</param>
        public void RegisterHandler(object instance, String method, String url, MethodInfo mi)
        {
            RestMethodInfo rmi;

            //
            // Create the root level if it does not exist.
            //
            if (registeredHandlers == null)
            {
                registeredHandlers = new ArrayList();
            }

            //
            // Create the REST state method information.
            //
            rmi = new RestMethodInfo(instance, method.ToUpper(), url, mi);

            //
            // Add the new method information into the list of handlers.
            //
            registeredHandlers.Add(rmi);
        }


        /// <summary>
        /// Look for any WCF style methods that contain a WebGet or WebInvoke
        /// attribute. On any found methods, register the method as a url
        /// handler for that instance.
        /// </summary>
        /// <param name="baseUrl">The base URL to use when registering methods for this instance, pass an empty string for no base url.</param>
        /// <param name="instance">The object whose methods will be registered into the URL handlers.</param>
        public void RegisterObjectContractHandlers(String baseUrl, object instance, Type objectType)
        {
            //
            // Strip any trailing "/" character.
            //
            if (baseUrl.Length > 0 && baseUrl[baseUrl.Length - 1] == '/')
                baseUrl = baseUrl.Substring(0, baseUrl.Length - 1);

            foreach (MethodInfo mi in objectType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy))
            {
                WebGetAttribute[] webgets;
                WebInvokeAttribute[] webinvokes;
                String url;

                //
                // Get any "WebGet" attributes for this method.
                //

                webgets = (WebGetAttribute[])mi.GetCustomAttributes(typeof(WebGetAttribute), true);
                if (webgets.Length > 0)
                {
                    url = webgets[0].UriTemplate;
                    if (url.Length > 0 && url[0] == '/')
                        url = baseUrl + url;
                    else
                        url = baseUrl + "/" + url;

                    RegisterHandler(instance, "GET", url, mi);
                    continue;
                }

                //
                // Get any "WebInvoke" attributes for this method.
                //
                webinvokes = (WebInvokeAttribute[])mi.GetCustomAttributes(typeof(WebInvokeAttribute), true);
                if (webinvokes.Length > 0)
                {
                    url = webinvokes[0].UriTemplate;
                    if (url.Length > 0 && url[0] == '/')
                        url = baseUrl + url;
                    else
                        url = baseUrl + "/" + url;

                    RegisterHandler(instance, webinvokes[0].Method, url, mi);

                    continue;
                }
            }

            foreach (Type t in objectType.GetInterfaces())
            {
                RegisterObjectContractHandlers(baseUrl, instance, t);
            }
        }


        /// <summary>
        /// Given the URL, find the associated method handler.
        /// </summary>
        /// <param name="baseUrl">The base URL of the web service handler.</param>
        /// <param name="url">The relative URL of the web service for the specific request.</param>
        /// <param name="match">The UriTemplateMatch object that contains information about the UriTemplate that was matched.</param>
        /// <returns>Either null or a valid MethodInfo reference to the method to be invoked.</returns>
        RestMethodInfo FindHandler(String method, Uri baseUrl, Uri url, ref UriTemplateMatch match)
        {
            if (registeredHandlers == null)
                return null;

            //
            // Loop through and look for a matching method signature.
            //
            foreach (RestMethodInfo rmi in registeredHandlers)
            {
                //
                // Ensure the proper method is in use.
                //
                if (rmi.method.ToUpper() != method.ToUpper())
                    continue;

                //
                // See if there is a match on the URI.
                //
                if ((match = rmi.uriTemplate.Match(baseUrl, url)) != null)
                {
                    return rmi;
                }
            }

            return null;
        }

        #endregion


        #region Http Handler methods

        /// <summary>
        /// Process the web request.
        /// </summary>
        /// <param name="context">The context of this single web request.</param>
        public void ProcessRequest(HttpContext context)
        {
            UriTemplateMatch templateMatch = null;
            RestMethodInfo rmi = null;
            ArrayList finalParameters = null;
            Object result = null, p;

            //
            // Initialization phase, register all handlers and then find a match.
            //
            try
            {
                //
                // Register all handlers.
                //
                RegisterHandlers();

                String baseUrl = context.Request.Url.Scheme + "://" + context.Request.Url.Authority + context.Request.FilePath;
                rmi = FindHandler(context.Request.HttpMethod.ToUpper(), new Uri(baseUrl), context.Request.Url, ref templateMatch);
                if (rmi == null)
                    throw new MissingMethodException();
            }
            catch (Exception e)
            {
                context.Response.Write(String.Format("Exception occurred at init: {0}", e.Message + e.StackTrace));

                return;
            }

            //
            // Parse out any parameters for the method call.
            //
            try
            {
                finalParameters = new ArrayList();

                //
                // Walk each parameter in the method and see if we can convert
                // one of the query variables to the proper type.
                //
                foreach (ParameterInfo pi in rmi.methodInfo.GetParameters())
                {
                    try
                    {
                        p = null;
                        if (typeof(Stream).IsAssignableFrom(pi.ParameterType))
                        {
                            p = context.Request.InputStream;
                        }
                        else if (templateMatch.BoundVariables.AllKeys.Contains(pi.Name.ToUpper()) == true)
                        {
                            p = templateMatch.BoundVariables[pi.Name.ToUpper()];
                            if (p != null)
                            {
                                if (typeof(List<String>).IsAssignableFrom(pi.ParameterType))
                                {
                                    p = p.ToString().Split(new char[1] { ',' }).ToList<String>();
                                }
                                else
                                    p = Convert.ChangeType(p, pi.ParameterType);
                            }
                        }
                    }
                    catch
                    {
                        p = null;
                    }

                    finalParameters.Add(p);
                }
            }
            catch (Exception e)
            {
                context.Response.Write(String.Format("Exception occurred at parameter parse: {0} at {1}", e.Message, e.StackTrace));

                return;
            }

            //
            // Force the context to be anonymous, then authenticate if the user
            // is calling a non-anonymous method.
            //
            try
            {
                ArenaContext.Current.SetWebServiceProperties(ArenaContext.Current.CreatePrincipal(""), new Arena.Core.Person());
                String PathAndQuery = String.Empty;

                if (rmi.uriTemplate.ToString() != "/version" &&
                    rmi.uriTemplate.ToString() != "/login" &&
                    rmi.uriTemplate.ToString() != "/help" &&
                    rmi.methodInfo.GetCustomAttributes(typeof(RestApiAnonymous), true).Length == 0 &&
                    rmi.methodInfo.GetCustomAttributes(typeof(RestApiCustom), true).Length == 0)
                {
                    PathAndQuery = context.Request.Path + "?" + context.Server.UrlDecode(context.Request.QueryString.ToString());
                    PathAndQuery = PathAndQuery.Substring(context.Request.FilePath.Length + 1).ToLower();
                    PathAndQuery = PathAndQuery.Substring(0, PathAndQuery.IndexOf("api_sig") - 1);

                    string apiSession = context.Request.Params["api_session"];

                    // Loop through non-production environments to determine where we are
                    bool checkForExpiration = true;
                    if (ConfigurationSettings.AppSettings["EnableAutomaticSessionExtension"] == "true")
                    {
                        foreach (String url in ConfigurationSettings.AppSettings["Environments"].Split(','))
                        {
                            if (HttpContext.Current.Request.Url.ToString().IndexOf(url) > -1)
                            {
                                checkForExpiration = false;
                            }
                        }
                    }
                    if (!checkForExpiration)
                    {
                        CoreApiSessionService.ExtendExpiration(apiSession);
                    }

                    AuthenticationManager.SetupSessionForRequest(apiSession, false);
                    AuthenticationManager.VerifySignature(context.Request.Url, PathAndQuery, apiSession);
                }
                else if (rmi.methodInfo.GetCustomAttributes(typeof(RestApiCustom), true).Length != 0)
                {
                    PathAndQuery = context.Request.Path;
                    PathAndQuery = PathAndQuery.Substring(context.Request.FilePath.Length + 1).ToLower();

                    string secret = String.Empty;

                    foreach (String element in context.Request.Form.AllKeys)
                    {
                        if (element != "api_sig" && element.IndexOf("btn_") == -1)
                        {
                            PathAndQuery += (PathAndQuery.IndexOf("?") > 0 ? "&" : "?");
                            PathAndQuery += element.Trim() + "=" + context.Request.Form[element];
                        }
                        if (element == "api_session")
                        {
                            CoreApiSessionService cass = new CoreApiSessionService();
                            secret = cass.GetSecretBySessionId(context.Request.Form[element]);
                        }
                    }

                    if (!String.IsNullOrEmpty(secret))
                    {
                        string sig = EncryptString(secret + "_" + PathAndQuery.ToLower());
                        if (sig.ToLower() != context.Request.Params["api_sig"].ToLower())
                        {
                            throw new Exception("Invalid signature. " + "Path: " + context.Request.Path + "?" + context.Server.UrlDecode(context.Request.QueryString.ToString()) + "; Signatured string: " + secret + "_" + PathAndQuery.ToLower());
                        }
                    }
                    else
                    {
                        throw new Exception("Invalid session.");
                    }
                }
            }
            catch (Exception e)
            {
                RESTException restEx = e as RESTException;

                if (restEx != null)
                {
                    // result = new RestErrorMessage(System.Net.HttpStatusCode.Conflict, e.Message + " " + e.StackTrace, string.Empty);
                    result = new RestErrorMessage(restEx);
                }
                else
                {
                    result = new RestErrorMessage(System.Net.HttpStatusCode.OK, e.Message, string.Empty);
                }
            }

            //
            // Perform the actual method call.
            //
            if (result == null)
            {
                // NPM: Capture session and user id's for logging
                string sessionid = System.Web.HttpContext.Current.Request.Params["sessionid"];
                int? personid = PersonId();

                try
                {
                    //
                    // Set some default response information.
                    //
                    context.Response.ContentType = "application/xml; charset=utf-8";

                    if (TypeIsServiceContract(rmi.instance.GetType()) == true)
                    {
                        //
                        // Run the request inside of a operation context so response information
                        // can be set. This is a bit of a cheat, but it works.
                        //
                        WebChannelFactory<NoOp> factory = new WebChannelFactory<NoOp>(new Uri("http://localhost/"));
                        NoOp channel = factory.CreateChannel();
                        using (new OperationContextScope((IContextChannel)channel))
                        {
                            result = rmi.methodInfo.Invoke(rmi.instance, (object[])finalParameters.ToArray(typeof(object)));
                            if (WebOperationContext.Current.OutgoingResponse.ContentType != null)
                                context.Response.ContentType = WebOperationContext.Current.OutgoingResponse.ContentType;
                        }
                    }
                    else
                    {
                        //
                        // This is a standard method call, just call it.
                        //
                        result = rmi.methodInfo.Invoke(rmi.instance, (object[])finalParameters.ToArray(typeof(object)));
                    }

                    // NPM: Log the call as a SUCCESS
                    ApiSessionLogService.Log(sessionid, personid, context.Request.PathInfo.ToLower(), "SUCCESS", "");
                }
                catch (Exception e)
                {
                    // NPM: Log the call as a FAILURE
                    ApiSessionLogService.Log(sessionid, personid, context.Request.PathInfo.ToLower(), "FAILURE", e.InnerException.Message);

                    RESTException restEx;

                    if (e.InnerException != null)
                        e = e.InnerException;

                    restEx = e as RESTException;
                    if (restEx != null)
                    {
                        result = new RestErrorMessage(restEx);
                    }
                    else
                    {
                        result = new RestErrorMessage(System.Net.HttpStatusCode.OK, e.Message, string.Empty);
                    }
                }
            }

            //
            // Deal with the response that was generated.
            //
            try
            {
                if (result != null)
                {
                    //
                    // There is probably a better way to do this, but this is the best
                    // I can come up with. Somebody feel free to make this cleaner.
                    //
                    if (typeof(Stream).IsAssignableFrom(result.GetType()) == true)
                    {
                        Stream s = (Stream)result;
                        int count;

                        //
                        // Response is a data stream, just copy it to the response
                        // stream.
                        //
                        do
                        {
                            byte[] buf = new byte[8192];

                            count = s.Read(buf, 0, 8192);
                            context.Response.BinaryWrite(buf);
                        } while (count > 0);
                    }
                    else if (typeof(Message).IsAssignableFrom(result.GetType()) == true)
                    {
                        Message msg = (Message)result;
                        StringBuilder sb = new StringBuilder();
                        StringWriter sw = new StringWriter(sb);
                        XmlTextWriter xtw = new XmlTextWriter(sw);

                        //
                        // Response is a Message object. Write it out as an XML
                        // stream.
                        //
                        msg.WriteMessage(xtw);
                        context.Response.Write(sb.ToString());

                    }
                    else
                    {
                        DataContractSerializer serializer = new DataContractSerializer(result.GetType());

                        //
                        // Otherwise, use the DataContractSerializer to convert the object into
                        // an XML stream.
                        //
                        serializer.WriteObject(context.Response.OutputStream, result);
                    }
                }
            }
            catch (Exception e)
            {
                context.Response.Write(String.Format("Exception sending response: {0}", e.Message));
                return;
            }
        }

        /// <summary>
        /// Check the object type and any interfaces to see if it has any
        /// ServiceContract attributes.
        /// </summary>
        /// <param name="objectType">The object type to check.</param>
        /// <returns>true if the objectType or it's interfaces has a ServiceContract.</returns>
        private bool TypeIsServiceContract(Type objectType)
        {
            if (objectType.GetCustomAttributes(typeof(ServiceContractAttribute), true).Count() > 0)
                return true;

            foreach (Type t in objectType.GetInterfaces())
            {
                if (TypeIsServiceContract(t) == true)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// This HTTP handler is not reusable. Whatever that means.
        /// </summary>
        public bool IsReusable
        {
            get { return false; }
        }

        /// <summary>
        /// Convert the HttpRequest's InputStream (post data) into a
        /// String object.
        /// </summary>
        /// <param name="request">The request whose POST data we are intersted in.</param>
        /// <returns>String representation of the input stream.</returns>
        private String RequestString(HttpRequest request)
        {
            StringBuilder strmContents;
            Int32 counter, strLen, strRead;

            //
            // Convert the input stream into a byte array.
            //
            strLen = Convert.ToInt32(request.InputStream.Length);
            byte[] strArr = new byte[strLen];
            strRead = request.InputStream.Read(strArr, 0, strLen);

            //
            // Convert byte array to a text string.
            //
            strmContents = new StringBuilder();
            for (counter = 0; counter < strLen; counter++)
            {
                strmContents.AppendFormat("{0}", (char)strArr[counter]);
            }

            return strmContents.ToString();
        }

        public static string EncryptString(string message)
        {
            MD5 md5 = MD5.Create();
            byte[] inputBytes = Encoding.ASCII.GetBytes(message);
            byte[] hash = md5.ComputeHash(inputBytes);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }
            return sb.ToString();
        }

        #endregion


        #region Convenience methods for called api methods

        /// <summary>
        /// Determines if the personID has access to perform the
        /// indicated operation on the person field in question.
        /// </summary>
        /// <param name="personID">The ID number of the person whose security access we are checking.</param>
        /// <param name="field">The ID number of the PersonField that the user wants access to.</param>
        /// <param name="operation">The type of access the user needs to proceed.</param>
        /// <returns>true/false indicating if the operation is allowed.</returns>
        static public bool PersonFieldOperationAllowed(int personID, int field, OperationType operation)
        {
            PermissionCollection permissions;

            //
            // Load the permissions.
            //
            permissions = new PermissionCollection(ObjectType.PersonField, field);

            return PermissionsOperationAllowed(permissions, personID, operation);
        }

        /// <summary>
        /// Determines if the personID has access to perform the
        /// indicated operation on the profile in question.
        /// </summary>
        /// <param name="personID">The ID number of the person whose security access we are checking.</param>
        /// <param name="profileID">The ID number of the profile the user wants access to.</param>
        /// <param name="operation">The type of access the user needs to proceed.</param>
        /// <returns>true/false indicating if the operation is allowed.</returns>
        static public bool ProfileOperationAllowed(int personID, int profileID, OperationType operation)
        {
            PermissionCollection permissions;

            //
            // Load the permissions.
            //
            permissions = new PermissionCollection(ObjectType.Tag, profileID);

            return PermissionsOperationAllowed(permissions, personID, operation);
        }

        /// <summary>
        /// Determines if the personID has access to perform the indicated operation
        /// on the small group cluster in question.
        /// </summary>
        /// <param name="personID">The ID number of the person whose security access we are checkin.</param>
        /// <param name="clusterID">The ID number of the profile the user wants access to.</param>
        /// <param name="operation">The type of access the user needs to proceed.</param>
        /// <returns>true/false indicating if the operation is allowed.</returns>
        static public bool GroupClusterOperationAllowed(int personID, int clusterID, OperationType operation)
        {
            PermissionCollection permissions;

            //
            // Load the permissions.
            //
            permissions = new PermissionCollection(ObjectType.Group_Cluster, clusterID);

            return PermissionsOperationAllowed(permissions, personID, operation);
        }

        /// <summary>
        /// Checks the PermissionCollection class to determine if the
        /// indicated operation is allowed for the person identified by
        /// their ID number.
        /// </summary>
        /// <param name="permissions">The collection of permissions to check. These should be object permissions.</param>
        /// <param name="personID">The ID number of the user whose security access we are checking.</param>
        /// <param name="operation">The type of access the user needs to proceed.</param>
        /// <returns>true/false indicating if the operation is allowed.</returns>
        static public bool PermissionsOperationAllowed(PermissionCollection permissions, int personID, OperationType operation)
        {
            RoleCollection roles;
            int i;

            //
            // Check if the person has direct permission.
            //
            if (permissions.ContainsSubjectOperation(SubjectType.Person, personID, operation) == true)
                return true;

            //
            // Now check all roles for the given person.
            //
            roles = new RoleCollection(DefaultOrganizationID(), personID);
            for (i = 0; i < roles.Count; i++)
            {
                if (permissions.ContainsSubjectOperation(SubjectType.Role, roles[i].RoleID, operation) == true)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Retrieve the default organization ID for this web
        /// service. This is retrieved via the "Organization"
        /// application setting in the web.config file.
        /// </summary>
        /// <returns>An integer indicating the organization ID.</returns>
        static public int DefaultOrganizationID()
        {
            return Convert.ToInt32(ConfigurationSettings.AppSettings["Organization"]);
        }

        /// <summary>
        /// Retrieve the base url (the portion of the URL without the last path
        /// component, that is the filename and query string) of the current
        /// web request.
        /// </summary>
        /// <returns>Base url as a string.</returns>
        static public string BaseUrl()
        {
            StringBuilder url = new StringBuilder();
            string[] segments;
            int i;


            url.Append(HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority));
            segments = HttpContext.Current.Request.Url.Segments;
            for (i = 0; i < segments.Length - 1; i++)
            {
                url.Append(segments[i]);
            }

            return url.ToString();
        }


        static public int? PersonId()
        {
            string sessionId = String.Empty;
            try
            {
                // Todo: Track down and consolidate all forms of session id being passed into the webservice.
                sessionId = System.Web.HttpContext.Current.Request.Params["sessionid"];
                if (String.IsNullOrEmpty(sessionId))
                {
                    sessionId = System.Web.HttpContext.Current.Request.Params["session_id"];
                    if (String.IsNullOrEmpty(sessionId))
                    {
                        sessionId = System.Web.HttpContext.Current.Request.Params["sid"];
                        if (String.IsNullOrEmpty(sessionId))
                        {
                            sessionId = System.Web.HttpContext.Current.Request.Params["api_session"];                            
                        }
                    }
                }

                if (!String.IsNullOrEmpty(sessionId))
                {
                    return SessionAPI.GetPersonIdBySessionId(System.Web.HttpContext.Current.Request.Params["sessionid"]);
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }

        static public string PersonName()
        {
            string sessionId = String.Empty;
            string apiKey = String.Empty;
            try
            {
                // Todo: Track down and consolidate all forms of session id being passed into the webservice.
                sessionId = System.Web.HttpContext.Current.Request.Params["sessionid"];
                if (String.IsNullOrEmpty(sessionId))
                {
                    sessionId = System.Web.HttpContext.Current.Request.Params["session_id"];
                    if (String.IsNullOrEmpty(sessionId))
                    {
                        sessionId = System.Web.HttpContext.Current.Request.Params["sid"];
                        if (String.IsNullOrEmpty(sessionId))
                        {
                            sessionId = System.Web.HttpContext.Current.Request.Params["api_session"];
                            if (String.IsNullOrEmpty(sessionId))
                            {
                                sessionId = System.Web.HttpContext.Current.Request.Params["sessionid"];
                                if (String.IsNullOrEmpty(apiKey))
                                {
                                    apiKey = System.Web.HttpContext.Current.Request.Params["apikey"];
                                    if (String.IsNullOrEmpty(apiKey))
                                    {
                                        apiKey = System.Web.HttpContext.Current.Request.Params["api_key"];
                                        if (String.IsNullOrEmpty(apiKey))
                                        {
                                            apiKey = System.Web.HttpContext.Current.Request.Params["key"];
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (!String.IsNullOrEmpty(sessionId))
                {
                    return SessionAPI.GetPersonNameBySessionId(sessionId);
                }
                else if (!String.IsNullOrEmpty(apiKey))
                {
                    return SessionAPI.GetPersonNameByApiKey(apiKey);
                }
                else
                {
                    return "Anonymous";
                }
            }
            catch
            {
                return "Session was not found.";
            }
        }
        #endregion
    }
}