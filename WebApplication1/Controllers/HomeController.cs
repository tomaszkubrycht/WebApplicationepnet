using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Epanet.Enums;
using Epanet.Hydraulic;
using Epanet.Hydraulic.IO;
using Epanet.Hydraulic.Structures;
using Epanet.Network;
using Epanet.Network.IO.Input;
using Epanet.Util;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration.Json;
using Newtonsoft.Json.Linq;
using WebApplication1.Helpers;
using WebApplication1.Models;


namespace WebApplication1.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    public static Retrive.Structure json;
    
    public class listofsteps
    {
        public List<Retrive.LinkVar> links { get; set; }
        public List<Retrive.NodeVariable> node { get; set; }
    }
    public HomeController(ILogger<HomeController> logger )
    {
        _logger = logger;
    

    }

    public IActionResult Index()
    {
      
        string inFile = @"c:\users\tomas\desktop\SLC_HGW.inp";
        Retrive rewrite=new Retrive();
        string hydFile="temp.678";
        string qualFile="qual.879";
        List<string> args = new List<string>{"-w"};
        
        json =rewrite.retrivedata(inFile,hydFile,qualFile,args);
        
        return View(json);
    }

    public JsonResult Recalculation(List<double> EmitterList, Network net)
    {
        
        return Json(null);
    }
    public JsonResult data()
    {
        string inFile = @"c:\users\tomas\desktop\SLC_HGW.inp";
        Retrive rewrite=new Retrive();
        string hydFile="temp.678";
        string qualFile="qual.879";
        List<string> args = new List<string>{"-w"};
        json =rewrite.retrivedata(inFile,hydFile,qualFile,args);
        return Json(json);
    }
    public IActionResult Privacy()
    {
        return View();
    }

    public IActionResult test()
    {
        string inFile = @"c:\users\tomas\desktop\SLC_HGW.inp";
        Retrive rewrite=new Retrive();
        string hydFile="temp.678";
        string qualFile="qual.879";
        List<string> args = new List<string>{"-w"};
        json =rewrite.retrivedata(inFile,hydFile,qualFile,args);
        return View(json);
    }
    [HttpGet]
    public  JsonResult Revert(FieldType type, double val)
    {
        var data11=typeof(Retrive.Structure).GetProperty("network");
        var net = data11.GetValue(json);
        Retrive retrive = new Retrive();
        var elevation = retrive.Revertvalue(net, type, val);
        var j = Json((elevation));
        return Json(elevation);
    }
    public JsonResult listofresults(int data1)
    {
        var listofchoice = json.nodesresults.GroupBy(y=>y.Timestep.Step).AsEnumerable().Select((x,p)=>new {value=x.Key.ToString(),id=p}).ToList();
        var time = listofchoice.Where(x => x.id == data1).Select(y => y.value).First();
        var nodedata = json.nodesresults.Where(x=>x.Timestep.Step.ToString()==  time).ToList();
        var linkdata = json.linksresults.Where(x => x.step.Step.ToString() == time).ToList();
        listofsteps listOfSteps = new listofsteps();
        listOfSteps.node = nodedata;
        listOfSteps.links = linkdata;
        return Json(listOfSteps);
    }
    public JsonResult listdata()
    {
     var listofchoice = json.nodesresults.GroupBy(y=>y.Timestep.Step).AsEnumerable().Select((x,p)=>new {value=x.Key.ToString(),id=p}).ToList();
        return Json(listofchoice);
        
    }
    public ActionResult Networklist()
    {
        String tom = "test";
        Network network = json.network;

        return PartialView("test_", network);
    }
    
   
    
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}