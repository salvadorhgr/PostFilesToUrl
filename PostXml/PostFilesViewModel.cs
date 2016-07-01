using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using Microsoft.Win32;
using PostXml.Annotations;

namespace PostXml
{
    public class PostFilesViewModel: INotifyPropertyChanged
    {
        private ObservableCollection<string> _files;
        private string _url;
        private string _log;
        private RelayCommand _selectFilesCommand;
        private RelayCommand<IList> _postFilesCommand;

        public PostFilesViewModel()
        {
            _files = new ObservableCollection<string>();
            _url = "http://";
            _log = string.Empty;
            InitializeCommands();
        }

        public ObservableCollection<string> Files
        {
            get { return _files; }
            set
            {
                if (Equals(value, _files)) return;
                _files = value;
                OnPropertyChanged();
            }
        }

        public string Url
        {
            get { return _url; }
            set
            {
                if (value == _url) return;
                _url = value;
                OnPropertyChanged();
            }
        }

        public string Log
        {
            get { return _log; }
            set
            {
                if (value == _log) return;
                _log = value;
                OnPropertyChanged();
            }
        }

        public ICommand SelectFilesCommand => _selectFilesCommand;

        public RelayCommand<IList> PostFilesCommand => _postFilesCommand;

        private void InitializeCommands()
        {
            _selectFilesCommand = new RelayCommand(SelectFiles);
            _postFilesCommand = new RelayCommand<IList>(PostFiles); //, CanPostFiles);
        }

        private bool CanPostFiles(IEnumerable<string> files)
        {
            return Files != null && Files.Any();
        }

        private void PostFiles(IList files)
        {
            foreach (var file in files)
            {
                var fileName = file as string;
                var response = PostFileToUrl(fileName);
                Logger(fileName, response);
            }
        }

        private HttpResponseMessage PostFileToUrl(string fileName)
        {
            HttpResponseMessage result;
            using (var client = new HttpClient())
            {
                var fileContent = File.ReadAllText(fileName, Encoding.UTF8);
                var content = new StringContent(fileContent, Encoding.UTF8, "text/xml");
                result = client.PostAsync(Url, content).Result;
            }
            return result;
        }

        private void Logger(string fileName, HttpResponseMessage response)
        {
            Log += $"\n{DateTime.Now.ToShortTimeString()} >> File: {fileName} \n {response.StatusCode} {response.Content.ReadAsStringAsync().Result}\n";
        }

        private void SelectFiles()
        {
            var dialog = new OpenFileDialog
            {
                Multiselect = true
            };
            if (!(dialog.ShowDialog() ?? false)) return;
            foreach (var fileName in dialog.FileNames)
            {
                Files.Add(fileName);
            }
            PostFilesCommand.RaiseCanExecuteChanged();
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

    }
}