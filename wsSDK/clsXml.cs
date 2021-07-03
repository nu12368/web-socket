using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace x150
{
    public class clsXml
    {
        public bool CreateXML(string filename)
        {
            System.IO.File.WriteAllText(filename, "<?xml version='1.0' encoding='utf-8'?>" + Environment.NewLine +
                "<root>" + Environment.NewLine +
                "</root>");
            return true;
        }

        public bool SetWriteXML(string strNode, string strElement, string strValue, string filename)
        {
            if (strNode == "" || strNode == null)
            {
                return false;
            }

            if (System.IO.File.Exists(filename) == false)
            {
                string xmlText = "<?xml version='1.0' encoding='utf-8'?>" + Environment.NewLine +
                        "<" + strNode + ">" + Environment.NewLine +
                        "</" + strNode + ">";
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(xmlText);
                xmlDoc.Save(filename);
            }

            try
            {
                if (strValue.Trim().Length == 0)
                {
                    strValue = "-";
                }
                XmlTextWriter objXMLTW = new XmlTextWriter(filename, Encoding.UTF8);
                objXMLTW.IndentChar = '\t';   //vbTab;
                objXMLTW.Indentation = 1;
                objXMLTW.Formatting = Formatting.Indented;
                objXMLTW.WriteStartDocument();
                objXMLTW.WriteStartElement(strNode);
                objXMLTW.WriteElementString(strElement, strValue);
                objXMLTW.WriteEndElement();
                objXMLTW.WriteEndDocument();
                objXMLTW.Flush();
                objXMLTW.Close();
                return true;
            }
            catch
            {
                return false;
            }

        }

        public string GetReadXML(string strNode, string strElement, string filename)
        {
            try
            {
                string getvalue = "";
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(filename);
                getvalue = xmlDoc.SelectSingleNode(strNode + "/" + strElement).InnerText;
                return getvalue;
            }
            catch
            {
                return "";
            }
        }

        public bool ModifyElement(string strNode, string strElement, string strValue, string filename)
        {
            try
            {
                if (strNode == "" || strNode == null)
                {
                    return false;
                }

                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(filename);
                XmlNode node = xmlDoc.SelectSingleNode(strNode + "/" + strElement);
                if (node != null)
                {
                    node.ChildNodes[0].InnerText = strValue;
                }
                else
                {
                    return false;
                }

                xmlDoc.Save(filename);
                return true;
            }
            catch
            {
                return false;
            }

        }

        public bool AppendElement(string strNode, string strElement, string strValue, string filename)
        {
            try
            {
                if (strNode == "" || strNode == null)
                {
                    return false;
                }

                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(filename);
                XmlNode node = xmlDoc.SelectSingleNode(strNode + "/" + strElement);
                if (node == null)
                {
                    System.Xml.XPath.XPathNavigator xmlNavigator = xmlDoc.CreateNavigator();
                    xmlNavigator = xmlNavigator.SelectSingleNode(strNode);
                    xmlNavigator.AppendChildElement("", strElement, "", strValue);
                    xmlDoc.Save(filename);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        public bool RemoveElement(string strNode, string strElement, string filename)
        {
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(filename);
                XmlNode node = xmlDoc.SelectSingleNode(strNode + "/" + strElement);
                if (node != null)
                {
                    node.ParentNode.RemoveChild(node);
                    xmlDoc.Save(filename);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        public bool CheckExistElement(string strNode, string strElement, string filename)
        {
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(filename);
                int numberElement = 0;
                numberElement = xmlDoc.GetElementsByTagName(strElement).Count;
                if (numberElement > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

    }
}
