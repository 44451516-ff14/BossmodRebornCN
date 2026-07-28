#pragma warning disable CS8625, 8604, 8603, 8600, 8601, 8602, 8618

namespace EarcutNet;

[SkipLocalsInit]
public sealed class Earcut
{
	public static List<int> Tessellate(ReadOnlySpan<double> data, List<int> holeIndices)
	{
		var hasHoles = holeIndices.Count > 0;
		var outerLen = hasHoles ? holeIndices[0] * 2 : data.Length;
		var outerNode = LinkedList(data, 0, outerLen, true);
		int vertexCount = data.Length >> 1;

		var triangles = new List<int>((vertexCount - 2) * 3);

		if (outerNode == null)
		{
			return triangles;
		}

		var minX = double.MaxValue;
		var minY = double.MaxValue;
		var maxX = double.MinValue;
		var maxY = double.MinValue;
		var invSize = default(double);

		if (hasHoles)
		{
			outerNode = EliminateHoles(data, holeIndices, outerNode);
		}

		// if the shape is not too simple, we'll use z-order curve hash later; calculate polygon bbox
		if (data.Length > 160)
		{
			for (int i = 0; i < outerLen; i += 2)
			{
				var x = data[i];
				var y = data[i + 1];
				minX = x < minX ? x : minX;
				minY = y < minY ? y : minY;
				maxX = x > maxX ? x : maxX;
				maxY = y > maxY ? y : maxY;
			}

			// minX, minY and invSize are later used to transform coords into integers for z-order calculation
			invSize = Math.Max(maxX - minX, maxY - minY);
			invSize = invSize != 0d ? 1d / invSize : 0d;
		}

		EarcutLinked(outerNode, triangles, minX, minY, invSize, 0);

		return triangles;
	}

	// Creates a circular doubly linked list from polygon points in the specified winding order.
	private static Node LinkedList(ReadOnlySpan<double> data, int start, int end, bool clockwise)
	{
		var last = default(Node);

		if (clockwise == (SignedArea(data, start, end) > 0d))
		{
			for (int i = start; i < end; i += 2)
			{
				last = InsertNode(i, data[i], data[i + 1], last);
			}
		}
		else
		{
			for (int i = end - 2; i >= start; i -= 2)
			{
				last = InsertNode(i, data[i], data[i + 1], last);
			}
		}

		if (last != null && Equals(last, last.next))
		{
			RemoveNode(last);
			last = last.next;
		}

		return last;
	}

	// eliminate colinear or duplicate points
	private static Node FilterPoints(Node start, Node end = null)
	{
		if (start == null)
		{
			return start;
		}

		if (end == null)
		{
			end = start;
		}

		var p = start;
		bool again;

		do
		{
			again = false;

			if (!p.steiner && (Equals(p, p.next) || Area(p.prev, p, p.next) == 0d))
			{
				RemoveNode(p);
				p = end = p.prev;
				if (p == p.next)
				{
					break;
				}

				again = true;

			}
			else
			{
				p = p.next;
			}
		} while (again || p != end);

		return end;
	}

	// main ear slicing loop which triangulates a polygon (given as a linked list)
	private static void EarcutLinked(Node ear, List<int> triangles, double minX, double minY, double invSize, int pass = 0)
	{
		if (ear == null)
		{
			return;
		}

		// interlink polygon nodes in z-order
		if (pass == 0 && invSize != 0d)
		{
			IndexCurve(ear, minX, minY, invSize);
		}

		var stop = ear;
		Node prev;
		Node next;
		var useHash = invSize != 0d;

		// iterate through ears, slicing them one by one
		while (ear.prev != ear.next)
		{
			prev = ear.prev;
			next = ear.next;

			if (useHash ? IsEarHashed(ear, minX, minY, invSize) : IsEar(ear))
			{
				// cut off the triangle
				triangles.Add(prev.i >> 1);
				triangles.Add(ear.i >> 1);
				triangles.Add(next.i >> 1);

				RemoveNode(ear);

				// skipping the next vertex leads to less sliver triangles
				ear = next.next;
				stop = next.next;

				continue;
			}

			ear = next;

			// if we looped through the whole remaining polygon and can't find any more ears
			if (ear == stop)
			{
				// try filtering points and slicing again
				if (pass == 0)
				{
					EarcutLinked(FilterPoints(ear), triangles, minX, minY, invSize, 1);

					// if this didn't work, try curing all small self-intersections locally
				}
				else if (pass == 1)
				{
					ear = CureLocalIntersections(ear, triangles);
					EarcutLinked(ear, triangles, minX, minY, invSize, 2);

					// as a last resort, try splitting the remaining polygon into two
				}
				else if (pass == 2)
				{
					SplitEarcut(ear, triangles, minX, minY, invSize);
				}

				break;
			}
		}
	}

	// check whether a polygon node forms a valid ear with adjacent nodes
	private static bool IsEar(Node ear)
	{
		var a = ear.prev;
		var b = ear;
		var c = ear.next;

		if (Area(a, b, c) >= 0d)
		{
			return false; // reflex, can't be an ear
		}

		// now make sure we don't have other points inside the potential ear
		var p = ear.next.next;
		var ax = a.x;
		var ay = a.y;

		var bx = b.x;
		var by = b.y;

		var cx = c.x;
		var cy = c.y;

		while (p != a)
		{
    		if (PointInTriangle(ax, ay, bx, by, cx, cy, p.x, p.y) && Area(p.prev, p, p.next) >= 0d)
			{
				return false;
			}

			p = p.next;
		}

		return true;
	}

	private static bool IsEarHashed(Node ear, double minX, double minY, double invSize)
	{
		var a = ear.prev;
		var b = ear;
		var c = ear.next;

		if (Area(a, b, c) >= 0d)
		{
			return false; // reflex, can't be an ear
		}

		// triangle bbox; min & max are calculated like this for speed
		var ax = a.x;
		var bx = b.x;
		var ay = a.y;
		var by = b.y;
		var cx = c.x;
		var cy = c.y;
		var minTX = ax < bx ? (ax < cx ? ax : cx) : (bx < cx ? bx : cx);
		var minTY = ay < by ? (ay < cy ? ay : cy) : (by < cy ? by : cy);
		var maxTX = ax > bx ? (ax > cx ? ax : cx) : (bx > cx ? bx : cx);
		var maxTY = ay > by ? (ay > cy ? ay : cy) : (by > cy ? by : cy);

		// z-order range for the current triangle bbox;
		var minZ = ZOrder(minTX, minTY, minX, minY, invSize);
		var maxZ = ZOrder(maxTX, maxTY, minX, minY, invSize);

		var p = ear.prevZ;
		var n = ear.nextZ;

		// look for points inside the triangle in both directions
		while (p != null && p.z >= minZ && n != null && n.z <= maxZ)
		{
			if (p != a && p != c &&	PointInTriangle(ax, ay, bx, by, cx, cy, p.x, p.y) && Area(p.prev, p, p.next) >= 0d)
			{
				return false;
			}

			p = p.prevZ;

			if (n != a && n != c &&	PointInTriangle(ax, ay, bx, by, cx, cy, n.x, n.y) && Area(n.prev, n, n.next) >= 0d)
			{
				return false;
			}

			n = n.nextZ;
		}

		// look for remaining points in decreasing z-order
		while (p != null && p.z >= minZ)
		{
			if (p != a && p != c && PointInTriangle(ax, ay, bx, by, cx, cy, p.x, p.y) && Area(p.prev, p, p.next) >= 0d)
			{
				return false;
			}

			p = p.prevZ;
		}

		// look for remaining points in increasing z-order
		while (n != null && n.z <= maxZ)
		{
			if (n != a && n != c &&	PointInTriangle(ax, ay, bx, by, cx, cy, n.x, n.y) && Area(n.prev, n, n.next) >= 0d)
			{
				return false;
			}

			n = n.nextZ;
		}

		return true;
	}

	// go through all polygon nodes and cure small local self-intersections
	private static Node CureLocalIntersections(Node start, List<int> triangles)
	{
		var p = start;
		do
		{
			var a = p.prev;
			var b = p.next.next;

			if (!Equals(a, b) && Intersects(a, p, p.next, b) && LocallyInside(a, b) && LocallyInside(b, a))
			{

				triangles.Add(a.i >> 1);
				triangles.Add(p.i >> 1);
				triangles.Add(b.i >> 1);

				// remove two nodes involved
				RemoveNode(p);
				RemoveNode(p.next);

				p = start = b;
			}
			p = p.next;
		} while (p != start);

		return p;
	}

	// try splitting polygon into two and triangulate them independently
	private static void SplitEarcut(Node start, List<int> triangles, double minX, double minY, double invSize)
	{
		// look for a valid diagonal that divides the polygon into two
		var a = start;
		do
		{
			var b = a.next.next;
			while (b != a.prev)
			{
				if (a.i != b.i && IsValidDiagonal(a, b))
				{
					// split the polygon in two by the diagonal
					var c = SplitPolygon(a, b);

					// filter colinear points around the cuts
					a = FilterPoints(a, a.next);
					c = FilterPoints(c, c.next);

					// run earcut on each half
					EarcutLinked(a, triangles, minX, minY, invSize);
					EarcutLinked(c, triangles, minX, minY, invSize);
					return;
				}
				b = b.next;
			}
			a = a.next;
		} while (a != start);
	}

	// link every hole into the outer loop, producing a single-ring polygon without holes
	private static Node EliminateHoles(ReadOnlySpan<double> data, List<int> holeIndices, Node outerNode)
	{
		var countH = holeIndices.Count;
		var queue = new List<Node>(countH);

		var lenData = data.Length;

		for (var i = 0; i < countH; ++i)
		{
			var start = holeIndices[i] * 2;
			var end = i < countH - 1 ? holeIndices[i + 1] * 2 : lenData;
			var list = LinkedList(data, start, end, false);
			if (list == list.next)
			{
				list.steiner = true;
			}

			queue.Add(GetLeftmost(list));
		}

		queue.Sort(static (a, b) =>
		{
			var ax = a.x;
			var bx = b.x;
			if (ax < bx)
			{
				 return -1;
			}
			if (ax > bx)
			{
				return 1;
			}
			return 0;
		});

		// process holes from left to right
		var countQueue = queue.Count;
		for (var i = 0; i < countQueue; ++i)
		{
			EliminateHole(queue[i], outerNode);
			outerNode = FilterPoints(outerNode, outerNode.next);
		}

		return outerNode;
	}

	// find a bridge between vertices that connects hole with an outer ring and and link it
	private static void EliminateHole(Node hole, Node outerNode)
	{
		outerNode = FindHoleBridge(hole, outerNode);
		if (outerNode != null)
		{
			var b = SplitPolygon(outerNode, hole);
			FilterPoints(b, b.next);
		}
	}

	// David Eberly's algorithm for finding a bridge between hole and outer polygon
	private static Node FindHoleBridge(Node hole, Node outerNode)
	{
		var p = outerNode;
		var hx = hole.x;
		var hy = hole.y;
		var qx = double.NegativeInfinity;
		Node m = null;

		// find a segment intersected by a ray from the hole's leftmost point to the left;
		// segment's endpoint with lesser x will be potential connection point
		do
		{
			var next = p.next;
			var py = p.y;
			var nexty = next.y;
			if (hy <= py && hy >= nexty && nexty != py)
			{
				var nextx = next.x;
				var px = p.x;
				var x = px + (hy - py) * (nextx - px) / (nexty - py);
				if (x <= hx && x > qx)
				{
					qx = x;
					if (x == hx)
					{
						if (hy == py)
						{
							return p;
						}

						if (hy == nexty)
						{
							return next;
						}
					}
					m = px < nextx ? p : next;
				}
			}
			p = next;
		} while (p != outerNode);

		if (m == null)
		{
			return null;
		}

		if (hx == qx)
		{
			return m.prev; // hole touches outer segment; pick lower endpoint
		}

		// look for points inside the triangle of hole point, segment intersection and endpoint;
		// if there are no points found, we have a valid connection;
		// otherwise choose the point of the minimum angle with the ray as connection point

		var stop = m;
		var mx = m.x;
		var my = m.y;
		var tanMin = double.PositiveInfinity;
		double tan;

		p = m.next;

		while (p != stop)
		{
			var px = p.x;
			if (hx >= px && px >= mx && hx != px && PointInTriangle(hy < my ? hx : qx, hy, mx, my, hy < my ? qx : hx, hy, p.x, p.y))
			{

				tan = Math.Abs(hy - p.y) / (hx - px); // tangential

				if ((tan < tanMin || (tan == tanMin && px > mx)) && LocallyInside(p, hole))
				{
					m = p;
					tanMin = tan;
				}
			}

			p = p.next;
		}

		return m;
	}

	// interlink polygon nodes in z-order
	private static void IndexCurve(Node start, double minX, double minY, double invSize)
	{
		Node p = start;
		do
		{
			if (p.z == Node.InvalidZ)
			{
				p.z = ZOrder(p.x, p.y, minX, minY, invSize);
			}

			p.prevZ = p.prev;
			p.nextZ = p.next;
			p = p.next;
		} while (p != start);

		p.prevZ.nextZ = null;
		p.prevZ = null;

		SortLinked(p);
	}

	// Simon Tatham's linked list merge sort algorithm
	// http://www.chiark.greenend.org.uk/~sgtatham/algorithms/listsort.html
	private static Node SortLinked(Node list)
	{
		int i;
		Node p;
		Node q;
		Node e;
		Node tail;
		int numMerges;
		int pSize;
		int qSize;
		int inSize = 1;

		do
		{
			p = list;
			list = null;
			tail = null;
			numMerges = 0;

			while (p != null)
			{
				++numMerges;
				q = p;
				pSize = 0;
				for (i = 0; i < inSize; ++i)
				{
					++pSize;
					q = q.nextZ;
					if (q == null)
					{
						break;
					}
				}
				qSize = inSize;

				while (pSize > 0 || (qSize > 0 && q != null))
				{

					if (pSize != 0 && (qSize == 0 || q == null || p.z <= q.z))
					{
						e = p;
						p = p.nextZ;
						--pSize;
					}
					else
					{
						e = q;
						q = q.nextZ;
						--qSize;
					}

					if (tail != null)
					{
						tail.nextZ = e;
					}
					else
					{
						list = e;
					}

					e.prevZ = tail;
					tail = e;
				}

				p = q;
			}

			tail.nextZ = null;
			inSize *= 2;

		} while (numMerges > 1);

		return list;
	}

	// z-order of a point given coords and inverse of the longer side of data bbox
	private static int ZOrder(double x, double y, double minX, double minY, double invSize)
	{
		// coords are transformed into non-negative 15-bit integer range
		int intX = (int)(32767 * (x - minX) * invSize);
		int intY = (int)(32767 * (y - minY) * invSize);

		intX = (intX | (intX << 8)) & 0x00FF00FF;
		intX = (intX | (intX << 4)) & 0x0F0F0F0F;
		intX = (intX | (intX << 2)) & 0x33333333;
		intX = (intX | (intX << 1)) & 0x55555555;

		intY = (intY | (intY << 8)) & 0x00FF00FF;
		intY = (intY | (intY << 4)) & 0x0F0F0F0F;
		intY = (intY | (intY << 2)) & 0x33333333;
		intY = (intY | (intY << 1)) & 0x55555555;

		return intX | (intY << 1);
	}

	// find the leftmost node of a polygon ring
	private static Node GetLeftmost(Node start)
	{
		Node p = start;
		Node leftmost = start;
		do
		{
			if (p.x < leftmost.x)
			{
				leftmost = p;
			}

			p = p.next;
		} while (p != start);

		return leftmost;
	}

	// check if a point lies within a convex triangle
	private static bool PointInTriangle(double ax, double ay, double bx, double by, double cx, double cy, double px, double py)
	{
		var ab = (cx - px) * (ay - py) - (ax - px) * (cy - py);

		if (ab < 0d)
		{
			return false;
		}

		var bc = (ax - px) * (by - py) - (bx - px) * (ay - py);

		if (bc < 0d)
		{
			return false;
		}

		return (bx - px) * (cy - py) - (cx - px) * (by - py) >= 0d;
	}

	// check if a diagonal between two polygon nodes is valid (lies in polygon interior)
	private static bool IsValidDiagonal(Node a, Node b)
	{
		var bi = b.i;
		return a.next.i != bi && a.prev.i != bi && !IntersectsPolygon(a, b) &&
			   LocallyInside(a, b) && LocallyInside(b, a) && MiddleInside(a, b);
	}

	// signed area of a triangle
	private static double Area(Node p, Node q, Node r)
	{
		return (q.y - p.y) * (r.x - q.x) - (q.x - p.x) * (r.y - q.y);
	}

	// check if two points are equal
	private static bool Equals(Node p1, Node p2)
	{
		return p1.x == p2.x && p1.y == p2.y;
	}

	// check if two segments intersect
	private static bool Intersects(Node p1, Node q1, Node p2, Node q2)
	{
		if ((Equals(p1, q1) && Equals(p2, q2)) ||
			(Equals(p1, q2) && Equals(p2, q1)))
		{
			return true;
		}

		return Area(p1, q1, p2) > 0d != Area(p1, q1, q2) > 0d &&
			   Area(p2, q2, p1) > 0d != Area(p2, q2, q1) > 0d;
	}

	// check if a polygon diagonal intersects any polygon segments
	private static bool IntersectsPolygon(Node a, Node b)
	{
		Node p = a;
		do
		{
			var next = p.next;
			var pi = p.i;
			var bi = b.i;
			var nexti = next.i;
			var ai = a.i;
			if (pi != ai && nexti != ai && pi != bi && nexti != bi && Intersects(p, next, a, b))
			{
				return true;
			}

			p = next;
		} while (p != a);

		return false;
	}

	// check if a polygon diagonal is locally inside the polygon
	private static bool LocallyInside(Node a, Node b)
	{
		var prev = a.prev;
		var next = a.next;
		return Area(prev, a, next) < 0d ?
			Area(a, b, next) >= 0d && Area(a, prev, b) >= 0d :
			Area(a, b, prev) < 0d || Area(a, next, b) < 0d;
	}

	// check if the middle point of a polygon diagonal is inside the polygon
	private static bool MiddleInside(Node a, Node b)
	{
		var p = a;
		var inside = false;
		var px = (a.x + b.x) * 0.5d;
		var py = (a.y + b.y) * 0.5d;
		do
		{
			var py2 = p.y;
			var next = p.next;
			var pnexty = next.y;
			var px2 = p.x;
			if (((py2 > py) != (pnexty > py)) && pnexty != py2 &&
					(px < (next.x - px2) * (py - py2) / (pnexty - py2) + px2))
			{
				inside = !inside;
			}

			p = p.next;
		} while (p != a);

		return inside;
	}

	// link two polygon vertices with a bridge; if the vertices belong to the same ring, it splits polygon into two;
	// if one belongs to the outer ring and another to a hole, it merges it into a single ring
	private static Node SplitPolygon(Node a, Node b)
	{
		var a2 = new Node(a.i, a.x, a.y);
		var b2 = new Node(b.i, b.x, b.y);
		var an = a.next;
		var bp = b.prev;

		a.next = b;
		b.prev = a;

		a2.next = an;
		an.prev = a2;

		b2.next = a2;
		a2.prev = b2;

		bp.next = b2;
		b2.prev = bp;

		return b2;
	}

	// create a node and optionally link it with previous one (in a circular doubly linked list)
	private static Node InsertNode(int i, double x, double y, Node last)
	{
		var p = new Node(i, x, y);

		if (last == null)
		{
			p.prev = p;
			p.next = p;

		}
		else
		{
			p.next = last.next;
			p.prev = last;
			last.next.prev = p;
			last.next = p;
		}
		return p;
	}

	private static void RemoveNode(Node p)
	{
		var prev = p.prev;
		var next = p.next;

		prev.next = next;
		next.prev = prev;

		var prevZ = p.prevZ;
		if (prevZ != null)
		{
			prevZ.nextZ = p.nextZ;
		}

		var nextZ = p.nextZ;
		if (nextZ != null)
		{
			nextZ.prevZ = prevZ;
		}
	}

	private sealed class Node(int i, double x, double y)
	{
		public int i = i;
		public double x = x;
		public double y = y;

		public const int InvalidZ = -1;
		public int z = InvalidZ;

		public Node? prev;
		public Node? next;

		public Node? prevZ;
		public Node? nextZ;

		public bool steiner;
	}

	private static double SignedArea(ReadOnlySpan<double> data, int start, int end)
	{
		var sum = default(double);

		for (int i = start, j = end - 2; i < end; i += 2)
		{
			sum += (data[j] - data[i]) * (data[i + 1] + data[j + 1]);
			j = i;
		}

		return sum;
	}
}
