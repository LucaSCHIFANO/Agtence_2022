using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding
{
    public class Heap<T> where T : IHeapItem<T>
    {
        private T[] items;

        private int count;
        public int Count => count;

        public Heap(int maxHeapSize)
        {
            items = new T[maxHeapSize];
        }

        public void Add(T _item)
        {
            _item.HeapIndex = count;
            items[count] = _item;
            
            SortUp(_item);

            count++;
        }

        public T RemoveFirst()
        {
            T firstItem = items[0];
            
            count--;

            items[0] = items[count];
            items[0].HeapIndex = 0;
            
            SortDown(items[0]);

            return firstItem;
        }

        public void UpdateItem(T _item)
        {
            SortUp(_item);
        }

        public bool Contains(T _item)
        {
            return Equals(items[_item.HeapIndex], _item);
        }

        private void SortDown(T _item)
        {
            while (true)
            {
                int childIndexLeft = _item.HeapIndex * 2 + 1;
                int childIndexRight = _item.HeapIndex * 2 + 2;
                int swapIndex = 0;
                
                if (childIndexLeft < count)
                {
                    swapIndex = childIndexLeft;

                    if (childIndexRight < count)
                    {
                        if (items[childIndexLeft].CompareTo(items[childIndexRight]) < 0)
                        {
                            swapIndex = childIndexRight;
                        }
                    }

                    if (_item.CompareTo(items[swapIndex]) < 0)
                    {
                        Swap(_item, items[swapIndex]);
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    return;
                }
            }
        }

        private void SortUp(T _item)
        {
            int parentIndex = (_item.HeapIndex - 1) / 2;
            
            while (true)
            {
                T parentItem = items[parentIndex];
                if (_item.CompareTo(parentItem) > 0)
                {
                    Swap(_item, parentItem);
                }
                else
                {
                    break;
                }

                parentIndex = (_item.HeapIndex - 1) / 2;
            }
        }

        private void Swap(T _itemA, T _itemB)
        {
            items[_itemA.HeapIndex] = _itemB;
            items[_itemB.HeapIndex] = _itemA;
            
            (_itemA.HeapIndex, _itemB.HeapIndex) = (_itemB.HeapIndex, _itemA.HeapIndex);
        }
    }

    public interface IHeapItem<T> : IComparable<T>
    {
        int HeapIndex { get; set; }
    }
}
