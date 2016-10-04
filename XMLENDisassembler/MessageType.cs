using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BizTalkComponents.PipelineComponents
{
    public struct MessageType
    {
        //Use struct to compare MessaType to MessageType instead of class to class
        public string RootName { get; set; }
        public string TargetNamespace { get; set; }
        public override string ToString()
        {
 	         return String.Format("{0}#{1}", this.TargetNamespace, this.RootName); ;
        }
       
    }
}
