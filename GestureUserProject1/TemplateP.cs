using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace GestureUserProject1
{
    public class TemplateP
    {
        public string Name { get; private set; }
        public List<PointStroke> Points { get; private set; }

        public TemplateP(string name, List<PointStroke> pointStroke)
        {
            Name = name;
            Points = new List<PointStroke>(pointStroke);
        }
    }
}
