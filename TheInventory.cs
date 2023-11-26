using System.Text;
using System.Xml;
using System.Xml.Serialization;

public class Inventory
{

    public Inventory()
    {
        xDoc = new XmlDocument();
        try { LoadTheFile().Wait(); } catch (Exception ex) { Console.WriteLine(ex.Message); SaveStorage(new Storage()); }
    }

    public readonly string TheStoragePath = Path.Combine(Environment.CurrentDirectory, "TheXml.xml");

    internal XmlReader theXR { get { return XmlReader.Create(File.Open(TheStoragePath,
                                            FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite),
                                            new XmlReaderSettings { DtdProcessing = DtdProcessing.Parse, IgnoreWhitespace = true }); } }


    internal XmlWriter theXW()
    {
        return XmlWriter.Create(File.Open(TheStoragePath,
                                            FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite),
                                            new XmlWriterSettings { Encoding = Encoding.Latin1, Indent = true, ConformanceLevel = ConformanceLevel.Document, Async= true }); }

    /// <summary>
    /// The XmlDocument
    /// </summary>
    private XmlDocument xDoc;

    internal readonly string theXmlDTD = "<!DOCTYPE Storage [ \n" +
        "<!ELEMENT Storage (StoredItems)> \n" +
        "<!ELEMENT StoredItems (StoredItem*)> \n" +
        "<!ELEMENT StoredItem EMPTY> \n" +
        "<!ATTLIST StoredItem " +
                    "Id ID #REQUIRED \n" +
                    "Name CDATA #REQUIRED \n" +
                    "Quantity CDATA #REQUIRED>]>";

    public bool SaveStorage(Storage theStorage)
    {
        XmlSerializer serializer = new XmlSerializer(typeof(Storage));
        // XmlNode? theStorageTag;
        try
        {
            StringWriter SW = new StringWriter();
            serializer.Serialize(SW, theStorage);
            xDoc = new XmlDocument();
            XmlDocument tempDoc = new XmlDocument();
            tempDoc.LoadXml(SW.ToString());
            using (XmlWriter xw = theXW())
            {
                xw.WriteStartDocument();
                xw.WriteRaw(theXmlDTD);
                tempDoc.DocumentElement?.WriteTo(xw);
                xw.Flush();
            }
            LoadTheFile().Wait();
            return true;
        }
        catch (Exception ex) { Console.WriteLine(ex.Message); return false; }
    }

    public Storage? CurrentStorage()
    {
        if (xDoc.GetElementsByTagName("Storage")[0] is not null)
        {
            XmlSerializer xs = new XmlSerializer(typeof(Storage));
            using StringReader SR = new StringReader(xDoc.GetElementsByTagName("Storage")[0]!.OuterXml);
            return (Storage?)xs.Deserialize(SR);
        }
        else return null;
    }

    public ReadOnlySpan<char> CurrentStorageXmlString()
    { return xDoc.GetElementsByTagName("Storage")[0]?.OuterXml ?? "<Storage><StoredItems></StoredItems></Storage>"; }



    public bool AddItem(StoredItem theStoredItem)
    {
        XmlElement xe = xDoc.CreateElement("StoredItem"); xe.SetAttribute("Id", theStoredItem.Id);
        XmlElement xName = xDoc.CreateElement("Name"); xName.InnerText = theStoredItem.Name; xe.AppendChild(xName);
        XmlElement xQuantity = xDoc.CreateElement("Quantity"); xQuantity.InnerText = theStoredItem.Quantity.ToString(); xe.AppendChild(xQuantity);

        if (xDoc.GetElementsByTagName("Storage")[0]?.FirstChild is not null)
        { xDoc.GetElementsByTagName("Storage")[0]?.FirstChild?.AppendChild(xe);
            using (XmlWriter XW = theXW()) {  xDoc.Save(XW); XW.Flush(); }
            return true; }
        else return false;
    }

    public StoredItem CheckItem(string id)
    {
        XmlElement? x = xDoc.GetElementById(id);
        if (x is not null) { return new StoredItem { Id = x.GetAttribute("Id"),
            Quantity = int.Parse(x.LastChild?.InnerText ?? "666666"),
            Name = x.FirstChild?.InnerText ?? "not parsable" }; }
        else return new StoredItem { Id = "nULLO", Name = "not found", Quantity = 0 };
    }

    public bool RemoveItem(string id)
    {
        XmlElement? x = xDoc.GetElementById(id);
        XmlNode? xN = xDoc.GetElementsByTagName("Storage")[0]; if (xN is null || x is null) { return false; }
        xN.RemoveChild(x);
        using (XmlWriter xw = theXW())
        {
            xDoc.Save(xw); xw.Flush();
        }
        return true;
    }

    public bool AddQuantity(string id, int value)
    {
        XmlElement? x = xDoc.GetElementById(id);
        if (x is null || x?.LastChild is null) { return false; }
        else
        {
            int currentQuantity = int.Parse(x.LastChild.InnerText);
            currentQuantity += value;
            x.LastChild.InnerText = currentQuantity.ToString();
            using (XmlWriter xw = theXW())
            {
                xDoc.Save(xw); xw.Flush();   
            }
            return true;
        }
    }

    public bool RemoveQuantity(string id, int value)
    {
        XmlElement? x = xDoc.GetElementById(id);
        if (x is null) { return false; }
        else
        {
            int currentQuantity;
            bool a = int.TryParse(x.LastChild?.InnerText, out currentQuantity);
            if (a is false || x.LastChild is null) { return false; }
            currentQuantity -= value;
            x.LastChild.InnerText = currentQuantity.ToString();
            using (XmlWriter xw = theXW())
            {
                xDoc.Save(xw); xw.Flush();
            }
            return true;
        }
    }

    public async Task<bool> RemoveQuantityAsync(string id, int value)
    {
        XmlElement? x = xDoc.GetElementById(id);
        if (x is null) { return false; }
        else
        {
            int currentQuantity;
            bool a = int.TryParse(x.LastChild?.InnerText, out currentQuantity);
            if (a is false || x.LastChild is null) { return false; }
            currentQuantity -= value;
            x.LastChild.InnerText = currentQuantity.ToString();
            using (XmlWriter xw = theXW())
            {
                xDoc.Save(xw); await xw.FlushAsync();
            }
            return true;
        } 
    }

    public Lazy<Task<bool>> RemoveOneLazyAsync(string id)
    {
        return new Lazy<Task<bool>>(async () =>
        {
            XmlElement? x = xDoc.GetElementById(id);
            if (x is null) { return false; }
            else
            {
                int currentQuantity;
                bool a = int.TryParse(x.LastChild?.InnerText, out currentQuantity);
                if (a is false || x.LastChild is null) { return false; }
                currentQuantity--;
                x.LastChild.InnerText = currentQuantity.ToString();
                using (XmlWriter xw = theXW())
                {
                    xDoc.Save(xw);
                    await xw.FlushAsync();
                }
                return true;
            }
        });
    }


    /// <summary>
    /// First loads a string from the file's content, then loads that string to the xmldoc
    /// </summary>
    /// <returns></returns>
    private Task LoadTheFile()
    {
        string ok = "";
        using (StreamReader theSR = new StreamReader(File.Open(TheStoragePath,
         FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite)))
        { ok = theSR.ReadToEnd(); }
        using (XmlReader XR = XmlReader.Create(new StringReader(ok), 
           new XmlReaderSettings { DtdProcessing = DtdProcessing.Parse, IgnoreWhitespace=true }))
        {
            try { xDoc.Load(XR); return Task.CompletedTask; }
            catch (Exception ex)
            { Console.WriteLine(ex.Message); return Task.FromException(ex); }
        }
    }

}

public class Storage
{
   [XmlArray] public StoredItem[] StoredItems = null! ?? new StoredItem[0];
}

/// <summary>
/// NGL i have no idea why all the properties are being serialized into attributes. Has to be a struct` thing, this was not the resulted serialization in classes.
/// </summary>
[Serializable]
public struct StoredItem
{
    [XmlAttribute] public string Id;
    public string Name;
    public int Quantity;
}

