using System.Collections;
using System.Threading.Tasks;

public static class CoroutineAsync
{
    public static IEnumerator WaitAsync(Task task)
    {
        while (!task.IsCompleted)
        {
            yield return null;
        }

        if (task.IsFaulted)
        {
            throw task.Exception;
        }
    }

    public static IEnumerator WaitAsync<T>(Task<T> task)
    {
        while (!task.IsCompleted)
        {
            yield return null;
        }

        if (task.IsFaulted)
        {
            throw task.Exception;
        }
    }
}
