using System;
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
            
            var subject = @"sd";

            // Получаем файл подписанный подписью с указанным CN
            var signedXml = CryptoSign.GetSignedMessage(doc, subject);
        }
    }
}
