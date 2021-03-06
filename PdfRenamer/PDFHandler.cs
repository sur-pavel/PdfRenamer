﻿using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace PdfRenamer
{
    internal class PDFHandler : System.Web.UI.Page
    {
        internal Log log;
        private Patterns patterns;
        private string inputFile;
        private string outputFile;
        private string firstPages;
        private string lastPages;

        public PDFHandler(Log log, Patterns patterns)
        {
            this.log = log;
            this.patterns = patterns;
        }

        internal Article GetPdfPageText(FileInfo file, Article article)
        {
            PdfReader pdfReader = GetPdfReader(file);
            if (pdfReader != null)
            {
                int pageNumber = GetArticlePdfText(article, pdfReader);
                pageNumber = GetAlterPdfText(article, pdfReader, pageNumber);
                GetBeginPageNumber(article, pdfReader, pageNumber);
                GetLastPageNumber(article, pdfReader);
                if (string.IsNullOrEmpty(article.PdfText.ToString()))
                {
                    log.WriteLine($"{file.Name} not contains any text");
                }
                pdfReader.Close();
            }
            return article;
        }
        private PdfReader GetPdfReader(FileInfo file)
        {
            try
            {
                return new PdfReader(file.FullName);
            }
            catch (iTextSharp.text.exceptions.InvalidPdfException)
            {
                log.WriteLine("Not a pdf file: " + file.Name);
                return null;
            }
        }
        private int GetArticlePdfText(Article article, PdfReader pdfReader)
        {
            int pageNumber = 1;
            for (; pageNumber <= 10; pageNumber++)
            {
                string pdfText = PdfTextExtractor.GetTextFromPage(pdfReader, pageNumber, new LocationTextExtractionStrategy());
                if (pdfText.Contains("openedition.org") || pdfText.Contains("ISBN"))
                {
                    pdfText = Regex.Replace(pdfText, "\n", " ");
                    pdfText = Regex.Replace(pdfText, "  ", " ");
                    article.PdfText.Append(pdfText);
                    if (patterns.MatchBookEdition(pdfText).Success)
                    {
                        article.DocumentType = Article.DocType.Book;
                    }
                   else if (patterns.MatchWrongBook(pdfText).Success)
                    {
                        article.Journal.Title = patterns.MatchWrongBook(pdfText).Value.Replace("Collection : ", "");
                        article.DocumentType = Article.DocType.WrongBook;
                        GetOddPdfText(article, pdfReader, pageNumber);
                    }
                    else
                    {
                        article.DocumentType = Article.DocType.Article;
                    }

                    break;
                }
            }

            return pageNumber;
        }
        private int GetAlterPdfText(Article article, PdfReader pdfReader, int pageNumber)
        {
            if (article.DocumentType == Article.DocType.Article)
            {
                for (int alterPageNumber = 1; alterPageNumber <= 10; alterPageNumber++)
                {
                    string pdfText = PdfTextExtractor.GetTextFromPage(pdfReader, alterPageNumber, new LocationTextExtractionStrategy());
                    if (patterns.MatchStringWithPage(pdfText).Success)
                    {
                        firstPages = patterns.MatchStringWithPage(pdfText).Value;
                        article.Pages = firstPages.Substring(0, firstPages.Length - 2).Trim();
                        pageNumber = alterPageNumber;
                        break;
                    }
                }
            }
            return pageNumber;
        }
        private int GetBeginPageNumber(Article article, PdfReader pdfReader, int pageNumber)
        {
            if (string.IsNullOrEmpty(article.PdfText.ToString()))
            {
                for (; pageNumber <= 10; pageNumber++)
                {
                    string pdfText = PdfTextExtractor.GetTextFromPage(pdfReader, pageNumber, new LocationTextExtractionStrategy());
                    if (patterns.MatchTitlePage(pdfText).Success)
                    {
                        article.PdfText.Append(pdfText);
                        article.DocumentType = Article.DocType.Book;
                        break;
                    }
                }
            }

            return pageNumber;
        }
        private void GetLastPageNumber(Article article, PdfReader pdfReader)
        {
            int lastPageNumber = pdfReader.NumberOfPages;
            int pageNumberFor = lastPageNumber - 30;
            if (pageNumberFor < 0)
            {
                pageNumberFor = 1;
            }
            for (; lastPageNumber > pageNumberFor; lastPageNumber--)
            {
                string pdfText = PdfTextExtractor.GetTextFromPage(pdfReader, lastPageNumber, new LocationTextExtractionStrategy());
                if (article.DocumentType == Article.DocType.WrongBook && patterns.MatchOddJournalData(pdfText).Success)
                {
                    string oddJournalData = patterns.MatchOddJournalData(pdfText).Value;
                    article.Pages = Regex.Match(oddJournalData, @"^\d{1,4}").Value + " p";
                }
                if (patterns.MatchStringWithPage(pdfText).Success)
                {
                    lastPages = patterns.MatchStringWithPage(pdfText).Value;
                    int pages = int.Parse(patterns.MatchPageNumber(lastPages).Value);
                    if (pages > pageNumberFor)
                    {
                        if (article.DocumentType == Article.DocType.Book)
                        {
                            article.Pages = pages + " p";
                        }
                        else
                        {
                            article.Pages += "-" + pages + " p";
                        }
                        break;
                    }
                }
            }
        }

        private void GetOddPdfText(Article article, PdfReader pdfReader, int pageNumber)
        {
            StringBuilder stringBuilder = new StringBuilder();
            for (; pageNumber <= 10; pageNumber++)
            {
                string oddPdfText = PdfTextExtractor.GetTextFromPage(pdfReader, pageNumber, new LocationTextExtractionStrategy());
                if (patterns.MatchOddJournalData(oddPdfText).Success)
                {
                    string oddJournalData = patterns.MatchOddJournalData(oddPdfText).Value;
                    oddJournalData = Regex.Replace(oddJournalData, @"^\d{1,4}\s?", "");
                    article.OddPdfText = stringBuilder.Append(oddJournalData);
                }
            }
        }


    }
}