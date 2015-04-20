using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FoosLadder.Website.ViewModels
{
    public class ApplicationViewModel
    {
        public static string ApplicationName = "FoosLadder";
        public static string ClientId = "foosLadderApp";
        public static string ApiBaseUrl {
            get {
                var apiBaseUrl = System.Configuration.ConfigurationManager.AppSettings["ApiBaseUrl"];
                if (apiBaseUrl == null) return "/";
                return apiBaseUrl;
            }
        }
    }
}