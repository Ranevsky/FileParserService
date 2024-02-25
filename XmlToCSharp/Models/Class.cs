using XmlToCSharp.Extensions;

namespace XmlToCSharp.Models;

internal class Class
{
    public string Name { get; set; } = null!;
    public ICollection<Field> Fields { get; set; } = null!;

    public bool Equals(Class other)
    {
        return string.Equals(Name, other.Name) && Fields.Matches(other.Fields);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj.GetType() != GetType())
        {
            return false;
        }

        return Equals((Class)obj);
    }

    public override int GetHashCode()
    {
        return Name.GetHashCode();
    }
}