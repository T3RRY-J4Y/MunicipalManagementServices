using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MunicipalManagementServices
{
    // ══════════════════════════════════════════════════════════════════════════
    // BASIC TREE (generic N-ary tree — each node can have any number of children)
    // Used to model the municipal department hierarchy that categories belong to
    // ══════════════════════════════════════════════════════════════════════════
    public class BasicTreeNode
    {
        public string Name;
        public List<BasicTreeNode> Children = new List<BasicTreeNode>();
        public BasicTreeNode(string name) { Name = name; }

        public BasicTreeNode AddChild(string name)
        {
            var child = new BasicTreeNode(name);
            Children.Add(child);
            return child;
        }
    }

    public class CategoryBasicTree
    {
        public BasicTreeNode Root;

        public CategoryBasicTree()
        {
            Root = new BasicTreeNode("Municipal Services");

            var infra = Root.AddChild("Infrastructure & Engineering");
            infra.AddChild("Roads");
            infra.AddChild("Water");
            infra.AddChild("Electricity");
            infra.AddChild("Stormwater");

            var community = Root.AddChild("Community Services");
            community.AddChild("Parks");
            community.AddChild("Sanitation");

            var safety = Root.AddChild("Safety & Security");
            safety.AddChild("Public Safety");
        }

        // Returns indented text lines representing the tree shape
        public List<string> Print()
        {
            var lines = new List<string>();
            PrintRec(Root, 0, lines);
            return lines;
        }

        private void PrintRec(BasicTreeNode node, int depth, List<string> lines)
        {
            string prefix = depth == 0 ? "" : new string(' ', (depth - 1) * 4) + "  +-- ";
            lines.Add(prefix + node.Name);
            foreach (var child in node.Children)
                PrintRec(child, depth + 1, lines);
        }

        // Walks the tree to find which department a category belongs to.
        // This is what lets the app auto-route a new request to a department.
        public string FindDepartment(string category)
        {
            return FindDepartmentRec(Root, category);
        }

        private string FindDepartmentRec(BasicTreeNode node, string category)
        {
            foreach (var child in node.Children)
            {
                if (child.Name.Equals(category, StringComparison.OrdinalIgnoreCase))
                    return node.Name;
                var found = FindDepartmentRec(child, category);
                if (found != null) return found;
            }
            return null;
        }
    }

    // ══════════════════════════════════════════════════════════════════════════
    // BINARY TREE (plain — fixed shape, NOT a search tree)
    // Used to classify a request's priority number into a human-readable bucket
    // ══════════════════════════════════════════════════════════════════════════
    public class BinaryTreeNode
    {
        public string Label;
        public BinaryTreeNode Left, Right;
        public BinaryTreeNode(string label) { Label = label; }
    }

    public class PriorityBinaryTree
    {
        public BinaryTreeNode Root;

        public PriorityBinaryTree()
        {
            Root = new BinaryTreeNode("All Requests");
            Root.Left  = new BinaryTreeNode("Urgent");
            Root.Right = new BinaryTreeNode("Non-Urgent");

            Root.Left.Left   = new BinaryTreeNode("Critical (Priority 1)");
            Root.Left.Right  = new BinaryTreeNode("High (Priority 2)");
            Root.Right.Left  = new BinaryTreeNode("Medium (Priority 3)");
            Root.Right.Right = new BinaryTreeNode("Low (Priority 4)");
        }

        // Classifies a numeric priority into its matching leaf label.
        // Unlike a BST, this does not search — it is a fixed-shape decision tree.
        public string Classify(int priority)
        {
            switch (priority)
            {
                case 1: return "Critical (Priority 1)";
                case 2: return "High (Priority 2)";
                case 3: return "Medium (Priority 3)";
                default: return "Low (Priority 4)";
            }
        }

        public List<string> Print()
        {
            var lines = new List<string>();
            PrintRec(Root, 0, lines);
            return lines;
        }

        private void PrintRec(BinaryTreeNode node, int depth, List<string> lines)
        {
            if (node == null) return;
            string prefix = depth == 0 ? "" : new string(' ', (depth - 1) * 4) + "  +-- ";
            lines.Add(prefix + node.Label);
            PrintRec(node.Left, depth + 1, lines);
            PrintRec(node.Right, depth + 1, lines);
        }
    }

    // ══════════════════════════════════════════════════════════════════════════
    // BINARY SEARCH TREE (BST)
    // Used to store and retrieve ServiceRequests by RequestId in O(log n) avg
    // ══════════════════════════════════════════════════════════════════════════
    public class BSTNode
    {
        public ServiceRequest Data;
        public BSTNode Left, Right;
        public BSTNode(ServiceRequest d) { Data = d; }
    }

    public class ServiceRequestBST
    {
        private BSTNode _root;

        public void Insert(ServiceRequest req)
        {
            _root = InsertRec(_root, req);
        }

        private BSTNode InsertRec(BSTNode node, ServiceRequest req)
        {
            if (node == null) return new BSTNode(req);
            int cmp = string.Compare(req.RequestId, node.Data.RequestId, StringComparison.Ordinal);
            if (cmp < 0)      node.Left  = InsertRec(node.Left,  req);
            else if (cmp > 0) node.Right = InsertRec(node.Right, req);
            return node;
        }

        public ServiceRequest Search(string requestId)
        {
            var node = SearchRec(_root, requestId);
            return node?.Data;
        }

        private BSTNode SearchRec(BSTNode node, string id)
        {
            if (node == null) return null;
            int cmp = string.Compare(id, node.Data.RequestId, StringComparison.Ordinal);
            if (cmp == 0) return node;
            return cmp < 0 ? SearchRec(node.Left, id) : SearchRec(node.Right, id);
        }

        // In-order traversal returns all requests sorted by ID
        public List<ServiceRequest> InOrder()
        {
            var result = new List<ServiceRequest>();
            InOrderRec(_root, result);
            return result;
        }

        private void InOrderRec(BSTNode node, List<ServiceRequest> result)
        {
            if (node == null) return;
            InOrderRec(node.Left, result);
            result.Add(node.Data);
            InOrderRec(node.Right, result);
        }
    }

    // ══════════════════════════════════════════════════════════════════════════
    // AVL TREE (Self-balancing BST)
    // Guarantees O(log n) search even in worst case — prevents BST degradation
    // ══════════════════════════════════════════════════════════════════════════
    public class AVLNode
    {
        public ServiceRequest Data;
        public AVLNode Left, Right;
        public int Height;
        public AVLNode(ServiceRequest d) { Data = d; Height = 1; }
    }

    public class ServiceRequestAVL
    {
        private AVLNode _root;

        private int Height(AVLNode n) => n?.Height ?? 0;
        private int BalanceFactor(AVLNode n) => n == null ? 0 : Height(n.Left) - Height(n.Right);
        private void UpdateHeight(AVLNode n) { if (n != null) n.Height = 1 + Math.Max(Height(n.Left), Height(n.Right)); }

        private AVLNode RotateRight(AVLNode y)
        {
            var x = y.Left; var T2 = x.Right;
            x.Right = y; y.Left = T2;
            UpdateHeight(y); UpdateHeight(x);
            return x;
        }

        private AVLNode RotateLeft(AVLNode x)
        {
            var y = x.Right; var T2 = y.Left;
            y.Left = x; x.Right = T2;
            UpdateHeight(x); UpdateHeight(y);
            return y;
        }

        public void Insert(ServiceRequest req) { _root = InsertRec(_root, req); }

        private AVLNode InsertRec(AVLNode node, ServiceRequest req)
        {
            if (node == null) return new AVLNode(req);
            int cmp = string.Compare(req.RequestId, node.Data.RequestId, StringComparison.Ordinal);
            if (cmp < 0)      node.Left  = InsertRec(node.Left,  req);
            else if (cmp > 0) node.Right = InsertRec(node.Right, req);
            else return node; // duplicate

            UpdateHeight(node);
            int bf = BalanceFactor(node);

            // LL
            if (bf > 1 && string.Compare(req.RequestId, node.Left.Data.RequestId, StringComparison.Ordinal) < 0)
                return RotateRight(node);
            // RR
            if (bf < -1 && string.Compare(req.RequestId, node.Right.Data.RequestId, StringComparison.Ordinal) > 0)
                return RotateLeft(node);
            // LR
            if (bf > 1 && string.Compare(req.RequestId, node.Left.Data.RequestId, StringComparison.Ordinal) > 0)
            { node.Left = RotateLeft(node.Left); return RotateRight(node); }
            // RL
            if (bf < -1 && string.Compare(req.RequestId, node.Right.Data.RequestId, StringComparison.Ordinal) < 0)
            { node.Right = RotateRight(node.Right); return RotateLeft(node); }

            return node;
        }

        public ServiceRequest Search(string id)
        {
            var n = _root;
            while (n != null)
            {
                int cmp = string.Compare(id, n.Data.RequestId, StringComparison.Ordinal);
                if (cmp == 0) return n.Data;
                n = cmp < 0 ? n.Left : n.Right;
            }
            return null;
        }

        public List<ServiceRequest> InOrder()
        {
            var r = new List<ServiceRequest>();
            InOrderRec(_root, r);
            return r;
        }

        private void InOrderRec(AVLNode n, List<ServiceRequest> r)
        {
            if (n == null) return;
            InOrderRec(n.Left, r);
            r.Add(n.Data);
            InOrderRec(n.Right, r);
        }
    }

    // ══════════════════════════════════════════════════════════════════════════
    // RED-BLACK TREE (used for category-based grouping)
    // Guarantees balanced tree with O(log n) — used to group requests by category
    // ══════════════════════════════════════════════════════════════════════════
    public enum RBColor { Red, Black }

    public class RBNode
    {
        public ServiceRequest Data;
        public RBNode Left, Right, Parent;
        public RBColor Color;
        public RBNode(ServiceRequest d) { Data = d; Color = RBColor.Red; }
    }

    public class ServiceRequestRBTree
    {
        private RBNode _root;
        private RBNode _nil = new RBNode(null) { Color = RBColor.Black };

        public ServiceRequestRBTree()
        {
            _root = _nil;
        }

        public void Insert(ServiceRequest req)
        {
            var z = new RBNode(req) { Left = _nil, Right = _nil, Parent = _nil };
            var y = _nil;
            var x = _root;

            while (x != _nil)
            {
                y = x;
                x = string.Compare(req.RequestId, x.Data.RequestId, StringComparison.Ordinal) < 0
                    ? x.Left : x.Right;
            }

            z.Parent = y;
            if (y == _nil) _root = z;
            else if (string.Compare(req.RequestId, y.Data.RequestId, StringComparison.Ordinal) < 0)
                y.Left = z;
            else y.Right = z;

            InsertFixup(z);
        }

        private void InsertFixup(RBNode z)
        {
            while (z.Parent.Color == RBColor.Red)
            {
                if (z.Parent == z.Parent.Parent.Left)
                {
                    var y = z.Parent.Parent.Right;
                    if (y.Color == RBColor.Red)
                    {
                        z.Parent.Color = RBColor.Black;
                        y.Color = RBColor.Black;
                        z.Parent.Parent.Color = RBColor.Red;
                        z = z.Parent.Parent;
                    }
                    else
                    {
                        if (z == z.Parent.Right) { z = z.Parent; RotateLeft(z); }
                        z.Parent.Color = RBColor.Black;
                        z.Parent.Parent.Color = RBColor.Red;
                        RotateRight(z.Parent.Parent);
                    }
                }
                else
                {
                    var y = z.Parent.Parent.Left;
                    if (y.Color == RBColor.Red)
                    {
                        z.Parent.Color = RBColor.Black;
                        y.Color = RBColor.Black;
                        z.Parent.Parent.Color = RBColor.Red;
                        z = z.Parent.Parent;
                    }
                    else
                    {
                        if (z == z.Parent.Left) { z = z.Parent; RotateRight(z); }
                        z.Parent.Color = RBColor.Black;
                        z.Parent.Parent.Color = RBColor.Red;
                        RotateLeft(z.Parent.Parent);
                    }
                }
                if (z == _root) break;
            }
            _root.Color = RBColor.Black;
        }

        private void RotateLeft(RBNode x)
        {
            var y = x.Right;
            x.Right = y.Left;
            if (y.Left != _nil) y.Left.Parent = x;
            y.Parent = x.Parent;
            if (x.Parent == _nil) _root = y;
            else if (x == x.Parent.Left) x.Parent.Left = y;
            else x.Parent.Right = y;
            y.Left = x; x.Parent = y;
        }

        private void RotateRight(RBNode x)
        {
            var y = x.Left;
            x.Left = y.Right;
            if (y.Right != _nil) y.Right.Parent = x;
            y.Parent = x.Parent;
            if (x.Parent == _nil) _root = y;
            else if (x == x.Parent.Right) x.Parent.Right = y;
            else x.Parent.Left = y;
            y.Right = x; x.Parent = y;
        }

        public List<ServiceRequest> InOrder()
        {
            var r = new List<ServiceRequest>();
            InOrderRec(_root, r);
            return r;
        }

        private void InOrderRec(RBNode n, List<ServiceRequest> r)
        {
            if (n == _nil || n == null) return;
            InOrderRec(n.Left, r);
            if (n.Data != null) r.Add(n.Data);
            InOrderRec(n.Right, r);
        }
    }

    // ══════════════════════════════════════════════════════════════════════════
    // MIN-HEAP (Priority Queue)
    // Used to always surface the highest-priority (lowest number) service request
    // ══════════════════════════════════════════════════════════════════════════
    public class ServiceRequestHeap
    {
        private readonly List<ServiceRequest> _heap = new List<ServiceRequest>();

        private int Parent(int i) => (i - 1) / 2;
        private int Left(int i)   => 2 * i + 1;
        private int Right(int i)  => 2 * i + 2;

        private void Swap(int i, int j)
        {
            var t = _heap[i]; _heap[i] = _heap[j]; _heap[j] = t;
        }

        public void Insert(ServiceRequest req)
        {
            _heap.Add(req);
            int i = _heap.Count - 1;
            // Bubble up
            while (i > 0 && _heap[Parent(i)].Priority > _heap[i].Priority)
            {
                Swap(i, Parent(i));
                i = Parent(i);
            }
        }

        public ServiceRequest PeekMin() => _heap.Count > 0 ? _heap[0] : null;

        public ServiceRequest ExtractMin()
        {
            if (_heap.Count == 0) return null;
            var min = _heap[0];
            _heap[0] = _heap[_heap.Count - 1];
            _heap.RemoveAt(_heap.Count - 1);
            Heapify(0);
            return min;
        }

        private void Heapify(int i)
        {
            int smallest = i;
            int l = Left(i), r = Right(i);
            if (l < _heap.Count && _heap[l].Priority < _heap[smallest].Priority) smallest = l;
            if (r < _heap.Count && _heap[r].Priority < _heap[smallest].Priority) smallest = r;
            if (smallest != i) { Swap(i, smallest); Heapify(smallest); }
        }

        public List<ServiceRequest> GetAll() => new List<ServiceRequest>(_heap)
            .OrderBy(x => x.Priority).ThenBy(x => x.RequestId).ToList();

        public int Count => _heap.Count;
    }

    // ══════════════════════════════════════════════════════════════════════════
    // GRAPH — Ward/District adjacency
    // Models relationships between municipal wards for service routing
    // Supports BFS, DFS, and Minimum Spanning Tree (Prim's algorithm)
    // ══════════════════════════════════════════════════════════════════════════
    public class GraphEdge
    {
        public string From   { get; set; }
        public string To     { get; set; }
        public int    Weight { get; set; }  // distance/cost between wards
    }

    public class MunicipalGraph
    {
        // Adjacency list: ward name → list of (neighbour, weight)
        private readonly Dictionary<string, List<(string neighbour, int weight)>> _adj
            = new Dictionary<string, List<(string, int)>>(StringComparer.OrdinalIgnoreCase);

        // Ward → service requests in that ward
        private readonly Dictionary<string, List<ServiceRequest>> _wardRequests
            = new Dictionary<string, List<ServiceRequest>>(StringComparer.OrdinalIgnoreCase);

        public IEnumerable<string> Vertices => _adj.Keys;

        public void AddVertex(string ward)
        {
            if (!_adj.ContainsKey(ward))
            {
                _adj[ward] = new List<(string, int)>();
                _wardRequests[ward] = new List<ServiceRequest>();
            }
        }

        public void AddEdge(string from, string to, int weight)
        {
            AddVertex(from); AddVertex(to);
            _adj[from].Add((to, weight));
            _adj[to].Add((from, weight));  // undirected
        }

        public void AddRequestToWard(string ward, ServiceRequest req)
        {
            AddVertex(ward);
            _wardRequests[ward].Add(req);
        }

        public void RemoveRequestFromWard(string ward, ServiceRequest req)
        {
            if (_wardRequests.ContainsKey(ward))
                _wardRequests[ward].Remove(req);
        }

        public List<ServiceRequest> GetRequestsForWard(string ward)
        {
            return _wardRequests.ContainsKey(ward)
                ? _wardRequests[ward]
                : new List<ServiceRequest>();
        }

        public int GetRequestCountForWard(string ward)
            => GetRequestsForWard(ward).Count;

        // ── BFS Traversal ─────────────────────────────────────
        public List<string> BFS(string start)
        {
            var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var queue   = new Queue<string>();
            var result  = new List<string>();

            if (!_adj.ContainsKey(start)) return result;
            queue.Enqueue(start);
            visited.Add(start);

            while (queue.Count > 0)
            {
                var node = queue.Dequeue();
                result.Add(node);
                foreach (var (neighbour, _) in _adj[node])
                {
                    if (!visited.Contains(neighbour))
                    {
                        visited.Add(neighbour);
                        queue.Enqueue(neighbour);
                    }
                }
            }
            return result;
        }

        // ── DFS Traversal ─────────────────────────────────────
        public List<string> DFS(string start)
        {
            var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var result  = new List<string>();
            DFSRec(start, visited, result);
            return result;
        }

        private void DFSRec(string node, HashSet<string> visited, List<string> result)
        {
            if (!_adj.ContainsKey(node) || visited.Contains(node)) return;
            visited.Add(node);
            result.Add(node);
            foreach (var (neighbour, _) in _adj[node])
                DFSRec(neighbour, visited, result);
        }

        // ── Prim's MST ────────────────────────────────────────
        // Returns list of edges forming the Minimum Spanning Tree
        public List<GraphEdge> PrimMST()
        {
            var mst      = new List<GraphEdge>();
            var inMST    = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var minEdge  = new Dictionary<string, (string from, int weight)>(StringComparer.OrdinalIgnoreCase);

            if (!_adj.Keys.Any()) return mst;

            string start = _adj.Keys.First();
            inMST.Add(start);

            foreach (var (nb, w) in _adj[start])
                minEdge[nb] = (start, w);

            while (inMST.Count < _adj.Count)
            {
                // Find minimum weight edge to a vertex not yet in MST
                string best = null;
                int bestW = int.MaxValue;
                foreach (var kv in minEdge)
                {
                    if (!inMST.Contains(kv.Key) && kv.Value.weight < bestW)
                    {
                        best  = kv.Key;
                        bestW = kv.Value.weight;
                    }
                }
                if (best == null) break;

                mst.Add(new GraphEdge { From = minEdge[best].from, To = best, Weight = bestW });
                inMST.Add(best);
                minEdge.Remove(best);

                // Update min edges from newly added vertex
                if (_adj.ContainsKey(best))
                    foreach (var (nb, w) in _adj[best])
                        if (!inMST.Contains(nb) && (!minEdge.ContainsKey(nb) || w < minEdge[nb].weight))
                            minEdge[nb] = (best, w);
            }

            return mst;
        }

        public List<string> GetAllWards() => _adj.Keys.OrderBy(w => w).ToList();
        public List<(string neighbour, int weight)> GetNeighbours(string ward)
            => _adj.ContainsKey(ward) ? _adj[ward] : new List<(string, int)>();
    }

    // ══════════════════════════════════════════════════════════════════════════
    // SERVICE REQUEST DATA STORE — ties all structures together
    // ══════════════════════════════════════════════════════════════════════════
    public class ServiceRequestStore
    {
        private readonly ServiceRequestBST    _bst    = new ServiceRequestBST();
        private readonly ServiceRequestAVL    _avl    = new ServiceRequestAVL();
        private readonly ServiceRequestRBTree _rbt    = new ServiceRequestRBTree();
        private readonly ServiceRequestHeap   _heap   = new ServiceRequestHeap();
        public  readonly MunicipalGraph       Graph   = new MunicipalGraph();

        // Basic Tree and plain Binary Tree — added for Task 3 technical requirement
        public readonly CategoryBasicTree   CategoryTree = new CategoryBasicTree();
        public readonly PriorityBinaryTree  PriorityTree = new PriorityBinaryTree();

        private readonly List<ServiceRequest> _all    = new List<ServiceRequest>();
        private int _counter = 1;

        public ServiceRequestStore() { SeedGraph(); SeedRequests(); }

        public void AddRequest(ServiceRequest req)
        {
            req.RequestId = $"SR-{_counter:D5}";
            _counter++;
            // Give seeded requests (which omit SubmittedAt) a realistic recent date
            if (req.SubmittedAt == default(DateTime))
                req.SubmittedAt = DateTime.Now.AddDays(-(_counter));
            _all.Add(req);
            _bst.Insert(req);
            _avl.Insert(req);
            _rbt.Insert(req);
            _heap.Insert(req);
            Graph.AddRequestToWard(req.Ward, req);
        }

        public ServiceRequest FindById(string id) => _avl.Search(id);  // AVL for guaranteed O(log n)

        public List<ServiceRequest> GetAllSortedById() => _bst.InOrder();

        // Red-Black tree in-order traversal — also returns sorted order,
        // demonstrating a second, differently-balanced tree holding the same data.
        public List<ServiceRequest> GetAllSortedByIdRedBlack() => _rbt.InOrder();

        public List<ServiceRequest> GetByPriority() => _heap.GetAll();

        public List<ServiceRequest> GetAll() => new List<ServiceRequest>(_all);

        public List<ServiceRequest> GetByStatus(string status)
            => _all.Where(r => r.Status.Equals(status, StringComparison.OrdinalIgnoreCase)).ToList();

        public List<ServiceRequest> GetByCategory(string category)
            => _all.Where(r => r.Category.Equals(category, StringComparison.OrdinalIgnoreCase)).ToList();

        public bool UpdateStatus(string id, string newStatus)
        {
            var req = FindById(id);
            if (req == null) return false;
            req.Status = newStatus;
            if (newStatus == "Resolved" || newStatus == "Closed")
                req.ResolvedAt = DateTime.Now;
            return true;
        }

        // ── ADMIN: reassign a request to a different ward ──────
        public bool ReassignWard(string id, string newWard)
        {
            var req = FindById(id);
            if (req == null) return false;
            Graph.RemoveRequestFromWard(req.Ward, req);
            req.Ward = newWard;
            Graph.AddRequestToWard(newWard, req);
            return true;
        }

        // ── ADMIN: approve or reject a request ─────────────────
        public bool SetApproval(string id, string approval)
        {
            var req = FindById(id);
            if (req == null) return false;
            req.Approval = approval;
            // When a request is rejected, close it; when approved, move it forward.
            if (approval == "Rejected") req.Status = "Closed";
            else if (approval == "Approved" && req.Status == "Pending") req.Status = "In Progress";
            return true;
        }

        public int TotalCount    => _all.Count;
        public int PendingCount  => _all.Count(r => r.Status == "Pending");
        public int InProgressCount => _all.Count(r => r.Status == "In Progress");
        public int ResolvedCount => _all.Count(r => r.Status == "Resolved" || r.Status == "Closed");

        // ── Seed the ward graph ────────────────────────────────
        private void SeedGraph()
        {
            // Ekurhuleni wards — edges represent adjacency with travel distance (km)
            Graph.AddEdge("Ward 1 - Benoni CBD",      "Ward 2 - Rynfield",      4);
            Graph.AddEdge("Ward 1 - Benoni CBD",      "Ward 3 - Northmead",     3);
            Graph.AddEdge("Ward 2 - Rynfield",        "Ward 4 - Lakefield",     5);
            Graph.AddEdge("Ward 3 - Northmead",       "Ward 4 - Lakefield",     4);
            Graph.AddEdge("Ward 3 - Northmead",       "Ward 5 - Farrarmere",    6);
            Graph.AddEdge("Ward 4 - Lakefield",       "Ward 6 - Birchleigh",    7);
            Graph.AddEdge("Ward 5 - Farrarmere",      "Ward 6 - Birchleigh",    5);
            Graph.AddEdge("Ward 6 - Birchleigh",      "Ward 7 - Benoni North",  8);
            Graph.AddEdge("Ward 7 - Benoni North",    "Ward 1 - Benoni CBD",    9);
        }

        // ── Seed sample service requests ──────────────────────
        private void SeedRequests()
        {
            AddRequest(new ServiceRequest { Title = "Pothole on Main Street",         Category = "Roads",        Location = "Main St, Benoni CBD",       Description = "Large pothole causing vehicle damage near traffic light.",                    Priority = 1, Ward = "Ward 1 - Benoni CBD",   Status = "In Progress" });
            AddRequest(new ServiceRequest { Title = "Broken Street Light",            Category = "Electricity",  Location = "Rynfield Ave",               Description = "Street light out for 3 days, safety concern at night.",                      Priority = 2, Ward = "Ward 2 - Rynfield",      Status = "Pending" });
            AddRequest(new ServiceRequest { Title = "Water Pipe Burst",               Category = "Water",        Location = "Oak Road, Northmead",        Description = "Water gushing from burst pipe, flooding the road.",                          Priority = 1, Ward = "Ward 3 - Northmead",     Status = "Resolved", Approval = "Approved", ResolvedAt = DateTime.Now.AddDays(-1) });
            AddRequest(new ServiceRequest { Title = "Illegal Dumping Site",           Category = "Sanitation",   Location = "Lakefield Industrial Area",  Description = "Large pile of construction waste dumped illegally on public land.",           Priority = 3, Ward = "Ward 4 - Lakefield",     Status = "Pending" });
            AddRequest(new ServiceRequest { Title = "Park Equipment Damaged",         Category = "Parks",        Location = "Farrarmere Recreation Park", Description = "Children's swings broken and slide damaged, safety hazard.",                  Priority = 2, Ward = "Ward 5 - Farrarmere",    Status = "In Progress" });
            AddRequest(new ServiceRequest { Title = "Overgrown Vegetation",           Category = "Parks",        Location = "Birchleigh North Road",      Description = "Tree branches blocking road signs and overhead cables.",                      Priority = 3, Ward = "Ward 6 - Birchleigh",    Status = "Pending" });
            AddRequest(new ServiceRequest { Title = "Storm Drain Blocked",            Category = "Stormwater",   Location = "Benoni North Highway",       Description = "Storm drain completely blocked, causing flooding after rain.",                Priority = 1, Ward = "Ward 7 - Benoni North",  Status = "In Progress" });
            AddRequest(new ServiceRequest { Title = "Graffiti on Public Building",    Category = "Public Safety",Location = "Benoni Civic Centre",        Description = "Graffiti vandalism on the eastern wall of the civic centre.",                Priority = 4, Ward = "Ward 1 - Benoni CBD",    Status = "Pending" });
            AddRequest(new ServiceRequest { Title = "Traffic Light Malfunction",      Category = "Roads",        Location = "Northmead Mall Intersection",Description = "Traffic light stuck on red during peak hours, causing congestion.",          Priority = 1, Ward = "Ward 3 - Northmead",     Status = "Resolved", Approval = "Approved", ResolvedAt = DateTime.Now.AddDays(-2) });
            AddRequest(new ServiceRequest { Title = "Water Meter Reading Request",    Category = "Water",        Location = "Rynfield Residential Area",  Description = "Meter has not been read for 3 months, requesting manual reading.",           Priority = 4, Ward = "Ward 2 - Rynfield",      Status = "Closed", Approval = "Approved", ResolvedAt = DateTime.Now.AddDays(-5) });
            AddRequest(new ServiceRequest { Title = "Bin Collection Missed",          Category = "Sanitation",   Location = "Lakefield Suburbs",          Description = "Refuse collection skipped our street for the second week in a row.",         Priority = 2, Ward = "Ward 4 - Lakefield",     Status = "Resolved", Approval = "Approved", ResolvedAt = DateTime.Now });
            AddRequest(new ServiceRequest { Title = "Sinkhole on Pavement",           Category = "Roads",        Location = "Farrarmere Drive",           Description = "Sinkhole approximately 1m wide appeared on the pavement near house 42.",     Priority = 1, Ward = "Ward 5 - Farrarmere",    Status = "Pending" });
            AddRequest(new ServiceRequest { Title = "Streetlight Pole Leaning",       Category = "Electricity",  Location = "Birchleigh East",            Description = "Streetlight pole leaning dangerously after vehicle collision.",              Priority = 2, Ward = "Ward 6 - Birchleigh",    Status = "In Progress" });
            AddRequest(new ServiceRequest { Title = "Abandoned Vehicle",              Category = "Public Safety",Location = "Benoni North Industrial",    Description = "Burnt-out vehicle abandoned on the pavement for over a week.",               Priority = 3, Ward = "Ward 7 - Benoni North",  Status = "Pending" });
            AddRequest(new ServiceRequest { Title = "Noise Complaint due to Construction", Category = "Public Safety",Location = "Benoni CBD South",           Description = "Construction site operating beyond permitted hours, disturbing residents.",   Priority = 3, Ward = "Ward 1 - Benoni CBD",    Status = "In Progress" });
        }
    }
}
