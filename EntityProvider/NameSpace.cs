using System;

namespace EntityProvider
{
    public struct NameSpace : IEquatable<NameSpace>
    {
        public readonly string Name;

        public NameSpace(string name)
        {
            Name = name;
        }

        public bool Equals(NameSpace other)
        {
            return Name == other.Name;
        }

        public override string ToString()
        {
            return Name;
        }

        public static implicit operator string(NameSpace ns)
        {
            return ns.Name;
        }

        public static implicit operator NameSpace(string ns)
        {
            return new NameSpace(ns);
        }
    }
}
