﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml;
using System.IO;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace LINE2D
{
    /// <summary>
    /// Serializes and deserializes list of template pyramids into/from XML file.
    /// If a template implements <see cref="IXmlSerializable"/> interface additional data provided by a user may be serialized/deserialized as well.
    /// </summary>
    public class XMLTemplateSerializer<TTemplatePyramid, TTemplate>
        where TTemplatePyramid: ITemplatePyramid<TTemplate>, new()
        where TTemplate: ITemplate, new()
    {
        private static XElement SerializeTemplateFeature(Feature feature)
        {
            XElement xElem = new XElement("Feature");
            xElem.SetAttributeValue("X", feature.X);
            xElem.SetAttributeValue("Y", feature.Y);
            xElem.SetAttributeValue("AngleLabel", feature.AngleIndex);

            return xElem;
        }

        private static XElement SerializeTemplate(TTemplate t, int pyrLevel)
        {
            XElement xElem = new XElement("Template");

            xElem.SetAttributeValue("width", t.Size.Width);
            xElem.SetAttributeValue("height", t.Size.Height);
            xElem.SetAttributeValue("pyramidLevel", pyrLevel);
            xElem.SetAttributeValue("numOfFeatures", t.Features.Length);

            foreach (var feature in t.Features)
            {
                xElem.Add(SerializeTemplateFeature(feature));
            }

            if (t is IXmlSerializable)
            {
                XElement xElemAditionalData = new XElement("AditionalData");
                using (var v = xElemAditionalData.CreateWriter())
                {
                    ((IXmlSerializable)t).WriteXml(v);
                    v.Flush();
                }

                if (string.IsNullOrEmpty(xElemAditionalData.Value) == false)
                {
                    xElem.Add(xElemAditionalData);
                }
            }

            return xElem;
        }

        private static XElement SerializeTemplatePyramid(TTemplatePyramid templatePyramid)
        {
            XElement xElem = new XElement("TemplatePyramid");

            for (int i = 0; i < templatePyramid.Templates.Length; i++)
            {
                xElem.Add(SerializeTemplate((TTemplate)templatePyramid.Templates[i], i));
            }

            return xElem;
        }

        public static XElement SerializeTemplatePyramidClass(IEnumerable<TTemplatePyramid> c)
        {
            XElement xElem = new XElement("TemplatePyramidClass");
            xElem.SetAttributeValue("classLabel", c.First().Templates.First().ClassLabel);
            xElem.SetAttributeValue("numOfTemplatePyrs", c.Count());

            foreach (var templatePyr in c)
            {
                xElem.Add(SerializeTemplatePyramid(templatePyr));
            }

            return xElem;
        }

        public static void Save(IEnumerable<TTemplatePyramid> cluster, string fileName)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = "\t";
            settings.NewLineChars = "\r\n";
            settings.NewLineHandling = NewLineHandling.Replace;
            settings.CloseOutput = true;

            XmlWriter xmlWriter = XmlWriter.Create(new StreamWriter(fileName, false), settings);

            SerializeTemplatePyramidClass(cluster).Save(xmlWriter);

            xmlWriter.Flush();
            xmlWriter.Close();
        }


        public static IEnumerable<TTemplatePyramid> Load(string fileName)
        { 
            XDocument xDoc = XDocument.Load(new StreamReader(fileName));

            IEnumerable<XElement> templatePyrClusters = from pyrCluster in xDoc.Descendants()
                                                        where pyrCluster.Name == "TemplatePyramidClass"
                                                        select pyrCluster;

            return DeserializeTemplatePyramidClass(templatePyrClusters.First());
        }

        private static IEnumerable<TTemplatePyramid> DeserializeTemplatePyramidClass(XElement templatePyrClassNode)
        {
            string templateClass = templatePyrClassNode.Attribute("classLabel").Value;

            IEnumerable<XElement> templatePyrsNodes = from templatePyr in templatePyrClassNode.Descendants()
                                                      where templatePyr.Name == "TemplatePyramid"
                                                      select templatePyr;

            object syncObj = new object();
            var templPyrs = new List<TTemplatePyramid>();

            Parallel.ForEach(templatePyrsNodes, delegate(XElement templatePyrNode) 
            {
                lock (syncObj)
                {
                    templPyrs.Add(DeserializeTemplatePyramid(templatePyrNode, templateClass));
                }
            });

            return templPyrs;
        }

        private static TTemplatePyramid DeserializeTemplatePyramid(XElement templatePyrNode, string templateClass)
        {
            IEnumerable<XElement> templateNodes = from templateNode in templatePyrNode.Descendants()
                                                  where templateNode.Name == "Template"
                                                  select templateNode;

            var templates = new List<TTemplate>();
            foreach (XElement templateNode in templateNodes)
            {           
                templates.Add(DeserializeTemplate(templateNode, templateClass));
            }

            var pyr = new TTemplatePyramid(); pyr.Initialize(templates.ToArray());
            return pyr;
        }

        private static TTemplate DeserializeTemplate(XElement templateNode, string templateClass)
        {
            int width = (int)templateNode.Attribute("width");
            int height = (int)templateNode.Attribute("height");

            IEnumerable<XElement> featureNodes = from featureNode in templateNode.Descendants()
                                                  where featureNode.Name == "Feature"
                                                  select featureNode;
            
            List<Feature> features = new List<Feature>();
            foreach (XElement featureNode in featureNodes)
            {
                features.Add(DeserializeFeature(featureNode));
            }

            TTemplate t = new TTemplate();
            t.Initialize(features.ToArray(), new System.Drawing.Size(width, height), templateClass);

            if (t is IXmlSerializable)
            {
                XElement aditionalDataNode = templateNode.Descendants("AditionalData").FirstOrDefault();
                if (aditionalDataNode != null)
                    ((IXmlSerializable)t).ReadXml(aditionalDataNode.CreateReader());
            }
            return t;
        }

        private static Feature DeserializeFeature(XElement featureNode)
        {
            int x = (int)featureNode.Attribute("X");
            int y = (int)featureNode.Attribute("Y");
            byte angleLabel = (byte)(int)featureNode.Attribute("AngleLabel");

            return new Feature(x, y, Feature.GetAngleBinaryForm(angleLabel));
        }

    }
}
