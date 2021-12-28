using System;

namespace FluentApi.Graph
{
    public enum NodeShape
    {
        Box, Ellipse
    }

    public interface IGraphBuilder
    {
        IAttributeWriter<NodeBuilder> AddNode(string nodeName);
        IAttributeWriter<EdgeBuilder> AddEdge(string head, string tail);
        string Build();
    }

    public interface IAttributeWriter<TBuilder> : IGraphBuilder
    {
        IGraphBuilder With(Action<TBuilder> func);
    }

    public class DotGraphBuilder : IGraphBuilder, IAttributeWriter<NodeBuilder>, IAttributeWriter<EdgeBuilder>
    {
        static Graph graph;
        object lastElement;

        public static IGraphBuilder DirectedGraph(string graphName)
        {
            graph = new Graph(graphName, true, true);
            return new DotGraphBuilder();
        }

        public static IGraphBuilder UndirectedGraph(string graphName)
        {
            graph = new Graph(graphName, false, true);
            return new DotGraphBuilder();
        }

        public string Build()
        {
            return graph.ToDotFormat();
        }

        public IAttributeWriter<NodeBuilder> AddNode(string nodeName)
        {
            var node = graph.AddNode(nodeName);
            lastElement = node;
            return this;
        }

        public IAttributeWriter<EdgeBuilder> AddEdge(string head, string tail)
        {
            var edge = graph.AddEdge(head, tail);
            lastElement = edge;
            return this;
        }

        public IGraphBuilder With(Action<NodeBuilder> func)
        {
            var nodeBuilder = new NodeBuilder((GraphNode)lastElement);
            func(nodeBuilder);
            return this;
        }

        public IGraphBuilder With(Action<EdgeBuilder> func)
        {
            var edgeBuilder = new EdgeBuilder((GraphEdge)lastElement);
            func(edgeBuilder);
            return this;
        }
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
