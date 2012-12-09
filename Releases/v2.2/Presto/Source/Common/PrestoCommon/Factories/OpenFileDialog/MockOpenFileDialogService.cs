using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;

namespace PrestoCommon.Factories.OpenFileDialog
{
    public class MockOpenFileDialogService : IOpenFileDialogService
    {
        private string _initialDirectory;
        private static string _fileName;
        private string[] _fileNames;
        private bool _multiselect;

        public string InitialDirectory
        {
            get
            {
                return this._initialDirectory;
            }
            set
            {
                this._initialDirectory = value;
            }
        }

        public string FileName
        {
            get
            {
                return _fileName;
            }
            set
            {
                _fileName = value;
            }
        }

        public string[] FileNames
        {
            get { return this._fileNames; }
        }

        public bool Multiselect
        {
            get
            {
                return this._multiselect;
            }
            set
            {
                this._multiselect = value;
            }
        }

        public DialogResult ShowDialog()
        {
            return DialogResult.OK;
        }

        public static void SetFileName(string fileName)
        {
            _fileName = fileName;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        private void Dispose(bool disposing)
        {
            if (disposing == false) { return; }

            // Nothing to dispose
        }
    }
}
