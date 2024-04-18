using System;
using System.IO;

public static class ConfigReader
{
    public static string ReadConfigValue(string filePath, string variableName)
    {
        var value = string.Empty;
        try
        {
            string[] lines = File.ReadAllLines(filePath);

            foreach (string line in lines)
            {
                if (line.StartsWith(variableName + "="))
                {
                    value = line.Substring(variableName.Length + 1);
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error reading config file: " + ex.Message);
        }
        return value;
    }
}