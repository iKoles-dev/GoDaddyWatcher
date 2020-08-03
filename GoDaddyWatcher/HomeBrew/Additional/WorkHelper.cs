﻿using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

#if WPF
using System.Windows.Controls;
using System.Windows.Documents;
#endif
#if WP
using System.Windows.Forms;
#endif

namespace Homebrew.Additional
{
    public static class WorkHelper
    {
        public static string ClearLineBreaks(this string text)
        {
            return text.Replace("\n", "").Replace("\r", "");
        }

        /// <summary>
        ///     Модуль парсинга "от" первого указанного значения и "до" второго указанного значения
        /// </summary>
        /// <param name="line"></param>
        /// <param name="parsFrom">Значение от которого нужно парсить.</param>
        /// <param name="parsTo">Значение до которого нужно парсить.</param>
        /// <param name="includeParametres">Нужно ли оставлять входные значения?</param>
        /// <returns>Возвращает пустую строку, если ничего не нашёл.</returns>
        public static string ParsFromTo(this string line, string parsFrom, string parsTo, bool includeParametres = false)
        {
            if (line == null) return "";
            var regex = new Regex(parsFrom +"(.*?)"+ parsTo, RegexOptions.Singleline);
            var matches = regex.Match(line);
            return matches.Groups[includeParametres ? 0 : 1].Value;
        }

        /// <summary>
        ///     Модуль парсинга "от" первого вхождения и до конца строки
        /// </summary>
        /// <param name="line"></param>
        /// <param name="parsFrom">Значение от которого нужно парсить.</param>
        /// <param name="includeParametres">Нужно ли оставлять входные значения?</param>
        /// <returns>Возвращает пустую строку, если ничего не нашёл.</returns>
        public static string ParsFromToEnd(this string line, string parsFrom, bool includeParametres = false)
        {
            if (line == null) return "";
            var regex = new Regex(parsFrom +"(.*?)$", RegexOptions.Singleline);
            var matches = regex.Match(line);
            return matches.Groups[includeParametres ? 0 : 1].Value;
        }
        /// <summary>
        ///     Модуль парсинга от начала строки и до первого вхождения
        /// </summary>
        /// <param name="line"></param>
        /// <param name="parsTo">Значение до которого нужно парсить.</param>
        /// <param name="includeParametres">Нужно ли оставлять входные значения?</param>
        /// <returns>Возвращает пустую строку, если ничего не нашёл.</returns>
        public static string ParsFromStartTo(this string line, string parsTo, bool includeParametres = false)
        {
            if (line == null) return "";
            var regex = new Regex("^(.*?)" + parsTo, RegexOptions.Singleline);
            var matches = regex.Match(line);
            return matches.Groups[includeParametres ? 0 : 1].Value;
        }

        /// <summary>
        ///     Модуль парсинга по регулярному выражению
        /// </summary>
        /// <param name="regularExpression">Регулярное выражение</param>
        /// <param name="group">Группа для парсинга</param>
        /// <returns>Возвращает результат в виде List<string></returns>
        public static List<string> ParsRegex(this string line, string regularExpression, int group = 0)
        {
            var rawDataList = new List<string>();
            if (line == null) return rawDataList;
            var regex = new Regex(regularExpression, RegexOptions.Singleline);
            MatchCollection matches = regex.Matches(line);
            if (matches.Count > 0)
                foreach (Match match in matches)
                    rawDataList.Add(match.Groups[group].Value);
            return rawDataList;
        }
    }
}