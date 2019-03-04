using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace Core.Text
{
    public partial class CharacterSeparatedValues
    {
        public CharacterSeparatedValues()
        {
            Separators = new string[] { ",", ";", " ", @"\t", };
            SeparatorsNewLine = new string[] { Environment.NewLine, @"\n" };
            CommentStrings = new string[] { "#", "//" };
            HasHeader = false;
            NumberFormatInfo = CultureInfo.CurrentCulture.NumberFormat;

            ContainedType = typeof((string Ivek, string Jozo));

            return;
        }

        public string Text
        {
            get;
            set;
        }

        public IEnumerable<string> ColumnNames
        {
            get;
            set;
        }

        public string[] Separators 
        { 
            get; 
            set; 
        }

        public string[] SeparatorsNewLine
        {
            get;
            set;
        }

        public string[] CommentStrings 
        { 
            get; 
            set; 
        }

        public bool HasHeader 
        { 
            get; 
            set; 
        }

        public NumberFormatInfo NumberFormatInfo
        {
            get;
            set;
        }

        public Func
                <
                    string, 
                    IEnumerable<string>, 
                    NumberFormatInfo, 
                    string[], 
                    IEnumerable<string[]>
                > ParseMethod
        {
            get;
            set;
        }

    }
}