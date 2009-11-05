using System.Xml;
using System.Xml.Serialization;

namespace Microsoft.SnippetDesigner.ContentTypes
{
    /// <summary>
    /// Represents a snippet meta data in the index file
    /// </summary>
    public class SnippetIndexItem
    {

        //metadata values
        private string file = null;
        private string title = null;
        private string author = null;
        private string description = null;
        private string keywords = null;
        private string language = null;
        private string code = null;
        private string dateAdded = null;
        private string userRating = null;
        private string averageRating = null;
        private string usesNum = null;


        #region public properties

        /// <summary>
        /// The file path to a local snippet or the unique 
        /// primary key id for an online snippet
        /// </summary>
        [XmlElement("File")]
        public string File
        {
            get
            {
                return file;
            }
            set
            {
                file = value;
            }
        }

        /// <summary>
        /// Title of the snippet
        /// </summary>
        [XmlElement("Title")]
        public string Title
        {
            get
            {
                return title;
            }
            set
            {
                title = value;
            }
        }

        /// <summary>
        /// Author of the snippet
        /// </summary>
        [XmlElement("Author")]
        public string Author
        {
            get
            {
                return author;
            }
            set
            {
                author = value;
            }
        }

        /// <summary>
        /// Description of the snippet
        /// </summary>
        [XmlElement("Description")]
        public string Description
        {
            get
            {
                return description;
            }
            set
            {
                description = value;
            }
        }

        /// <summary>
        /// Keywords which describe the snippet
        /// </summary>
        [XmlElement("Keywords")]
        public string Keywords
        {
            get
            {
                return keywords;
            }
            set
            {
                keywords = value;
            }
        }

        /// <summary>
        /// Code language of the snippet
        /// </summary>
        [XmlElement("Language")]
        public string Language
        {
            get
            {
                return language;
            }
            set
            {
                language = value;
            }
        }

        /// <summary>
        /// Code of the snippet
        /// </summary>
        [XmlElement("Code")]
        public string Code
        {
            get
            {
                return code;
            }
            set
            {
                code = value;
            }
        }


        /// <summary>
        /// Date snippet was added to the index
        /// </summary>
        [XmlElement("DateAdded")]
        public string DateAdded
        {
            get
            {
                return dateAdded;
            }
            set
            {
                dateAdded = value;
            }
        }


        /// <summary>
        /// Number of times snippet was used
        /// </summary>
        [XmlElement("UsesNum")]
        public string UsesNum
        {
            get
            {
                return usesNum;
            }
            set
            {
                usesNum = value;
            }
        }


        /// <summary>
        /// Rating given by the user
        /// </summary>
        [XmlElement("UserRating")]
        public string UserRating
        {
            get
            {
                return userRating;
            }
            set
            {
                userRating = value;
            }
        }


        /// <summary>
        /// Average rating of online users
        /// </summary>
        [XmlElement("AverageRating")]
        public string AverageRating
        {
            get
            {
                return averageRating;
            }
            set
            {
                averageRating = value;
            }
        }
        #endregion


        /// <summary>
        /// Default constructor doesnt do anything
        /// </summary>
        public SnippetIndexItem()
        {

        }










    }
}
