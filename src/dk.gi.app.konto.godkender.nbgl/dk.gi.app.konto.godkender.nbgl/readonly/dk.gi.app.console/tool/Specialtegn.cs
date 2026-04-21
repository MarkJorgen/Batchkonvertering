using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dk.gi.app
{
    /// <summary>
    /// Hjælpefunktioner til at konvertere special tegn til (Html/XML) Entity Navn
    /// </summary>
    public static class Specialtegn
    {
        /// <summary>
        /// Liste af integer værdi af tegn og det tilførende Html/xml navn
        /// Denne liste er sammensat af lidt forskellige tegnsæt, Dels fra ASCII Tabellen og så fra Latin 1 - Her er kun de første 255 tegn (kun special tegn ikke alm bogstaver og tal)
        /// Det rigtige havde været og lave en komplet liste med alle UTF 8 Tegn, men det er lidt tidskrævende
        /// Se: https://unicode-table.com/en/html-entities/
        /// Tabel er <int>,<<string>
        /// </summary>
        public static readonly Hashtable dkSpecialTegn = new Hashtable()
        {
        #region Values
        {32, "&sp;"},
        {33, "&excl;"},
        {34, "&quot;"},
        {35, "&num;"},
        {36, "&dollar;"},
        {37, "&percnt;"},
        {38, "&amp;"},
        {39, "&apos;"},
        {40, "&lpar;"},
        {41, "&rpar;"},
        {42, "&ast;"},
        {43, "&plus;"},
        {44, "&comma;"},
        {45, "&minus;"},
        {46, "&period;"},
        {47, "&sol;"},
        {58, "&colon;"},
        {59, "&semi;"},
        {60, "&lt;"},
        {61, "&equals;"},
        {62, "&gt;"},
        {63, "&quest;"},
        {64, "&commat;"},
        {91, "&lsqb;"},
        {92, "&bsol;"},
        {93, "&rsqb;"},
        {94, "&circ;"},
        {95, "&lowbar;"},
        {96, "&grave;"},
        {123, "&lcub;"},
        {124, "&verbar;"},
        {125, "&rcub;"},
        {126, "&tilde;"},
        {160, "&nbsp;"},
        {161, "&iexcl;"},
        {162, "&cent;"},
        {163, "&pound;"},
        {164, "&curren;"},
        {165, "&yen;"},
        {166, "&brkbar;"},
        {167, "&sect;"},
        {168, "&uml;"},
        {169, "&copy;"},
        {170, "&ordf;"},
        {171, "&laquo;"},
        {172, "&not;"},
        {173, "&shy;"},
        {174, "&reg;"},
        {175, "&macr;"},
        {176, "&deg;"},
        {177, "&plusmn;"},
        {178, "&sup2;"},
        {179, "&sup3;"},
        {180, "&acute;"},
        {181, "&micro;"},
        {182, "&para;"},
        {183, "&middot;"},
        {184, "&cedil;"},
        {185, "&sup1;"},
        {186, "&ordm;"},
        {187, "&raquo;;"},
        {188, "&frac14;"},
        {189, "&frac12;"},
        {190, "&frac34;"},
        {191, "&iquest;"},
        {192, "&Agrave;"},
        {193, "&Aacute;"},
        {194, "&Acirc;"},
        {195, "&Atilde;"},
        {196, "&Auml;"},
        {197, "&Aring;"},
        {198, "&AElig;"},
        {199, "&Ccedil;"},
        {200, "&Egrave;"},
        {201, "&Eacute;"},
        {202, "&Ecirc;"},
        {203, "&Euml;"},
        {204, "&Igrave;"},
        {205, "&Iacute;"},
        {206, "&Icirc;"},
        {207, "&Iuml;"},
        {208, "&ETH;"},
        {209, "&Ntilde;"},
        {210, "&Ograve;"},
        {211, "&Oacute;"},
        {212, "&Ocirc;"},
        {213, "&Otilde;"},
        {214, "&Ouml;"},
        {215, "&times;"},
        {216, "&Oslash;"},
        {217, "&Ugrave;;"},
        {218, "&Uacute;"},
        {219, "&Ucirc;"},
        {220, "&Uuml;"},
        {221, "&Yacute;"},
        {222, "&THORN;"},
        {223, "&szlig;"},
        {224, "&agrave;"},
        {225, "&aacute;"},
        {226, "&acirc;"},
        {227, "&atilde;"},
        {228, "&auml;"},
        {229, "&aring;"},
        {230, "&aelig;"},
        {231, "&ccedil;"},
        {232, "&egrave;"},
        {233, "&eacute;"},
        {234, "&ecirc;"},
        {235, "&euml;"},
        {236, "&igrave;"},
        {237, "&iacute;"},
        {238, "&icirc;"},
        {239, "&iuml;"},
        {240, "&eth;"},
        {241, "&ntilde;"},
        {242, "&ograve;"},
        {243, "&oacute;"},
        {244, "&ocirc;"},
        {245, "&otilde;"},
        {246, "&ouml;"},
        {247, "&divide;"},
        {248, "&oslash;"},
        {249, "&ugrave;"},
        {250, "&uacute;"},
        {251, "&ucirc;"},
        {252, "&uuml;"},
        {253, "&yacute;"},
        {254, "&thorn;"},
        {255, "&yuml;"},
#endregion
        };

        /// <summary>
        /// Omdan et tal til det tilhørende navn
        /// </summary>
        /// <param name="dkTegn">Byte for tegn (max er derfor 255)</param>
        /// <returns>Html/Xml navn</returns>
        public static string EncodeToHtmlEntityName(int dkTegn)
        {
            // N.B. Pas på det er integer i tabellen
            if (dkSpecialTegn.ContainsKey(dkTegn) == true)
                return dkSpecialTegn[dkTegn].ToString();
            else
                return ((char)dkTegn).ToString();
        }
        //Test disse 2 funktioner inden de implementeres
        //public static string EncodeToHtmlEntityName(char dkTegn)
        //{
        //    // N.B. Pas på det er integer i tabellen
        //    return EncodeToHtmlEntityName((int)dkTegn);
        //}
        //public static string EncodeToHtmlEntityName(byte dkTegn)
        //{
        //    // N.B. Pas på det er integer i tabellen
        //    return EncodeToHtmlEntityName((int)dkTegn);
        //}


        /// <summary>
        /// Omdan en streng til en ny streng, hvor de enkelte tegn testes hver for sig og enten omdannes til et Entityname (f.eks. &amp;) eller forbliver enkelt tegn
        /// </summary>
        /// <param name="value">En tekst som skal konverteres</param>
        /// <returns>Efter konvertering</returns>
        public static string EncodeToHtmlEntityName(string value)
        {
            string result = string.Empty;
            foreach (char ch in value)
            {
                byte by = (byte)ch;  // Denne laver cast til byte, derfor vil dette ikke fungere med tegn som har en højere værdi end 255
                result += EncodeToHtmlEntityName(by);
            }
            return result;
        }

        /// <summary>
        /// Pak en string ud igen efter den har været pakket ind
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string DecodeFromHtmlEntityName(string value)
        {
            string result = value;
            foreach (DictionaryEntry item in dkSpecialTegn)
            {
                string searchValue = (string)item.Value;
                char ch = Char.ConvertFromUtf32((int)item.Key)[0];
                string replaceValue = ch.ToString();
                result = result.Replace(searchValue, replaceValue);
            }
            return result;
        }

        /// <summary>
        /// Konvertr et angivet EntityName (f.eks. &amp;) til et tegn
        /// </summary>
        /// <param name="value">Html/Xml Entity navn</param>
        /// <returns>char med tegn</returns>
        /// <exception cref="ArgumentOutOfRangeException">Hvis angivet value ikke kan konverteres</exception>
        public static char DecodeCharFromHtmlEntityName(string value)
        {
            char ch;
            if (dkSpecialTegn.ContainsValue(value) == true)
            {
                foreach (DictionaryEntry item in dkSpecialTegn)
                {
                    string searchValue = (string)item.Value;
                    if (searchValue == value)
                    {
                        ch = Char.ConvertFromUtf32((int)item.Key)[0];
                        return ch;
                    }
                }
            }
            throw new ArgumentOutOfRangeException($"Værdien '{value}' kan ikke konverteres til et tegn!");
        }
    }
}