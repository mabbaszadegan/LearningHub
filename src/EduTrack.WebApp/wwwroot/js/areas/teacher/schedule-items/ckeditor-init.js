(async () => {
    let mod;
    try {
        // Prefer import map specifier
        mod = await import('ckeditor5');
    } catch (e) {
        console.warn('Import map for ckeditor5 not resolved, falling back to absolute path.', e);
        mod = await import('/lib/ckeditor/ckeditor5.js');
    }

    const {
        ClassicEditor,
        Essentials,
        Paragraph,
        Bold,
        Italic,
        Font,
        Image,
        ImageCaption,
        ImageResize,
        ImageStyle,
        ImageToolbar,
        ImageUpload,
        LinkImage,
        SourceEditing,
        Base64UploadAdapter,
        Autoformat,
        BlockQuote,
        Heading,
        List,
        Table,
        TableToolbar,
        Clipboard,
        MediaEmbed,
        GeneralHtmlSupport,
        HtmlEmbed
    } = mod;

    // Expose for legacy code paths
    window.ClassicEditor = ClassicEditor;
    window.CKEditorPlugins = {
        Essentials,
        Paragraph,
        Bold,
        Italic,
        Font,
        Image,
        ImageCaption,
        ImageResize,
        ImageStyle,
        ImageToolbar,
        ImageUpload,
        LinkImage,
        SourceEditing,
        Base64UploadAdapter,
        Autoformat,
        BlockQuote,
        Heading,
        List,
        Table,
        TableToolbar,
        Clipboard,
        MediaEmbed,
        GeneralHtmlSupport,
        HtmlEmbed
    };

    // Initialize description editor if container exists
    const initializeDescriptionEditor = () => {
        const descriptionEditorElement = document.querySelector('#descriptionEditorContainer');
        if (!descriptionEditorElement) return;

        ClassicEditor.create(descriptionEditorElement, {
            licenseKey: 'GPL',
            language: {
                ui: 'fa',
                content: 'fa'
            },
            plugins: [
                Base64UploadAdapter, Essentials, Paragraph, Bold, Italic, Font,
                Image, ImageToolbar, ImageUpload, ImageCaption, ImageStyle, ImageResize,
                LinkImage, SourceEditing, Autoformat, BlockQuote, Heading, List,
                Table, TableToolbar, Clipboard, MediaEmbed,
                GeneralHtmlSupport, HtmlEmbed
            ],
            toolbar: [
                'undo', 'redo', '|', 'bold', 'italic', '|',
                'fontSize', 'fontFamily', 'fontColor', 'fontBackgroundColor', '|',
                'imageUpload', 'blockQuote', 'numberedList', 'bulletedList', '|',
                'heading', 'mediaEmbed', '|', 'sourceEditing'
            ],
            image: { toolbar: [ 'toggleImageCaption', 'imageTextAlternative' ] },
            htmlSupport: {
                allow: [
                    {
                        name: /^(div|span|p|h[1-6]|img|table|thead|tbody|tr|td|th|ul|ol|li|a|blockquote|hr|pre|code)$/i,
                        attributes: true,
                        classes: true,
                        styles: {
                            'text-align': true,
                            'direction': true,
                            'color': true,
                            'background-color': true,
                            'font-size': true,
                            'font-weight': true,
                            'font-style': true,
                            'text-decoration': true,
                            'white-space': true,
                            'width': true,
                            'height': true,
                            'max-width': true,
                            'max-height': true,
                            'margin': true,
                            'margin-left': true,
                            'margin-right': true,
                            'margin-top': true,
                            'margin-bottom': true,
                            'padding': true,
                            'padding-left': true,
                            'padding-right': true,
                            'padding-top': true,
                            'padding-bottom': true,
                            'border': true,
                            'border-left': true,
                            'border-right': true,
                            'border-top': true,
                            'border-bottom': true,
                            'border-color': true,
                            'border-width': true,
                            'border-style': true,
                            'float': true,
                            'display': true
                        }
                    },
                    {
                        name: 'img',
                        attributes: { 'src': true, 'alt': true, 'title': true, 'width': true, 'height': true, 'style': true },
                        classes: true,
                        styles: true
                    },
                    {
                        name: 'a',
                        attributes: { 'href': true, 'target': true, 'rel': true, 'title': true },
                        classes: true,
                        styles: true
                    }
                ]
            },
            link: {
                addTargetToExternalLinks: true,
                decorators: [ { mode: 'manual', label: 'Nofollow', attributes: { rel: 'nofollow' }, defaultValue: false } ]
            }
        }).then(editor => {
            window.descriptionEditor = editor;
            const hiddenTextarea = document.getElementById('descriptionHidden');
            if (hiddenTextarea) {
                if (hiddenTextarea.value) editor.setData(hiddenTextarea.value);
                editor.model.document.on('change:data', () => {
                    hiddenTextarea.value = editor.getData();
                    try {
                        // Trigger native input event so global change detection picks it up
                        const evt = new Event('input', { bubbles: true, cancelable: true });
                        hiddenTextarea.dispatchEvent(evt);
                    } catch {}
                    // Also directly mark step 1 as changed if form manager exists
                    if (window.scheduleItemForm && typeof window.scheduleItemForm.markStepAsChanged === 'function') {
                        window.scheduleItemForm.markStepAsChanged(1);
                    }
                });
            }
        }).catch(console.error);
    };

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initializeDescriptionEditor);
    } else {
        initializeDescriptionEditor();
    }
})();
