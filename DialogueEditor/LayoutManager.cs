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

			/* Step 0: Sort all nodes into Strongly connected components (look up Graph Theory) */
			//TODO: @Up
			HashSet<Node> nodeLookup = new HashSet<Node>(nodes);    //Just to make .Contains() a little bit faster -> From O(n) to O(1)


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
						if(nodesSeen.Contains(from) == false)
						{
							nodesSeen.Add(from);
							nodesToCheck.Push(new NodeInfo(from, currentNode.layer - 1));
						}
					}
				}
				foreach (var connection in currentNode.node.outputConnections)
				{
					Node to = connection.parentTo;
					if(nodeLookup.Contains(to))
					{
						if(nodesSeen.Contains(to) == false)
						{
							nodesSeen.Add(to);
							nodesToCheck.Push(new NodeInfo(to, currentNode.layer + 1));
						}
					}
				}

			}



			/* Step 2: LayoutHorizontal() each layer (after putting them all on set height, to create sorta tree-like structure */
			{
				int[] layerIndices = layers.Keys.ToArray();
				Array.Sort(layerIndices);
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
		
		private static Point GetCenter(List<Node> nodes)
		{
			double minX = double.PositiveInfinity, minY = double.PositiveInfinity;
			double maxX = double.NegativeInfinity, maxY = double.NegativeInfinity;

			foreach (var node in nodes)
			{
				Point p = node.GetPosition();
				minX = Math.Min(minX, p.X);
				minY = Math.Min(minY, p.Y);
				maxX = Math.Max(maxX, p.X + node.ActualWidth);
				maxY = Math.Max(maxY, p.X + node.ActualHeight);
			}

			return new Point((minX + maxX) / 2, (minY + maxY) / 2);
		}

	}
}
