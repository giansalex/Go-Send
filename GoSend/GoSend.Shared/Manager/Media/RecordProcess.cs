using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Windows.Media;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using System.Diagnostics;
using Windows.Storage;
using Windows.Storage.Streams;

namespace IMM.Manager.Media
{
    public enum TipoMediaError { FAILED,EXCEEDED }
    public class EventArgsMediaError : EventArgs
    {
        public TipoMediaError Tipo { set; get; }
        public String Mensaje { set; get; }
    }
    public sealed class RecordProcess
    {
        private MediaCapture _mediaCaptureManager;
       //private StorageFile _recordStorageFile;
        private bool _recording;
        private IRandomAccessStream stream;
        #region eventos 
        public delegate void AddOnExceptionOccuredDelegate(object sender, EventArgsMediaError e);
        public event AddOnExceptionOccuredDelegate ExceptionOccured;
        private void OnExceptionOccuredEvent(object sender, EventArgsMediaError error)
        {
            if (ExceptionOccured != null)
                ExceptionOccured(sender, error);
        }
        #endregion
        public RecordProcess()
        {
            InitializeAudioRecording();   
        }
        private async void InitializeAudioRecording()
        {

            _mediaCaptureManager = new MediaCapture();
            var settings = new MediaCaptureInitializationSettings();
            settings.StreamingCaptureMode = StreamingCaptureMode.Audio;// Elegir Audio,video
            settings.MediaCategory = MediaCategory.Other;
            settings.AudioProcessing = AudioProcessing.Default;

            await _mediaCaptureManager.InitializeAsync(settings);

            Debug.WriteLine("Dispositivo inicializado");

            // Control de Excepciones
            _mediaCaptureManager.RecordLimitationExceeded += new RecordLimitationExceededEventHandler((er) => 
            {
                this.StopRecord();
                OnExceptionOccuredEvent(this, new EventArgsMediaError() { Tipo = TipoMediaError.EXCEEDED,
                                              Mensaje = "Se excedio el limite" });
            });
            _mediaCaptureManager.Failed += new MediaCaptureFailedEventHandler((sender, error) =>
            {
                OnExceptionOccuredEvent(this, new EventArgsMediaError(){ Tipo = TipoMediaError.FAILED,
                                               Mensaje = error.Message});
            });
        }
        /// <summary>
        /// Inicia la Grabacion
        /// </summary>
        public async void StartRecord()
        {
            try
            {
                Debug.WriteLine("Iniciando Grabacion");
                this.stream = null;
                this.stream = new InMemoryRandomAccessStream();

                MediaEncodingProfile recordProfile = MediaEncodingProfile.CreateM4a(AudioEncodingQuality.Auto);

                //await _mediaCaptureManager.StartRecordToStorageFileAsync(recordProfile, this._recordStorageFile);
                await _mediaCaptureManager.StartRecordToStreamAsync(recordProfile, this.stream);
                Debug.WriteLine("Grabacion iniciada");

                _recording = true;
            }
            catch (Exception e)
            {
                Debug.WriteLine("Fallo captura, Error: " + e.Message);
            }
        }
        /// <summary>
        /// Guarda archivo
        /// </summary>
        public async Task<String> SaveFile()
        {
            String fileName = DateTime.Now.ToString("dd-MM-yyyy-HH") + ".m4a";
            var local = ApplicationData.Current.LocalFolder;
            var Folder = await local.CreateFolderAsync(GoSend.constants.GetFolderByExt(".m4a"),
                              CreationCollisionOption.OpenIfExists);
            var _recordStorageFile = await Folder.CreateFileAsync(fileName, CreationCollisionOption.GenerateUniqueName);

            Debug.WriteLine("Creando archivo");

            var s = await _recordStorageFile.OpenAsync(FileAccessMode.ReadWrite);

            Windows.Storage.Streams.Buffer buff = new Windows.Storage.Streams.Buffer((uint)this.stream.Size);

            using (var inputStream = stream.GetInputStreamAt(0))
            {
                await inputStream.ReadAsync(buff, (uint)this.stream.Size, InputStreamOptions.ReadAhead);
            }

            using (var outputStream = s.GetOutputStreamAt(0))
            {
                await outputStream.WriteAsync(buff);
            }
            await this.stream.FlushAsync();
            this.stream.Dispose();
            string ruta = _recordStorageFile.Path;
            _recordStorageFile = null;
            return ruta;
        }
        /// <summary>
        /// Finaliza la grabacion
        /// </summary>
        public async void StopRecord()
        {
            if (_recording)
            {
                Debug.WriteLine("Parando grabacion");
                await _mediaCaptureManager.StopRecordAsync();
                _recording = false;
            }
        }

        /// <summary>
        /// Devuelve el Stream del audio
        /// </summary>
        /// <returns></returns>
        public IRandomAccessStream PlayRecordedCapture()
        {
            if (!_recording)
            {
                return  this.stream;
            }
            return null;
        }
        /// <summary>
        /// Retorna el Type del archivo creado despues de la grabacion
        /// </summary>
        /// <returns></returns>
        public string getFileType()
        {
            //if(_recordStorageFile!=null)
            //    return _recordStorageFile.FileType;
            //else
            return "audio/mp4";
        }
    }
}
