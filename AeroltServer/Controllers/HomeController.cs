using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;

namespace AeroltServer.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";
            return View();
        }

        public void spitJson()
        {
            var weatherForecast = new WeatherForecast
            {
                message = "fuck you bubbet"
                
            };
            string jsonString = "{\r\n  \"Name\": \"Susan\",\r\n)";
        }
    }
    public class WeatherForecast
    {
        public string message;
    }
}