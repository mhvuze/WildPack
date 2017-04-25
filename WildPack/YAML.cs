using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;

namespace WildPack
{
    class YAML
    {
        enum ByamlNodeType
        {
            String = 0xa0,
            Data = 0xa1,
            Boolean = 0xd0,
            Int = 0xd1,
            Single = 0xd2,
            SignedInt = 0xd3, //??
            UnamedNode = 0xc0,
            NamedNode = 0xc1,
            StringList = 0xc2,
            BinaryDataList = 0xc3,
            Null = 0xff,
        }

        abstract class ByamlNode
        {
            public long Address { get; set; }
            public long Length { get; set; }

            public abstract ByamlNodeType Type { get; }
            public virtual bool CanBeAttribute { get { return false; } }

            public virtual void ToXml(XmlDocument yaml, XmlNode node, List<string> nodes, List<string> values, List<byte[]> data)
            {
                throw new NotImplementedException();
            }

            public class String : ByamlNode
            {
                public int Value { get; set; }

                public override ByamlNodeType Type
                {
                    get { return ByamlNodeType.String; }
                }

                public String(EndianBinaryReader reader)
                {
                    Address = reader.BaseStream.Position;

                    Value = reader.ReadInt32();

                    Length = reader.BaseStream.Position - Length;
                }

                public String(int value)
                {
                    Value = value;
                }

                public override void ToXml(XmlDocument yaml, XmlNode node, List<string> nodes, List<string> values, List<byte[]> data)
                {
                    XmlAttribute attr = yaml.CreateAttribute("type");
                    attr.Value = "string";
                    node.Attributes.Append(attr);
                    node.InnerText = values[Value];
                }
            }

            public class Data : ByamlNode
            {
                public int Value { get; set; }

                public override ByamlNodeType Type
                {
                    get { return ByamlNodeType.Data; }
                }

                public Data(EndianBinaryReader reader)
                {
                    Address = reader.BaseStream.Position;

                    Value = reader.ReadInt32();

                    Length = reader.BaseStream.Position - Length;
                }

                public Data(int value)
                {
                    Value = value;
                }

                public override void ToXml(XmlDocument yaml, XmlNode node, List<string> nodes, List<string> values, List<byte[]> data)
                {
                    XmlAttribute attr = yaml.CreateAttribute("type");
                    attr.Value = "path";
                    node.Attributes.Append(attr);
                    using (EndianBinaryReader rd = new EndianBinaryReader(new MemoryStream(data[Value])))
                    {
                        while (rd.BaseStream.Position != rd.BaseStream.Length)
                        {
                            XmlElement point = yaml.CreateElement("point");
                            point.SetAttribute("x", rd.ReadSingle().ToString(CultureInfo.InvariantCulture) + "f");
                            point.SetAttribute("y", rd.ReadSingle().ToString(CultureInfo.InvariantCulture) + "f");
                            point.SetAttribute("z", rd.ReadSingle().ToString(CultureInfo.InvariantCulture) + "f");
                            point.SetAttribute("nx", rd.ReadSingle().ToString(CultureInfo.InvariantCulture) + "f");
                            point.SetAttribute("ny", rd.ReadSingle().ToString(CultureInfo.InvariantCulture) + "f");
                            point.SetAttribute("nz", rd.ReadSingle().ToString(CultureInfo.InvariantCulture) + "f");
                            point.SetAttribute("val", rd.ReadInt32().ToString(CultureInfo.InvariantCulture));
                            node.AppendChild(point);
                        }
                        rd.Close();
                    }
                }
            }

            public class Boolean : ByamlNode
            {
                public bool Value { get; set; }

                public override ByamlNodeType Type
                {
                    get { return ByamlNodeType.Boolean; }
                }
                public override bool CanBeAttribute
                {
                    get { return true; }
                }

                public Boolean(EndianBinaryReader reader)
                {
                    Address = reader.BaseStream.Position;

                    Value = reader.ReadInt32() != 0;

                    Length = reader.BaseStream.Position - Length;
                }

                public Boolean(bool value)
                {
                    Value = value;
                }

                public override void ToXml(XmlDocument yaml, XmlNode node, List<string> nodes, List<string> values, List<byte[]> data)
                {
                    node.InnerText = Value.ToString().ToLowerInvariant();
                }
            }

            public class Int : ByamlNode
            {
                public int Value { get; set; }

                public override ByamlNodeType Type
                {
                    get { return ByamlNodeType.Int; }
                }
                public override bool CanBeAttribute
                {
                    get { return true; }
                }

                public Int(EndianBinaryReader reader)
                {
                    Address = reader.BaseStream.Position;

                    Value = reader.ReadInt32();

                    Length = reader.BaseStream.Position - Length;
                }

                public Int(int value)
                {
                    Value = value;
                }

                public override void ToXml(XmlDocument yaml, XmlNode node, List<string> nodes, List<string> values, List<byte[]> data)
                {
                    node.InnerText = Value.ToString(CultureInfo.InvariantCulture);
                }
            }

            public class Single : ByamlNode
            {
                public float Value { get; set; }

                public override ByamlNodeType Type
                {
                    get { return ByamlNodeType.Single; }
                }
                public override bool CanBeAttribute
                {
                    get { return true; }
                }

                public Single(EndianBinaryReader reader)
                {
                    Address = reader.BaseStream.Position;

                    Value = reader.ReadSingle();

                    Length = reader.BaseStream.Position - Length;
                }

                public Single(float value)
                {
                    Value = value;
                }

                public override void ToXml(XmlDocument yaml, XmlNode node, List<string> nodes, List<string> values, List<byte[]> data)
                {
                    node.InnerText = Value.ToString(CultureInfo.InvariantCulture) + "f";
                }
            }

            public class SignedInt : ByamlNode
            {
                public Int32 Value { get; set; }

                public override ByamlNodeType Type
                {
                    get { return ByamlNodeType.SignedInt; }
                }
                public override bool CanBeAttribute
                {
                    get { return true; }
                }

                public SignedInt(EndianBinaryReader reader)
                {
                    Address = reader.BaseStream.Position;

                    Value = reader.ReadInt32();

                    Length = reader.BaseStream.Position - Length;
                }

                public SignedInt(Int32 value)
                {
                    Value = value;
                }

                public override void ToXml(XmlDocument yaml, XmlNode node, List<string> nodes, List<string> values, List<byte[]> data)
                {
                    node.InnerText = Value.ToString(CultureInfo.InvariantCulture) + "f";
                }
            }

            public class UnamedNode : ByamlNode
            {
                public Collection<ByamlNode> Nodes { get; private set; }

                public override ByamlNodeType Type
                {
                    get { return ByamlNodeType.UnamedNode; }
                }

                public UnamedNode(EndianBinaryReader reader)
                {
                    Address = reader.BaseStream.Position;

                    Nodes = new Collection<ByamlNode>();

                    int count = reader.ReadInt32() & 0xffffff;
                    byte[] types = reader.ReadBytes(count);

                    while (reader.BaseStream.Position % 4 != 0)
                        reader.ReadByte();

                    long start = reader.BaseStream.Position;

                    for (int i = 0; i < count; i++)
                    {
                        ByamlNodeType type = (ByamlNodeType)types[i];

                        switch (type)
                        {
                            case ByamlNodeType.String:
                                Nodes.Add(new String(reader));
                                break;
                            case ByamlNodeType.Data:
                                Nodes.Add(new Data(reader));
                                break;
                            case ByamlNodeType.Boolean:
                                Nodes.Add(new Boolean(reader));
                                break;
                            case ByamlNodeType.Int:
                                Nodes.Add(new Int(reader));
                                break;
                            case ByamlNodeType.Single:
                                Nodes.Add(new Single(reader));
                                break;
                            case ByamlNodeType.SignedInt:
                                Nodes.Add(new SignedInt(reader));
                                break;
                            case ByamlNodeType.UnamedNode:
                                reader.BaseStream.Position = reader.ReadInt32();
                                Nodes.Add(new UnamedNode(reader));
                                break;
                            case ByamlNodeType.NamedNode:
                                reader.BaseStream.Position = reader.ReadInt32();
                                Nodes.Add(new NamedNode(reader));
                                break;
                            case ByamlNodeType.Null:
                                Nodes.Add(new Null(reader));
                                break;
                            default:
                                throw new InvalidDataException();
                        }

                        reader.BaseStream.Position = start + (i + 1) * 4;
                    }

                    Length = reader.BaseStream.Position - Length;
                }

                public UnamedNode()
                {
                    Nodes = new Collection<ByamlNode>();
                }

                public override void ToXml(XmlDocument yaml, XmlNode node, List<string> nodes, List<string> values, List<byte[]> data)
                {
                    int i = 0;
                    node.Attributes.Append(yaml.CreateAttribute("type"));
                    node.Attributes["type"].Value = "array";
                    foreach (var item in Nodes)
                    {
                        XmlElement element = yaml.CreateElement("value");
                        item.ToXml(yaml, element, nodes, values, data);
                        node.AppendChild(element);
                        i++;
                    }
                }
            }

            public class NamedNode : ByamlNode
            {
                public Collection<KeyValuePair<int, ByamlNode>> Nodes { get; private set; }

                public override ByamlNodeType Type
                {
                    get { return ByamlNodeType.NamedNode; }
                }

                public NamedNode(EndianBinaryReader reader)
                {
                    Address = reader.BaseStream.Position;

                    Nodes = new Collection<KeyValuePair<int, ByamlNode>>();

                    int count = reader.ReadInt32() & 0xffffff;

                    for (int i = 0; i < count; i++)
                    {
                        uint temp = reader.ReadUInt32();
                        int name = (int)(temp >> 8);
                        ByamlNodeType type = (ByamlNodeType)(byte)temp;

                        switch (type)
                        {
                            case ByamlNodeType.String:
                                Nodes.Add(new KeyValuePair<int, ByamlNode>(name, new String(reader)));
                                break;
                            case ByamlNodeType.Data:
                                Nodes.Add(new KeyValuePair<int, ByamlNode>(name, new Data(reader)));
                                break;
                            case ByamlNodeType.Boolean:
                                Nodes.Add(new KeyValuePair<int, ByamlNode>(name, new Boolean(reader)));
                                break;
                            case ByamlNodeType.Int:
                                Nodes.Add(new KeyValuePair<int, ByamlNode>(name, new Int(reader)));
                                break;
                            case ByamlNodeType.Single:
                                Nodes.Add(new KeyValuePair<int, ByamlNode>(name, new Single(reader)));
                                break;
                            case ByamlNodeType.SignedInt:
                                Nodes.Add(new KeyValuePair<int, ByamlNode>(name, new SignedInt(reader)));
                                break;
                            case ByamlNodeType.UnamedNode:
                                reader.BaseStream.Position = reader.ReadInt32();
                                Nodes.Add(new KeyValuePair<int, ByamlNode>(name, new UnamedNode(reader)));
                                break;
                            case ByamlNodeType.NamedNode:
                                reader.BaseStream.Position = reader.ReadInt32();
                                Nodes.Add(new KeyValuePair<int, ByamlNode>(name, new NamedNode(reader)));
                                break;
                            case ByamlNodeType.Null:
                                Nodes.Add(new KeyValuePair<int, ByamlNode>(name, new Null(reader)));
                                break;
                            default:
                                throw new InvalidDataException();
                        }

                        reader.BaseStream.Position = Address + (i + 1) * 8 + 4;
                    }

                    Length = reader.BaseStream.Position - Length;
                }

                public NamedNode()
                {
                    Nodes = new Collection<KeyValuePair<int, ByamlNode>>();
                }

                public override void ToXml(XmlDocument yaml, XmlNode node, List<string> nodes, List<string> values, List<byte[]> data)
                {
                    foreach (var item in Nodes)
                    {
                        if (item.Value.CanBeAttribute &&
                            !string.Equals(nodes[item.Key], "type", StringComparison.OrdinalIgnoreCase))
                        {
                            XmlAttribute element = yaml.CreateAttribute(nodes[item.Key]);
                            item.Value.ToXml(yaml, element, nodes, values, data);
                            node.Attributes.Append(element);
                        }
                        else
                        {
                            XmlElement element = yaml.CreateElement(nodes[item.Key]);
                            item.Value.ToXml(yaml, element, nodes, values, data);
                            node.AppendChild(element);
                        }
                    }
                }
            }

            public class StringList : ByamlNode
            {
                public Collection<string> Strings { get; private set; }

                public override ByamlNodeType Type
                {
                    get { return ByamlNodeType.StringList; }
                }

                public StringList(EndianBinaryReader reader)
                {
                    Address = reader.BaseStream.Position;

                    Strings = new Collection<string>();

                    int count = reader.ReadInt32() & 0xffffff;
                    int[] offsets = reader.ReadInt32s(count);

                    foreach (var item in offsets)
                    {
                        reader.BaseStream.Seek(Address + item, SeekOrigin.Begin);
                        Strings.Add(reader.ReadStringNT(Encoding.ASCII));
                    }

                    Length = reader.BaseStream.Position - Length;
                }

                public StringList()
                {
                    Strings = new Collection<string>();
                }
            }

            public class BinaryDataList : ByamlNode
            {
                public Collection<byte[]> DataList { get; private set; }

                public override ByamlNodeType Type
                {
                    get { return ByamlNodeType.BinaryDataList; }
                }

                public BinaryDataList(EndianBinaryReader reader)
                {
                    Address = reader.BaseStream.Position;

                    DataList = new Collection<byte[]>();

                    int count = reader.ReadInt32() & 0xffffff;
                    int[] offsets = reader.ReadInt32s(count + 1);

                    for (int i = 0; i < count; i++)
                    {
                        reader.BaseStream.Seek(Address + offsets[i], SeekOrigin.Begin);
                        DataList.Add(reader.ReadBytes(offsets[i + 1] - offsets[i]));
                    }

                    Length = reader.BaseStream.Position - Length;
                }

                public BinaryDataList()
                {
                    DataList = new Collection<byte[]>();
                }
            }

            public class Null : ByamlNode
            {
                public override ByamlNodeType Type
                {
                    get { return ByamlNodeType.Null; }
                }

                public Null(EndianBinaryReader reader)
                {
                    Address = reader.BaseStream.Position;

                    Length = reader.BaseStream.Position - Length;
                }

                public Null()
                {
                }

                public override void ToXml(XmlDocument yaml, XmlNode node, List<string> nodes, List<string> values, List<byte[]> data)
                {
                    XmlAttribute attr = yaml.CreateAttribute("type");
                    attr.Value = "null";
                    node.Attributes.Append(attr);
                }
            }

            public static ByamlNode FromXml(XmlDocument doc, XmlNode xmlNode, List<string> nodes, List<string> values, List<string> data)
            {
                XmlNode child = xmlNode.FirstChild;
                while (child != null && child.NodeType == XmlNodeType.Comment)
                    child = child.NextSibling;

                if (child == null || child.NodeType == XmlNodeType.Element)
                {
                    if (xmlNode.Attributes["type"] != null && xmlNode.Attributes["type"].Value == "array")
                    {
                        UnamedNode node = new UnamedNode();
                        foreach (XmlNode item in xmlNode.ChildNodes)
                            if (item.NodeType == XmlNodeType.Element)
                                node.Nodes.Add(FromXml(doc, item, nodes, values, data));
                        return node;
                    }
                    else if (xmlNode.Attributes["type"] != null && xmlNode.Attributes["type"].Value == "path")
                    {
                        string value;
                        using (MemoryStream ms = new MemoryStream())
                        {
                            using (EndianBinaryWriter wr = new EndianBinaryWriter(ms))
                            {
                                foreach (XmlNode item in xmlNode.ChildNodes)
                                {
                                    if (item.NodeType == XmlNodeType.Element && string.Equals(item.Name, "point", StringComparison.OrdinalIgnoreCase))
                                    {
                                        wr.Write(float.Parse(item.Attributes["x"].Value.Remove(item.Attributes["x"].Value.Length - 1), CultureInfo.InvariantCulture));
                                        wr.Write(float.Parse(item.Attributes["y"].Value.Remove(item.Attributes["y"].Value.Length - 1), CultureInfo.InvariantCulture));
                                        wr.Write(float.Parse(item.Attributes["z"].Value.Remove(item.Attributes["z"].Value.Length - 1), CultureInfo.InvariantCulture));
                                        wr.Write(float.Parse(item.Attributes["nx"].Value.Remove(item.Attributes["nx"].Value.Length - 1), CultureInfo.InvariantCulture));
                                        wr.Write(float.Parse(item.Attributes["ny"].Value.Remove(item.Attributes["ny"].Value.Length - 1), CultureInfo.InvariantCulture));
                                        wr.Write(float.Parse(item.Attributes["nz"].Value.Remove(item.Attributes["nz"].Value.Length - 1), CultureInfo.InvariantCulture));
                                        wr.Write(int.Parse(item.Attributes["val"].Value, CultureInfo.InvariantCulture));
                                    }
                                }
                            }
                            value = System.Convert.ToBase64String(ms.ToArray());
                        }
                        if (!data.Contains(value))
                            data.Add(value);
                        return new Data(data.IndexOf(value));
                    }
                    else if (xmlNode.Attributes["type"] != null && xmlNode.Attributes["type"].Value == "null")
                    {
                        return new Null();
                    }
                    else
                    {
                        NamedNode node = new NamedNode();
                        foreach (XmlNode item in xmlNode.ChildNodes)
                        {
                            if (item.NodeType == XmlNodeType.Element)
                            {
                                if (!nodes.Contains(item.Name))
                                    nodes.Add(item.Name);
                                node.Nodes.Add(new KeyValuePair<int, ByamlNode>(nodes.IndexOf(item.Name), FromXml(doc, item, nodes, values, data)));
                            }
                        }
                        foreach (XmlAttribute item in xmlNode.Attributes)
                        {
                            if (item.Prefix != "xmlns" && item.NamespaceURI != "yamlconv")
                            {
                                if (!nodes.Contains(item.Name))
                                    nodes.Add(item.Name);
                                node.Nodes.Add(new KeyValuePair<int, ByamlNode>(nodes.IndexOf(item.Name), FromXml(doc, item, nodes, values, data)));
                            }
                        }
                        return node;
                    }
                }
                else
                {
                    if (xmlNode.Attributes != null && xmlNode.Attributes["type"] != null)
                    {
                        if (xmlNode.Attributes["type"].Value == "string")
                        {
                            if (!values.Contains(xmlNode.InnerText))
                                values.Add(xmlNode.InnerText);
                            return new String(values.IndexOf(xmlNode.InnerText));
                        }
                    }

                    int value_int;
                    bool value_bool;

                    if (xmlNode.InnerText.EndsWith("f", StringComparison.OrdinalIgnoreCase))
                        return new Single(float.Parse(xmlNode.InnerText.Remove(xmlNode.InnerText.Length - 1), CultureInfo.InvariantCulture));
                    else if (int.TryParse(xmlNode.InnerText, out value_int))
                        return new Int(value_int);
                    else if (bool.TryParse(xmlNode.InnerText, out value_bool))
                        return new Boolean(value_bool);
                    else
                        throw new InvalidDataException();
                }
            }
        }

        public static void Convert(string path)
        {
            Console.WriteLine("Converting {0}", path);
            using (var reader = new EndianBinaryReader(new FileStream(path, FileMode.Open)))
            {
                string outpath;
                string magic;
                bool toyaml;

                magic = reader.ReadString(Encoding.ASCII, 2);
                if (magic == "BY")
                {
                    toyaml = true;
                    outpath = Path.ChangeExtension(path, "xml");
                    if (outpath == path)
                        outpath = path + ".xml";
                    reader.Endianness = Endianness.BigEndian;
                }
                else if (magic == "YB")
                {
                    toyaml = true;
                    outpath = Path.ChangeExtension(path, "xml");
                    if (outpath == path)
                        outpath = path + ".xml";
                    reader.Endianness = Endianness.LittleEndian;
                }
                else
                {
                    toyaml = false;
                    outpath = Path.ChangeExtension(path, "byaml");
                    if (outpath == path)
                        outpath = path + ".byaml";
                }

                reader.BaseStream.Seek(0, SeekOrigin.Begin);

                if (toyaml)
                {
                    ConvertFromByaml(reader, outpath);
                }
                else
                {
                    ConvertToByaml(reader, outpath);
                }

                reader.Close();
            }
        }

        private static void ConvertFromByaml(EndianBinaryReader reader, string outpath)
        {
            if (reader.ReadUInt16() != 0x4259)
                throw new InvalidDataException();
            if (reader.ReadUInt16() != 0x0002) // edited for BOTW
                throw new InvalidDataException();

            uint nodeOffset = reader.ReadUInt32();
            if (nodeOffset > reader.BaseStream.Length)
                throw new InvalidDataException();

            // Number of offset values.
            // Splatoon byamls are missing dataOffset.
            uint offsetCount = nodeOffset == 0x10 ? 3u : 4u;

            uint valuesOffset = reader.ReadUInt32();
            if (valuesOffset > reader.BaseStream.Length)
                throw new InvalidDataException();

            uint dataOffset = offsetCount > 3 ? reader.ReadUInt32() : 0;
            if (dataOffset > reader.BaseStream.Length)
                throw new InvalidDataException();

            uint treeOffset = reader.ReadUInt32();
            if (treeOffset > reader.BaseStream.Length)
                throw new InvalidDataException();


            List<string> nodes = new List<string>();
            List<string> values = new List<string>();
            List<byte[]> data = new List<byte[]>();

            if (nodeOffset != 0)
            {
                reader.BaseStream.Seek(nodeOffset, SeekOrigin.Begin);
                nodes.AddRange(new ByamlNode.StringList(reader).Strings);
            }
            if (valuesOffset != 0)
            {
                reader.BaseStream.Seek(valuesOffset, SeekOrigin.Begin);
                values.AddRange(new ByamlNode.StringList(reader).Strings);
            }
            if (dataOffset != 0)
            {
                reader.BaseStream.Seek(dataOffset, SeekOrigin.Begin);
                data.AddRange(new ByamlNode.BinaryDataList(reader).DataList);
            }

            ByamlNode tree;
            ByamlNodeType rootType;
            reader.BaseStream.Seek(treeOffset, SeekOrigin.Begin);
            rootType = (ByamlNodeType)reader.ReadByte();
            reader.BaseStream.Seek(-1, SeekOrigin.Current);
            if (rootType == ByamlNodeType.UnamedNode)
                tree = new ByamlNode.UnamedNode(reader);
            else
                tree = new ByamlNode.NamedNode(reader);

            XmlDocument yaml = new XmlDocument();
            yaml.AppendChild(yaml.CreateXmlDeclaration("1.0", "UTF-8", null));
            XmlElement root = yaml.CreateElement("yaml");
            XmlAttribute xmlnsAttribute = yaml.CreateAttribute("xmlns:yamlconv");
            xmlnsAttribute.InnerText = "yamlconv";
            root.Attributes.Append(xmlnsAttribute);
            XmlAttribute endianAttribute = yaml.CreateAttribute("endianness", "yamlconv");
            endianAttribute.InnerText = reader.Endianness == Endianness.BigEndian ? "big" : "little";
            root.Attributes.Append(endianAttribute);
            XmlAttribute offsetCountAttribute = yaml.CreateAttribute("offsetCount", "yamlconv");
            offsetCountAttribute.InnerText = offsetCount.ToString();
            root.Attributes.Append(offsetCountAttribute);
            yaml.AppendChild(root);

            tree.ToXml(yaml, root, nodes, values, data);

            using (StreamWriter writer = new StreamWriter(new FileStream(outpath, FileMode.Create), Encoding.UTF8))
            {
                yaml.Save(writer);
            }
        }

        private static void ConvertToByaml(EndianBinaryReader reader, string outpath)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(reader.BaseStream);

            if (doc.LastChild.Name != "yaml")
                throw new InvalidDataException();

            uint offsetCount = 4;
            if (doc.LastChild.Attributes["yamlconv:offsetCount"] != null)
                offsetCount = uint.Parse(doc.LastChild.Attributes["yamlconv:offsetCount"].Value);

            Endianness endianness = Endianness.BigEndian;
            if (doc.LastChild.Attributes["yamlconv:endianness"] != null)
                endianness = doc.LastChild.Attributes["yamlconv:endianness"].Value == "little" ? Endianness.LittleEndian : Endianness.BigEndian;

            List<string> nodes = new List<string>();
            List<string> values = new List<string>();
            List<string> data = new List<string>();

            ByamlNode tree = ByamlNode.FromXml(doc, doc.LastChild, nodes, values, data);

            List<ByamlNode> flat = new List<ByamlNode>();
            Stack<ByamlNode> process = new Stack<ByamlNode>();

            List<string> sorted_nodes = new List<string>();
            sorted_nodes.AddRange(nodes);
            sorted_nodes.Sort(StringComparer.Ordinal);
            List<string> sorted_values = new List<string>();
            sorted_values.AddRange(values);
            sorted_values.Sort(StringComparer.Ordinal);

            process.Push(tree);
            while (process.Count > 0)
            {
                ByamlNode current = process.Pop();
                flat.Add(current);

                if (current.GetType() == typeof(ByamlNode.NamedNode))
                {
                    ByamlNode.NamedNode cur = current as ByamlNode.NamedNode;
                    SortedDictionary<int, ByamlNode> dict = new SortedDictionary<int, ByamlNode>();
                    Stack<ByamlNode> reverse = new Stack<ByamlNode>();
                    foreach (var item in cur.Nodes)
                    {
                        dict.Add(sorted_nodes.IndexOf(nodes[item.Key]), item.Value);
                        reverse.Push(item.Value);
                    }
                    while (reverse.Count > 0)
                        process.Push(reverse.Pop());
                    cur.Nodes.Clear();
                    foreach (var item in dict)
                        cur.Nodes.Add(item);
                }
                else if (current.GetType() == typeof(ByamlNode.UnamedNode))
                {
                    Stack<ByamlNode> reverse = new Stack<ByamlNode>();
                    foreach (var item in (current as ByamlNode.UnamedNode).Nodes)
                    {
                        reverse.Push(item);
                    }
                    while (reverse.Count > 0)
                        process.Push(reverse.Pop());
                }
                else if (current.GetType() == typeof(ByamlNode.String))
                {
                    ByamlNode.String cur = current as ByamlNode.String;

                    cur.Value = sorted_values.IndexOf(values[cur.Value]);
                }
            }

            using (EndianBinaryWriter writer = new EndianBinaryWriter(new FileStream(outpath, FileMode.Create)))
            {
                writer.Endianness = endianness;
                uint[] off = new uint[offsetCount];

                for (int i = 0; i < 2; i++)
                {
                    writer.BaseStream.Position = 0;

                    writer.Write((UInt16)0x4259);
                    writer.Write((UInt16)0x0001);
                    writer.Write(off, 0, (int)offsetCount);

                    if (sorted_nodes.Count > 0)
                    {
                        off[0] = (uint)writer.BaseStream.Position;

                        int len = 8 + 4 * sorted_nodes.Count;
                        writer.Write(sorted_nodes.Count | ((int)ByamlNodeType.StringList << 24));
                        foreach (var item in sorted_nodes)
                        {
                            writer.Write(len);
                            len += item.Length + 1;
                        }
                        writer.Write(len);
                        foreach (var item in sorted_nodes)
                            writer.Write(item, Encoding.ASCII, true);
                        writer.WritePadding(4, 0);
                    }
                    else
                        off[0] = 0;

                    if (sorted_values.Count > 0)
                    {
                        off[1] = (uint)writer.BaseStream.Position;

                        int len = 8 + 4 * sorted_values.Count;
                        writer.Write(sorted_values.Count | ((int)ByamlNodeType.StringList << 24));
                        foreach (var item in sorted_values)
                        {
                            writer.Write(len);
                            len += item.Length + 1;
                        }
                        writer.Write(len);
                        foreach (var item in sorted_values)
                            writer.Write(item, Encoding.ASCII, true);
                        writer.WritePadding(4, 0);
                    }
                    else
                        off[1] = 0;

                    if (offsetCount > 3 && data.Count > 0)
                    {
                        off[2] = (uint)writer.BaseStream.Position;

                        int len = 8 + 4 * data.Count;
                        writer.Write(data.Count | ((int)ByamlNodeType.BinaryDataList << 24));
                        foreach (var item in data)
                        {
                            byte[] val = System.Convert.FromBase64String(item);
                            writer.Write(len);
                            len += val.Length;
                        }
                        writer.Write(len);
                        foreach (var item in data)
                        {
                            byte[] val = System.Convert.FromBase64String(item);
                            writer.Write(val, 0, val.Length);
                        }
                        writer.WritePadding(4, 0);
                    }
                    else
                        off[2] = 0;

                    off[off.Length - 1] = (uint)writer.BaseStream.Position;

                    foreach (var current in flat)
                    {
                        current.Address = writer.BaseStream.Position;
                        if (current.GetType() == typeof(ByamlNode.NamedNode))
                        {
                            ByamlNode.NamedNode cur = current as ByamlNode.NamedNode;
                            writer.Write(cur.Nodes.Count | ((int)cur.Type << 24));
                            foreach (var item in cur.Nodes)
                            {
                                writer.Write(((int)item.Value.Type) | (item.Key << 8));
                                switch (item.Value.Type)
                                {
                                    case ByamlNodeType.String:
                                        writer.Write((item.Value as ByamlNode.String).Value);
                                        break;
                                    case ByamlNodeType.Data:
                                        writer.Write((item.Value as ByamlNode.Data).Value);
                                        break;
                                    case ByamlNodeType.Boolean:
                                        writer.Write((item.Value as ByamlNode.Boolean).Value ? 1 : 0);
                                        break;
                                    case ByamlNodeType.Int:
                                        writer.Write((item.Value as ByamlNode.Int).Value);
                                        break;
                                    case ByamlNodeType.Single:
                                        writer.Write((item.Value as ByamlNode.Single).Value);
                                        break;
                                    case ByamlNodeType.SignedInt:
                                        writer.Write((item.Value as ByamlNode.SignedInt).Value);
                                        break;
                                    case ByamlNodeType.UnamedNode:
                                    case ByamlNodeType.NamedNode:
                                        writer.Write((int)item.Value.Address);
                                        break;
                                    case ByamlNodeType.Null:
                                        break;
                                    default:
                                        throw new NotImplementedException();
                                }
                            }
                        }
                        else if (current.GetType() == typeof(ByamlNode.UnamedNode))
                        {
                            ByamlNode.UnamedNode cur = current as ByamlNode.UnamedNode;
                            writer.Write(cur.Nodes.Count | ((int)cur.Type << 24));
                            foreach (var item in cur.Nodes)
                                writer.Write((byte)item.Type);
                            writer.WritePadding(4, 0);
                            foreach (var item in cur.Nodes)
                            {
                                switch (item.Type)
                                {
                                    case ByamlNodeType.String:
                                        writer.Write((item as ByamlNode.String).Value);
                                        break;
                                    case ByamlNodeType.Data:
                                        writer.Write((item as ByamlNode.Data).Value);
                                        break;
                                    case ByamlNodeType.Boolean:
                                        writer.Write((item as ByamlNode.Boolean).Value ? 1 : 0);
                                        break;
                                    case ByamlNodeType.Int:
                                        writer.Write((item as ByamlNode.Int).Value);
                                        break;
                                    case ByamlNodeType.Single:
                                        writer.Write((item as ByamlNode.Single).Value);
                                        break;
                                    case ByamlNodeType.UnamedNode:
                                    case ByamlNodeType.NamedNode:
                                        writer.Write((int)item.Address);
                                        break;
                                    case ByamlNodeType.Null:
                                        break;
                                    default:
                                        throw new NotImplementedException();
                                }
                            }
                        }

                    }
                }

                writer.Close();
            }
        }
    }
}
