﻿using System;
using System.Collections.Generic;
using System.Linq;
using MoreLinq;

namespace Accord.Extensions.Math.Geometry
{
    /// <summary>
    /// Implements Floyd-Warshall algorithm as an extension function.
    /// </summary>
    public static class FloydWarshallExtensions
    {
        /// <summary>
        /// Finds all paths withing the given graph by using Floyd-Warshall algorithm.
        /// See <a href="http://en.wikipedia.org/wiki/Floyd%E2%80%93Warshall_algorithm" /> for details.
        /// </summary>
        /// <typeparam name="TVertex">Vertex type.</typeparam>
        /// <typeparam name="TEdge">Edge type.</typeparam>
        /// <param name="graph">Graph.</param>
        /// <param name="distanceFunc">Distance function between two vertices.</param>
        /// <param name="costMat">Cost matrix.</param>
        /// <returns>2D matrix where each element is path from a source to a destination.</returns>
        public static Dictionary<TVertex, Dictionary<TVertex, List<TEdge>>> FindAllPaths<TVertex, TEdge>(this Dictionary<TVertex, Dictionary<TVertex, TEdge>> graph,
                                                                                                         Func<TEdge, double> distanceFunc,
                                                                                                         out Dictionary<TVertex, Dictionary<TVertex, double>> costMat)
            where TEdge : Edge<TVertex>
        {
            var edges = graph.AsEnumerable();
            var vertices = edges.GetVertices<TVertex, TEdge>(); //graph.Keys; => does not pick destination vertices (some vertices may only exist as destinations)

            var costMatrix = edges.ToMatrix(x => x.Source, y => y.Destination, value => distanceFunc(value));
            vertices.ForEach(x => costMatrix.Add(x, x, 0)); //add 0 path cost

            var nextVertexMatrix = edges.ToMatrix(x => x.Source, y => y.Destination, value => value.Source);

            foreach (var kV in vertices)
            {
                foreach (var iV in vertices)
                {
                    foreach (var jV in vertices)
                    {
                        if (!costMatrix.Contains(iV, kV) || !costMatrix.Contains(kV, jV))
                            continue;

                        var newDist = costMatrix[iV][kV] + costMatrix[kV][jV];

                        if (!costMatrix.Contains(iV, jV) || newDist < costMatrix[iV][jV])
                        {
                            costMatrix.AddOrUpdate(iV, jV, newDist);
                            nextVertexMatrix.AddOrUpdate(iV, jV, nextVertexMatrix[kV][jV]);
                        }
                    }
                }
            }

            /*************************** path reconstruction *************************/
            var pathMatrix = edges.ToMatrix(x => x.Source, y => y.Destination, value => new List<TEdge>());

            foreach (var iV in vertices)
            {
                foreach (var jV in vertices)
                {
                    var path = getPath(graph, nextVertexMatrix, iV, jV);
                    pathMatrix.AddOrUpdate(iV, jV, path);
                }
            }

            costMat = costMatrix;
            return pathMatrix;
        }

        private static List<TEdge> getPath<TVertex, TEdge>(Dictionary<TVertex, Dictionary<TVertex, TEdge>> graph,
                                                           Dictionary<TVertex, Dictionary<TVertex, TVertex>> nextVertexMatrix,
                                                           TVertex source, TVertex destination)
        {
            if (!nextVertexMatrix.Contains(source, destination))
                return new List<TEdge>();

            var intermediate = nextVertexMatrix[source][destination];
            if (intermediate.Equals(source))
            {
                return new List<TEdge>() { graph[source][destination] };
            }
            else
            {
                var firstHalf = getPath(graph, nextVertexMatrix, source, intermediate);
                var secondHalf = getPath(graph, nextVertexMatrix, intermediate, destination);

                return firstHalf.Union(secondHalf).ToList();
            }
        }
    }
}
