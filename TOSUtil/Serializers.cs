using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Serialization;

namespace TOSP.Util
{
    public class Serializers
    {
        public static object DeserializeXML(string fileName, XmlSerializer xmlSerializer)
        {
            object result = null;
            FileStream fileStream = null;
            try
            {
                fileStream = new FileStream(fileName, FileMode.Open);
                result = xmlSerializer.Deserialize(fileStream);

            }
            catch (FileNotFoundException)
            {
            }
            finally
            {
                if (fileStream != null)
                {
                    fileStream.Close();
                }
            }

            return result;
        }


        public static bool SerializeXML(string fileName, XmlSerializer xmlSerializer, object obj)
        {
            FileStream fileStream = null;
            try
            {
                fileStream = new FileStream(fileName, FileMode.Create);
                xmlSerializer.Serialize(fileStream, obj);
                return false;
            }
            catch (Exception e)
            {
                throw e;
                //return true;
            }
            finally
            {
                if (fileStream != null)
                {
                    fileStream.Close();
                }
            }
        }

    }
}
