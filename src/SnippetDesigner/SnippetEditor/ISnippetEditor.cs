using Microsoft.SnippetLibrary;

namespace Microsoft.SnippetDesigner
{
    /// <summary>
    /// These properties define the interaction with the snippet codeWindowHost gui
    /// each of these represents a specidfc field in the snippter schema and each 
    /// of thesse properties retrives from the GUI or sets the value to the gui
    /// </summary>
    public interface ISnippetEditor
    {
        #region Properties

        /// <summary>
        /// File name of the snippet
        /// </summary>
        string SnippetFileName { get; }

        /// <summary>
        /// Get the list of snippet titles form the codeWindowHost
        /// Set the list of items in the codeWindowHost
        /// </summary>
        CollectionWithEvents<string> SnippetTitles { get; set; }


        /// <summary>
        /// Return the title of the snippet from the gui
        /// Set the title of the snippet in the gui
        /// </summary>
        string SnippetTitle { get; set; }

        /// <summary>
        /// Return the description of the snippet from the gui
        /// Set the description of the snippet in the gui
        /// </summary>
        string SnippetDescription { get; set; }

        /// <summary>
        /// Return the author of the snippet from the gui
        /// Set the author of the snippet in the gui
        /// </summary>
        string SnippetAuthor { get; set; }

        /// <summary>
        /// Return the help url of the snippet from the gui
        /// Set the help url of the snippet in the gui
        /// </summary>
        string SnippetHelpUrl { get; set; }

        /// <summary>
        /// Return the shortcut for the snippet from the gui
        /// Set the shortcut of the snippet in the gui
        /// </summary>
        string SnippetShortcut { get; set; }

        /// <summary>
        /// Return the cope of the snippet from the gui
        /// Set the code of the snippet in the gui
        /// </summary>
        string SnippetCode { get; set; }

        /// <summary>
        /// Return the list of keywords of the snippet from the gui
        /// Set the keywords of the snippet in the gui
        /// </summary>
        CollectionWithEvents<string> SnippetKeywords { get; set; }

        /// <summary>
        /// Return the list of types of the snippet from the gui
        /// Set the types of the snippet in the gui
        /// </summary>
        CollectionWithEvents<SnippetType> SnippetTypes { get; set; }

        CollectionWithEvents<AlternativeShortcut> SnippetAlternativeShortcuts { get; set; }

        /// <summary>
        /// Return the kind of the snippet from the gui
        /// Set the kind of the snippet in the gui
        /// </summary>
        string SnippetKind { get; set; }

        /// <summary>
        /// The delimiter used to indicate replacements in snippets
        /// </summary>
        string SnippetDelimiter { get; set; }

        /// <summary>
        /// Return the language of the snippet from the gui
        /// Set the language of the snippet in the gui
        /// </summary>
        string SnippetLanguage { get; set; }

        /// <summary>
        /// Return the list of imports of the snippet from the gui
        /// Set the imports of the snippet in the gui
        /// </summary>
        CollectionWithEvents<string> SnippetImports { get; set; }

        /// <summary>
        /// Return the list of references of the snippet from the gui
        /// Set the references of the snippet in the gui
        /// </summary>
        CollectionWithEvents<string> SnippetReferences { get; set; }

        /// <summary>
        /// Return the list of replacements of the snippet from the gui
        /// Set the replacements of the snippet in the gui
        /// </summary>
        CollectionWithEvents<Literal> SnippetReplacements { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Load a snippet from file
        /// </summary>
        /// <param name="fileName">Name of file</param>
        /// <returns>true if success</returns>
        bool LoadSnippet(string fileName);

        /// <summary>
        /// Save the current snippet to file under its current file name
        /// </summary>
        /// <returns>true if success</returns>
        bool SaveSnippet();

        /// <summary>
        /// Save a snippet using the specified filename
        /// </summary>
        /// <param name="fileName">the name of the file</param>
        /// <returns>true if success</returns>
        bool SaveSnippetAs(string fileName);

        /// <summary>
        /// Takes data from in memory snippet file and populates the gui form
        /// </summary>
        void PullFieldsFromActiveSnippet();


        /// <summary>
        /// Takes the data from the form and adds it to the in memory xml document
        /// </summary>
        void PushFieldsIntoActiveSnippet();

        #endregion
    }
}