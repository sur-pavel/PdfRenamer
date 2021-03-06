﻿using System;
using System.Text.RegularExpressions;

namespace PdfRenamer
{
    public class Patterns
    {
        public string NumberPattern = @"\d+";
        public string cleanUpPattern = @"^\s*(\.|\,|\:|\;)|\,|(\,|\:)\s*$";

        public string AutorPattern = @"-([А-я]|\w|((?!__))+|\s)*-" +
            @"([А-я]|\w|((?!__))+|\s)+\,\s" +
            @"(([А-я]|\w|((?!__))+|-|\(|\))+\s?){1,3}";

        public string WordPattern = @"(\s|^)([\D]|((?!__))+)+(\s|$)";

        public string FileName = @"((\w|[А-я])+\s(\w|[А-я])\.)?_.+_" +
            @"\[?(\w|[А-я])*\]?_\[?(\d|-){4}\]?_\d+?(p|с)?=.+\d+_.*";

        public string TownPattern = @"\.\s([А-я]|\w|((?!__))+)+\s\:";
        public string YearPattern = @"\d{4}";

        public string BookEditionPattern = @"\.\s([А-я]|\w|((?!__))+)+\s\:\s([\D]|((?!__))+)+\,\s\d{4}";

        public string EscapedSymbols = @"(\||\\|\;|\:|\/|\?|\*|\>|\<)";

//        private string fileNameExample = "Иванов П._Название статьи_М_19--_12с=Название журнала_номер 193_Т.1";

        internal Match MatchBookEdition(string str)
        {
            str = Regex.Replace(str, @"\?", "");
            
            return Regex.Match(str, @"([\u00C0-\u017FA-zА-я'’?-]+\s?){1,3}\s?\.?\:\s?[\u00C0-\u017FA-zА-я'’,?-]{1}.+\d{4}");
        }

        internal Match MatchBookAutor(string str)
        {
            return Regex.Match(str, @"([\u00C0-\u017Fa-zA-Z']+([- ][\u00C0-\u017Fa-zA-Z']+)*,\s[\u00C0-\u017Fa-zA-Z']+([- ][\u00C0-\u017Fa-zA-Z']+)*(\s\(dir.\))?(\s\;\s)?){1,2}");
        }

        internal Match MatchTitlePage(string str)
        {
            return Regex.Match(str, @"([\u00C0-\u017Fa-zA-Z'’?-]+\s?\n{0,5})+.+([\u00C0-\u017Fa-zA-Z'’?-]+\s?)+\d{4}");
        }

        internal Match MatchStringWithPage(string str)
        {
            return Regex.Match(str, @"^\d{1,4}\s.?\s?\w|^.+\s\s?\d{1,4}\n");
        }

        internal Match MatchPageNumber(string str)
        {
            return Regex.Match(str, @"\d{1,4}");
        }

        internal Match MatchBookTown(string str)
        {
            return Regex.Match(str, @"([А-я]|\w|((?!__))+)+\s?\:");
        }

        internal Match MatchDirectoryPath(string str)
        {
            return Regex.Match(str, @"[a-zA-Z]:\\((?:.*?\\)*)");
        }

        internal Match MatchEscapedSymbols(string str)
        {
            return Regex.Match(str, @"(\||\\|\;|\:|\/|\?|\*|\>|\<)");
        }

        internal Match MatchJVolumeYear(string str)
        {
            return Regex.Match(str, @"\s\d\s\|\s\d{4}");
        }

        internal Match MatchTitle(string str)
        {
            return Regex.Match(str, @"([\u00C0-\u017Fa-zA-Z'’,\-]+\s|(\(?\d{1,4}\)?\-?){1,2})+[\u00C0-\u017Fa-zA-Z'’\-]*\s?(\.|\:|\?|\!)(\s?\(?\d{1,4}\)?\-?){0,2}");
        }

        internal Match MatchYear(string str)
        {
            return Regex.Match(str, @"\d{4}");
        }

        internal Match MatchWord(string str)
        {
            return Regex.Match(str, @"(\s|^)([\D]|((?!__))+)+(\s|$)");
        }

        internal Match MatchArticleTitle(string str)
        {
            return Regex.Match(str, @"\«(\s|^)([\D]|((?!__))+)+(\s|$)\»");
        }

        internal Match FrenchLastNames(string str)
        {
            return Regex.Match(str, @"[\u00C0-\u017Fa-zA-Z']+([- ][\u00C0-\u017Fa-zA-Z']+)*");
        }

        internal Match MatchJournalVolume(string str)
        {
            return Regex.Match(str, @"-\d{1,4}");
        }

        internal Match SymbolsToEraise(string str)
        {
            return Regex.Match(str, @"^\s*(\.|\,|\:|\;)|(\.|\,|\:|\;)\s*$");
        }

        internal Match MatchJournalTitle(string str)
        {
            return Regex.Match(str, @"\d{4}");
        }

        internal Match MatchWrongBook(string str)
        {
            return Regex.Match(str, @"Collection : ([\u00C0-\u017Fa-zA-Z'’?-]+\s?)+\n");
        }

        internal Match MatchOddJournalData(string str)
        {
            return Regex.Match(str, @"\d{1,5}\s?[\u00C0-\u017Fa-zA-Z'’-]+.+\d{4}\-\d");
        }
    }
}