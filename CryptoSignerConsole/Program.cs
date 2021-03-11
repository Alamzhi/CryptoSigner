using System;
using System.Text;
using System.Xml;
using CryptoSigner;

namespace CryptoSignerConsole
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            // Создаем новый объект xml документа.
            XmlDocument doc = new XmlDocument();

            // Пробельные символы участвуют в вычислении подписи и должны быть сохранены для совместимости с другими реализациями.
            doc.PreserveWhitespace = true;

            // Загружаем в объект созданный XML документ.
            doc.Load(new XmlTextReader("ClinicalDocument.xml"));
            
            //отпечаток сертификата. Должен быть другой у вас
            var thumbprint = @"cc3b30d3b76a4c0296be9f5d174c2f42ef5ba121";

            // Получаем файл подписанный подписью с указанным CN
            var signature = CryptoSign.GetSignature(doc, thumbprint);

            // Сохранить подписываемый документ в файле.
            using (XmlTextWriter xmltw = new XmlTextWriter("Signature.xml",
	            new UTF8Encoding(false)))
            {
	            xmltw.WriteStartDocument();
	            signature.GetXml().WriteTo(xmltw);
            }
        }
    }
}
