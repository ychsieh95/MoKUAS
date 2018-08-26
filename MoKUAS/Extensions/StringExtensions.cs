namespace MoKUAS.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        /// 傳回字串 UTF-8 編碼長度
        /// </summary>
        /// <param name="myString"></param>
        /// <returns></returns>
        public static int GetUTF8BytesCount(this string myString)
        {
            return (string.IsNullOrEmpty(myString) ? 0 : System.Text.Encoding.GetEncoding("UTF-8").GetBytes(myString).Length);
        }
    }
}
