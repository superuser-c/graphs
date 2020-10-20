using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace grafy {
    class Vertex : IEnumerable<Tuple<int, float>> {
        private Dictionary<int, float> paths = new Dictionary<int, float>();

        public Vertex() {
            
        }

        public IEnumerator<Tuple<int, float>> GetEnumerator() {
            foreach (var x in paths) {
                yield return new Tuple<int, float>(x.Key, x.Value);
            }
        }
        IEnumerator IEnumerable.GetEnumerator() {
            foreach (var x in paths) {
                yield return new Tuple<int, float>(x.Key, x.Value);
            }
        }

        public float? this[int x] {
            get {
                if (paths.ContainsKey(x))
                    return paths[x];
                return null;
            }
            set {
                if (value == null) {
                    if (paths.ContainsKey(x)) {
                        paths.Remove(x);
                        return;
                    }
                    throw new ArgumentException("Hey! I am not able to remove non-existing path!");
                }
                if (paths.ContainsKey(x))
                    paths[x] = value.Value;
                else
                    paths.Add(x, value.Value);
            }
        }
    }

    class Graph : IEnumerable<Vertex> {
        private List<Vertex> verts = new List<Vertex>();
        public int vertc { get { return verts.Count; } }
        bool directed;

        public Graph(int x) {
            for (int i = 0; i < x; i++) {
                verts.Add(new Vertex());
            }
        }
        public IEnumerator<Vertex> GetEnumerator() {
            foreach (var x in verts) {
                yield return x;
            }
        }
        IEnumerator IEnumerable.GetEnumerator() {
            foreach (var x in verts) {
                yield return x;
            }
        }
        public Vertex this[int x] {
            get {
                return verts[x];
            }
        }
        public float? this[int from, int to] {
            get {
                return verts[from][to];
            }
            set {
                verts[from][to] = value;
            }
        }

        private Tuple<float, float> VertToPos(int i, int scale) {
            float a = 3.141592f * 2 / verts.Count;
            float ax = (float)Math.Sin(a * i), ay = (float)Math.Cos(a * i);
            return new Tuple<float, float>((.5f + ax * .45f) * scale, (.5f + ay * .45f) * scale);
        }
        public void Render(Graphics g, int scale) {
            g.Clear(Color.Black);
            float a = 3.141592f * 2 / verts.Count;
            for (int i = 0; i < verts.Count; i++) {
                float x = (float)Math.Sin(a * i), y = (float)Math.Cos(a * i);
                float px = (.5f + x * .45f) * scale, py = (.5f + y * .45f) * scale;
                foreach (var path in verts[i]) {
                    if (directed && i > path.Item1)
                        continue;
                    Color c = Color.FromArgb(0, (int)(192 * path.Item2), 255);
                    float ax = (float)Math.Sin(a * path.Item1), ay = (float)Math.Cos(a * path.Item1);
                    float vx = (.5f + ax * .45f) * scale, vy = (.5f + ay * .45f) * scale;
                    g.DrawLine(new Pen(c), px, py, vx, vy);
                }
                g.FillEllipse(Brushes.DeepSkyBlue, px - 5, py - 5, 10, 10);
            }
        }
        public void Render(Graphics g, int scale, int[] path) {
            g.Clear(Color.Black);
            float a = 3.141592f * 2 / verts.Count;
            for (int i = 0; i < verts.Count; i++) {
                Tuple<float, float> point = VertToPos(i, scale);
                foreach (var p in verts[i]) {
                    if (directed && i > p.Item1)
                        continue;
                    Color c = Color.FromArgb(0, (int)(192 * p.Item2), 255);
                    Tuple<float, float> v = VertToPos(p.Item1, scale);
                    bool inpath = false;
                    for (int j = 0; j < path.Length - 1; j++) {
                        if ((path[j] == i && path[j + 1] == p.Item1) ||
                            (path[j] == p.Item1 && path[j + 1] == i)) {
                            inpath = true;
                            break;
                        }
                    }
                    if (!inpath)
                        g.DrawLine(new Pen(c), point.Item1, point.Item2, v.Item1, v.Item2);
                }
                g.FillEllipse(Brushes.DeepSkyBlue, point.Item1 - 5, point.Item2 - 5, 10, 10);
            }
            if (path.Length > 0) {
                Tuple<float, float> point2 = VertToPos(path[0], scale);
                for (int i = 1; i < path.Length; i++) {
                    Tuple<float, float> point = VertToPos(path[i], scale);
                    g.DrawLine(Pens.Lime, point.Item1, point.Item2, point2.Item1, point2.Item2);
                    point2 = point;
                }
            }
        }

        public static Graph Random(int verts, bool directed, int pathrate) {
            Random r = new Random();
            Graph g = new Graph(verts);
            g.directed = directed;
            for (int i = 0; i < verts; i++) {
                for (int j = directed ? 0 : i + 1; j < verts; j++) {
                    if (i != j && r.Next(100) < pathrate) {
                        g[i, j] = (float)r.NextDouble();
                        if (!directed)
                            g[j, i] = g[i, j];
                    }
                }
            }
            //if (!g.Connected())
            //    g = Random(verts, directed, pathrate);
            return g;
        }

        private void AddVerts(List<int> v, int pos) {
            v.Add(pos);
            foreach (var p in this[pos]) {
                if (!v.Contains(p.Item1))
                    AddVerts(v, p.Item1);
            }
        }
        public bool Connected() {
            List<int> found = new List<int>();
            AddVerts(found, 0);
            return found.Count == vertc;
        }

        private Tuple<float, List<int>> SearchPath(float[] dists, int to, int v, float dist, List<int> path) {
            List<int> mypath = new List<int>(path) { v };
            dists[v] = dist;
            List<int> best = null;
            float bestdist = float.MaxValue;
            foreach (var p in this[v]) {
                Tuple<float, List<int>> a = new Tuple<float, List<int>>(float.MaxValue, null);
                if (p.Item1 == to) {
                    a = new Tuple<float, List<int>>(dist + p.Item2, new List<int>(mypath) { p.Item1 });
                } else if (dists[p.Item1] > dist + p.Item2) {
                    a = SearchPath(dists, to, p.Item1, dist + p.Item2, mypath);
                }
                if (a.Item1 < bestdist) {
                    bestdist = a.Item1;
                    best = a.Item2;
                }
            }
            return new Tuple<float, List<int>>(bestdist, best);
        }
        public Tuple<float, List<int>> FindPath(int from, int to) {
            if (from == to)
                return new Tuple<float, List<int>>(0, new List<int>() { to });
            float[] dists = new float[vertc];
            for (int i = 0; i < dists.Length; i++) {
                dists[i] = float.MaxValue;
            }
            return SearchPath(dists, to, from, 0, new List<int>());
        }
    }
}
