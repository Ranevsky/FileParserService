namespace XmlToCSharp.Models;

internal class Field
{
    public string Name { get; set; } = null!;
    public string TypeName { get; set; } = null!;
    public FieldType Type { get; set; }

    private bool Equals(Field other)
    {
        return Equals(Type, other.Type) &&
               string.Equals(Name, other.Name) &&
               string.Equals(TypeName, other.TypeName);
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

        return Equals((Field)obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = Name.GetHashCode();
            hashCode = (hashCode * 397) ^ TypeName.GetHashCode();
            hashCode = (hashCode * 397) ^ Type.GetHashCode();

            return hashCode;
        }
    }
}