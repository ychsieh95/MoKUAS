using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MoKUAS.Extensions
{
    public static class BytesExtensions
    {
        /// <summary>
        /// 將指定位元組儲存為檔案
        /// </summary>
        /// <param name="fileBytes">檔案位元組</param>
        /// <param name="filename">檔案名稱</param>
        /// <param name="saveDir">儲存路徑</param>
        /// <param name="trueDir">實體路徑</param>
        /// <param name="deleteKey">是否刪除含有關鍵字的檔案，null 表示不刪除，預設為 null</param>
        /// <returns>檔案資源之 URL</returns>
        public static string SaveToFile(this byte[] fileBytes, string filename, string saveDir, string trueDir, string deleteKey = null)
        {
            try
            {
                // Check dir is exits or not. if not, create it
                if (!Directory.Exists($"{ trueDir }{ saveDir }"))
                    Directory.CreateDirectory($"{ trueDir }{ saveDir }");

                // Remove file about user create
                if (!string.IsNullOrEmpty(deleteKey))
                {
                    List<string> files = Directory.GetFiles($"{ trueDir }{ saveDir }").ToList();
                    foreach (string delFile in files.FindAll(x => x.Remove(0, x.LastIndexOf('\\')).Contains(deleteKey)))
                        File.Delete(delFile);
                }

                // File path with name
                string filePath = $"{ trueDir }{ saveDir }{ filename }";
                using (var fs = new FileStream(filePath, FileMode.OpenOrCreate))
                    fs.Write(fileBytes, 0, fileBytes.Length);

                // Return full file url
                return $"{ saveDir }{ filename }";
            }
            catch (Exception) { return null; }
        }
    }
}
