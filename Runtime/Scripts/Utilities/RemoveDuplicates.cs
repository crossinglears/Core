using System.Collections.Generic;

public static class RemoveDuplicatesExtension
{
    private static List<T> RemoveDuplicates<T>(IEnumerable<T> source)
    {
        HashSet<T> seen = new HashSet<T>();
        List<T> result = new List<T>();

        foreach (T item in source)
        {
            if (seen.Add(item))
            {
                result.Add(item);
            }
        }

        return result;
    }

    public static void ClearDuplicates<T>(this List<T> list)
    {
        if (list == null)
        {
            return;
        }

        List<T> result = RemoveDuplicates(list);
        list.Clear();

        for (int i = 0; i < result.Count; i++)
        {
            list.Add(result[i]);
        }
    }

    public static void ClearDuplicates<T>(this Queue<T> queue)
    {
        if (queue == null)
        {
            return;
        }

        List<T> result = RemoveDuplicates(queue);
        queue.Clear();

        for (int i = 0; i < result.Count; i++)
        {
            queue.Enqueue(result[i]);
        }
    }

    public static T[] ClearDuplicates<T>(this T[] array)
    {
        if (array == null)
        {
            return null;
        }

        return RemoveDuplicates(array).ToArray();
    }
}
