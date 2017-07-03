using System;
using System.Collections.Generic;
using System.Linq;

namespace RevolutionaryStuff.Core.Collections
{
    public class TreeNode<TData>
    {
        public TData Data { get; private set; }

        public TreeNode<TData> Parent { get; set; }

        public IList<TreeNode<TData>> Children { get; } = new List<TreeNode<TData>>();

        public bool HasChildren => Children.Count > 0;

        public TreeNode<TData> this[int index] => Children[index];

        public void Add(TreeNode<TData> child)
        {
            child.Parent = this;
            Children.Add(child);
        }

        public TreeNode<TData> Add(TData child) => AddChildren(child)[0];

        public IList<TreeNode<TData>> AddChildren(params TData[] children) => AddChildren((IEnumerable<TData>)children);

        public IList<TreeNode<TData>> AddChildren(IEnumerable<TData> children)
        {
            var added = new List<TreeNode<TData>>();
            if (children != null)
            {
                foreach (var kid in children)
                {
                    var tn = new TreeNode<TData>(kid, this);
                    added.Add(tn);
                }
            }
            return added;
        }

        public override string ToString() => $"TreeNode: Children={this.Children.Count} Data={Data?.ToString()}";

        public TreeNode(TData data, TreeNode<TData> parent = null, IEnumerable<TData> children = null)
        {
            Data = data;
            if (parent != null)
            {
                Parent = parent;
                parent.Children.Add(this);
            }
            AddChildren(children);
        }

#if false
        public static IList<TreeNode<TData>> CreateRoots<ID>(IEnumerable<TData> items, Func<TData, ID> getItemId, Func<TData, ID> getParentId, Func<IList<TreeNode<TData>>> childListCreator = null, int? depth = null)
        {
            childListCreator = childListCreator ?? delegate { return new List<TreeNode<TData>>(); };
            var d = items.ToDictionary(getItemId, item => new TreeNode<TData>(item, children: childListCreator()));
            var roots = new List<TreeNode<TData>>();
            foreach (var node in d.Values)
            {
                var parentId = getParentId(node.Data);
                if (parentId != null && d.ContainsKey(parentId))
                {
                    d[parentId].Children.Add(node);
                }
                else
                {
                    roots.Add(node);
                }
            }
            if (depth.HasValue)
            {
                foreach (var r in roots)
                {
                    r.Walk((a, b, c) =>
                    {
                        if (c > depth && a != null)
                        {
                            a.Children.Clear();
                            return false;
                        }
                        return true;
                    });
                }
            }
            return roots;
        }
#endif

        public enum WalkOrder { Breadth, Depth }

        public void Walk(Action<TreeNode<TData>, int> visit, WalkOrder order = WalkOrder.Depth) => Walk((tn, d) => { visit(tn, d); return true; }, order);

        public void Walk(Func<TreeNode<TData>, int, bool> visit, WalkOrder order = WalkOrder.Depth) => Walk(visit, order, 0);

        private void Walk(Func<TreeNode<TData>, int, bool> visit, WalkOrder order, int depth)
        {
            if (visit(this, depth))
            {
                switch (order)
                {
                    case WalkOrder.Breadth:
                        var entrances = new List<TreeNode<TData>>(Children.Count);
                        foreach (var kid in Children)
                        {
                            if (visit(kid, depth+1))
                            {
                                entrances.Add(kid);
                            }
                        }
                        entrances.ForEach(e => e.Walk(visit, order, depth+1));
                        break;
                    case WalkOrder.Depth:
                        foreach (var kid in Children)
                        {
                            kid.Walk(visit, order, depth+1);
                        }
                        break;
                    default:
                        throw new UnexpectedSwitchValueException(order);
                }
            }
        }

        public static IDictionary<K, TreeNode<TData>> Flatten<K>(IEnumerable<TreeNode<TData>> level, Func<TreeNode<TData>, K> getKey)
        {
            var d = new Dictionary<K, TreeNode<TData>>();
            foreach (var z in level)
            {
                d[getKey(z)] = z;
                z.Walk(delegate (TreeNode<TData> parent, TreeNode<TData> item, int depth)
                {
                    d[getKey(item)] = item;
                    return true;
                });
            }
            return d;
        }

        public void Walk(Func<TreeNode<TData>, TreeNode<TData>, int, bool> pre, Action<TreeNode<TData>, TreeNode<TData>, int> post = null, Comparison<TData> walkOrder = null)
        {
            Walk(pre, null, 0, post, walkOrder);
        }

        private void Walk(Func<TreeNode<TData>, TreeNode<TData>, int, bool> pre, TreeNode<TData> parent, int depth, Action<TreeNode<TData>, TreeNode<TData>, int> post = null, Comparison<TData> walkOrder = null)
        {
            if (pre == null || pre(parent, this, depth))
            {
                var kids = this.Children.ToList();
                if (walkOrder != null)
                {
                    kids.Sort((a, b) => walkOrder(a.Data, b.Data));
                }
                foreach (var kid in kids)
                {
                    kid.Walk(pre, this, depth + 1, post, walkOrder);
                }
                post?.Invoke(parent, this, depth);
            }
        }
    }
}
