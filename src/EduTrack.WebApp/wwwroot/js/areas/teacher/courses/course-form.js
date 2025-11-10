document.addEventListener('DOMContentLoaded', () => {
    const fileInput = document.getElementById('courseThumbnailInput');
    if (!fileInput) {
        return;
    }

    const thumbnailInput = document.querySelector('input[name="Thumbnail"]');
    const thumbnailIdInput = document.querySelector('input[name="ThumbnailFileId"]');
    const wrapper = document.querySelector('[data-thumbnail-wrapper]');
    const placeholder = document.querySelector('[data-thumbnail-placeholder]');
    const removeButton = document.querySelector('[data-remove-thumbnail]');
    const statusElement = document.querySelector('[data-thumbnail-status]');

    fileInput.addEventListener('change', async (event) => {
        const file = event.target.files && event.target.files[0];
        if (!file) {
            return;
        }

        clearStatus();

        if (file.size > 5 * 1024 * 1024) {
            setStatus('حجم فایل نباید بیشتر از ۵ مگابایت باشد.', true);
            resetFileInput();
            return;
        }

        setStatus('در حال آپلود تصویر...', false);

        try {
            const formData = new FormData();
            formData.append('file', file);
            formData.append('type', 'image');

            const response = await fetch('/FileUpload/UploadContentFile', {
                method: 'POST',
                body: formData
            });

            if (!response.ok) {
                throw new Error('خطا در آپلود تصویر');
            }

            const result = await response.json();
            if (!result?.success) {
                throw new Error(result?.message || 'خطا در آپلود تصویر');
            }

            const data = result.data || {};
            const fileId = data.id;
            const url = data.url || (fileId ? `/FileUpload/GetFile/${fileId}` : null);

            if (!fileId || !url) {
                throw new Error('پاسخ نامعتبر از سرور');
            }

            updatePreview(url);
            updateHiddenFields(fileId, url);
            setStatus('تصویر با موفقیت بارگذاری شد.', false);
        } catch (error) {
            console.error('Course thumbnail upload error:', error);
            setStatus(error.message || 'خطا در آپلود تصویر', true);
            resetPreview();
            updateHiddenFields(null, null);
        } finally {
            resetFileInput();
        }
    });

    removeButton?.addEventListener('click', (event) => {
        event.preventDefault();
        resetPreview();
        updateHiddenFields(null, null);
        setStatus('تصویر دوره حذف شد.', false);
        resetFileInput();
    });

    function updateHiddenFields(fileId, url) {
        if (thumbnailIdInput) {
            thumbnailIdInput.value = fileId ? String(fileId) : '';
        }
        if (thumbnailInput) {
            thumbnailInput.value = url ?? '';
        }
    }

    function updatePreview(url) {
        if (!wrapper) {
            return;
        }

        let previewImage = wrapper.querySelector('[data-thumbnail-preview]');
        if (!previewImage) {
            previewImage = document.createElement('img');
            previewImage.classList.add('course-thumbnail-preview', 'img-fluid');
            previewImage.setAttribute('data-thumbnail-preview', '');
            wrapper.innerHTML = '';
            wrapper.appendChild(previewImage);
        }

        previewImage.src = url;
        previewImage.alt = 'تصویر دوره';

        if (placeholder) {
            placeholder.style.display = 'none';
        }
    }

    function resetPreview() {
        if (!wrapper) {
            return;
        }

        const previewImage = wrapper.querySelector('[data-thumbnail-preview]');
        if (previewImage) {
            previewImage.remove();
        }

        if (placeholder) {
            placeholder.style.display = '';
        } else if (wrapper) {
            wrapper.innerHTML = `
                <div class="course-thumbnail-placeholder" data-thumbnail-placeholder>
                    <i class="fas fa-image"></i>
                    <span>تصویری انتخاب نشده است</span>
                </div>`;
        }
    }

    function resetFileInput() {
        if (fileInput) {
            fileInput.value = '';
        }
    }

    function setStatus(message, isError) {
        if (!statusElement) {
            return;
        }

        statusElement.textContent = message;
        statusElement.classList.toggle('text-danger', Boolean(isError));
        statusElement.classList.toggle('text-muted', !isError);
    }

    function clearStatus() {
        if (!statusElement) {
            return;
        }
        statusElement.textContent = '';
        statusElement.classList.remove('text-danger');
    }
});

