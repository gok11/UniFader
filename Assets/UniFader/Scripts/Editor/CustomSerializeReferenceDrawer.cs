using System;

namespace MB.UniFader
{
    public class CustomSerializeReferenceDrawer : Attribute
    {
        public Type type;
    
        public CustomSerializeReferenceDrawer(Type type)
        {
            this.type = type;
        }
    }
}