using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MultiLangTrans
{
    class Program
    {
        const string targetFileType = "*.cshtml";

        static void Main(string[] args)
        {
            //先做个测试

            string resourceFilePath = $@"App_GlobalResources\\LanguageResource.resx";

            string directoryPath = @"D:\Workspace\Projects\EPRICE\Lucky.Admin\Areas\Service\Views\SerReport";
            #region 读取所有的资源key
            Dictionary<string, string> resourceKeyValueList = GetResourceKeyValueList(resourceFilePath);

            if (resourceKeyValueList.Count == 0)
            {
                Console.WriteLine("资源文件未包含任何值");
                return;
            }
            #endregion

            var targetFileList = GetTargetFileList(directoryPath);

            foreach (var fileInfo in targetFileList)
            {
                Console.WriteLine($"处理文件{fileInfo.FullName}");

                var targetFilePath = fileInfo.FullName;

                var fileOrignalLines = File.ReadAllLines(targetFilePath).ToList();

                var attachedUsingStatement = "@using Resources";

                if (!fileOrignalLines.Contains(attachedUsingStatement))
                {
                    fileOrignalLines.Insert(0, attachedUsingStatement);
                }

                var writtenFileLines = new List<string>();

                foreach (var item in fileOrignalLines)
                {
                    Regex reg = new Regex("[\u4e00-\u9fa5]+");

                    var replaceItem = item;

                    foreach (var word in reg.Matches(item))
                    {
                        //过滤规则
                        if (item.Contains($"{word}.xlsx") ||
                            item.Contains($"{word}，") || //你好，  不需要
                            item.Contains($"，{word}") || //，你好  不需要 
                            item.Contains($"//{word}") ||
                            item.Contains($"/{word}") ||
                            item.Contains($"{word} ") ||
                            item.Contains($" {word} ") ||
                            item.Contains($"{word}。") ||
                            item.Contains($"//") ||
                            item.Contains($"*/") ||
                            item.Contains($"/*")//注释的不需要
                            )
                        {
                            continue;
                        }
                        var originalKey = word.ToString();

                        var labelKey = resourceKeyValueList.FirstOrDefault(s => s.Value == originalKey);

                        if (labelKey.Key != null)
                        {
                            //替换规则
                            replaceItem = replaceItem.Replace(originalKey, $" @LanguageResource.{labelKey.Key} ");

                            Console.WriteLine($"已替换Label{originalKey}为{labelKey}");
                        }
                        else
                        {
                            Console.WriteLine($"{originalKey}未在文件中找到");
                        }
                    }


                    replaceItem = replaceItem
                            .Replace("【", "[")
                            .Replace("】", "]")
                            .Replace("（", "(")
                            .Replace("）", ")")
                            .Replace("！", "!")
                            .Replace("、", ",")
                            .Replace("：", ":");

                    writtenFileLines.Add(replaceItem);
                }

                File.Delete(targetFilePath);

                using (File.Create(targetFilePath))
                {

                }


                File.AppendAllLines(targetFilePath, writtenFileLines);
            }

            Console.ReadKey();
        }

        private static List<FileInfo> GetTargetFileList(string directoryPath)
        {
            DirectoryInfo taregtFileDirectory = new DirectoryInfo(directoryPath);

            return taregtFileDirectory.GetFiles(targetFileType, SearchOption.AllDirectories).ToList();
        }

        private static Dictionary<string, string> GetResourceKeyValueList(string resourceFilePath)
        {
            Dictionary<string, string> resourceKeyValues = new Dictionary<string, string>();

            ResXResourceSet resSet = new ResXResourceSet(resourceFilePath);

            IDictionaryEnumerator dict = resSet.GetEnumerator();

            while (dict.MoveNext())
            {
                string key = (string)dict.Key;

                if (dict.Value is string)
                {
                    resourceKeyValues.Add(key, resSet.GetString(key));
                }
                else
                {
                    resourceKeyValues.Add(key, resSet.GetObject(key).ToString());
                }
            }

            return resourceKeyValues;
        }
    }
}
