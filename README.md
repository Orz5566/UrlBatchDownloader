# UrlBatchDownloader
【連番檔案下載器】可在指定網址中置入「{id}」參數，程式將遞增id值並逐一下載檔案到指定資料夾。


主要功能
--------
- 輸入網址（需包含 `{id}` 字串）與遞增的上限
- 選擇儲存資料夾
- 逐一發出 GET 請求，根據 Content-Type 或 Content-Disposition 決定檔名與副檔名
- UI 內即時顯示每筆操作的時間、結果與訊息

系統需求
--------
- Windows
- .NET Framework 4.8
- 建議使用 __Visual Studio 2019__ 以上版本

使用說明
--------
1. 在「目標網址」輸入含 `{id}` 的網址，例如：`https://example.com/download?id={id}` 或 `https://site.com/files/{id}`。  
2. 點擊「…」選擇儲存資料夾。  
3. 設定「{id}上限」為正整數，程式下載到該 id 為止。  
4. 點擊「開始」執行批次下載，下方訊息欄會顯示逐筆記錄。

支援的 Content-Type
------------------------------
- image/png → .png
- image/jpeg → .jpg
- image/gif → .gif
- image/bmp → .bmp
- image/webp → .webp
- application/pdf → .pdf
- text/plain → .txt
- application/zip → .zip
- application/octet-stream → .bin
