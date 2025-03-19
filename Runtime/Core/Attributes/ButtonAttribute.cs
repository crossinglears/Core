using System;

namespace CrossingLears
{
    [AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public sealed class ButtonAttribute : Attribute
    {
        private string name = null;
        public string Name => name;

        public ButtonAttribute() { }
        public ButtonAttribute(string name) { this.name = name; }
    }
}