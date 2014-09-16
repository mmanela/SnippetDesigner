// Copyright (C) Microsoft Corporation. All rights reserved.


namespace Microsoft.SnippetDesigner
{
    /// <summary>
    /// Represents a point in the TextBuffer
    /// </summary>
    public class TextPoint
    {
        private int bufferLine;
        private int lineIndex;


        /// <summary>
        ///  The line in the buffer
        /// </summary>
        public int Line
        {
            get
            {
                return bufferLine;
            }
            set
            {
                bufferLine = value;
            }
        }

        /// <summary>
        /// The index into the line
        /// </summary>
        public int Index
        {
            get
            {
                return lineIndex;
            }
            set
            {
                lineIndex = value;
            }
        }

        public TextPoint(int line, int index)
        {
            bufferLine = line;
            lineIndex = index;

        }


        public TextPoint()
        {
            bufferLine = 0;
            lineIndex = 0;

        }

        /// <summary>
        /// Set one text point equal to the other
        /// </summary>
        /// <param name="point"></param>
        public void SetEqualTo(TextPoint point)
        {
            this.lineIndex = point.Index;
            this.bufferLine = point.Line;

        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        // Override the Object.Equals(object o) method:
        public override bool Equals(object obj)
        {
            if(obj == null)
            {
                return false;
            }
            try
            {
                TextPoint point2 = obj as TextPoint;
                if (point2 == null)
                {
                    return false;
                }

                return (bool)(Index == point2.Index && Line == point2.Line);
            }
            catch
            {
                return false;
            }
        }




        // Overloading '<' operator:
        public static bool operator <(TextPoint point1, TextPoint point2)
        {
            return (point1.Index < point2.Index && point1.Line <= point2.Line);
        }

        // Overloading '>' operator:
        public static bool operator >(TextPoint point1, TextPoint point2)
        {
            return (point1.Index > point2.Index && point1.Line >= point2.Line);
        }
    }
}
