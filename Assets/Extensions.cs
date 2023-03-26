using System;
using System.Collections.Generic;
using System.Linq;

internal static class Extensions
{
    public static IEnumerable<T> Column<T>(this T[,] arr, int column)
    {
        return Enumerable.Range(0, arr.GetLength(1)).Select(i => arr[column, i]);
    }

    public static IEnumerable<T> Row<T>(this T[,] arr, int row)
    {
        return Enumerable.Range(0, arr.GetLength(0)).Select(i => arr[i, row]);
    }

    public static IEnumerable<IEnumerable<T>> Rows<T>(this T[,] arr)
    {
        return Enumerable.Range(0, arr.GetLength(1)).Select(i => arr.Row(i));
    }

    public static IEnumerable<IEnumerable<T>> Columns<T>(this T[,] arr)
    {
        return Enumerable.Range(0, arr.GetLength(0)).Select(i => arr.Column(i));
    }

    public static IEnumerable<T> Flat<T>(this T[,] arr)
    {
        return arr.Rows().SelectMany(i => i);
    }

    public static IEnumerable<IEnumerable<T>> AllArrangements<T>(this IEnumerable<T> e)
    {
        int l = e.Count();
        int res = 1;
        for(int step = 2; step < l; ++step)
            res *= step;

        for(int iter = 0; iter < res; ++iter)
            yield return e.Arranged(iter);
    }

    public static IEnumerable<T> Arranged<T>(this IEnumerable<T> e, int arrangement)
    {
        List<T> l = e.ToList();
        while(l.Count > 0)
        {
            yield return l[arrangement % l.Count];
            arrangement /= l.Count;
            l.RemoveAt(arrangement % l.Count);
        }
    }

    public static int SumPositive(this IEnumerable<int> e)
    {
        int total = 0;
        foreach(int el in e)
            if(el > 0)
                total += el;
        return total;
    }
}