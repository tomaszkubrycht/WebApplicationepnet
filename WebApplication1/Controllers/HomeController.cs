using System.Diagnostics;
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
using Newtonsoft.Json.Linq;
using WebApplication1.Helpers;
using WebApplication1.Models;

namespace WebApplication1.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private static Retrive.Structure json =null ;

    public class listofsteps
    {
        public List<Retrive.LinkVar> links { get; set; }
        public List<Retrive.NodeVariable> node { get; set; }
    }
    public HomeController(ILogger<HomeController> logger)
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
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}