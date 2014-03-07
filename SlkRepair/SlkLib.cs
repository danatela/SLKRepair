using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Collections.Specialized;
using SlkRepair.Properties;
using System.Collections;

namespace SlkRepair
{
    public static class SlkLib
    {
        public enum RecordType { Header, Bounds, Cell, Format, Pattern, Options, NU, NE, NL, NN, Window, EndFile };
         internal static void Liposuct(string fatSlkName)
         {
            StreamReader fatSlk = new StreamReader(File.OpenRead(fatSlkName), Encoding.Default);
            StreamWriter thinSlk = null;
            StringCollection patterns = new StringCollection(), validPatterns = new StringCollection();
            StringCollection styles = new StringCollection(), validStyles = new StringCollection();
            StringCollection buffer = new StringCollection();
            Regex rPattern = new Regex(Settings.Default.exPattern);
            Regex rFormat = new Regex(Settings.Default.exFormat);
            Regex rStyle = new Regex(Settings.Default.exStyle);
            Regex rX = new Regex(Settings.Default.exAbsciss);
            Regex rY = new Regex(Settings.Default.exOrdinate);
            string mX = "", mY = "", lastX = null, lastY = null;
            string mPattern = null, mFormat = null, mStyle = null;
            string nextRec = null;
            while (true)
            {
                string rec = nextRec ?? fatSlk.ReadLine();
                nextRec = fatSlk.ReadLine();
                if (rec == null)
                    break;
                switch (rec[0])
                {
                    case 'P': switch (rec[2])
                        {
                            case 'P':
                                if (validPatterns.Count == 0)
                                    validPatterns.Add(rec);
                                patterns.Add(rec);
                                break;
                            case 'E':
                            case 'F':
                                if (validStyles.Count == 0)
                                    validStyles.Add(rec);
                                styles.Add(rec);
                                break;
                        }
                        continue; // читаем заголовок
                    case 'C':
                    case 'F':
                        if (rec.StartsWith("F;W"))
                            goto default;
                        if (rec.StartsWith("F;M"))
                            continue;
                        mX = rX.Match(rec + mX, 1).Value;
                        if (mX.Length > 0)
                            rec = rec.Replace(mX, null);
                        mY = rY.Match(rec + mY, 1).Value;
                        if (mY.Length > 0)
                            rec = rec.Replace(mY, null);
                        if (rec[0] == 'C')
                            goto case 'Я';
                        Match matchStyle = rStyle.Match(rec, 1);
                        mStyle = matchStyle.Value;
                        //*
                        if (nextRec.StartsWith("F"))
                            if (!mStyle.ContainsAny("LRTB".ToCharArray()))
                                break;
                        //*/
                        mPattern = rPattern.Match(rec, 1).Value;
                        mFormat = rFormat.Match(rec, 1).Value;
                        if (mPattern != "")
                        {
                            string pattern = patterns[int.Parse(mPattern.Replace(";P", null))];
                            int pIndex = validPatterns.IndexOf(pattern);
                            if (pIndex == -1)
                                pIndex = validPatterns.Add(pattern);
                            rec = rec.Replace(mPattern, ";P" + pIndex);
                        }
                        if (mStyle != "")
                        {
                            int link = int.Parse(matchStyle.Groups["num"].Value);
                            if (link == 4)
                                goto case 'Я';
                            if (link > 4)
                                link--;
                            string style = styles[link];
                            int sIndex = validStyles.IndexOf(style);
                            if (sIndex == -1)
                                sIndex = validStyles.Add(style);
                            if (sIndex > 3)
                                sIndex++;
                            rec = rec.Replace(mStyle, matchStyle.Groups["prefix"].Value + sIndex);
                            goto case 'Я';
                        }
                        break;
                    default:
                        buffer.Add(rec);
                        break;
                    case 'Я': // специальная метка, чтоб не писать два раза обработку записей
                        if (mX != "") if (mX == lastX)
                                mX = "";
                            else
                                lastX = mX;
                        if (mY != "") if (mY == lastY)
                                mY = "";
                            else
                                lastY = mY;
                        if (Settings.Default.Convert && rec[3] == '\"')
                            rec = Sylka2Alpha(rec);
                        rec = rec[0] + mY + mX + rec.Substring(1);
                        goto default;
                }
            }
            fatSlk.Close();
            thinSlk = new StreamWriter(fatSlkName + "_new", false, Settings.Default.Convert ? Encoding.GetEncoding(Settings.Default.CodePage) : Encoding.Default);
            thinSlk.WriteLine(buffer[0].Replace(";PWXL", ";PSLKREPAIR"));
            foreach (string pattern in validPatterns)
                thinSlk.WriteLine(pattern);
            foreach (string style in validStyles)
                thinSlk.WriteLine(style);
            for (int i = 1; i < buffer.Count; i++)
            {
                thinSlk.WriteLine(buffer[i]);
            }
            thinSlk.Close();
            File.Replace(fatSlkName + "_new", fatSlkName, fatSlkName + "~");
        }
        private static string alphabet = "АБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯабвгдеёжзийклмнопрстуфхцчшщъыьэюя";
        private static string[] sylkabet;
        public static string Alphabet { get { return alphabet; } }
        public static string[] Sylkabet
        {
            get
            {
                if (sylkabet == null || sylkabet.Length < 66)
                    InitSylkabet();
                return sylkabet;
            }
        }

        private static void InitSylkabet()
        {
            sylkabet = Resources.sylkabet.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            if (sylkabet.Length < 66)
                throw new ArgumentOutOfRangeException("Размер файла с алфавитом слишком мал!");
        }
        public static string Sylka2Alpha(string sylkaStr)
        {
            for (int i = 0; i < Sylkabet.Length; i++)
            {
                sylkaStr = sylkaStr.Replace(Sylkabet[i], Alphabet[i].ToString()); 
            }
            return sylkaStr;
        }
        public static string Sylka2Alpha(params string[] sylkaParams)
        {
            string result = "";
            foreach (string sylka in sylkaParams)
                result += Sylka2Alpha(sylka);
            return result;
        }
        public static string Alpha2Sylka(char alpha)
        {
            int i = Alphabet.IndexOf(alpha);
            if (i > -1)
                return Sylkabet[i];
            else
                return alpha.ToString();
        }
        public static string Alpha2Sylka(string alphas)
        {
            string result = "";
            foreach (char alpha in alphas)
                result += Alpha2Sylka(alpha);
            return result;
        }

    }
    public static class RegexEx
    {
        public static bool ContainsAny(this string s, params char[] chars)
        {
            bool result = false;
            foreach (char c in chars)
            {
                result |= s.Contains(c);
                if (result)
                    break;
            }
            return result;
        }
    }
}
