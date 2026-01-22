using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;

namespace CrossingLears.Editor
{
    public static class CoreFeedbackSender
    {
        private static string webhookUrl = "https://discord.com/api/webhooks/1353515379456217108/BnAvNadJTfMlCEWBwWO34ojksNf70CPs-ErQ2L9xkjXwva8OtCzDojlEUUIcYJluaLDN"; 
        
        public static void SendFeedback(string feedback)
        {
            PostToDiscord(feedback);
        }

        private static async void PostToDiscord(string message)
        {
            WWWForm form = new WWWForm();
            form.AddField("content", message);

            using (UnityWebRequest www = UnityWebRequest.Post(webhookUrl, form))
            {
                var operation = www.SendWebRequest();
                while (!operation.isDone) await Task.Yield(); // Wait until request is done

                if (www.result != UnityWebRequest.Result.Success)
                    Debug.LogError("Feedback failed: " + www.error);
                else
                    Debug.Log("Feedback sent successfully!");
            }
        }
    }
}
