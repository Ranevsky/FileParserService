using System.Xml.Linq;
using XmlToCSharp.Models;

namespace XmlToCSharp.Extensions;

internal static class XElementExtension
{
    public static IEnumerable<Class> ExtractClassInfo(this XElement element)
    {
        var classes = new List<Class>();
        ElementToClass(element, classes);

        return classes.GroupBy(
            @class => @class.Name,
            (name, classesGroup) =>
            {
                var classesList = classesGroup as ICollection<Class> ?? classesGroup.ToList();

                if (classesList.Count > 1)
                {
                    return new Class
                    {
                        Name = name,
                        Fields = classesList
                            .SelectMany(@class => @class.Fields)
                            .DistinctBy(field => field.Name)
                            .Select(field => new Field
                            {
                                Name = field.Name,
                                TypeName = field.TypeName,
                                Type = field.Type.Add(FieldType.Nullable),
                            })
                            .ToList(),
                    };
                }

                return classesList.First();
            });

        static Class ElementToClass(XElement element, ICollection<Class> classes)
        {
            var @class = new Class
            {
                Name = element.Name.LocalName,
                Fields = ReplaceDuplicatesWithLists(ExtractFields(element, classes)).ToList(),
            };

            if (element.Parent == null || (!classes.Contains(@class) && @class.Fields.Count != 0))
            {
                classes.Add(@class);
            }

            return @class;
        }

        static IEnumerable<Field> ReplaceDuplicatesWithLists(IEnumerable<Field> fields)
        {
            return fields.GroupBy(
                field => field.Name,
                (_, fieldsGroup) =>
                {
                    Field anyElement;
                    bool hasManyFields;
                    if (fieldsGroup is ICollection<Field> fieldsCollection)
                    {
                        hasManyFields = fieldsCollection.Count > 1;
                        anyElement = fieldsCollection.First();
                    }
                    else
                    {
                        var twoElements = fieldsGroup.Take(2).ToList();
                        hasManyFields = twoElements.Count > 1;
                        anyElement = twoElements.First();
                    }

                    if (hasManyFields)
                    {
                        anyElement.Type = anyElement.Type.Add(FieldType.Collection);
                    }

                    return anyElement;
                });
        }

        static IEnumerable<Field> ExtractFields(XElement element, ICollection<Class> classes)
        {
            foreach (var childElement in element.Elements())
            {
                var tempClass = ElementToClass(childElement, classes);

                var isEmpty = childElement is { HasAttributes: false, HasElements: false };
                string type;
                FieldType fieldType = 0;
                if (isEmpty)
                {
                    type = GetType(childElement.Value);
                }
                else
                {
                    type = tempClass.Name;
                    fieldType.Add(FieldType.NotPrimitive);
                }

                yield return new Field
                {
                    Name = tempClass.Name,
                    TypeName = type,
                    Type = fieldType,
                };
            }

            if (element.HasAttributes)
            {
                if (!element.HasElements)
                {
                    yield return new Field
                    {
                        Name = "Value",
                        TypeName = GetType(element.Value),
                    };
                }

                foreach (var attribute in element.Attributes())
                {
                    yield return new Field
                    {
                        Name = attribute.Name.LocalName,
                        TypeName = GetType(attribute.Value),
                    };
                }
            }
        }

        static string GetType(string value)
        {
            if (IsBoolean(value))
            {
                return PrimitiveTypeConst.Bool;
            }

            var hasOneSeparator = false;
            foreach (var symbol in value)
            {
                var isDigit = char.IsDigit(symbol);
                if (isDigit)
                {
                    continue;
                }

                var isSeparator = symbol is '.' or ',';
                if (!isSeparator || hasOneSeparator)
                {
                    return PrimitiveTypeConst.String;
                }

                hasOneSeparator = true;
            }

            return hasOneSeparator ? PrimitiveTypeConst.Double : PrimitiveTypeConst.Int;

            static bool IsBoolean(string value)
            {
                const int trueValueLength = 4; // true
                const int falseValueLength = 5; // false

                return value.Length switch
                {
                    trueValueLength => IsTrue(value),
                    falseValueLength => IsFalse(value),
                    _ => false,
                };

                static bool IsTrue(string value)
                {
                    const string trueValue = "true";

                    return !value.Where((t, i) => char.ToLower(t) != trueValue[i]).Any();
                }

                static bool IsFalse(string value)
                {
                    const string falseValue = "false";

                    return !value.Where((t, i) => char.ToLower(t) != falseValue[i]).Any();
                }
            }
        }
    }
}