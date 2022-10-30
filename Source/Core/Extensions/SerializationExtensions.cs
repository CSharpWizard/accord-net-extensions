﻿using System.IO;
using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Accord.Extensions
{
    /// <summary>
    /// <para>Defined functions can be used as object extensions.</para>
    /// Provides serialization extensions. 
    /// </summary>
    public static class SerializationExtensions
    {
        /// <summary>
        /// Serializes specified object to <see cref="XElement"/>.
        /// </summary>
        /// <typeparam name="T">An object generic type.</typeparam>
        /// <param name="obj">An input object.</param>
        /// <returns>Serialized object.</returns>
        public static XElement ToXElement<T>(this T obj)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (TextWriter streamWriter = new StreamWriter(memoryStream))
                {
                    var xmlSerializer = new XmlSerializer(typeof(T));
                    xmlSerializer.Serialize(streamWriter, obj);
                    return XElement.Parse(Encoding.ASCII.GetString(memoryStream.ToArray()));
                }
            }
        }

        /// <summary>
        /// Deserializes the specified <see cref="XElement"/> to an object.
        /// </summary>
        /// <typeparam name="T">Destination generic object type.</typeparam>
        /// <param name="xElement">An element to deserialize.</param>
        /// <returns>Deserialized object.</returns>
        public static T FromXElement<T>(this XElement xElement)
        {
            using (var memoryStream = new MemoryStream(Encoding.ASCII.GetBytes(xElement.ToString())))
            { 
                var xmlSerializer = new XmlSerializer(typeof(T));
                return (T)xmlSerializer.Deserialize(memoryStream);
            }
        }
    }
}
