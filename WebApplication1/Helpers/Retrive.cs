using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Text;
using Epanet.Enums;
using Epanet.Hydraulic;
using Epanet.Hydraulic.IO;
using Epanet.Network;
using Epanet.Network.IO.Input;
using Epanet.Network.Structures;
using Epanet.Quality;
using Epanet.Util;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor.Infrastructure;
using Newtonsoft.Json;
using WebApplication1.Models;
using EpanetNetwork=Epanet.Network.Network;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace WebApplication1.Helpers{

    public class Retrive
    {
        public class Structure
        {
            public List<NodeVariable> nodesresults { get; set; }
            public List<LinkVar> linksresults { get; set; }
            public Network network { get; set; }
            public List<double> Emitter { get; set; }


        }

        public Structure structure = new Structure();

        public class NodeVariable
        {
            public int Nodeid { get; set; }
            public AwareStep Timestep { get; set; }
            public double Elevation { get; set; }
            public double Pressure { get; set; }
            public double Head { get; set; }
            public double Quality { get; set; }
            public double InitQuality { get; set; }
            public double BaseDemand { get; set; }
            public double Demand { get; set; }
            public string Name { get; set; }
            public Node Node { get; set; }
        }

        public class LinkVar
        {
            public Link link { get; set; }
            public AwareStep step { get; set; }
            public double Lenght { get; set; }
            public double Diameter { get; set; }
            public double FLOW { get; set; }
            public double VELOCITY { get; set; }
            public double ROUGHNESS { get; set; }
            public double UNITHEADLOSS { get; set; }
            public double FRICTIONFACTOR { get; set; }
            public double QUALITY { get; set; }
        }

        public double retriveemitter(string nodename,Network net)
        {
            double ke=0;
            double uflow = net.FieldsMap.GetUnits(FieldType.FLOW);
            double upressure = net.FieldsMap.GetUnits(FieldType.PRESSURE);
            double qexp = net.QExp;

            foreach (Node node  in  net.Junctions) {
                if (node.Ke.IsZero()) continue;
                   ke = uflow / Math.Pow(upressure * node.Ke, 1.0 / qexp);
                
            }

            return ke;
        }
        public void composeemmiter(Network _net,double emitter)
        {
            for(int i=0;i<_net.Junctions.Count();i++)
            {
                double Ke = 0;
                if (emitter > 0.0)
                {
                    double ucf = Math.Pow(_net.FieldsMap.GetUnits(FieldType.FLOW), _net.QExp) /
                                 _net.FieldsMap.GetUnits(FieldType.PRESSURE);

                    Ke = ucf / Math.Pow(emitter, _net.QExp);
                    
                }

                _net.Nodes[i].Ke = Ke;
            }
            

            
        }
        public Structure retrivedata(string inFile, string hydFile, string qualFile, List<string> args)
        {
            TraceSource log = new TraceSource(typeof(Program).FullName ?? nameof(Program), SourceLevels.All);
            string json = "";
            //Structure structure = new Structure();
            List<NodeVariableType> nodesVariables = new List<NodeVariableType>();
            List<LinkVariableType> linksVariables = new List<LinkVariableType>();
            List<TimeSpan> targetTimes = new List<TimeSpan>();
            List<string> targetNodes = new List<string>();
            List<string> targetLinks = new List<string>();
            //Epanet.Hydraulic.HydraulicSim hydraulic = new HydraulicSim(new Network());
            //InputParser parserInp1 = InputParser.Create(FileType.INP_FILE);
            //poprawic parsowanie
            //EpanetNetwork net1 = parserInp1.Parse(new EpanetNetwork(), inFile);
            int parseMode = 0;
            //args.Add("-w");
            foreach (string arg in args)
            {

                switch (arg)
                {
                    case "-T":
                    case "-t":
                        parseMode = 1;
                        continue;
                    case "-N":
                    case "-n":
                        parseMode = 2;
                        continue;
                    case "-L":
                    case "-l":
                        parseMode = 3;
                        continue;
                }

                switch (parseMode)
                {
                    case 1:
                        targetTimes.Add(Utilities.ToTimeSpan(arg));
                        break;
                    case 2:
                        targetNodes.Add(arg);
                        break;
                    case 3:
                        targetLinks.Add(arg);
                        break;
                }
            }


            try
            {
                InputParser parserInp = InputParser.Create(FileType.INP_FILE);
                //poprawic parsowanie
                EpanetNetwork net = parserInp.Parse(new EpanetNetwork(), inFile);
                List<double> emitter = new List<double>();
                composeemmiter(net,2);
                foreach (var item in net.Nodes)
                {
                    var tom = item.KeOryg;
                    emitter.Add(item.KeOryg);
                }

                if (targetTimes.Count > 0)
                    foreach (var time in targetTimes)
                    {
                        string epanetTime = time.GetClockTime();
                        if (time < net.RStart)
                            throw new Exception("Target time \"" + epanetTime +
                                                "\" smaller than simulation start time");

                        if (time > net.Duration)
                            throw new Exception("Target time \"" + epanetTime + "\" bigger than simulation duration");

                        // FIXME: precision 
                        if ((time - net.RStart).Ticks % net.RStep.Ticks != 0)
                            throw new Exception("Target time \"" + epanetTime + "\" not found");
                    }

                foreach (string nodeName in targetNodes)
                    if (net.GetNode(nodeName) == null)
                        throw new Exception("Node \"" + nodeName + "\" not found");

                foreach (string linkName in targetLinks)
                    if (net.GetLink(linkName) == null)
                        throw new Exception("Link \"" + linkName + "\" not found");

                nodesVariables.Add(NodeVariableType.ELEVATION);
                nodesVariables.Add(NodeVariableType.BASEDEMAND);

                if (net.QualFlag != QualType.NONE)
                    nodesVariables.Add(NodeVariableType.INITQUALITY);

                nodesVariables.Add(NodeVariableType.PRESSURE);
                nodesVariables.Add(NodeVariableType.HEAD);
                nodesVariables.Add(NodeVariableType.DEMAND);

                if (net.QualFlag != QualType.NONE)
                    nodesVariables.Add(NodeVariableType.QUALITY);

                linksVariables.Add(LinkVariableType.LENGHT);
                linksVariables.Add(LinkVariableType.DIAMETER);
                linksVariables.Add(LinkVariableType.ROUGHNESS);
                linksVariables.Add(LinkVariableType.FLOW);
                linksVariables.Add(LinkVariableType.VELOCITY);
                linksVariables.Add(LinkVariableType.UNITHEADLOSS);
                linksVariables.Add(LinkVariableType.FRICTIONFACTOR);

                if (net.QualFlag != QualType.NONE)
                    linksVariables.Add(LinkVariableType.QUALITY);

                hydFile = Path.GetTempFileName(); // "hydSim.bin"

                Console.WriteLine("START_RUNNING");
               // Network net1 = DeepClone(net);

                HydraulicSim hydSim = new HydraulicSim(net, log);

                hydSim.Simulate(hydFile);
                if (net.QualFlag != QualType.NONE)
                {
                    qualFile = Path.GetTempFileName(); // "qualSim.bin"

                    QualitySim q = new QualitySim(net, log);
                    q.Simulate(hydFile, qualFile);
                }

                HydraulicReader hydReader = new HydraulicReader(new BinaryReader(File.OpenRead(hydFile)));

                StreamWriter nodesTextWriter = null;
                StreamWriter linksTextWriter = null;
                string nodesOutputFile = null;
                List<NodeVariable> listofnodes = new List<NodeVariable>();
                List<LinkVar> ListLinks = new List<LinkVar>();

                if (targetNodes.Count == 0 && targetLinks.Count == 0 || targetNodes.Count > 0)
                {
                    nodesOutputFile = Path.GetFullPath(inFile) + ".nodes.out";
                    nodesTextWriter = new StreamWriter(nodesOutputFile, false, Encoding.UTF8);

                    nodesTextWriter.Write('\t');
                    foreach (NodeVariableType nodeVar in nodesVariables)
                    {
                        nodesTextWriter.Write('\t');
                        nodesTextWriter.Write(nodeVar.ToString());
                    }

                    nodesTextWriter.Write("\n\t");

                    foreach (NodeVariableType nodeVar in nodesVariables)
                    {
                        nodesTextWriter.Write('\t');
                        nodesTextWriter.Write(net.FieldsMap.GetField(ToFieldType(nodeVar)).Units);
                    }

                    nodesTextWriter.Write('\n');
                }

                if (targetNodes.Count == 0 && targetLinks.Count == 0 || targetLinks.Count > 0)
                {
                    string linksOutputFile = Path.GetFullPath(inFile) + ".links.out";
                    linksTextWriter = new StreamWriter(linksOutputFile, false, Encoding.UTF8);

                    linksTextWriter.Write('\t');
                    foreach (LinkVariableType linkVar in linksVariables)
                    {
                        linksTextWriter.Write('\t');
                        linksTextWriter.Write(linkVar.ToString());
                    }

                    linksTextWriter.Write("\n\t");

                    foreach (LinkVariableType linkVar in linksVariables)
                    {
                        linksTextWriter.Write('\t');
                        if (linkVar < 0) continue;

                        linksTextWriter.Write(net.FieldsMap.GetField((FieldType)linkVar).Units);
                    }

                    linksTextWriter.Write('\n');
                }


                for (TimeSpan time = net.RStart; time <= net.Duration; time += net.RStep)
                {
                    AwareStep step = hydReader.GetStep(time);

                    int i = 0;

                    if (targetTimes.Count > 0 && !targetTimes.Contains(time))
                        continue;

                    if (nodesTextWriter != null)
                        foreach (Node node in net.Nodes)
                        {
                            if (targetNodes.Count > 0 && !targetNodes.Contains(node.Name))
                                continue;
                            NodeVariable nodeVariable = new NodeVariable();
                            nodesTextWriter.Write(node.Name);

                            nodesTextWriter.Write('\t');
                            nodesTextWriter.Write(time.GetClockTime());

                            foreach (NodeVariableType nodeVar in nodesVariables)
                            {
                                nodesTextWriter.Write('\t');
                                double val = GetNodeValue(nodeVar, net.FieldsMap, step, node, i);
                                Epanet.Network.FieldsMap fmap = new FieldsMap();
                                FieldType type = 0;
                                switch (nodeVar)
                                {
                                    case NodeVariableType.ELEVATION:
                                        type = Epanet.Enums.FieldType.ELEV;
                                        nodeVariable.Elevation = val;
                                        break;
                                    case NodeVariableType.HEAD:
                                        type = Epanet.Enums.FieldType.HEAD;
                                        nodeVariable.Head = val;
                                        break;
                                    case NodeVariableType.QUALITY:
                                        type = Epanet.Enums.FieldType.QUALITY;
                                        nodeVariable.Quality = val;
                                        break;
                                    case NodeVariableType.DEMAND:
                                        type = Epanet.Enums.FieldType.DEMAND;
                                        nodeVariable.Demand = val;
                                        break;
                                    case NodeVariableType.PRESSURE:
                                        type = Epanet.Enums.FieldType.PRESSURE;
                                        nodeVariable.Pressure = val;
                                        break;
                                    case NodeVariableType.BASEDEMAND:

                                        nodeVariable.BaseDemand = val;
                                        break;
                                    case NodeVariableType.INITQUALITY:
                                        nodeVariable.InitQuality = val;
                                        break;
                                }

                                nodesTextWriter.Write(ConvertToScientifcNotation(val, 1000, 0.01, 2));
                            }

                            nodeVariable.Timestep = step;
                            nodeVariable.Nodeid = i;
                            nodeVariable.Name = node.Name;
                            nodeVariable.Node = node;
                            listofnodes.Add(nodeVariable);

                            nodesTextWriter.Write('\n');

                            i++;
                        }

                    i = 0;

                    if (linksTextWriter != null)
                        foreach (Link link in net.Links)
                        {
                            if (targetLinks.Count > 0 && !targetLinks.Contains(link.Name))
                                continue;

                            linksTextWriter.Write(link.Name);

                            linksTextWriter.Write('\t');
                            linksTextWriter.Write(time.GetClockTime());
                            LinkVar linkvar = new LinkVar();
                            foreach (LinkVariableType linkVar in linksVariables)
                            {

                                linksTextWriter.Write('\t');
                                double val = GetLinkValue(
                                    linkVar,
                                    net.FormFlag,
                                    net.FieldsMap,
                                    step,
                                    link,
                                    i);
                                switch (linkVar)
                                {
                                    case LinkVariableType.FLOW:
                                        linkvar.FLOW = val;
                                        break;
                                    case LinkVariableType.LENGHT:
                                        linkvar.Lenght = val;
                                        break;
                                    case LinkVariableType.QUALITY:
                                        linkvar.QUALITY = val;
                                        break;
                                    case LinkVariableType.DIAMETER:
                                        linkvar.Diameter = val;
                                        break;
                                    case LinkVariableType.VELOCITY:
                                        linkvar.VELOCITY = val;
                                        break;
                                    case LinkVariableType.ROUGHNESS:
                                        linkvar.ROUGHNESS = val;
                                        break;
                                    case LinkVariableType.UNITHEADLOSS:
                                        linkvar.UNITHEADLOSS = val;
                                        break;
                                    case LinkVariableType.FRICTIONFACTOR:
                                        linkvar.FRICTIONFACTOR = val;
                                        break;
                                }


                                linksTextWriter.Write(ConvertToScientifcNotation(val, 1000, 0.01, 2));

                            }

                            linkvar.link = link;
                            linkvar.step = step;
                            ListLinks.Add(linkvar);
                            linksTextWriter.Write('\n');

                            i++;
                        }
                }


                structure.linksresults = ListLinks;
                structure.nodesresults = listofnodes;
                structure.network = net;
                structure.Emitter = emitter;

                if (nodesTextWriter != null)
                {
                    nodesTextWriter.Close();
                    Console.WriteLine("NODES FILE \"" + nodesOutputFile + "\"");
                }

                if (linksTextWriter != null)
                {
                    linksTextWriter.Close();
                    Console.WriteLine("LINKS FILES \"" + nodesOutputFile + "\"");
                }

                hydReader.Close();
                Console.WriteLine("END_RUN_OK");
                //json = structure

            }
            catch (Epanet.EnException e)
            {
                Console.WriteLine("END_RUN_ERR");
                Debug.Print(e.ToString());
            }
            catch (IOException e)
            {
                Console.WriteLine("END_RUN_ERR");
                Debug.Print(e.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine("END_RUN_ERR");
                Debug.Print(e.ToString());
            }

            
            return structure;
        }

        public static string ConvertToScientifcNotation(
            double value,
            double maxThreshold,
            double minThreshold,
            int @decimal)
        {
            if (double.IsNaN(value))
                return null;

            if (Math.Abs(value) > double.Epsilon
                && (Math.Abs(value) > maxThreshold || Math.Abs(value) < minThreshold))
                return value.ToString("E" + @decimal.ToString(CultureInfo.InvariantCulture));

            return value.ToString("F" + @decimal.ToString(CultureInfo.InvariantCulture));
        }

        private static FieldType ToFieldType(NodeVariableType value)
        {
            return (FieldType)((int)value & ~0x1000);
        }

        private static double GetNodeValue(NodeVariableType type, FieldsMap fmap, AwareStep step, Node node, int index)
        {
            switch (type)
            {
                case NodeVariableType.BASEDEMAND:
                    return fmap.RevertUnit((FieldType)type, node.Demands.Sum(demand => demand.Base));
                case NodeVariableType.ELEVATION:
                    return fmap.RevertUnit((FieldType)type, node.Elevation);
                case NodeVariableType.DEMAND:
                    return step?.GetNodeDemand(index, fmap) ?? 0.0;
                case NodeVariableType.HEAD:
                    return step?.GetNodeHead(index, fmap) ?? 0.0;
                //jakis blad do sprawdzenia

                //case NodeVariableType.INITQUALITY:
                //    return fmap.RevertUnit((FieldType)type, node.C0);
                case NodeVariableType.PRESSURE:
                    return step?.GetNodePressure(index, node, fmap) ?? 0.0;
                case NodeVariableType.QUALITY:
                    return step?.GetNodeQuality(index) ?? 0.0;
                default:
                    return 0.0;
            }
        }

        public double Revertvalue(Object net, FieldType type, double val)
        {

            var tom = net.GetType().Name;
            if (tom == "Network")
            {
                Network tom1 = (Network)net;
                var test = tom1.FieldsMap.RevertUnit(type, val);
                return test;
            }

            return 0; // structure.network.FieldsMap.RevertUnit(type, val);

        }

        public static double GetLinkValue(
            LinkVariableType type,
            FormType formType,
            FieldsMap fmap,
            AwareStep step,
            Link link,
            int index)
        {
            switch (type)
            {
                case LinkVariableType.LENGHT:
                    return fmap.RevertUnit((FieldType)type, link.Lenght);

                case LinkVariableType.DIAMETER:
                    return fmap.RevertUnit((FieldType)type, link.Diameter);

                case LinkVariableType.ROUGHNESS:
                    return link.LinkType == LinkType.PIPE && formType == FormType.DW
                        ? fmap.RevertUnit(FieldType.DIAM, link.Kc)
                        : link.Kc;

                case LinkVariableType.FLOW:
                    return Math.Abs(step?.GetLinkFlow(index, fmap) ?? 0);

                case LinkVariableType.VELOCITY:
                    return Math.Abs(step?.GetLinkVelocity(index, link, fmap) ?? 0);

                case LinkVariableType.UNITHEADLOSS:
                    return step?.GetLinkHeadLoss(index, link, fmap) ?? 0;

                case LinkVariableType.FRICTIONFACTOR:
                    return step?.GetLinkFriction(index, link, fmap) ?? 0;

                case LinkVariableType.QUALITY:
                    return fmap.RevertUnit((FieldType)type, step?.GetLinkAvrQuality(index) ?? 0);

                default:
                    return 0.0;
            }
        }

        public enum NodeVariableType
        {
            ELEVATION = 0,
            PRESSURE = 3,
            HEAD = 2,
            QUALITY = 4,
            INITQUALITY = 4 | 0x1000,
            BASEDEMAND = 1,
            DEMAND = 1 | 0x1000
        }

        public enum LinkVariableType
        {
            LENGHT = FieldType.LENGTH,
            DIAMETER = FieldType.DIAM,
            ROUGHNESS = -1,
            FLOW = FieldType.FLOW,
            VELOCITY = FieldType.VELOCITY,
            UNITHEADLOSS = FieldType.HEADLOSS,
            FRICTIONFACTOR = FieldType.FRICTION,
            QUALITY = FieldType.QUALITY
        }

        static T DeepClone<T>(T instance)
            where T : class, new()
        {
            var visited = new Dictionary<object, object>();

            object DoClone(object inst)
            {
                if (inst is null)
                    return null;
                if (visited.TryGetValue(inst, out var prev))
                    return prev;

                var type = inst.GetType();
                var result = visited[inst] = Activator.CreateInstance(type);

                var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                foreach (var field in fields)
                {
                    var value = field.GetValue(inst);
                    if (field.FieldType.IsValueType || field.FieldType == typeof(string))
                        field.SetValue(result, value);
                    else
                        field.SetValue(result, DoClone(value));
                }

                return result;
            }

            return (T)DoClone(instance);
        }
    }
}

