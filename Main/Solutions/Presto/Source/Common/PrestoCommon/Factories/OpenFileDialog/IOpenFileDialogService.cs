using System;
using System.Windows.Forms;

namespace PrestoCommon.Factories.OpenFileDialog
{
    public interface IOpenFileDialogService : IDisposable
    {
        string InitialDirectory { get; set; }
        string FileName { get; set; }
        string[] FileNames { get; }
        bool Multiselect { get; set; }      
        DialogResult ShowDialog();
    }
}
