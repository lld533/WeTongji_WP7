using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WeTongji.Api.Domain
{
    public class CalendarGroup<T> : IEnumerable<T> where T : CalendarNode
    {
        public CalendarGroup(DateTime key, IEnumerable<T> items)
        {
            this.Key = key;
            this.Items = items.OrderBy((T) => T).ToList<T>();
        }

        public override bool Equals(object obj)
        {
            CalendarGroup<T> that = obj as CalendarGroup<T>;

            return (that != null) && (this.Key.Equals(that.Key));
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public DateTime Key
        {
            get;
            set;
        }

        public IList<T> Items
        {
            get;
            set;
        }

        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator()
        {
            return this.Items.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.Items.GetEnumerator();
        }

        #endregion
    }

    public static class CalendarGroupUtil
    {
        public static CalendarNode GetNextCalendarNode(this List<CalendarGroup<CalendarNode>> list)
        {
            //...Remove NoArrangement Node
            for (int i = 0; i < list.Count; ++i)
            {
                var group = list[i];

                if (group.Items.Count > 0 && group.Items.Last().IsNoArrangementNode)
                {
                    group.Items.RemoveAt(group.Items.Count - 1);
                    break;
                }
            }


            CalendarNode result = null;
            var yesterday = (DateTime.Now - TimeSpan.FromDays(1)).Date;
            var today = DateTime.Now.Date;

            var yesterdayGroup = list.Where((g) => g.Key == yesterday).SingleOrDefault();
            var todayGroup = list.Where((g) => g.Key == today).SingleOrDefault();

            if (yesterdayGroup != null)
            {
                var nodeToRemove = yesterdayGroup.Where((node) => node.IsNoArrangementNode).SingleOrDefault();
                if (nodeToRemove != null)
                    yesterdayGroup.Items.Remove(nodeToRemove);
            }

            if (todayGroup == null)
            {
                int groupIdx = -1;

                groupIdx = list.Where((g) => g.Key < today).Count();
                list.Insert(groupIdx, new CalendarGroup<CalendarNode>(today, new CalendarNode[] { CalendarNode.NoArrangementNode }));
                result = list[groupIdx].Single();
            }
            else
            {
                if (todayGroup.Last().IsNoArrangementNode)
                {
                    result = todayGroup.Last();
                }
                else
                {
                    int nodeIdx = todayGroup.Where((node) => node.BeginTime < DateTime.Now).Count();

                    if (nodeIdx == todayGroup.Count())
                    {
                        todayGroup.Items.Add(CalendarNode.NoArrangementNode);
                        result = todayGroup.Items.Last();
                    }
                    else
                    {
                        result = todayGroup.Items[nodeIdx];
                    }
                }
            }

            return result;
        }

        public static void InsertCalendarNode(this List<CalendarGroup<CalendarNode>> list, CalendarNode node)
        {
            //...Refresh
            list.GetNextCalendarNode();

            var g = list.Where((group) => group.Key == node.BeginTime.Date).SingleOrDefault();

            //...The date of the activity exists in Agenda
            if (g != null)
            {
                //...Remove no arrangement node if there exists.
                if (g.Count() > 0 && g.Last().IsNoArrangementNode)
                {
                    g.Items.RemoveAt(g.Count() - 1);
                }

                var targetNode = g.Where((n) => n == node).SingleOrDefault();
                if (targetNode == null)
                {
                    int idx = g.Items.Where((n) => n.BeginTime < node.BeginTime).Count();
                    g.Items.Insert(idx, node);
                }
            }
            //...The date of the activity does not exist in Agenda
            else
            {
                int idx = list.Where((group) => group.Key < node.BeginTime.Date).Count();
                list.Insert(idx, new CalendarGroup<CalendarNode>(node.BeginTime.Date, new CalendarNode[] { node }));
            }

            list.GetNextCalendarNode();
        }

        public static void RemoveCalendarNode(this List<CalendarGroup<CalendarNode>> list, CalendarNode node)
        {
            var group = list.Where((g) => g.Key == node.BeginTime.Date).SingleOrDefault();

            if (group != null)
            {
                var target = group.Where((n) => n.CompareTo(node) == 0).SingleOrDefault();

                if (target != null)
                    group.Items.Remove(target);

                if (group.Count() == 0)
                {
                    if (group.Key != DateTime.Now.Date)
                        list.Remove(group);
                    else
                        group.Items.Add(CalendarNode.NoArrangementNode);
                }
            }
        }
    }
}
