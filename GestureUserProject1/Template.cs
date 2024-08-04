using System.Collections.Generic;
using System.Windows;

namespace GestureUserProject1
{
    public class Template
    {
        public string Name { get; private set; }
        public List<Point> Points { get; private set; }

        public Template(string name, List<Point> pointStroke)
        {
            Name = name;
            Points = new List<Point>(pointStroke);
        }
    }
}
