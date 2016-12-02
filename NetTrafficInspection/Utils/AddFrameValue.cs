using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NMParser.Dataunit;
namespace NetTrafficInspection.Utils
{
    class AddFrameValue
    {
        public List<string> fieldsString = new List<string>();
        public List<string> propertyString = new List<string>();
        public AddFrameValue()
        {
           
          
        }
        public void AddField(string fieldName)
        {
            fieldsString.Add(fieldName);
        }
        public void AddProperty(string propertyName)
        {
            propertyString.Add(propertyName);
        }

    }
}
