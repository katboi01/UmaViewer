using UnityEngine;
using System.Collections;

namespace RootMotion
{

    /// <summary>
    /// Comment attribute for Editor.
    /// </summary>
    public class InspectorComment : PropertyAttribute
    {

        public string name;
        public string color = "white";

        public InspectorComment(string name)
        {
            this.name = name;
            this.color = "white";
        }

        public InspectorComment(string name, string color)
        {
            this.name = name;
            this.color = color;
        }
    }
}
