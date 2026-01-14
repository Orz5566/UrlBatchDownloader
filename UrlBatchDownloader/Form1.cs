using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UrlBatchDownloader
{
    public partial class Form1 : Form
    {
        private readonly HttpClient httpClient = new HttpClient();

        public Form1()
        {
            InitializeComponent();
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    txtSaveFolder.Text = fbd.SelectedPath;
                }
            }
        }

        private async void btnStart_Click(object sender, EventArgs e)
        {
            string urlTemplate = txtUrlTemplate.Text.Trim();
            string saveFolder = txtSaveFolder.Text.Trim();
            string upperLimitText = txtUpperLimit.Text.Trim();

            // 檢查網址格式
            if (!urlTemplate.Contains("{id}"))
            {
                MessageBox.Show("網址中必須包含 {id} 作為遞增替換位置。");
                return;
            }

            // 檢查資料夾
            if (!Directory.Exists(saveFolder))
            {
                MessageBox.Show("請選擇有效的儲存資料夾。");
                return;
            }

            // 檢查 upperLimit 是否為自然數
            if (!int.TryParse(upperLimitText, out int upperLimit) || upperLimit < 1)
            {
                MessageBox.Show("請輸入正整數作為下載上限。");
                return;
            }

            int id = 0;
            while (id < upperLimit)
            {
                id++;
                string url = urlTemplate.Replace("{id}", id.ToString());

                try
                {
                    var sw = Stopwatch.StartNew();
                    var response = await httpClient.GetAsync(url);

                    if (!response.IsSuccessStatusCode)
                    {
                        Log($"停止下載：{url} 無法存取 (HTTP {response.StatusCode})");
                        continue;
                    }

                    var contentType = response.Content.Headers.ContentType?.MediaType ?? "";

                    // 根據 Content-Type 判斷副檔名
                    string extension = GetFileExtensionFromContentType(contentType);
                    if (extension == null)
                    {
                        Log($"停止下載：無法辨識的 Content-Type: {contentType}");
                        continue;
                    }

                    var bytes = await response.Content.ReadAsByteArrayAsync();

                    // 儲存檔名處理：先看有無 Content-Disposition: attachment; filename=xxx
                    string fileName = GetFileNameFromContentDisposition(response) ?? $"file_{id}{extension}";
                    string savePath = Path.Combine(saveFolder, fileName);

                    File.WriteAllBytes(savePath, bytes);
                    sw.Stop();

                    Log($"下載成功：{fileName}, {bytes.Length} bytes, {sw.ElapsedMilliseconds} ms");
                }
                catch (Exception ex)
                {
                    Log($"停止下載：{ex.Message}");
                    continue;
                }
            }

            Log("下載完成。");
        }


        private string GetFileExtensionFromContentType(string contentType)
        {
            switch (contentType.ToLower())
            {
                case "image/png": return ".png";
                case "image/jpeg": return ".jpg";
                case "image/gif": return ".gif";
                case "image/bmp": return ".bmp";
                case "image/webp": return ".webp";
                case "application/pdf": return ".pdf";
                case "text/plain": return ".txt";
                case "application/zip": return ".zip";
                case "application/octet-stream": return ".bin";
                default: return null; // 無法識別的類型
            }
        }

        private string GetFileNameFromContentDisposition(HttpResponseMessage response)
        {
            if (response.Content.Headers.ContentDisposition != null &&
                !string.IsNullOrEmpty(response.Content.Headers.ContentDisposition.FileName))
            {
                return response.Content.Headers.ContentDisposition.FileName.Trim('"');
            }

            // 嘗試從 Raw headers 抓出 filename（適用於某些伺服器）
            if (response.Content.Headers.TryGetValues("Content-Disposition", out var values))
            {
                foreach (string value in values)
                {
                    var parts = value.Split(';');
                    foreach (var part in parts)
                    {
                        if (part.Trim().StartsWith("filename=", StringComparison.OrdinalIgnoreCase))
                        {
                            return part.Substring(part.IndexOf('=') + 1).Trim().Trim('"');
                        }
                    }
                }
            }

            return null;
        }


        private void Log(string message)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            txtLog.AppendText($"[{timestamp}] {message}{Environment.NewLine}");
        }
    }
}
