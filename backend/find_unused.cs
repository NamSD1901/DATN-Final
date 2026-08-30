using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;
using System.Collections.Generic;

class Program
{
    static void Main()
    {
        string backendDir = @"e:\DATN\MyPetClinic\backend\src";
        string appDir = Path.Combine(backendDir, "MyPetClinic.Application");
        string webApiDir = Path.Combine(backendDir, "WebApi");
        
        var csFiles = Directory.GetFiles(backendDir, "*.cs", SearchOption.AllDirectories).ToList();
        var interfaceFiles = Directory.GetFiles(Path.Combine(appDir, "Interfaces", "Services"), "*.cs", SearchOption.AllDirectories);
        
        foreach (var iface in interfaceFiles)
        {
            string content = File.ReadAllText(iface);
            // Regex to find method signatures in interface
            var matches = Regex.Matches(content, @"(?m)^\s*(?:Task(?:<[^>]+>)?|void|[\w\[\]<>]+)\s+([A-Z]\w+)\s*\(");
            foreach (Match m in matches)
            {
                string methodName = m.Groups[1].Value;
                // Count occurrences in all cs files
                int count = 0;
                foreach (var file in csFiles)
                {
                    string fileContent = File.ReadAllText(file);
                    // Match methodName as a whole word
                    count += Regex.Matches(fileContent, $@"\b{methodName}\b").Count;
                }
                
                // Usually it appears in:
                // 1. Interface definition
                // 2. Service implementation
                // If count <= 2, it's likely unused outside of its own definition
                if (count <= 2)
                {
                    Console.WriteLine($""UNUSED METHOD: {methodName} in {Path.GetFileName(iface)}"");
                }
            }
        }
    }
}
