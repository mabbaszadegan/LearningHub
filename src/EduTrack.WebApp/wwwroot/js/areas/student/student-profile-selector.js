document.addEventListener('DOMContentLoaded', () => {
    const form = document.getElementById('studentProfileSelectorForm');
    const select = document.getElementById('studentProfileSelectorSelect');

    if (!form || !select) {
        return;
    }

    const antiForgeryInput = form.querySelector('input[name="__RequestVerificationToken"]');

    const handleResponse = (result) => {
        if (result?.success) {
            if (window.toastr) {
                window.toastr.success(result.message || 'پروفایل فعال به‌روزرسانی شد');
            }
            if (result.reload === true) {
                window.location.reload();
            }
        } else {
            if (window.toastr) {
                window.toastr.error(result?.error || 'خطا در به‌روزرسانی پروفایل فعال');
            }
        }
    };

    select.addEventListener('change', async () => {
        const formData = new FormData();
        formData.append('profileId', select.value || '');

        if (antiForgeryInput) {
            formData.append('__RequestVerificationToken', antiForgeryInput.value);
        }

        try {
            const response = await fetch(form.action, {
                method: 'POST',
                body: formData,
                headers: {
                    'X-Requested-With': 'XMLHttpRequest'
                }
            });

            if (!response.ok) {
                throw new Error(`Request failed with status ${response.status}`);
            }

            const result = await response.json();
            handleResponse(result);
        } catch (error) {
            console.error('Failed to update active student profile', error);
            if (window.toastr) {
                window.toastr.error('خطا در برقراری ارتباط با سرور');
            }
        }
    });
});

