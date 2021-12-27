using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace FluentApi.Graph
{
    public class DotGraphBuilder : IGraphBuilder<NodeBuilder>, IGraphBuilder<EdgeBuilder>
    {
        static Graph graph;
        object lastElement;

        public static DotGraphBuilder DirectedGraph(string graphName)
        {
            graph = new Graph(graphName, true, true);
            return new DotGraphBuilder();
        }

        public static DotGraphBuilder UndirectedGraph(string graphName)
        {
            graph = new Graph(graphName, false, true);
            return new DotGraphBuilder();
        }

        public string Build()
        {
            return graph.ToDotFormat();
        }

        public IGraphBuilder<NodeBuilder> AddNode(string nodeName)
        {
            var node = graph.AddNode(nodeName);
            lastElement = node;
            return this;
        }

        public IGraphBuilder<EdgeBuilder> AddEdge(string head, string tail)
        {
            var edge = graph.AddEdge(head, tail);
            lastElement = edge;
            return this;
        }

        public IGraphBuilder<NodeBuilder> With(Action<NodeBuilder> func)
        {
            var nodeBuilder = new NodeBuilder((GraphNode)lastElement);
            func(nodeBuilder);
            return this;
        }

        public IGraphBuilder<EdgeBuilder> With(Action<EdgeBuilder> func)
        {
            var nodeBuilder = new EdgeBuilder((GraphEdge)lastElement);
            func(nodeBuilder);
            return this;
        }
    }

    public enum NodeShape
    {
        Box, Ellipse
    }

    public interface IGraphBuilder<T2>
    {
        IGraphBuilder<NodeBuilder> AddNode(string nodeName);
        IGraphBuilder<EdgeBuilder> AddEdge(string head, string tail);
        IGraphBuilder<T2> With(Action<T2> func);
        string Build();
    }

    public abstract class IElementBuilder<T>
    {
        public abstract IElementBuilder<T> Color(string color);
        public abstract IElementBuilder<T> FontSize(int size);
        public abstract IElementBuilder<T> Label(string label);
    }

    public class NodeBuilder 
    {
        GraphNode node;
        public NodeBuilder(GraphNode node) => this.node = node;
        public NodeBuilder Color(string color)
        {
            node.Attributes.Add("color", color);
            return this;
        }

        public NodeBuilder FontSize(int size)
        {
            node.Attributes.Add("fontsize", size.ToString());
            return this;
        }

        public NodeBuilder Label(string label)
        {
            node.Attributes.Add("label", label);
            return this;
        }

        public NodeBuilder Shape(NodeShape shape)
        {
            node.Attributes.Add("shape", shape.ToString().ToLower());
            return this;
        }
    }

    public class EdgeBuilder 
    {
        GraphEdge edge;
        public EdgeBuilder(GraphEdge edge) => this.edge = edge;
        public EdgeBuilder Color(string color)
        {
            edge.Attributes.Add("color", color);
            return this;
        }

        public EdgeBuilder FontSize(int size)
        {
            edge.Attributes.Add("fontsize", size.ToString());
            return this;
        }

        public EdgeBuilder Label(string label)
        {
            edge.Attributes.Add("label", label);
            return this;
        }

        public EdgeBuilder Weight(double weight)
        {
            edge.Attributes.Add("weight", weight.ToString());
            return this;
        }
    }
}
