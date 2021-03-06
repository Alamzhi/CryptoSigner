using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using CryptoPro.Sharpei.Xml;

namespace CryptoSigner
{
	public static class CryptoSign
	{
        /// <summary>
        /// Возвращается файл подписанный подписью с указанным CN
        /// </summary>
        /// <param name="message"></param>
        /// <param name="subject"></param>
        /// <returns></returns>
		public static XmlDocument GetSignedMessage(XmlDocument message, string subject)
		{
			X509Certificate2 certificate = FindCertificate(subject);
			if (certificate == null)
				throw new Exception("Не найден сертификат");
			// Получаем секретный ключ.
			AsymmetricAlgorithm Key = certificate.PrivateKey;
			// Подписываем документ.
			SignXmlDocument(ref message, Key, certificate);

			return message;
		}

		// Поиск сертификата в хранилище MY
		private static X509Certificate2 FindCertificate(string subject)
		{
			X509Store store = new X509Store("My", StoreLocation.CurrentUser);
			store.Open(OpenFlags.OpenExistingOnly | OpenFlags.ReadOnly);

			// Ищем сертификат и ключ для подписи.
			X509Certificate2Collection found =
				store.Certificates.Find(X509FindType.FindBySubjectName,
					subject, false);

			// Проверяем, что нашли ровно один сертификат.
			if (found.Count == 0)
			{
				throw new Exception($"Сертификат {subject} не найден.");
				return null;
			}

			if (found.Count > 1)
			{
				throw new Exception($"Найдено больше одного сертификата {subject} для подписи.");
			}

			return found[0];
		}
        // Подписывает XML документ и сохраняем подпись в документе.
        private static void SignXmlDocument(ref XmlDocument doc,
            AsymmetricAlgorithm Key, X509Certificate Certificate)
        {
            // Создаем объект SignedXml по XML документу.
            SignedXml signedXml = new SignedXml(doc);

            // Добавляем ключ в SignedXml документ. 
            signedXml.SigningKey = Key;

            // Создаем ссылку на node для подписи.
            // При подписи всего документа проставляем "".
            Reference reference = new Reference();
            reference.Uri = "";

            // Явно проставляем алгоритм хэширования,
            // по умолчанию SHA1.
            reference.DigestMethod =
                CPSignedXml.XmlDsigGost3411Url;

            // Добавляем transform на подписываемые данные
            // для удаления вложенной подписей, не только
            // собственной.
            XmlDsigXPathTransform xpath = CreateXPathTransform();
            reference.AddTransform(xpath);

            // Добавляем ссылку на подписываемые данные
            signedXml.AddReference(reference);

            // Создаем объект KeyInfo.
            KeyInfo keyInfo = new KeyInfo();

            // Добавляем сертификат в KeyInfo
            keyInfo.AddClause(new KeyInfoX509Data(Certificate));

            // Добавляем KeyInfo в SignedXml.
            signedXml.KeyInfo = keyInfo;

            // Можно явно проставить алгоритм подписи: ГОСТ Р 34.10.
            // Если сертификат ключа подписи ГОСТ Р 34.10
            // и алгоритм ключа подписи не задан, то будет использован
            // XmlDsigGost3410Url
            // signedXml.SignedInfo.SignatureMethod =
            //     CPSignedXml.XmlDsigGost3410Url;

            // Вычисляем подпись.
            signedXml.ComputeSignature();

            // Получаем XML представление подписи и сохраняем его 
            // в отдельном node.
            XmlElement xmlDigitalSignature = signedXml.GetXml();

            // Добавляем node подписи в XML документ.
            doc.DocumentElement.AppendChild(doc.ImportNode(
                xmlDigitalSignature, true));
        }
        
        // Создаем XML transform.
        private static XmlDsigXPathTransform CreateXPathTransform()
        {
	        // Создаем новый XMLDocument.
	        XmlDocument doc = new XmlDocument();

	        // Создаем новый XmlElement.
	        doc.LoadXml("<XPath xmlns:dsig=\"http://www.w3.org/2000/09/xmldsig#\">"
	                    + "not(ancestor-or-self::dsig:Signature)</XPath>");
	        XmlElement xPathElem = (XmlElement)doc.SelectSingleNode("/XPath");

	        // Создаем новый объект XmlDsigXPathTransform.
	        XmlDsigXPathTransform xForm = new XmlDsigXPathTransform();

	        // Загружаем XPath XML из элемента. 
	        xForm.LoadInnerXml(xPathElem.SelectNodes("."));

	        // Возвращаем XML, осуществляющий преобразование.
	        return xForm;
        }
    }
}
