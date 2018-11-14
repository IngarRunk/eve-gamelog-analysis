using System;

namespace eve_gamelog_analysis {
    public class CynoEvent 
    {
        public DateTime When { get; set; }
        public string System { get; set; }
        public string Station { get; set; }
        public string Character { get; set; }

        public override string ToString() {
            return $"[{When:yyyy-MM-dd hh:mm:ss}] {Character,-25} in {System,-20} at ( {Station} )";
        }
    }
}
