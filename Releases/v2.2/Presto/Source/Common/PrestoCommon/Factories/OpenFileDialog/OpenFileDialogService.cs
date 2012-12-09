using System;
using System.Windows.Forms;
using Forms = System.Windows.Forms;

namespace PrestoCommon.Factories.OpenFileDialog
{
    public class OpenFileDialogService : IOpenFileDialogService
    {
        Forms.OpenFileDialog _dialog = new Forms.OpenFileDialog();

        public string InitialDirectory
        {
            get
            {
                return _dialog.InitialDirectory;
            }
            set
            {
                _dialog.InitialDirectory = value;
            }
        }

        public string FileName
        {
            get
            {
                return _dialog.FileName;
            }
            set
            {
                _dialog.FileName = value;
            }
        }

        public string[] FileNames
        {
            get
            {
                return _dialog.FileNames;
            }
        }

        public bool Multiselect
        {
            get
            {
                return _dialog.Multiselect;
            }
            set
            {
                _dialog.Multiselect = value;
            }
        }

        public DialogResult ShowDialog()
        {
            return _dialog.ShowDialog();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing == false) { return; }

            if (this._dialog != null) { this._dialog.Dispose(); }
        }  
    }
}
