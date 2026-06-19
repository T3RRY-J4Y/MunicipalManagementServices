# Municipal Services Application — Part 3 (Full Functioning App)
## ST10202185 | AAPD7112

This is the final implementation of the Municipal Services Application. It brings together all three parts: Report Issues (Part 1), Local Events and Announcements (Part 2), and the new Service Request Status feature (Part 3), which is built on a wide range of trees, heaps, and graph algorithms. A staff-only Admin area was also added, giving municipal staff a dashboard with analytics and the ability to manage requests, events, and reported issues, including an approve/reject workflow.

---

## HOW TO COMPILE AND RUN

### Requirements
- Windows OS
- Visual Studio 2019 or 2022 (Community edition is free at visualstudio.microsoft.com)
- .NET Framework 4.7.2 (installed with Visual Studio)

### Steps to run
1. Extract the zip file to a folder.
2. Open Visual Studio.
3. Click File then Open then Project/Solution.
4. Select MunicipalManagementServices.sln.
5. Press F5 (or click the green Start button) to build and run.

### If it does not build
- Right-click the solution in Solution Explorer and choose Rebuild Solution.
- Make sure .NET Framework 4.7.2 is installed via the Visual Studio Installer.
- If the program is already running, stop it (Shift+F5) before rebuilding, otherwise the .exe stays locked.

---

## HOW TO USE THE SERVICE REQUEST STATUS FEATURE

From the main menu, click Service Request Status. The page has six tabs:

1. All Requests — every request sorted by ID, with status and category filters.
2. By Priority — requests ordered most-urgent-first using the min-heap.
3. Tree Explorer — buttons that print each tree structure so you can see how they organise the data.
4. By Ward / Graph — shows the ward map, request counts per ward, and buttons to run BFS, DFS, and the Minimum Spanning Tree.
5. Search by ID — type a request ID (for example SR-00001) to fetch it instantly via the AVL tree, then update its status.
6. Submit New — citizens lodge a new request, which is inserted into every data structure at once.

---

## HOW TO USE THE ADMIN AREA (STAFF ONLY)

The main menu shows the staff login credentials in a banner at the bottom of the page for marking purposes (username `admin`, password `admin123`). Click the Admin Login card and sign in to reach the admin dashboard, which has four tabs:

1. Dashboard — live counts of requests, events, announcements, and reported issues, plus a bar chart of requests by status.
2. Manage Requests — select any request to update its status, reassign it to a different ward, or approve/reject it. Approved requests move to In Progress; rejected requests are closed. An Approval column and row colours show each request's decision at a glance.
3. Manage Events — add a new event or announcement, edit an existing one, or delete it. Changes appear immediately in the citizen-facing Local Events page.
4. Reported Issues — view every issue submitted through the Report Issues feature, approve or reject each one, and double-click for full details.

---

## DATA STRUCTURES — ROLE AND CONTRIBUTION TO EFFICIENCY

Every structure below is implemented from scratch in DataStructures.cs (no built-in tree or graph library is used). Each one was chosen to solve a specific problem in the Service Request Status feature.

### 1. Basic Tree (generic N-ary tree)
Class: CategoryBasicTree, BasicTreeNode

Role: Models the municipal department hierarchy. The root is "Municipal Services"; below it are departments such as Infrastructure and Engineering, Community Services, and Safety and Security; and below those are the individual categories such as Roads, Water, Parks, and Sanitation.

Contribution to efficiency: A basic tree is the natural way to represent a parent-child hierarchy of arbitrary width. When a new request is logged, FindDepartment walks the tree to determine which department owns that category, so the request can be routed correctly. The search visits each node at most once, giving O(n) over the small, fixed category set, which is effectively constant time in practice.

Example: FindDepartment("Water") walks from the root into Infrastructure and Engineering and returns "Infrastructure & Engineering".

### 2. Binary Tree (plain, fixed-shape, not a search tree)
Class: PriorityBinaryTree, BinaryTreeNode

Role: A fixed decision tree that classifies a numeric priority into a readable bucket. The root splits into Urgent and Non-Urgent; Urgent splits into Critical (1) and High (2); Non-Urgent splits into Medium (3) and Low (4).

Contribution to efficiency: This demonstrates a binary tree used purely for classification rather than searching. Classify runs in O(1) because the depth is fixed at two levels. It keeps presentation logic (how a priority is described to the user) separate from the data itself.

Example: Classify(1) returns "Critical (Priority 1)".

### 3. Binary Search Tree (BST)
Class: ServiceRequestBST, BSTNode

Role: Stores every service request keyed by its RequestId. An in-order traversal returns all requests neatly sorted by ID, which powers the All Requests tab.

Contribution to efficiency: Insertion and search are O(log n) on average because each comparison discards half of the remaining tree. In-order traversal produces sorted output in O(n) without a separate sort step.

Example: Inserting SR-00005, SR-00002, SR-00009 and traversing in-order yields SR-00002, SR-00005, SR-00009.

### 4. AVL Tree (self-balancing BST)
Class: ServiceRequestAVL, AVLNode

Role: Holds the same requests but rebalances itself with rotations after every insert. The Search by ID tab uses this tree so that looking up a request is always fast, even if IDs are inserted in sorted order (which would turn a plain BST into a slow linked list).

Contribution to efficiency: Guarantees O(log n) search, insert, and delete in the worst case, not just on average. The height is kept at roughly 1.44 log n by checking the balance factor of each node and applying LL, RR, LR, or RL rotations as needed.

Example: Searching SR-00010 among the seeded requests takes about four comparisons rather than scanning the whole list.

### 5. Red-Black Tree (balanced BST)
Class: ServiceRequestRBTree, RBNode

Role: A second self-balancing tree holding the same data with a different balancing scheme based on node colours and rotations. It demonstrates an alternative, widely-used balanced tree (the kind that underpins many standard library maps).

Contribution to efficiency: Guarantees that the longest root-to-leaf path is at most twice the shortest, so search stays O(log n). Red-Black trees typically perform fewer rotations on insert than AVL trees, which makes them efficient when data changes often.

Example: In-order traversal of the Red-Black tree returns the requests sorted by ID, matching the BST output but produced from a differently-balanced structure.

### 6. Min-Heap (priority queue)
Class: ServiceRequestHeap

Role: Keeps the most urgent request (lowest priority number) immediately available. The By Priority tab is populated from this heap.

Contribution to efficiency: Peeking at the most urgent request is O(1). Inserting and extracting the minimum are O(log n) because an element only bubbles up or sifts down the height of the heap. This is far cheaper than re-sorting the whole list every time a request is added.

Example: With Critical, Medium, and High requests inserted in that order, ExtractMin returns the Critical request first.

### 7. Graph (adjacency list) with BFS, DFS, and Minimum Spanning Tree
Class: MunicipalGraph, GraphEdge

Role: Models the seven Ekurhuleni wards as vertices and the roads between them as weighted edges. Each ward also holds the list of service requests located in it.

Contribution to efficiency and purpose:
- BFS (Breadth-First Search) visits wards layer by layer from a starting ward. Useful for finding the nearest wards first. Runs in O(V + E).
- DFS (Depth-First Search) explores as far down one branch as possible before backtracking. Useful for reachability checks. Runs in O(V + E).
- Minimum Spanning Tree via Prim's algorithm finds the cheapest set of roads that still connects every ward. This is directly useful for planning efficient maintenance or inspection routes that touch every ward at minimum total distance.

Example: Running the MST on the seeded ward graph returns the set of connecting roads with the smallest total kilometres, printed in the Ward / Graph tab along with the total distance.

---

## WHICH STRUCTURE IS USED WHERE (quick reference)

| Feature in the app | Structure used | Why |
|--------------------|----------------|-----|
| All Requests tab (sorted list) | Binary Search Tree | In-order traversal gives sorted output |
| Search by ID tab | AVL Tree | Guaranteed O(log n) lookup |
| Alternative balanced storage | Red-Black Tree | Demonstrates a second balanced tree |
| By Priority tab | Min-Heap | Most urgent request always on top |
| Department routing | Basic Tree | Category-to-department hierarchy |
| Priority labelling | Binary Tree | Fixed-shape classification |
| Ward map and routing | Graph + BFS/DFS/MST | Models ward connections and optimal routes |

---

## PROJECT STRUCTURE

```
MunicipalManagementServices/
  Program.cs                  Entry point
  UITheme.cs                  Shared colours and fonts
  CustomControls.cs           Styled buttons, panels, placeholder textbox
  MainMenuForm.cs             Main menu (all four features now active)
  ReportIssueForm.cs          Report Issues (Part 1)
  LocalEvent.cs                Event model (Part 2)
  EventDataStore.cs           Part 2 data structures
  LocalEventsForm.cs          Local Events and Announcements (Part 2)
  ServiceRequest.cs           Service request model (Part 3)
  DataStructures.cs           Basic Tree, Binary Tree, BST, AVL, Red-Black,
                              Min-Heap, Graph (BFS/DFS/MST), and the store
  ServiceRequestForm.cs       Service Request Status UI (Part 3)
  AppData.cs                  Shared data layer + admin login authentication
  AdminLoginForm.cs           Staff login dialog
  AdminForm.cs                Admin dashboard (analytics, request/event/issue management)
  MunicipalManagementServices.csproj
  MunicipalManagementServices.sln
```

---

C# .NET Framework 4.7.2, Windows Forms, Visual Studio 2022.
