using System;

namespace CrossingLears
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    public class CL_CommandAttribute : Attribute
    {
        public string Key { get; set; }
        public MethodType m_type { get; set; } = MethodType.Asset;
        public bool CanCallAll { get; set; }

        public CL_CommandAttribute(string key, MethodType methodType, bool CanCallAll = false)
        {
            Key = key;
            m_type = methodType;
            this.CanCallAll = CanCallAll;
        }

        public CL_CommandAttribute(MethodType methodType)
        {
            m_type = methodType;
        }
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
    public class CL_CommandHolderAttribute : Attribute
    {
        public CL_CommandHolderAttribute() { }
    }

    public enum MethodType
    {
        Asset,
        SceneObject,
        Static
    }
}
