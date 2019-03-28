using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Text
{
    public partial class CharacterSeparatedValues
    {
        public IEnumerable<IEnumerable<string>> ParseUsingString
                                                        (
                                                            char column_delimiter,
                                                            string row_delimiter
                                                        )
        {
            return this.ParseUsingString(column_delimiter, row_delimiter);
        }


        public Type ContainedType
        {
            get;
            set;
        }

        public IEnumerable<string[]> ParseTemporaryImplementation(bool has_header = false)
        {
            string[] lines = Text.Split
                                        (
                                            new string[] { Environment.NewLine, @"\n" },
                                            StringSplitOptions.None
                                        );
            int index_start = 0;
            if (has_header)
            {
                index_start = 1;
            }


            for (int i = index_start; i < lines.Length; i++)
            {
                string[] columns = lines[i].Split
                                        (
                                            new char[] { ',' },
                                            StringSplitOptions.None
                                        );

                yield return columns;
            }
        }

        public delegate IEnumerable<RowType> TransformationMethod<RowType>(IEnumerable<string[]> untyped_data);

        protected TransformationMethod<string[]> TransformationDefault;

        public IEnumerable<string[]> Transformation(IEnumerable<string[]> untyped_data)
        {
            return untyped_data;
        }
    }
}