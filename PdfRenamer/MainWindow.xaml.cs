using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Forms = System.Windows.Forms;

namespace PdfRenamer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        internal Log log = new Log();
        private FileHandler fileHandler;
        private ExcelHandler excelHandler;

        private Patterns patterns = new Patterns();
        private Article currentArticle;
        private ArticleParser articleParser;

        private PDFHandler pdfHandler;
        private List<FileInfo> filesInfoList;
        private HashSet<string> filesToDelete;
        private string nameForFile = string.Empty;
        private string tempFileFullName = string.Empty;
        private int infoListIndex;
        private bool fromShowMethod;
        private StackFrame stackTraceFrame;
        private const string DefaultOutputText = "Выберите папку с pdf-файлами";

        public MainWindow()
        {
            WindowState = WindowState.Maximized;

            try
            {
                fileHandler = new FileHandler();
                excelHandler = new ExcelHandler(log);
                articleParser = new ArticleParser(log, patterns);
                pdfHandler = new PDFHandler(log, patterns);
                FileUtil fileUtil = new FileUtil();
                fileUtil.KillProcesses("excel");
                filesToDelete = new HashSet<string>();
                fromShowMethod = true;

                fileHandler.Log = log;
                excelHandler.CreatExcelObject();
                log.CreateLogFile();
                InitializeComponent();
                infoListIndex = 0;
            }
            catch (Exception e)
            {
                log.WriteLine(e.Message, e.StackTrace);
                throw;
            }
        }


        private List<FileInfo> GetFiles()
        {
            if (patterns.MatchDirectoryPath(InputPath.Text).Success)
            {
                filesInfoList = fileHandler.GetFileNames(InputPath.Text);
                return filesInfoList;
            }

            InfoLabel.Content = "Выберите папку с pdf-файлами и папку назначения";
            return new List<FileInfo>();
        }

        private void ShowPdf()
        {
            fromShowMethod = true;
            stackTraceFrame = new StackTrace().GetFrame(0);
            currentArticle = GetCurrentArticle();
            log.WriteLine($"{stackTraceFrame.GetMethod()} Old fileName: {filesInfoList[infoListIndex].Name}:");
            ClearControls();
            FillInputs(currentArticle);
            ManageNewFileName();

            tempFileFullName = fileHandler.CreateTempFile(filesInfoList[infoListIndex]);
            webBrowser.Navigate(tempFileFullName);

            OldFileNameLabel.Content = filesInfoList[infoListIndex].Name;
            InfoLabel.Content = string.Empty;
            fromShowMethod = false;
        }

        private Article GetCurrentArticle()
        {
            currentArticle = new Article();
            currentArticle = pdfHandler.GetPdfPageText(filesInfoList[infoListIndex], currentArticle);
            while (currentArticle.PdfText.ToString().Equals(""))
            {
                infoListIndex++;
                currentArticle = pdfHandler.GetPdfPageText(filesInfoList[infoListIndex], currentArticle);
            }

            currentArticle = articleParser.ParsePdfText(currentArticle);
            return currentArticle;
        }

        private void FillInputs(Article article)
        {
            AutorInput.Text = article.Autor;
            TitleInput.Text = article.Title;
            TownInput.Text = article.Town;
            YearInput.Text = article.Year;
            PagesInput.Text = article.Pages;

            JTitleInput.Text = article.Journal.Title;
            JNumberInput.Text = article.Journal.Number;
            JVolumeInput.Text = article.Journal.Volume;
            if (article.DocumentType == Article.DocType.Book)
            {
                DocType.SelectedItem = "Книга";
            }
            else if (article.DocumentType == Article.DocType.WrongBook)
            {
                DocType.SelectedItem = "Книга?";
            }
            else if (article.DocumentType == Article.DocType.Article)
            {
                DocType.SelectedItem = "Статья";
            }
        }

        private void CreateNameForFile()
        {
            if (!fromShowMethod)
            {
                currentArticle.Autor = string.IsNullOrEmpty(AutorInput.Text) ? currentArticle.Autor : AutorInput.Text;
                currentArticle.Title = string.IsNullOrEmpty(TitleInput.Text) ? currentArticle.Title : TitleInput.Text;
                currentArticle.Town = string.IsNullOrEmpty(TownInput.Text) ? currentArticle.Town : TownInput.Text;
                currentArticle.Year = string.IsNullOrEmpty(YearInput.Text) ? currentArticle.Year : YearInput.Text;
                currentArticle.Pages = string.IsNullOrEmpty(PagesInput.Text) ? currentArticle.Pages : PagesInput.Text;

                currentArticle.Journal.Title = string.IsNullOrEmpty(JTitleInput.Text)
                    ? currentArticle.Journal.Title
                    : JTitleInput.Text;
                currentArticle.Journal.Number = string.IsNullOrEmpty(JNumberInput.Text)
                    ? currentArticle.Journal.Number
                    : JNumberInput.Text;
                currentArticle.Journal.Volume = string.IsNullOrEmpty(JVolumeInput.Text)
                    ? currentArticle.Journal.Volume
                    : JVolumeInput.Text;

                ManageNewFileName();
            }
        }

        private void NextFileClick(object sender, RoutedEventArgs e)
        {
        

            if (NewFileNameInput.Text.Contains(".pdf") && patterns.MatchDirectoryPath(OutputPath.Text).Success)
            {
                currentArticle.FileName = NewFileNameInput.Text;
                stackTraceFrame = new StackTrace().GetFrame(0);
                log.WriteLine(stackTraceFrame.GetMethod() + " New fileName:" + nameForFile);
                log.WriteLine(stackTraceFrame.GetMethod() + currentArticle.ToString());
                bool moved = fileHandler.Move(filesInfoList[infoListIndex], OutputPath.Text + currentArticle.FileName);
                if (moved)
                {
                    infoListIndex++;
                    if (infoListIndex < filesInfoList.Count)
                    {
                        if (!string.IsNullOrEmpty(tempFileFullName))
                        {
                            filesToDelete.Add(tempFileFullName);
                        }

                        Article articleForExcel = currentArticle;
                        Task.Factory.StartNew(() =>
                        {
                            excelHandler.AddRow(articleForExcel);
                            excelHandler.SaveFile();
                        });
                        ShowPdf();
                    }
                    else
                    {
                        MessageBox.Show("Все файлы обработаны");
                        ClearControls();
                    }
                }
            }
            else
            {
                InfoLabel.Content = "Выберите папку с pdf-файлами и папку назначения";
            }
        }

        private void ManageNewFileName()
        {
            nameForFile = articleParser.GetFileName(currentArticle);
            nameForFile = articleParser.CheckFileName(nameForFile);
            if (!nameForFile.Contains(".pdf"))
            {
                InfoLabel.Content = nameForFile;
            }
            else
            {
                NewFileNameInput.Text = nameForFile;
            }

            log.WriteLine($"{stackTraceFrame.GetMethod()}: {currentArticle}");
        }

        private void ClearControls()
        {
            AutorInput.Text = string.Empty;
            TitleInput.Text = string.Empty;
            TownInput.Text = string.Empty;
            YearInput.Text = string.Empty;
            PagesInput.Text = string.Empty;

            JTitleInput.Text = string.Empty;
            JNumberInput.Text = string.Empty;
            JVolumeInput.Text = string.Empty;
            NewFileNameInput.Text = string.Empty;
            InfoLabel.Content = string.Empty;
        }

        private void InputPath_Click(object sender, RoutedEventArgs e)
        {
            Forms.FolderBrowserDialog fbd = new Forms.FolderBrowserDialog();
            if (fbd.ShowDialog() == Forms.DialogResult.OK)
            {
                InputPath.Text = fbd.SelectedPath + @"\";
            }
        }

        private void OutputPath_Click(object sender, RoutedEventArgs e)
        {
            Forms.FolderBrowserDialog fbd = new Forms.FolderBrowserDialog();
            if (fbd.ShowDialog() == Forms.DialogResult.OK)
            {
                OutputPath.Text = fbd.SelectedPath + @"\";
            }
        }

        private void AutorInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!fromShowMethod)
            {
                CreateNameForFile();
            }
        }

        private void TitleInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!fromShowMethod)
            {
                CreateNameForFile();
            }
        }

        private void YearInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!fromShowMethod)
            {
                CreateNameForFile();
            }
        }

        private void JTitleInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!fromShowMethod)
            {
                CreateNameForFile();
            }
        }

        private void JNumberInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!fromShowMethod)
            {
                CreateNameForFile();
            }
        }

        private void JVolumeInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!fromShowMethod)
            {
                CreateNameForFile();
            }
        }

        private void TownInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!fromShowMethod)
            {
                CreateNameForFile();
            }
        }

        private void PagesInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!fromShowMethod)
            {
                CreateNameForFile();
            }
        }

        private void InputPath_TextChanged(object sender, TextChangedEventArgs e)
        {
            filesInfoList = GetFiles();
            if (filesInfoList.Count > 0 && !InputPath.Text.Equals(DefaultOutputText))
            {
                ShowPdf();
            }
        }

        private void OutputPath_TextChanged(object sender, TextChangedEventArgs e)
        {
        }
    }
}