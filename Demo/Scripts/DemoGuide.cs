using UnityEngine;

namespace CrossingLears
{
    /// <summary>
    /// Draws an on-screen guide for the Crossing Lears Core demo scene.
    /// Uses IMGUI so it has no font, TextMeshPro, or prefab dependencies and
    /// works immediately in Play mode.
    /// </summary>
    public class DemoGuide : MonoBehaviour
    {
        [TextArea(2, 4)]
        public string Title = "Crossing Lears Core - Demo";

        private Vector2 scroll;

        private static readonly string[] Sections =
        {
            "1) Inspector Attributes  [Button] / [ReadOnly]\n" +
            "   Select 'Attribute Showcase' in the Hierarchy.\n" +
            "   - 'readOnlyStatus' is drawn but locked ([ReadOnly]).\n" +
            "   - Click the 'Log Demo Message' / 'ResetDemoState' buttons\n" +
            "     that [Button] adds to the Inspector.\n" +
            "   Setup: add [Button] on a method, [ReadOnly] on a field.",

            "2) StartState / StartStateController\n" +
            "   'StartState Sphere (Close)' has a StartState set to Close.\n" +
            "   On Play, StartStateController triggers it and the sphere hides.\n" +
            "   Setup: add StartState to objects, then one StartStateController\n" +
            "   per scene (use its 'Get All StartStates' button to collect them).",

            "3) PlatformDependent\n" +
            "   'PlatformDependent Cube' is limited to selected build targets.\n" +
            "   Objects whose target does not match are excluded at build time.\n" +
            "   Setup: add PlatformDependent, click 'Add All' or pick targets.",

            "4) OnEnableScript\n" +
            "   'Attribute Showcase' also has OnEnableScript exposing\n" +
            "   OnEnable / OnDisable UnityEvents you can wire in the Inspector.",

            "5) UI: RadialMenu + SmoothScrollRect\n" +
            "   The 'Radial Menu' arranges its child items in a circle.\n" +
            "   SmoothScrollRect adds eased scrolling to a ScrollRect.\n" +
            "   Setup: add RadialMenu to a RectTransform with child items;\n" +
            "   use SmoothScrollRect.ReplaceWithSmoothScrollRect on a ScrollRect.",

            "Toolbox: open 'Window > Crossing Lears Core > Toolbox' for the editor tools.\n" +
            "Docs: Runtime/Documentation (Core).txt"
        };

        private void OnGUI()
        {
            float width = Mathf.Min(560f, Screen.width - 20f);
            float height = Mathf.Min(520f, Screen.height - 20f);

            GUILayout.BeginArea(new Rect(10f, 10f, width, height), GUI.skin.box);

            GUIStyle titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                wordWrap = true
            };
            GUILayout.Label(Title, titleStyle);

            GUIStyle bodyStyle = new GUIStyle(GUI.skin.label)
            {
                wordWrap = true
            };

            scroll = GUILayout.BeginScrollView(scroll);
            for (int i = 0; i < Sections.Length; i++)
            {
                GUILayout.Label(Sections[i], bodyStyle);
                GUILayout.Space(8f);
            }
            GUILayout.EndScrollView();

            GUILayout.EndArea();
        }
    }
}
