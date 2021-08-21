using System;
using System.Collections.Generic;
using System.Linq;

namespace BadgerBoss
{
    [AttributeUsage(AttributeTargets.Field)]
    public class SoundClipAttribute : Attribute
    {
        public readonly string[] Names;
        public SoundClipAttribute(params string[] names)
        {
            List<string> modifiedNames = names.ToList();
            for(int i = 0; i < names.Length; i++)
                if(names[i].StartsWith("Bagels"))
                    modifiedNames.Add(names[i].Replace("Bagels", "Jakk"));
            Names = modifiedNames.ToArray();
        }
    }
}