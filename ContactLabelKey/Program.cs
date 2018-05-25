using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

namespace ContactLabelKey
{
    class Program
    {
        static void Main(string[] args)
        {
            //先做个测试
            string targetFilePath = $"LabelKeyList.txt";

            var fileOrignalLines = File.ReadAllLines(targetFilePath).ToList();

            var writtenFileLines = new List<string>();

            foreach (var item in fileOrignalLines)
            {

                var labelKey = "Label_" + CultureInfo.CurrentCulture.TextInfo.ToTitleCase(
                    item.ToLower())
                    .Replace("'", "")
                    .Replace(" ", "")
                    .Replace(".", "")
                    .Replace("-", "")
                    .Replace("The", "")
                    .Replace("Please", "")
                    ;

                writtenFileLines.Add(labelKey);
            }

            Console.WriteLine($"生成LabeyKey完成，共有{writtenFileLines.Count}个");

            File.Delete(targetFilePath);

            using (File.Create(targetFilePath))
            {
            }

            File.AppendAllLines(targetFilePath, writtenFileLines);

            Console.ReadKey();
        }
    }
}
