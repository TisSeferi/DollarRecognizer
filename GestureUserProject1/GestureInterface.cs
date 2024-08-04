using System.Collections.Generic;
using System.Windows;

namespace GestureUserProject1
{
    public interface GestureInterface
    {
        (Template bestMatch, double score) Recognize(List<Point> userGesture, List<Template> templates);
        void addTemplate(string name, List<Point> points);
    }   
}
