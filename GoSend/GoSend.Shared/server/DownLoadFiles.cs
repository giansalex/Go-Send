using System;
using System.Collections.Generic;
using System.Text;
using Windows.Storage;
using Windows.Networking.BackgroundTransfer;
using System.Threading;
using System.Threading.Tasks;
using IMM.Manager;

namespace IMM.Server
{
    public class DownLoadFiles
    {
        private string Url;
        public static List<DownloadOperation> activeDownloads = new List<DownloadOperation>();
        public delegate void AddDelegateProgress(object sender, EventArgsProgressDownload e);
        public event AddDelegateProgress ProgressChanged;
        protected void onProgressChanged(double value, BackgroundTransferStatus s)
        {
            if (this.ProgressChanged != null)
                ProgressChanged(this, new EventArgsProgressDownload() { Percent = value, Status = s});
        } 

        public DownLoadFiles(string url)
        {
            this.Url = url;

        }

        public async Task<string> DownLoad(TipoMessage tipo)
        { 
                  
            try
            {
                Uri source = new Uri(this.Url);
                var ext = System.IO.Path.GetExtension(this.Url);
                string destination = DateTime.Now.ToString("dd_MM_yyyy_HH") + ext;
                
                var local = ApplicationData.Current.LocalFolder;
                var Folder = await local.CreateFolderAsync(GoSend.constants.GetFolderByExt(ext),
                                  CreationCollisionOption.OpenIfExists);
                StorageFile destinationFile = await Folder.CreateFileAsync(destination, CreationCollisionOption.GenerateUniqueName);

                Log(destinationFile.Path);
                BackgroundDownloader downloader = new BackgroundDownloader();
                DownloadOperation download = downloader.CreateDownload(source, destinationFile);

                // Attach progress and completion handlers.
                HandleDownloadAsync(download, true);
                return destinationFile.Path;
            }
            catch (Exception ex)
            {
                Log("Download Error:" + ex.Message);
            }
            return "";
        }

        private async void HandleDownloadAsync(DownloadOperation download, bool start)
        {
            //CancellationToken cts = new CancellationToken();
            try
            {
                // Store the download so we can pause/resume.
                DownLoadFiles.activeDownloads.Add(download);

                Progress<DownloadOperation> progressCallback = new Progress<DownloadOperation>(new Action<DownloadOperation>(DownLoadProgress));
                if (start)
                {
                    // Start the download and attach a progress handler.
                    await download.StartAsync().AsTask(progressCallback);
                }
                else
                {
                    // The download was already running when the application started, re-attach the progress handler.
                    await download.AttachAsync().AsTask(progressCallback);
                }

                ResponseInformation response = download.GetResponseInformation();
                Log(String.Format("Completed: {0}, Status Code: {1}", download.Guid, response.StatusCode));
            }
            catch (TaskCanceledException)
            {
                Log("Download cancelled.");
            }
            catch (Exception ex)
            {
                Log("Error" + ex.Message);
            }
            finally
            {
                DownLoadFiles.activeDownloads.Remove(download);
            }
        }
        private void DownLoadProgress(DownloadOperation op)
        {
            onProgressChanged((double)op.Progress.BytesReceived / (double)op.Progress.TotalBytesToReceive, op.Progress.Status);
        }
        private void Log(string t)
        {
            System.Diagnostics.Debug.WriteLine(t);
        }
    }

    public class EventArgsProgressDownload : EventArgs
    {
        public double Percent;
        public BackgroundTransferStatus Status;
    }
}
