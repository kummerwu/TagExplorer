using AnyTagNet;
using System.Collections.Generic;
using System.Windows;

namespace AnyTag.UI
{

    interface IGObjCollection
    {
        void AddGObjs(IEnumerable<GObj> all);
        void AddEdge(IEnumerable<PathEdge> edge);
        List<UIElement> GetAllTextBlocks();
        List<UIElement> GetAllLines();
    }
    interface IRectLayoutCalc
    {
        void Calc(ref Size initSize, IEnumerable<GObj> objs, LayoutOption o);
    }

    enum LayoutOption
    {
        FixWidth, FixHeight, FixRadio
    }
    interface IGLayoutResult
    {
        bool HasCalc(string tag);
        void AddCalc(string tag);
        void AddEdge(string parent, string child);
        IEnumerable<PathEdge> GetEdges();
        void Clear();
    }
}
