# Welcome to the Snippet Designer

The Snippet Designer is a plugin which enhances the Visual Studio IDE to allow a richer and more productive code snippet experience. 


## Update
The main branch targets only VS 2022. There is a branch (pre-2022) for other versions.

## Download
You can install the extension using the Visual Studio Extension Manager or by going to the marktplace

- [Snippet Designer 2022](https://marketplace.visualstudio.com/items?itemName=vs-publisher-2795.SnippetDesigner2022)
- [Snippet Designer Pre-2022](https://marketplace.visualstudio.com/items?itemName=vs-publisher-2795.SnippetDesigner)

## Recent News
* **4/29/2020** - Updated to support color themesand support for async extension model for VS 2019.
* **[Snippet Designer now supports C++](http://matthewmanela.com/blog/snippet-designer-now-supports-c/)**
* [Snippet Designer now supports Visual Studio 2012 RC](http://matthewmanela.com/blog/snippet-designer-now-supports-visual-studio-2012-rc/)
* [Snippet Designer 1.4.0 Released!](http://matthewmanela.com/blog/snippet-designer-1-4-0-released/)


## Content
* [Create a snippets from scratch](https://github.com/mmanela/SnippetDesginer/wiki/Creating-a-snippet-from-scratch)
* [Managing existing snippets](https://github.com/mmanela/SnippetDesginer/wiki/Manage-Existing-Snippets)
* [Building the code](https://github.com/mmanela/SnippetDesginer/wiki/Building-the-code)

## Prerequisites 

* Visual Studio 2022
* Visual Studio 2019
* Visual Studio 2017
* Visual Studio 2015

## Features
 
A Snippet editor integrated inside of the IDE which supports **C#**, **Visual Basic**, **JavaScript**, **HTML**, **XML** and **SQL**
* Access it by opening any .snippet file or going to File -> New -> File -> Code Snippet File
* It uses the native Visual Studio code editor so that you can write the snippets in the same enviorment you write your code. 
* It lets you easily mark replacements by a convenient right click menu. 
* It displays properties of the snippet inside the Visual Studio properties window.

  ![](https://raw.githubusercontent.com/mmanela/SnippetDesginer/master/images/Editor.png)

A Snippet Explorer tool window to search snippets on your computer.  
* It is located under View -> Other Windows -> Snippet Explorer
* This tool window contains a code preview window which lets to peek inside the snippet to see what it is without opening the file.
* Maintains an index of snippets on your computer for quick searching.
* Provides a quick way to find a code snippet to use, edit or delete

![](https://raw.githubusercontent.com/mmanela/SnippetDesginer/master/images/Explorer.png)

A right Click "Export as Snippet" menu option added to C#, VB, XML, JavaScript, HTML and SQL code editors to send highlighted code directly to the Snippet Editor

 ![](https://raw.githubusercontent.com/mmanela/SnippetDesginer/master/images/Export.png)
