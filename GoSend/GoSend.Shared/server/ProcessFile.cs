using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Windows.Web.Http;
using Windows.Storage;

namespace IMM.Server
{
    public class ProcessFile
    {
        public ProcessFile()
        {
            
        }
        public async Task<string> SendFile(string path)
        {
            StorageFile storageFile = await StorageFile.GetFileFromPathAsync(path);
            Windows.Storage.Streams.IRandomAccessStream fileStream =
                await storageFile.OpenAsync(Windows.Storage.FileAccessMode.Read);
            try
            {
                HttpClient httpClient = new HttpClient();
                HttpMultipartFormDataContent form = new HttpMultipartFormDataContent();

                var fileForm = new HttpStreamContent(fileStream);
                fileForm.Headers.ContentType = new Windows.Web.Http.Headers.HttpMediaTypeHeaderValue(storageFile.ContentType);

                form.Add(fileForm, "u", storageFile.Name);

                HttpResponseMessage response = await httpClient.PostAsync(new Uri("http://127.0.0.1/Project/GoSend/helper/sIm.php", UriKind.Absolute), form);

                if (response.IsSuccessStatusCode)
                {
                    httpClient.Dispose();
                    string result = await response.Content.ReadAsStringAsync();
                    if (!result.Equals(""))
                        return result;
                }
            }catch{}

            return "";
        }
        public async Task<string> CopyToLocal(string path)
        {
            string Ext = System.IO.Path.GetExtension(path);
            var local = ApplicationData.Current.LocalFolder;
            var Folder = await local.CreateFolderAsync(GoSend.constants.GetFolderByExt(Ext), 
                              CreationCollisionOption.OpenIfExists);
            var fileOrigen = await StorageFile.GetFileFromPathAsync(path);
            var r = await fileOrigen.CopyAsync(Folder, DateTime.Now.ToString("dd_MM_yyyy_HH") + Ext , NameCollisionOption.GenerateUniqueName);
            return r.Path;
        }
    }
}
