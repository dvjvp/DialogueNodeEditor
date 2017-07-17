using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace DialogueEditor
{
	static class LayoutManager
	{
		public static void LayoutHorizontal(List<Node> nodes)
		{
			const double distanceBetweenNodes = 15;
			List<double> totalWidthUpToThisPoint = new List<double>();
			totalWidthUpToThisPoint.Add(0);
			double totalWidth = 0;
			double centerY = 0;
			double centerX = 0;
			double minX = double.PositiveInfinity, maxX = double.NegativeInfinity;

			foreach (var n in nodes)
			{
				totalWidth += n.ActualWidth;
				totalWidth += distanceBetweenNodes;
				totalWidthUpToThisPoint.Add(totalWidth);
				Point p = n.GetPosition();
				centerY += p.Y;
				minX = Math.Min(minX, p.X);
				maxX = Math.Max(maxX, p.X + n.ActualWidth);
			}
			totalWidth -= distanceBetweenNodes;
			centerX = (minX + maxX) / 2;
			centerY /= nodes.Count;
			double left = centerX - (totalWidth / 2);

			for (int i = 0; i < nodes.Count; i++)
			{
				nodes[i].SetPosition(left + totalWidthUpToThisPoint[i], centerY);
			}

			foreach (var n in nodes)
			{
				n.ForceConnectionUpdate();
			}

		}

		public static void LayoutVertical(List<Node> nodes)
		{
			const double distanceBetweenNodes = 60;
			List<double> totalHeightUpToThisPoint = new List<double>();
			totalHeightUpToThisPoint.Add(0);
			double totalHeight = 0;
			double centerX = 0;
			double centerY = 0;
			double minY = double.PositiveInfinity, maxY = double.NegativeInfinity;

			foreach (var n in nodes)
			{
				totalHeight += n.ActualHeight;
				totalHeight += distanceBetweenNodes;
				totalHeightUpToThisPoint.Add(totalHeight);
				Point p = n.GetPosition();
				centerX += p.X;
				minY = Math.Min(minY, p.Y);
				maxY = Math.Max(maxY, p.Y + n.ActualHeight);
			}
			totalHeight -= distanceBetweenNodes;
			centerX /= nodes.Count;
			centerY = (minY + maxY) / 2;
			double top = centerY - (totalHeight / 2);

			for (int i = 0; i < nodes.Count; i++)
			{
				nodes[i].SetPosition(centerX, top + totalHeightUpToThisPoint[i]);
			}

			foreach (var n in nodes)
			{
				n.ForceConnectionUpdate();
			}
		}

		public static List<Node>[] SplitIslands(List<Node> nodes)
		{
			const double distanceBetweenIslands = 160;
			Vector distanceBetweenNodesInIsland = new Vector(10, 10);

			List<List<Node>> islands = new List<List<Node>>();
			HashSet<Node> editableNodes = new HashSet<Node>(nodes);
			Point initialCenter = GetCenter(nodes);

			/* Step 1: Find strongly connected components (SCC) */

			HashSet<Node> visited = new HashSet<Node>();
			for (int i = 0; i < nodes.Count; i++)
			{
				if(visited.Contains(nodes[i]))
				{
					continue;
				}

				List<Node> newIsland = new List<Node>();
				visited.Add(nodes[i]);
				Stack<Node> toVisit = new Stack<Node>();
				toVisit.Push(nodes[i]);

				while(toVisit.Any())
				{
					Node currentNode = toVisit.Pop();
					newIsland.Add(currentNode);
					foreach (var connection in currentNode.inputConnections)
					{
						Node target = connection.parentFrom;
						if(editableNodes.Contains(target))
						{
							if (visited.Add(target))
							{
								toVisit.Push(target);
							}
						}
					}
					foreach (var connection in currentNode.outputConnections)
					{
						Node target = connection.parentTo;
						if (editableNodes.Contains(target))
						{
							if (visited.Add(target))
							{
								toVisit.Push(target);
							}
						}
					}

				}


				islands.Add(newIsland);
			}


			/* Step 2: Put every node from the same SCC into the same place */

			double totalIslandWidthUpToThisPoint = 0;
			for (int i = 0; i < islands.Count; i++)
			{
				List<Node> currentIsland = islands[i];

				for (int j = 0; j < currentIsland.Count; j++)
				{
					Point newPosition = (Point)(j * distanceBetweenNodesInIsland);
					newPosition.X += totalIslandWidthUpToThisPoint;
					currentIsland[j].SetPosition(newPosition.X, newPosition.Y);
				}

				totalIslandWidthUpToThisPoint += GetBounds(currentIsland).Width;
				totalIslandWidthUpToThisPoint += distanceBetweenIslands;
			}



			/* Step 3: Move nodes back to the original place */

			Point newCenter = GetCenter(nodes);
			Vector offset = initialCenter - newCenter;
			foreach (var n in nodes)
			{
				var newPosition = n.GetPosition() + offset;
				n.SetPosition(newPosition.X, newPosition.Y);
			}



			return islands.ToArray();
		}

		public static List<Node> GetConnected(List<Node> nodes)
		{
			HashSet<Node> selected = new HashSet<Node>(nodes);
			Stack<Node> toVisit = new Stack<Node>(nodes);

			while (toVisit.Any())
			{
				Node node = toVisit.Pop();
				foreach (var connection in node.inputConnections)
				{
					Node target = connection.parentFrom;
					if (selected.Add(target))
					{
						toVisit.Push(target);
					}
				}
				foreach (var connection in node.outputConnections)
				{
					Node target = connection.parentTo;
					if(selected.Add(target))
					{
						toVisit.Push(target);
					}
				}
			}


			return selected.ToList();
		}

		protected struct NodeInfo
		{
			public Node node;
			public int layer;
			public NodeInfo(Node node, int layer)
			{
				this.node = node;
				this.layer = layer;
			}
		}

		public static void LayoutAuto(List<Node> nodes)
		{
			const double distanceBetweenLayers = 80;
			Point selectionCenter = GetCenter(nodes);

			//Just to make .Contains() a little bit faster -> From O(n) to O(1)
			HashSet<Node> nodeLookup = new HashSet<Node>(nodes);    //if node is selected

			/* Step 1: Sort all nodes into layers (going trough graph) */

			Dictionary<int, List<Node>> layers = new Dictionary<int, List<Node>>();

			HashSet<Node> nodesSeen = new HashSet<Node>();
			Stack<NodeInfo> nodesToCheck = new Stack<NodeInfo>();

			nodesToCheck.Push(new NodeInfo(nodes[0], 0));
			nodesSeen.Add(nodes[0]);

			while (nodesToCheck.Any()) 
			{
				NodeInfo currentNode = nodesToCheck.Pop();

				//Add to layer
				if(layers.ContainsKey(currentNode.layer) == false)
				{
					layers.Add(currentNode.layer, new List<Node>());
				}
				layers[currentNode.layer].Add(currentNode.node);

				//Run through connections to check if any of those nodes haven't been seen yet
				foreach (var connection in currentNode.node.inputConnections)
				{
					Node from = connection.parentFrom;
					if (nodeLookup.Contains(from)) 
					{
						if(nodesSeen.Add(from))
						{
							nodesToCheck.Push(new NodeInfo(from, currentNode.layer - 1));
						}
					}
				}
				foreach (var connection in currentNode.node.outputConnections)
				{
					Node to = connection.parentTo;
					if(nodeLookup.Contains(to))
					{
						if(nodesSeen.Add(to))
						{
							nodesToCheck.Push(new NodeInfo(to, currentNode.layer + 1));
						}
					}
				}

			}



			/* Step 2: LayoutHorizontal() each layer (after putting them all on set height, to create sorta tree-like structure */
			int[] layerIndices = layers.Keys.ToArray();
			Array.Sort(layerIndices);
			{
				double heightUntilThisPoint = 0;
				foreach (var layerIndex in layerIndices)
				{
					double maxHeight = double.NegativeInfinity;
					List<Node> layer = layers[layerIndex];
					foreach (var node in layer)
					{
						node.SetPosition(0, heightUntilThisPoint);
						maxHeight = Math.Max(maxHeight, node.ActualHeight);
					}
					LayoutHorizontal(layer);
					heightUntilThisPoint += maxHeight + distanceBetweenLayers;
				}
			}

			/* Step 3: Fiddle around with node order in order to minimize line crossing */
			{
// 				for (int i = 0; i < layerIndices.Length - 1; i++)
// 				{
// 					MinimizeCrossing(  layers[ layerIndices[i] ], layers[ layerIndices[i + 1] ]  );
// 				}

			}

			/* Step 4: Move everything in the right place */
			{
				Point newCenter = GetCenter(nodes);
				Vector offset = selectionCenter - newCenter;
				foreach (var n in nodes)
				{
					var newPosition = n.GetPosition() + offset;
					n.SetPosition(newPosition.X, newPosition.Y);
				}
			}

		}

		public static void MinimizeCrossing(List<Node> layerFrom, List<Node> layerTo)
		{
			List<Node> bestPermutation = layerTo;
			int leastCrosses = int.MaxValue;

			var permutations = GetPermutations(layerTo, layerTo.Count);

			foreach (var perm in permutations)
			{
				SimpleConnection[] connections = null;
				int numCrosses = NumCrosses(connections);
				//i give up. I thought that made sense, but it doesn't.
			}

		}

		private static Point GetCenter(List<Node> nodes)
		{
			Rect r = GetBounds(nodes);
			return new Point(r.X + (r.Width * .5), r.Y + (r.Height * .5));
		}

		private static Rect GetBounds(List<Node> nodes)
		{
			double xMin = double.PositiveInfinity, yMin = double.PositiveInfinity;
			double xMax = double.NegativeInfinity, yMax = double.NegativeInfinity;

			foreach (var node in nodes)
			{
				Point p = node.GetPosition();
				xMin = Math.Min(xMin, p.X);
				yMin = Math.Min(yMin, p.Y);
				xMax = Math.Max(xMax, p.X + node.ActualWidth);
				yMax = Math.Max(yMax, p.X + node.ActualHeight);
			}

			return new Rect(xMin, yMin, xMax - xMin, yMax - yMin);
		}

		public struct SimpleConnection
		{
			public int from;
			public int to;
			public SimpleConnection(int from, int to)
			{
				this.from = from;
				this.to = to;
			}
		}

		public static IEnumerable<IEnumerable<T>> GetPermutations<T>(IEnumerable<T> list, int length)
		{
			if (length == 1) return list.Select(t => new T[] { t });

			return GetPermutations(list, length - 1)
				.SelectMany(t => list.Where(e => !t.Contains(e)),
					(t1, t2) => t1.Concat(new T[] { t2 }));
		}

		public static int NumCrosses(SimpleConnection[] connections)
		{
			int furthest = -1;
			int counter = 0;

			foreach (var c in connections)
			{
				if (c.to < furthest)
				{
					counter++;
				}
				else if (c.to > furthest) 
				{
					furthest = c.to;
				}
			}
			
			return counter;
		}
	}
}
