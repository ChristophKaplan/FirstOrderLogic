using System.Collections;
using System.Collections.Generic;
using FirstOrderLogic;
using UnityEngine;



namespace Helpers {

    //PROTOTYPE

    public class StringGraphics {
        public class Cell {
            public enum CellMargin { left, right, center }

            public Vector2Int cellSize;
            public Vector2Int position;
            public string content;
            public Cell(Vector2Int position, Vector2Int cellSize) {
                this.position = position;
                this.cellSize = cellSize;
                SetContent(" ", ' ', CellMargin.left);
            }

            public void SetContent(string content, char fill, CellMargin cellMargin) {
                this.content = "";
                if (this.content.Length < cellSize.x) {
                    if (cellMargin == CellMargin.center) {
                        CenterContent(content, fill);
                        return;
                    }
                    if (cellMargin == CellMargin.right) {
                        int start = cellSize.x - content.Length;
                        for (int i = 0; i < start; i++) this.content += fill;
                        this.content += content;

                        return;
                    }


                    this.content = content;
                    for (int i = this.content.Length; i < cellSize.x; i++) this.content += fill;

                } else {
                    this.content = content;
                }
            }
            private void CenterContent(string contentToCenter, char fill) {
                int cellCenter = cellSize.x / 2;
                int contentCenter = contentToCenter.Length / 2;
                int start = cellCenter - contentCenter;

                for (int i = 0; i < start; i++) this.content += fill;
                this.content += contentToCenter;
                for (int i = this.content.Length; i < cellSize.x; i++) this.content += fill;
            }

            public string GetContent() {
                return content;
            }
            public string GetContent(int x, int y) {
                if (x >= cellSize.x && y >= cellSize.y) return " ";

                int index = x + (y * ((cellSize.x) + 1));
                return content.Substring(index, 1);
            }
        }

        private Vector2Int gridSize;
        private Vector2Int cellSize;
        private Cell[,] cells;
        public Cell[,] GetCells() => this.cells;
        private string rendering = "";
        public string GetRendering() => this.rendering;

        public StringGraphics(Vector2Int gridSize, Vector2Int cellSize) {
            this.gridSize = gridSize;
            this.cellSize = cellSize;
            cells = new Cell[this.gridSize.x, this.gridSize.y];

            //init
            for (int y = 0; y < this.gridSize.y; y++) {
                for (int x = 0; x < this.gridSize.x; x++) {
                    cells[x, y] = new Cell(new Vector2Int(x, y), this.cellSize);
                }
            }
            for (int y = 0; y < this.gridSize.y * cellSize.y; y++) {
                for (int x = 0; x < this.gridSize.x * cellSize.x; x++) {
                    rendering += " ";
                }
                rendering += "\n";
            }

            RenderCells();
        }

        public Cell GetCell(int x, int y) {
            if (!IsInRange(x, y)) {
                Debug.Log("out of range + " +x+"/"+y + " grid:" + gridSize);
                return null;
            }
            return GetCells()[x, y];
        }

        public bool IsInRange(int x, int y) {
            return (x < gridSize.x && y < gridSize.y && x >= 0 && y >= 0);
        }

        public void RenderCells() {
            for (int y = 0; y < this.gridSize.y; y++) {
                for (int x = 0; x < this.gridSize.x; x++) {
                    Cell current = cells[x, y];
                    PaintCell(current);
                }
            }
        }

        int PosToString(int x, int y) {
            return x + (y * ((gridSize.x * cellSize.x) + 1));
        }

        void PaintAt(string brush, int x, int y) {
            int indexPos = PosToString(x, y);
            rendering = rendering.Remove(indexPos, brush.Length).Insert(indexPos, brush);
        }

        void PaintCell(Cell cell) {
            for (int y = 0; y < cellSize.y; y++) {
                for (int x = 0; x < cellSize.x; x++) {
                    PaintAt(cell.GetContent(x, y), (cell.position.x * cellSize.x) + x, (cell.position.y * cellSize.y) + y);
                }
            }
        }

        public void SetCell(Cell cell) {
            //Debug.Log("SetCell:" + cell.position.x + "/" + cell.position.y);
            this.cells[cell.position.x, cell.position.y] = cell;
        }

        public void Draw() {
            Debug.Log(rendering);
        }


    }

    public class StringTree {
        private Node root;
        private int maxLevel;
        private int yGapSize = 2;
        private int xGapSize = 1;

        private Vector2Int nodeSize;
        private StringGraphics canvas;
        private int maxContentLength;

        public class Node {
            private int level;
            private int id;
            private Node parent;
            private List<Node> children;
            private string nodeContent;
            private List<string> edgeContent = new List<string>();
            public List<string> GetEdgeContent() => this.edgeContent;
            public int GetLevel() => this.level;
            public int GetID() => this.id;
            public Node GetParent() => this.parent;
            public List<Node> GetChildren() => this.children;


            public string GetNodeContent() => nodeContent;
            public void SetContent(string c) {
                this.nodeContent = c;
            }

            public Node(Node parent, int level, int id) {
                this.parent = parent;
                this.children = new List<Node>();
                this.id = id;
                this.level = level;
            }

            public bool IsLeaf() => this.children.Count == 0;
            public bool IsRoot() => this.parent == null;

            public void AddChild(Node n) {
                if (this.children.Contains(n)) {
                    //Debug.Log("already contained!");
                    return;
                }

                n.parent = this;
                n.level = this.level + 1;
                n.id = children.Count;
                this.children.Add(n);
            }
            public int GetDeepesLevel() {
                if (IsLeaf()) return this.level;
                int max = this.level;

                for (int i = 0; i < children.Count; i++) {
                    int cur = children[i].GetDeepesLevel();
                    if (cur > max) max = cur;
                }
                return max;
            }
            public List<Node> GetAllOnLevel(int level) {
                List<Node> collected = new List<Node>();
                if (GetLevel() == level) collected.Add(this);
                if (!IsLeaf()) {
                    for (int i = 0; i < GetChildren().Count; i++) {
                        List<Node> collectedChild = GetChildren()[i].GetAllOnLevel(level);
                        collected.AddRange(collectedChild);
                    }
                }
                return collected;
            }

            public List<Node> GetLeafs() {
                List<Node> collected = new List<Node>();
                if (this.IsLeaf()) {
                    collected.Add(this);
                } else {
                    for (int i = 0; i < GetChildren().Count; i++) {
                        List<Node> collectedChild = GetChildren()[i].GetLeafs();
                        collected.AddRange(collectedChild);
                    }
                }
                return collected;
            }

            public override string ToString() => this.nodeContent;

            /*
            public override bool Equals(object obj) {
                Node other = (Node)obj;
                return other.ToString().Equals(this.ToString());
            }
            public override int GetHashCode() {
                return ToString().GetHashCode();
            }*/
        }

        public StringTree() {

        }

        public string GetSyntaxTree(Sentence sentence) {
            this.root = new Node(null, 0, 0);
            FillSyntaxTree(this.root, sentence);
            this.nodeSize = new Vector2Int(this.maxContentLength, 1);
            this.Init();
            return RenderTree();
        }


        public void Init() {
            this.maxLevel = root.GetDeepesLevel();
            int nodeAmount = root.GetLeafs().Count;
            int gridSizeX = nodeAmount + ((nodeAmount - 1) * xGapSize);
            int gridSizeY = ((maxLevel + 1) + (maxLevel * yGapSize));
            //Debug.Log("max:" + gridSizeX + "/" + gridSizeY);

            canvas = new StringGraphics(new Vector2Int(gridSizeX, gridSizeY), this.nodeSize);
        }


        private void SetNodeCell(Vector2Int pos, string content) {
            canvas.GetCells()[pos.x, pos.y].SetContent(content, ' ', StringGraphics.Cell.CellMargin.center);
        }

        private void SetEdgeCells(Vector2Int posFrom, Vector2Int posTo, string edgeContent = null) {

            //same
            if (posFrom.x == posTo.x) {
                canvas.GetCells()[posFrom.x, posFrom.y + 1].SetContent("|", ' ', StringGraphics.Cell.CellMargin.center);
                canvas.GetCells()[posTo.x, posTo.y - 1].SetContent("|", ' ', StringGraphics.Cell.CellMargin.center);
            }

            if (posFrom.x != posTo.x) {
                canvas.GetCells()[posFrom.x, posFrom.y + 1].SetContent("/\\", '_', StringGraphics.Cell.CellMargin.center);
            }

            //right
            if (posFrom.x < posTo.x) {
                canvas.GetCells()[posTo.x, posTo.y - 1].SetContent("\\", ' ', StringGraphics.Cell.CellMargin.center);

                string cont = "";
                int a = Mathf.FloorToInt(nodeSize.x / 2f);
                for (int j = 0; j < a; j++) cont += "_";
                canvas.GetCells()[posTo.x, posFrom.y + 1].SetContent(cont, ' ', StringGraphics.Cell.CellMargin.left);

                if (posTo.x - posFrom.x > 1) {
                    for (int i = posFrom.x + 1; i < posTo.x; i++) {
                        canvas.GetCells()[i, posFrom.y + 1].SetContent("_", '_', StringGraphics.Cell.CellMargin.left);
                    }
                }

            }

            //left
            if (posFrom.x > posTo.x) {
                canvas.GetCells()[posTo.x, posTo.y - 1].SetContent("/", ' ', StringGraphics.Cell.CellMargin.center);
                string cont = "";
                int a = Mathf.FloorToInt(nodeSize.x / 2f);
                for (int j = 0; j < a; j++) cont += "_";
                canvas.GetCells()[posTo.x, posFrom.y + 1].SetContent(cont, ' ', StringGraphics.Cell.CellMargin.right);

                if (posFrom.x - posTo.x > 1) {
                    for (int i = posTo.x + 1; i < posFrom.x; i++) {
                        canvas.GetCells()[i, posFrom.y + 1].SetContent("_", '_', StringGraphics.Cell.CellMargin.left);
                    }
                }
            }

            if (yGapSize == 3 && edgeContent != null) {
                canvas.GetCells()[posTo.x, posTo.y - 2].SetContent(edgeContent, ' ', StringGraphics.Cell.CellMargin.center);
            }

        }



        public string RenderTree() {
            Dictionary<Node, Vector2Int> leafMap = new Dictionary<Node, Vector2Int>();

            List<Node> leafs = root.GetLeafs();
            for (int i = 0; i < leafs.Count; i++) {
                leafMap.Add(leafs[i], new Vector2Int(i + i * xGapSize, leafs[i].GetLevel() + (leafs[i].GetLevel() * yGapSize)));
            }

            Queue<Node> stack = new Queue<Node>();
            stack.Enqueue(root);

            while (stack.Count > 0) {

                Node current = stack.Dequeue();
                for (int i = 0; i < current.GetChildren().Count; i++) stack.Enqueue(current.GetChildren()[i]);

                Vector2Int nodePos = GetCenteredNodePosition(current, leafMap);
                SetNodeCell(nodePos, current.GetNodeContent());

                //edges
                for (int i = 0; i < current.GetChildren().Count; i++) {
                    Node child = current.GetChildren()[i];

                    Vector2Int childNodePos = GetCenteredNodePosition(child, leafMap);

                    string edgeContent = null;
                    if (current.GetEdgeContent().Count > 0) edgeContent = current.GetEdgeContent()[i];

                    SetEdgeCells(nodePos, childNodePos, edgeContent);
                }
            }

            canvas.RenderCells();
            return canvas.GetRendering();
        }

        private Vector2Int GetCenteredNodePosition(Node node, Dictionary<Node, Vector2Int> leafMap) {
            if (node.IsLeaf()) return leafMap[node];

            int minX = int.MaxValue;
            int maxX = int.MinValue;
            int y = int.MinValue;

            for (int i = 0; i < node.GetChildren().Count; i++) {
                Node child = node.GetChildren()[i];
                Vector2Int center = GetCenteredNodePosition(child, leafMap);
                if (center.x < minX) minX = center.x;
                if (center.x > maxX) maxX = center.x;
                y = center.y - 1 - yGapSize;
            }
            return new Vector2Int((minX + maxX) / 2, y);
        }


        public void FillSyntaxTree(Node node, Sentence cur) {
            string content = "";
            if (cur.IsAtom()) content = cur.ToString();
            if (cur.IsComplex()) content = cur.AsComplex().GetOperator().ToString();

            if (maxContentLength < content.Length) maxContentLength = content.Length;
            node.SetContent(content);

            if (cur.IsComplex()) {
                if (cur.IsConnective() && !cur.IsLiteral()) {
                    Sentence p = cur.AsComplex().GetP();
                    Sentence q = cur.AsComplex().GetQ();

                    Node pNode = new Node(node, node.GetLevel() + 1, 0);
                    Node qNode = new Node(node, node.GetLevel() + 1, 1);

                    node.AddChild(pNode);
                    node.AddChild(qNode);

                    FillSyntaxTree(pNode, p);
                    FillSyntaxTree(qNode, q);
                } else {
                    Sentence p = cur.AsComplex().GetP();
                    Node pNode = new Node(node, node.GetLevel() + 1, 0);
                    node.AddChild(pNode);

                    FillSyntaxTree(pNode, p);
                }
            }
        }



    }




}