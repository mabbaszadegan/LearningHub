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
        Underline,
        Font,
        Image,
        ImageCaption,
        ImageResize,
        ImageStyle,
        ImageToolbar,
        SourceEditing,
        Autoformat,
        BlockQuote,
        Heading,
        List,
        Alignment,
        Table,
        TableToolbar,
        Clipboard,
        GeneralHtmlSupport,
        HtmlEmbed,
        Plugin,
        Command,
        ButtonView
    } = mod;

    // Expose for legacy code paths
    window.ClassicEditor = ClassicEditor;
    window.CKEditorPlugins = {
        Essentials,
        Paragraph,
        Bold,
        Italic,
        Underline,
        Font,
        Image,
        ImageCaption,
        ImageResize,
        ImageStyle,
        ImageToolbar,
        SourceEditing,
        Autoformat,
        BlockQuote,
        Heading,
        List,
        Alignment,
        Table,
        TableToolbar,
        Clipboard,
        GeneralHtmlSupport,
        HtmlEmbed
    };

    if (Plugin && Command) {
        window.CKEditorCore = { Plugin, Command };
    } else {
        console.warn('CKEditor core classes not exposed; direction tools may be unavailable.');
    }

    if (ButtonView) {
        window.CKEditorUI = { ButtonView };
    } else {
        console.warn('CKEditor ButtonView not available; direction tools may be unavailable.');
    }

    const textDirectionIcons = {
        rtl: `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 20 20" role="img" focusable="false"><path d="M16 4.5a1 1 0 0 0-1-1H7.5a2.5 2.5 0 0 0 0 5H11v8a1 1 0 1 0 2 0v-8h2a1 1 0 0 0 1-1v-3zM11 7.5H7.5a0.5 0.5 0 0 1 0-1H11v1zM5.7 10.3a1 1 0 0 0-1.4 1.4L5.59 13H3a1 1 0 1 0 0 2h2.59l-1.29 1.3a1 1 0 0 0 1.42 1.4l3-3a1 1 0 0 0 0-1.4l-3-3z"/></svg>`,
        ltr: `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 20 20" role="img" focusable="false"><path d="M4 4.5a1 1 0 0 1 1-1h7.5a2.5 2.5 0 0 1 0 5H9v8a1 1 0 1 1-2 0v-8H5a1 1 0 0 1-1-1v-3zM9 7.5h3.5a0.5 0.5 0 0 0 0-1H9v1zM14.3 10.3a1 1 0 0 1 1.4 1.4L14.41 13H17a1 1 0 1 1 0 2h-2.59l1.29 1.3a1 1 0 1 1-1.42 1.4l-3-3a1 1 0 0 1 0-1.4l3-3z"/></svg>`
    };

    const alignmentIcons = {
        left: `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 20 20" role="img" focusable="false"><path d="M3 4.5a1 1 0 0 1 1-1h12a1 1 0 1 1 0 2H4a1 1 0 0 1-1-1zM3 8.5a1 1 0 0 1 1-1h8a1 1 0 1 1 0 2H4a1 1 0 0 1-1-1zM3 12.5a1 1 0 0 1 1-1h12a1 1 0 1 1 0 2H4a1 1 0 0 1-1-1zM3 16.5a1 1 0 0 1 1-1h8a1 1 0 1 1 0 2H4a1 1 0 0 1-1-1z"/></svg>`,
        center: `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 20 20" role="img" focusable="false"><path d="M3 4.5a1 1 0 0 1 1-1h12a1 1 0 1 1 0 2H4a1 1 0 0 1-1-1zM5 8.5a1 1 0 0 1 1-1h8a1 1 0 1 1 0 2H6a1 1 0 0 1-1-1zM3 12.5a1 1 0 0 1 1-1h12a1 1 0 1 1 0 2H4a1 1 0 0 1-1-1zM5 16.5a1 1 0 0 1 1-1h8a1 1 0 1 1 0 2H6a1 1 0 0 1-1-1z"/></svg>`,
        right: `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 20 20" role="img" focusable="false"><path d="M3 4.5a1 1 0 0 1 1-1h12a1 1 0 1 1 0 2H4a1 1 0 0 1-1-1zM7 8.5a1 1 0 0 1 1-1h8a1 1 0 1 1 0 2H8a1 1 0 0 1-1-1zM3 12.5a1 1 0 0 1 1-1h12a1 1 0 1 1 0 2H4a1 1 0 0 1-1-1zM7 16.5a1 1 0 0 1 1-1h8a1 1 0 1 1 0 2H8a1 1 0 0 1-1-1z"/></svg>`,
        justify: `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 20 20" role="img" focusable="false"><path d="M3 4.5a1 1 0 0 1 1-1h12a1 1 0 1 1 0 2H4a1 1 0 0 1-1-1zM3 8.5a1 1 0 0 1 1-1h12a1 1 0 1 1 0 2H4a1 1 0 0 1-1-1zM3 12.5a1 1 0 0 1 1-1h12a1 1 0 1 1 0 2H4a1 1 0 0 1-1-1zM3 16.5a1 1 0 0 1 1-1h12a1 1 0 1 1 0 2H4a1 1 0 0 1-1-1z"/></svg>`
    };

    const ensureCustomPlugins = () => {
        if (!window.CKEditorPlugins) {
            console.warn('CKEditor plugins container not available.');
            return;
        }

        const core = window.CKEditorCore;
        const ui = window.CKEditorUI;

        if (!core?.Plugin || !core?.Command) {
            console.warn('CKEditor custom plugins unavailable: core classes missing.');
            return;
        }

        if (!ui?.ButtonView) {
            console.warn('CKEditor custom plugins unavailable: ButtonView missing.');
            return;
        }

        const { Plugin: BasePlugin, Command: BaseCommand } = core;
        const ButtonCtor = ui.ButtonView;

        if (!window.CKEditorPlugins.TextDirection) {
            class SetTextDirectionCommand extends BaseCommand {
                execute(options = {}) {
                    const direction = options.value || null;
                    const editor = this.editor;
                    editor.model.change(writer => {
                        const selection = editor.model.document.selection;
                        const blocks = Array.from(selection.getSelectedBlocks());
                        const applyToBlocks = blocks.length > 0 ? blocks : [ selection.getFirstPosition()?.parent ].filter(Boolean);

                        applyToBlocks.forEach(block => {
                            if (!block || !editor.model.schema.checkAttribute(block, 'textDirection')) {
                                return;
                            }
                            if (direction) {
                                writer.setAttribute('textDirection', direction, block);
                            } else {
                                writer.removeAttribute('textDirection', block);
                            }
                        });
                    });
                }

                refresh() {
                    const editor = this.editor;
                    const selection = editor.model.document.selection;
                    const schema = editor.model.schema;

                    const blocks = Array.from(selection.getSelectedBlocks());
                    const primaryBlock = blocks.length > 0 ? blocks[0] : selection.getFirstPosition()?.parent || null;
                    const hasValidBlock = blocks.some(block => schema.checkAttribute(block, 'textDirection'));

                    this.isEnabled = hasValidBlock || schema.checkAttribute(selection.getFirstPosition()?.parent || null, 'textDirection');
                    this.value = primaryBlock && schema.checkAttribute(primaryBlock, 'textDirection')
                        ? primaryBlock.getAttribute('textDirection') || null
                        : null;
                }
            }

            class TextDirectionPlugin extends BasePlugin {
                init() {
                    const editor = this.editor;
                    editor.model.schema.extend('$block', { allowAttributes: 'textDirection' });
                    editor.model.schema.setAttributeProperties?.('textDirection', { isFormatting: true });

                    editor.conversion.attributeToAttribute({
                        model: {
                            key: 'textDirection',
                            values: ['rtl', 'ltr']
                        },
                        view: {
                            rtl: { key: 'dir', value: 'rtl' },
                            ltr: { key: 'dir', value: 'ltr' }
                        }
                    });

                    editor.commands.add('setTextDirection', new SetTextDirectionCommand(editor));

                    const command = editor.commands.get('setTextDirection');

                    const createButton = (name, value, label, icon) => {
                        editor.ui.componentFactory.add(name, locale => {
                            const button = new ButtonCtor(locale);
                            button.set({
                                label: label,
                                ariaLabel: label,
                                withText: false,
                                tooltip: true,
                                icon
                            });

                            button.bind('isOn').to(command, 'value', current => current === value);
                            button.bind('isEnabled').to(command, 'isEnabled');

                            button.on('execute', () => {
                                const current = command.value;
                                const nextValue = current === value ? null : value;
                                editor.execute('setTextDirection', { value: nextValue });
                                editor.editing.view.focus();
                            });

                            return button;
                        });
                    };

                    createButton('directionRtl', 'rtl', 'متن راست‌به‌چپ', textDirectionIcons.rtl);
                    createButton('directionLtr', 'ltr', 'متن چپ‌به‌راست', textDirectionIcons.ltr);
                }
            }

            window.CKEditorPlugins.TextDirection = TextDirectionPlugin;
        }

        if (!window.CKEditorPlugins.CustomAlignmentButtons) {
            class CustomAlignmentButtonsPlugin extends BasePlugin {
                init() {
                    const editor = this.editor;
                    const alignmentCommand = editor.commands.get('alignment');

                    if (!alignmentCommand) {
                        console.warn('Alignment command unavailable; custom alignment buttons not registered.');
                        return;
                    }

                    const addAlignmentButton = (name, value, label, icon) => {
                        if (editor.ui.componentFactory.has(name)) {
                            return;
                        }

                        editor.ui.componentFactory.add(name, locale => {
                            const button = new ButtonCtor(locale);
                            button.set({
                                label,
                                ariaLabel: label,
                                withText: false,
                                tooltip: true,
                                icon
                            });

                            button.bind('isOn').to(alignmentCommand, 'value', current => {
                                const normalizedCurrent = current || 'left';
                                const normalizedValue = value || 'left';
                                return normalizedCurrent === normalizedValue;
                            });
                            button.bind('isEnabled').to(alignmentCommand, 'isEnabled');

                            button.on('execute', () => {
                                const nextValue = alignmentCommand.value === value ? 'left' : value;
                                editor.execute('alignment', { value: nextValue });
                                editor.editing.view.focus();
                            });

                            return button;
                        });
                    };

                    addAlignmentButton('alignLeft', 'left', 'تراز چپ', alignmentIcons.left);
                    addAlignmentButton('alignCenter', 'center', 'تراز وسط', alignmentIcons.center);
                    addAlignmentButton('alignRight', 'right', 'تراز راست', alignmentIcons.right);
                    addAlignmentButton('alignJustify', 'justify', 'تراز دوطرفه', alignmentIcons.justify);
                }
            }

            window.CKEditorPlugins.CustomAlignmentButtons = CustomAlignmentButtonsPlugin;
        }
    };
    window.ensureScheduleItemCustomPlugins = ensureCustomPlugins;

    ensureCustomPlugins();

    // Initialize description editor if container exists
    const initializeDescriptionEditor = () => {
        const descriptionEditorElement = document.querySelector('#descriptionEditorContainer');
        if (!descriptionEditorElement) return;

        const alignmentToolbarItems = window.CKEditorPlugins.CustomAlignmentButtons
            ? ['alignLeft', 'alignCenter', 'alignRight', 'alignJustify']
            : ['alignment'];
        const directionToolbarItems = window.CKEditorPlugins.TextDirection
            ? ['directionRtl', 'directionLtr']
            : [];

        const toolbarItems = [
            'undo', 'redo', '|',
            'bold', 'italic', 'underline', '|',
            ...alignmentToolbarItems
        ];

        if (directionToolbarItems.length) {
            toolbarItems.push('|', ...directionToolbarItems);
        }

        toolbarItems.push(
            '|',
            'fontSize', 'fontFamily', 'fontColor', 'fontBackgroundColor', '|',
            'blockQuote', 'numberedList', 'bulletedList', '|',
            'heading', '|', 'sourceEditing'
        );

        ClassicEditor.create(descriptionEditorElement, {
            licenseKey: 'GPL',
            language: {
                ui: 'fa',
                content: 'fa'
            },
            plugins: [
                Essentials, Paragraph, Bold, Italic, Underline, Font,
                Image, ImageToolbar, ImageCaption, ImageStyle, ImageResize,
                SourceEditing, Autoformat, BlockQuote, Heading, List, Alignment,
                Table, TableToolbar, Clipboard,
                GeneralHtmlSupport, HtmlEmbed
            ],
            extraPlugins: [
                window.CKEditorPlugins.TextDirection ?? null,
                window.CKEditorPlugins.CustomAlignmentButtons ?? null
            ].filter(Boolean),
            toolbar: toolbarItems,
            ui: {
                viewportOffset: {
                    top: 120,
                    bottom: 24
                }
            },
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
