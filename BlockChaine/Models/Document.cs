using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BlockChaine.Models
{
    public class Document
    {
        public string Text { get; set;  }
        public byte[] Sign { get; set; }
        public byte[] PublicKey { get; set; }

        public Document(string text)
        {
            Text = text;
        }

        public static byte[] GetTextBytes(string text)
        {
            return Encoding.UTF8.GetBytes(text);
        }

        public byte[] GetDataToSign()
        {
            return Document.GetTextBytes(Text);
        }

        public bool VerifyDocument(string text, byte[] sign, byte[] publicKey)
        {
            using var ecdsa = System.Security.Cryptography.ECDsa.Create();
            ecdsa.ImportSubjectPublicKeyInfo(publicKey, out _);
            return ecdsa.VerifyData(Document.GetTextBytes(Text), sign, System.Security.Cryptography.HashAlgorithmName.SHA256);
        }

        public bool IsValid()
        {
            return VerifyDocument(Text, Sign, PublicKey);
        }
    }
}
