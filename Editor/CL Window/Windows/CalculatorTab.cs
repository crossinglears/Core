using UnityEngine;
using UnityEditor;

namespace CrossingLearsEditor
{
    public class CalculatorTab : CL_WindowTab_WIP
    {
        public override string TabName => "Calculator";

        private string a = "";
        private string b = "";
        private string result = "";

        public override void DrawContent()
        {
            a = EditorGUILayout.TextField("A", a);
            b = EditorGUILayout.TextField("B", b);

            if (GUILayout.Button("Add"))
            {
                float fa;
                float fb;
                if (float.TryParse(a, out fa) && float.TryParse(b, out fb))
                {
                    result = (fa + fb).ToString();
                }
            }

            if (GUILayout.Button("Subtract"))
            {
                float fa;
                float fb;
                if (float.TryParse(a, out fa) && float.TryParse(b, out fb))
                {
                    result = (fa - fb).ToString();
                }
            }

            if (GUILayout.Button("Multiply"))
            {
                float fa;
                float fb;
                if (float.TryParse(a, out fa) && float.TryParse(b, out fb))
                {
                    result = (fa * fb).ToString();
                }
            }

            if (GUILayout.Button("Divide"))
            {
                float fa;
                float fb;
                if (float.TryParse(a, out fa) && float.TryParse(b, out fb))
                {
                    if (fb != 0f)
                    {
                        result = (fa / fb).ToString();
                    }
                }
            }

            EditorGUILayout.LabelField("Result");
            EditorGUILayout.SelectableLabel(result, GUILayout.Height(20));
        }
    }
}
