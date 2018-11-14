using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;
using System.Linq;

namespace eve_gamelog_analysis
{
    class Program
    {
        static void Main(string[] args)
        {
            var gamelogs = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"EVE\logs\Gamelogs");
            var files = System.IO.Directory.GetFiles(gamelogs);
            
            var cynoEvents = new List<CynoEvent>();
            foreach (var file in files) {
                var lines = System.IO.File.ReadAllLines(file);
                var character = GetCharacter(lines);
                if (character != "") {
                    var evts = GetCynoEvents(character, lines);
                    if (evts.Count > 0) {
                        cynoEvents.AddRange(evts);
                    }
                }
            }

            foreach (var evt in cynoEvents) {
                Console.WriteLine(evt);
            }
            Console.WriteLine($"\n{cynoEvents.GroupBy(ce=>ce.Character).Count()} characters lit {cynoEvents.Count()} cynos since {cynoEvents.Min(ce=>ce.When):yyyy-MM-dd hh:mm:ss}");

            // var db = new Database();
            // db.ConnectionString = "Server=localhost;Database=catherder;Uid=root;Pwd=password;";
            // db.UpdateEvents(cynoEvents);
            var systemCounts = new List<PlaceCount>();
            foreach (var evt in cynoEvents)
            {
                var systemCount = systemCounts.FirstOrDefault(sc => sc.System == evt.System);
                if (systemCount != null) {
                    systemCount.Count +=1;
                } else {
                    systemCounts.Add(new PlaceCount { System = evt.System, Count = 1 });
                }
            }

            var topTwenty = systemCounts.OrderByDescending(sc => sc.Count).Take(20);
            Console.WriteLine($"{"System",-15}  Count");
            Console.WriteLine($"---------------  -----");
            foreach (var sys in topTwenty) {
                Console.WriteLine($"{sys.System,-15}  {sys.Count}");
            }
        }
        
        /*****************************************************/
        static string GetCharacter(string[] lines)
        {
            foreach (var line in lines) {
                int idx = line.IndexOf("Listener: ");
                if (idx > -1) {
                    return line.Substring(idx + 9).Trim();
                }
            }
            return "";
        }

        /*****************************************************/
        static List<CynoEvent> GetCynoEvents(string character, string[] lines) 
        {
            var events = new List<CynoEvent>();
            var rx = new Regex(@"\[ ([\. \d:]+) \] \(notify\) Requested to dock at ((\S+) \S+( \S+)?) - (Moon[^-]+ (- ))?(.+) station");
            // EX: "[ 2018.10.09 02:10:54 ] (notify) Requested to dock at Uplingur IV (Ndoria) - Moon 16 - Nurtura Plantation station"
            // g1:|2018.10.09 02:10:54|  g2:|Uplingur IV (Ndoria)|  g3:|Uplingur|  g4:|(Ndoria)|  g5:|Moon 16 -|  g6:|Moon 16|  g7:|Nurtura Plantation|
            
            // 0 = searching for "Cyno"; 1 = searching for docking accepted or blown up
            int state = 0;
            
            for (int i=0; i<lines.Length; i++)
            {
                switch (state) {
                    case 0: // searching for cyno
                        if (lines[i].IndexOf("You are unable to dock because while Cynosural Field Generator") > -1) {
                            state = 1;
                            // line[i-1] should have the name of a system in it
                            if (i > 0) {
                                int idx = lines[i-1].IndexOf("(notify) Requested to dock at ");
                                if (idx > -1) {
                                    var match = rx.Match(lines[i-1]);
                                    var evt = new CynoEvent();
                                    evt.Character = character;
                                    evt.System = match.Groups[3].ToString().Trim();
                                    evt.When = DateTime.Parse(match.Groups[1].ToString());
                                    evt.Station = $"{match.Groups[5].ToString()}{match.Groups[7].ToString()}";
                                    events.Add(evt);
                                }
                            }
                        }
                        break;
                    case 1:
                        if (lines[i].IndexOf("(notify) Your docking request has been accepted") > -1 ||
                            lines[i].IndexOf("(notify) Ship is out of control") > -1) {
                                state = 0;
                            }
                        break;   
                }
            }
            return events;
        }
    }

    public class PlaceCount {
        public string System {get; set;}
        public int  Count {get; set;}
    }
}
