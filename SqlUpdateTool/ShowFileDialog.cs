using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;

namespace SqlUpdateTool
{
    public class ShowFileDialog
    {
        public static string DisplaySaveFileDialog(string suggestedFileName, string filter = null, string initialDirectory = null, string title = null)
        {
            string fileName = null;
            _okPressed = false;

            var dialog = new SaveFileDialog();
            dialog.AddExtension = true;
            dialog.FileName = Path.GetFileName(suggestedFileName);

            if (initialDirectory != null && Directory.Exists(initialDirectory))
            {
                dialog.InitialDirectory = initialDirectory;
                dialog.FileName = Path.Combine(initialDirectory, suggestedFileName);
            }

            if (title != null)
                dialog.Title = title;

            dialog.Filter = filter ?? "All files (*.*) | *.*";

            dialog.FileOk += DialogOnFileOk;
            dialog.ShowDialog();
            if (_okPressed) // this is a hack for Ookii
                if (!string.IsNullOrWhiteSpace(dialog.FileName))
                    fileName = dialog.FileName;

            return fileName;
        }

        private static bool _okPressed = false;
        private static void DialogOnFileOk(object sender, CancelEventArgs cancelEventArgs)
        {
            _okPressed = true;
        }

        public static string DisplayForFileSelection(string filter = null, string initialDirectory = null, string title = null)
        {
            string fileName = null;
            _okPressed = false;

            var dialog = new OpenFileDialog();
            dialog.Multiselect = false;

            if (initialDirectory != null && Directory.Exists(initialDirectory))
            {
                dialog.InitialDirectory = initialDirectory + "\\";
                dialog.FileName = initialDirectory + "\\"; // this is a hack for Ookii
            }

            if (title != null)
                dialog.Title = title;

            if (filter != null)
                dialog.Filter = filter;

            dialog.FileOk += DialogOnFileOk;
            dialog.ShowDialog();
            if (_okPressed) // this is a hack for Ookii
                if (!string.IsNullOrWhiteSpace(dialog.FileName))
                    fileName = dialog.FileName;

            return fileName;
        }

    }
}
