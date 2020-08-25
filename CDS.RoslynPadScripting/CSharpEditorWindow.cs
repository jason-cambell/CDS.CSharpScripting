﻿using ICSharpCode.AvalonEdit.Highlighting;
using RoslynPad.Editor;
using RoslynPad.Roslyn;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace CDS.RoslynPadScripting
{
    /// <summary>
    /// Editor control for C#, pre-configured with keywords, dark theme, etc.
    /// </summary>
    public partial class CSharpEditorWindow: UserControl
    {
        RoslynCodeEditor editor;


        /// <summary>
        /// The editor text.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Always)]
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [Bindable(true)]
        public override string Text
        {
            get
            {
                if (editor?.Document != null) { return editor.Document.Text; }
                else { return null; }
            }

            set
            {
                if (editor?.Document != null)
                {
                    editor.Document.Text = value;
                }
            }
        }


        /// <summary>
        /// Fired when the editor text has changed.
        /// </summary>
        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        public new event EventHandler TextChanged;


        public CSharpEditorWindow()
        {
            InitializeComponent();
        }

        private void CSEditor_Load(object sender, EventArgs e)
        {
            if(DesignMode) { return; }

        }


        public void Initialize(Type[] referenceTypes)
        {
            Initialize(referenceTypes: referenceTypes, globalsType: null);
        }


        public void Initialize(Type[] referenceTypes, Type globalsType)
        {
            editor = new RoslynCodeEditor();
            var workingDirectory = Directory.GetCurrentDirectory();

            var referenceTypesIncludingGlobalsType = referenceTypes;
            if (globalsType != null)
            {
                var temp = referenceTypes.ToList();
                temp.Add(globalsType);
                referenceTypesIncludingGlobalsType = temp.ToArray();
            }

            var namespaceImports =
                RoslynHostReferences
                .Empty
                .With(assemblyReferences: new[] { typeof(int).Assembly })
                .With(typeNamespaceImports: referenceTypesIncludingGlobalsType);

            var roslynHost = new CustomRoslynHost(
                globalsType: globalsType,
                additionalAssemblies: new[]
                {
                    Assembly.Load("RoslynPad.Roslyn.Windows"),
                    Assembly.Load("RoslynPad.Editor.Windows"),
                },
                references: namespaceImports);

            editor.Initialize(roslynHost, new ClassificationHighlightColors(), workingDirectory, "");
            editor.FontFamily = new System.Windows.Media.FontFamily("Consolas");
            editor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("C#");
            editor.FontSize = 12.75f;
            wpfEditorHost.Child = editor;
            this.Controls.Add(wpfEditorHost);

            editor.TextChanged += Editor_TextChanged;
        }

        private void Editor_TextChanged(object sender, EventArgs e)
        {
            TextChanged?.Invoke(sender, e);
        }
    }
}
