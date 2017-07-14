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
			double distanceBetweenNodes = 5;
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
			double distanceBetweenNodes = 20;
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

		public static void LayoutAuto(List<Node> nodes)
		{

		}
		
		private static Point GetCenter(List<Node> nodes)
		{
			return new Point();
		}
	}
}
