﻿using System.Text;

namespace SeleniumBaseApi.Classes
{
    public class WordStemmer
    {
        string[] doubles = { "bb", "dd", "ff", "gg", "mm", "nn", "pp", "rr", "tt" };
        string[] validLiEndings = { "c", "d", "e", "g", "h", "k", "m", "n", "r", "t" };

        private string[,] step1bReplacements =
            {
            {"eedly","ee"},
            {"ingly",""},
            {"edly",""},
            {"eed","ee"},
            {"ing",""},
            {"ed",""}
        };

        string[,] step2Replacements =
            {
            {"ization","ize"},
            {"iveness","ive"},
            {"fulness","ful"},
            {"ational","ate"},
            {"ousness","ous"},
            {"biliti","ble"},
            {"tional","tion"},
            {"lessli","less"},
            {"fulli","ful"},
            {"entli","ent"},
            {"ation","ate"},
            {"aliti","al"},
            {"iviti","ive"},
            {"ousli","ous"},
            {"alism","al"},
            {"abli","able"},
            {"anci","ance"},
            {"alli","al"},
            {"izer","ize"},
            {"enci","ence"},
            {"ator","ate"},
            {"bli","ble"},
            {"ogi","og"},
            {"li",""}
        };

        string[,] step3Replacements =
            {
            {"ational","ate"},
            {"tional","tion"},
            {"alize","al"},
            {"icate","ic"},
            {"iciti","ic"},
            {"ative",""},
            {"ical","ic"},
            {"ness",""},
            {"ful",""}
        };

        string[] step4Replacements =
                {"ement",
            "ment",
            "able",
            "ible",
            "ance",
            "ence",
            "ate",
            "iti",
            "ion",
            "ize",
            "ive",
            "ous",
            "ant",
            "ism",
            "ent",
            "al",
            "er",
            "ic"
        };

        string[,] exceptions =
            {
        {"skis","ski"},
        {"skies","sky"},
        {"dying","die"},
        {"lying","lie"},
        {"tying","tie"},
        {"idly","idl"},
        {"gently","gentl"},
        {"ugly","ugli"},
        {"early","earli"},
        {"only","onli"},
        {"singly","singl"},
        {"sky","sky"},
        {"news","news"},
        {"howe","howe"},
        {"atlas","atlas"},
        {"cosmos","cosmos"},
        {"bias","bias"},
        {"andes","andes"}
        };

        string[] exceptions2 =
            {"inning","outing","canning","herring","earring","proceed",
            "exceed","succeed"};


        // A helper table lookup code - used for vowel lookup
        private bool arrayContains(string[] arr, string s)
        {
            for (int i = 0; i < arr.Length; ++i)
            {
                if (arr[i] == s) return true;
            }
            return false;
        }

        private bool isVowel(StringBuilder s, int offset)
        {
            switch (s[offset])
            {
                case 'a':
                case 'e':
                case 'i':
                case 'o':
                case 'u':
                case 'y':
                    return true;
                    break;
                default:
                    return false;
            }
        }

        private bool isShortSyllable(StringBuilder s, int offset)
        {
            if ((offset == 0) && (isVowel(s, 0)) && (!isVowel(s, 1)))
                return true;
            else
                if (
                    ((offset > 0) && (offset < s.Length - 1)) &&
                    isVowel(s, offset) && !isVowel(s, offset + 1) &&
                    (s[offset + 1] != 'w' && s[offset + 1] != 'x' && s[offset + 1] != 'Y')
                    && !isVowel(s, offset - 1))
                return true;
            else
                return false;
        }

        private bool isShortWord(StringBuilder s, int r1)
        {
            if ((r1 >= s.Length) && (isShortSyllable(s, s.Length - 2))) return true;

            return false;
        }

        private void changeY(StringBuilder sb)
        {
            if (sb[0] == 'y') sb[0] = 'Y';

            for (int i = 1; i < sb.Length; ++i)
            {
                if ((sb[i] == 'y') && (isVowel(sb, i - 1))) sb[i] = 'Y';
            }
        }

        private void computeR1R2(StringBuilder sb, ref int r1, ref int r2)
        {
            r1 = sb.Length;
            r2 = sb.Length;

            if ((sb.Length >= 5) && (sb.ToString(0, 5) == "gener" || sb.ToString(0, 5) == "arsen")) r1 = 5;
            if ((sb.Length >= 6) && (sb.ToString(0, 6) == "commun")) r1 = 6;

            if (r1 == sb.Length) // If R1 has not been changed by exception words
                for (int i = 1; i < sb.Length; ++i) // Compute R1 according to the algorithm
                {
                    if ((!isVowel(sb, i)) && (isVowel(sb, i - 1)))
                    {
                        r1 = i + 1;
                        break;
                    }
                }

            for (int i = r1 + 1; i < sb.Length; ++i)
            {
                if ((!isVowel(sb, i)) && (isVowel(sb, i - 1)))
                {
                    r2 = i + 1;
                    break;
                }
            }
        }

        private void step0(StringBuilder sb)
        {

            if ((sb.Length >= 3) && (sb.ToString(sb.Length - 3, 3) == "'s'"))
                sb.Remove(sb.Length - 3, 3);
            else
                if ((sb.Length >= 2) && (sb.ToString(sb.Length - 2, 2) == "'s"))
                sb.Remove(sb.Length - 2, 2);
            else
                    if (sb[sb.Length - 1] == '\'')
                sb.Remove(sb.Length - 1, 1);
        }

        private void step1a(StringBuilder sb)
        {

            if ((sb.Length >= 4) && sb.ToString(sb.Length - 4, 4) == "sses")
                sb.Replace("sses", "ss", sb.Length - 4, 4);
            else
                if ((sb.Length >= 3) && (sb.ToString(sb.Length - 3, 3) == "ied" || sb.ToString(sb.Length - 3, 3) == "ies"))
            {
                if (sb.Length > 4)
                    sb.Replace(sb.ToString(sb.Length - 3, 3), "i", sb.Length - 3, 3);
                else
                    sb.Replace(sb.ToString(sb.Length - 3, 3), "ie", sb.Length - 3, 3);
            }
            else
                    if ((sb.Length >= 2) && (sb.ToString(sb.Length - 2, 2) == "us" || sb.ToString(sb.Length - 2, 2) == "ss"))
                return;
            else
                        if ((sb.Length > 0) && (sb.ToString(sb.Length - 1, 1) == "s"))
            {
                for (int i = 0; i < sb.Length - 2; ++i)
                    if (isVowel(sb, i))
                    {
                        sb.Remove(sb.Length - 1, 1);
                        break;
                    }
            }

        }

        private void step1b(StringBuilder sb, int r1)
        {
            for (int i = 0; i < 6; ++i)
            {
                if ((sb.Length > step1bReplacements[i, 0].Length) && (sb.ToString(sb.Length - step1bReplacements[i, 0].Length, step1bReplacements[i, 0].Length) == step1bReplacements[i, 0]))
                {
                    switch (step1bReplacements[i, 0])
                    {
                        case "eedly":
                        case "eed":
                            if (sb.Length - step1bReplacements[i, 0].Length >= r1)
                                sb.Replace(step1bReplacements[i, 0], step1bReplacements[i, 1], sb.Length - step1bReplacements[i, 0].Length, step1bReplacements[i, 0].Length);
                            break;
                        default:
                            bool found = false;
                            for (int j = 0; j < sb.Length - step1bReplacements[i, 0].Length; ++j)
                            {
                                if (isVowel(sb, j))
                                {
                                    sb.Replace(step1bReplacements[i, 0], step1bReplacements[i, 1], sb.Length - step1bReplacements[i, 0].Length, step1bReplacements[i, 0].Length);
                                    found = true;
                                    break;
                                }
                            }
                            if (!found) return;
                            if (sb.Length >= 2)
                            {
                                switch (sb.ToString(sb.Length - 2, 2))
                                {
                                    case "at":
                                    case "bl":
                                    case "iz":
                                        sb.Append("e");
                                        return;
                                }
                                if (arrayContains(doubles, sb.ToString(sb.Length - 2, 2)))
                                {
                                    sb.Remove(sb.Length - 1, 1);
                                    return;
                                }
                            }
                            if (isShortWord(sb, r1))
                                sb.Append("e");
                            break;
                    }
                    return;
                }
            }
        }

        private void step1c(StringBuilder sb)
        {
            if ((sb.Length > 0) &&
                (sb[sb.Length - 1] == 'y' || sb[sb.Length - 1] == 'Y') &&
                (sb.Length > 2) && (!isVowel(sb, sb.Length - 2))
               )
                sb[sb.Length - 1] = 'i';
        }

        private void step2(StringBuilder sb, int r1)
        {
            for (int i = 0; i < 24; ++i)
            {
                if (
                    (sb.Length >= step2Replacements[i, 0].Length) &&
                    (sb.ToString(sb.Length - step2Replacements[i, 0].Length, step2Replacements[i, 0].Length) == step2Replacements[i, 0])
                    )
                {
                    if (sb.Length - step2Replacements[i, 0].Length >= r1)
                    {
                        switch (step2Replacements[i, 0])
                        {
                            case "ogi":
                                if ((sb.Length > 3) &&
                                    (sb[sb.Length - step2Replacements[i, 0].Length - 1] == 'l')
                                    )
                                    sb.Replace(step2Replacements[i, 0], step2Replacements[i, 1], sb.Length - step2Replacements[i, 0].Length, step2Replacements[i, 0].Length);
                                return;
                            case "li":
                                if ((sb.Length > 1) &&
                                    (arrayContains(validLiEndings, sb.ToString(sb.Length - 3, 1)))
                                    )
                                    sb.Remove(sb.Length - 2, 2);
                                return;
                            default:
                                sb.Replace(step2Replacements[i, 0], step2Replacements[i, 1], sb.Length - step2Replacements[i, 0].Length, step2Replacements[i, 0].Length);
                                return;
                                break;

                        }
                    }
                    else
                        return;
                }
            }
        }

        private void step3(StringBuilder sb, int r1, int r2)
        {
            for (int i = 0; i < 9; ++i)
            {
                if (
                    (sb.Length >= step3Replacements[i, 0].Length) &&
                    (sb.ToString(sb.Length - step3Replacements[i, 0].Length, step3Replacements[i, 0].Length) == step3Replacements[i, 0])
                    )
                {
                    if (sb.Length - step3Replacements[i, 0].Length >= r1)
                    {
                        switch (step3Replacements[i, 0])
                        {
                            case "ative":
                                if (sb.Length - step3Replacements[i, 0].Length >= r2)
                                    sb.Replace(step3Replacements[i, 0], step3Replacements[i, 1], sb.Length - step3Replacements[i, 0].Length, step3Replacements[i, 0].Length);
                                return;
                            default:
                                sb.Replace(step3Replacements[i, 0], step3Replacements[i, 1], sb.Length - step3Replacements[i, 0].Length, step3Replacements[i, 0].Length);
                                return;
                        }
                    }
                    else return;
                }
            }
        }

        private void step4(StringBuilder sb, int r2)
        {
            for (int i = 0; i < 18; ++i)
            {
                if (
                    (sb.Length >= step4Replacements[i].Length) &&
                    (sb.ToString(sb.Length - step4Replacements[i].Length, step4Replacements[i].Length) == step4Replacements[i])                    // >=
                    )
                {
                    if (sb.Length - step4Replacements[i].Length >= r2)
                    {
                        switch (step4Replacements[i])
                        {
                            case "ion":
                                if (
                                    (sb.Length > 3) &&
                                    (
                                        (sb[sb.Length - step4Replacements[i].Length - 1] == 's') ||
                                        (sb[sb.Length - step4Replacements[i].Length - 1] == 't')
                                    )
                                   )
                                    sb.Remove(sb.Length - step4Replacements[i].Length, step4Replacements[i].Length);
                                return;
                            default:
                                sb.Remove(sb.Length - step4Replacements[i].Length, step4Replacements[i].Length);
                                return;
                        }
                    }
                    else
                        return;
                }
            }

        }

        private void step5(StringBuilder sb, int r1, int r2)
        {
            if (sb.Length > 0)
                if (
                    (sb[sb.Length - 1] == 'e') &&
                    (
                        (sb.Length - 1 >= r2) ||
                        ((sb.Length - 1 >= r1) && (!isShortSyllable(sb, sb.Length - 3)))
                    )
                   )
                    sb.Remove(sb.Length - 1, 1);
                else
                    if (
                        (sb[sb.Length - 1] == 'l') &&
                            (sb.Length - 1 >= r2) &&
                            (sb[sb.Length - 2] == 'l')
                        )
                    sb.Remove(sb.Length - 1, 1);
        }

        public string stem(string word)
        {
            if (word.Length < 3) return word;

            StringBuilder sb = new StringBuilder(word.ToLower());

            if (sb[0] == '\'') sb.Remove(0, 1);

            for (int i = 0; i < exceptions.Length / 2; ++i)
                if (word == exceptions[i, 0])
                    return exceptions[i, 1];

            int r1 = 0, r2 = 0;
            changeY(sb);
            computeR1R2(sb, ref r1, ref r2);

            step0(sb);
            step1a(sb);

            for (int i = 0; i < exceptions2.Length; ++i)
                if (sb.ToString() == exceptions2[i])
                    return exceptions2[i];

            step1b(sb, r1);
            step1c(sb);
            step2(sb, r1);
            step3(sb, r1, r2);
            step4(sb, r2);
            step5(sb, r1, r2);


            return sb.ToString().ToLower();
        }
    }
}
