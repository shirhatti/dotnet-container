﻿using System.Security.Cryptography;
using System.Text;

namespace Dotnet.Container.RegistryClient.Helpers
{
    public static class SHAHelpers
    {
        public static string ComputeSHA256(string rawData)
        {
            // Create a SHA256   
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // ComputeHash - returns byte array  
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                // Convert byte array to a string   
                var builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return $"sha256:{builder.ToString()}";
            }
        }
    }
}
