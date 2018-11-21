using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Resources;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace ExportUnmatchedChineseWords
{
    class Program
    {
        const string targetFileType = "*.cshtml";

        static void Main(string[] args)
        {
            string resourceFilePath = $"App_GlobalResources\\LanguageResource.resx";

            string directoryPath = @"D:\Workspace\Projects\EPRICE\Lucky.Admin\Areas\Service\Views";

            string unmatchedchinesePhraseListFileName = @"Output\\UnmatchedchinesePhraseList.txt";

            #region 读取所有的资源key
            Dictionary<string, string> resourceKeyValueList = GetResourceKeyValueList(resourceFilePath);

            if (resourceKeyValueList.Count == 0)
            {
                Console.WriteLine("资源文件未包含任何值");
                //return;
            }
            #endregion

            #region 读取所有中文词组
            List<string> chinesePhraseList = GetChinesePhraseList(directoryPath);
            #endregion

            #region 比较获取未在资源文件中的中文词组,并输出到文件中
            var unmatchedchinesePhraseList = chinesePhraseList.Where(chinesePhrase => !resourceKeyValueList.Values.Any(value => chinesePhrase == value)).ToList();

            File.Delete(unmatchedchinesePhraseListFileName);

            File.AppendAllLines(unmatchedchinesePhraseListFileName, unmatchedchinesePhraseList);
            #endregion

            Console.WriteLine($"筛选完成，总共有{unmatchedchinesePhraseList.Count}个中文没有在资源文件中设置");

            Thread.Sleep(2000);

            //Console.ReadKey();
        }

        private static List<string> GetChinesePhraseList(string directoryPath)
        {
            var chinesePhraseList = new List<string>();

            var logList = new List<string>();

            var targetFileList = GetTargetFileList(directoryPath);

            foreach (var fileInfo in targetFileList)
            {
                var fileOrignalLines = File.ReadAllLines(fileInfo.FullName).ToList();

                fileOrignalLines.Insert(0, "@using Resources");

                foreach (var item in fileOrignalLines)
                {
                    Regex reg = new Regex("[\u4e00-\u9fa5]+");

                    foreach (var word in reg.Matches(item))
                    {
                        //移除.xlsx名称中文
                        if (item.Contains($"{word}.xlsx"))
                        {
                            continue;
                        }

                        //移除提示的句子
                        if (item.Contains($"{word}，") || //你好，  不需要
                            item.Contains($"，{word}") || //，你好  不需要 
                            item.Contains($"//{word}") ||
                            item.Contains($"/{word}") ||
                            item.Contains($"//") ||
                            item.Contains($"/*")//注释的不需要
                            )
                        {
                            continue;
                        }

                        logList.Add($"{fileInfo.FullName} - {word}");
                        chinesePhraseList.Add(word.ToString());
                    }
                }
            }

            File.Delete("log.txt");

            File.AppendAllLines("log.txt", logList);

            return chinesePhraseList.Distinct().ToList();
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