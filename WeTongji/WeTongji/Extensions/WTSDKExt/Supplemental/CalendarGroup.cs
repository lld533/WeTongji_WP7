﻿using System;
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

        public Boolean IsToday
        {
            get { return Key == DateTime.Now.Date; }
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
                if (yesterdayGroup.Items.Count == 1 && yesterdayGroup.Items.Single().IsNoArrangementNode)
                {
                    list.Remove(yesterdayGroup);
                }
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
                if (todayGroup.Count() == 0)
                {
                    todayGroup.Items.Add(CalendarNode.NoArrangementNode);
                    result = todayGroup.First();
                }
                else
                {
                    int nodeIdx = todayGroup.Where((node) => node.BeginTime < DateTime.Now).Count();

                    if (nodeIdx < todayGroup.Count())
                    {
                        result = todayGroup.Items[nodeIdx];
                    }
                    else
                    {
                        result = CalendarNode.NoArrangementNode;
                    }
                }
            }

            return result;
        }

        public static CalendarNode GetNextIconicTileCalendarNode(this List<CalendarGroup<CalendarNode>> list)
        {
            if (list == null || list.Count == 0)
                return null;

            var groups = list.Where(group => group.Key >= DateTime.Now.Date).OrderBy(group => group.Key);
            var firstGroup = groups.First();
            var firstNode = firstGroup.Where(node => node.BeginTime > DateTime.Now && !node.IsNoArrangementNode).FirstOrDefault();

            if (firstNode != null)
            {
                return firstNode;
            }
            else
            {
                var restGroups = groups.Skip(1).ToArray();

                if (restGroups == null || restGroups.Count() == 0)
                    return null;
                else
                    return restGroups.First().FirstOrDefault();
            }
        }

        public static void InsertCalendarNode(this List<CalendarGroup<CalendarNode>> list, CalendarNode node)
        {
            //...Refresh
            list.GetNextCalendarNode();

            var g = list.Where((group) => group.Key == node.BeginTime.Date).SingleOrDefault();

            //...The date of the activity exists in Agenda
            if (g != null)
            {
                //...Remove potential no arrangement node
                if (g.Key == DateTime.Now.Date && g.Items.Count == 1 && g.Single().IsNoArrangementNode)
                {
                    g.Items.Clear();
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
