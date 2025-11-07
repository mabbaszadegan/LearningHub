document.addEventListener('DOMContentLoaded', () => {
    if (window.bottomNavigation) {
        window.bottomNavigation.setActivePage('profile');
    }

    const updateProfileForm = document.getElementById('profileForm');
    const createStudentProfileForm = document.getElementById('createStudentProfileForm');
    const cancelProfileEditBtn = document.getElementById('cancelProfileEditBtn');
    const submitProfileBtn = document.getElementById('createStudentProfileSubmitBtn');
    const profileIdInput = document.getElementById('profileId');
    const profilesList = document.querySelector('.student-profiles-list');

    const antiForgeryToken = document.querySelector('input[name="__RequestVerificationToken"]')?.value ?? '';

    const fetchJson = async (url, data) => {
        const formData = new FormData();
        Object.entries(data).forEach(([key, value]) => {
            if (value !== undefined && value !== null) {
                formData.append(key, value);
            }
        });

        if (antiForgeryToken) {
            formData.append('__RequestVerificationToken', antiForgeryToken);
        }

        const response = await fetch(url, {
            method: 'POST',
            headers: {
                'X-Requested-With': 'XMLHttpRequest'
            },
            body: formData
        });

        if (!response.ok) {
            throw new Error(`Request failed with status ${response.status}`);
        }

        return response.json();
    };

    const resetStudentProfileForm = () => {
        if (!createStudentProfileForm) {
            return;
        }

        createStudentProfileForm.reset();
        if (profileIdInput) {
            profileIdInput.value = '';
        }
        if (submitProfileBtn) {
            submitProfileBtn.innerHTML = '<i class="fas fa-plus me-2"></i>ایجاد پروفایل جدید';
        }
        if (cancelProfileEditBtn) {
            cancelProfileEditBtn.classList.add('d-none');
        }
    };

    const handleResponse = (result, successMessage) => {
        if (result?.success) {
            if (window.toastr) {
                window.toastr.success(result.message || successMessage);
            }
            window.location.reload();
        } else {
            if (window.toastr) {
                window.toastr.error(result?.error || 'خطایی رخ داده است');
            }
        }
    };

    if (updateProfileForm) {
        updateProfileForm.addEventListener('submit', async (event) => {
            event.preventDefault();

            const formData = new FormData(updateProfileForm);

            try {
                const result = await fetchJson(updateProfileForm.action || window.location.pathname, {
                    firstName: formData.get('firstName')?.toString() ?? '',
                    lastName: formData.get('lastName')?.toString() ?? '',
                    bio: formData.get('bio')?.toString() ?? '',
                    phoneNumber: formData.get('phoneNumber')?.toString() ?? ''
                });

                handleResponse(result, 'پروفایل با موفقیت به‌روزرسانی شد');
            } catch (error) {
                console.error('Failed to update profile information', error);
                if (window.toastr) {
                    window.toastr.error('خطا در به‌روزرسانی پروفایل');
                }
            }
        });
    }

    if (createStudentProfileForm) {
        createStudentProfileForm.addEventListener('submit', async (event) => {
            event.preventDefault();

            const formData = new FormData(createStudentProfileForm);
            const displayName = formData.get('displayName')?.toString()?.trim() ?? '';
            if (!displayName) {
                if (window.toastr) {
                    window.toastr.error('نام نمایشی را وارد کنید');
                }
                return;
            }

            const profileIdValue = formData.get('profileId')?.toString()?.trim() ?? '';
            const payload = {
                displayName,
                gradeLevel: formData.get('gradeLevel')?.toString()?.trim() ?? '',
                notes: formData.get('notes')?.toString()?.trim() ?? ''
            };

            const dateOfBirthRaw = formData.get('dateOfBirth')?.toString()?.trim() ?? '';
            if (dateOfBirthRaw) {
                payload.dateOfBirth = dateOfBirthRaw;
            }

            const isEditMode = profileIdValue !== '';
            if (isEditMode) {
                payload.profileId = profileIdValue;
            }

            // Prefer explicit dataset values set on container
            const profilesContainer = document.querySelector('.student-profiles-list');
            const updateUrl = profilesContainer?.getAttribute('data-update-url') ?? '';
            const createUrl = createStudentProfileForm.getAttribute('action') ?? window.location.pathname;

            if (isEditMode && !updateUrl) {
                if (window.toastr) {
                    window.toastr.error('امکان به‌روزرسانی پروفایل در حال حاضر وجود ندارد.');
                }
                return;
            }

            if (!isEditMode && !createUrl) {
                if (window.toastr) {
                    window.toastr.error('امکان ایجاد پروفایل در حال حاضر وجود ندارد.');
                }
                return;
            }

            try {
                const result = await fetchJson(isEditMode ? updateUrl : createUrl, payload);
                handleResponse(result, isEditMode ? 'پروفایل با موفقیت به‌روزرسانی شد' : 'پروفایل جدید با موفقیت ایجاد شد');
            } catch (error) {
                console.error('Failed to submit student profile form', error);
                if (window.toastr) {
                    window.toastr.error('خطا در ذخیره پروفایل یادگیرنده');
                }
            }
        });
    }

    const enterEditMode = (entry) => {
        if (!createStudentProfileForm || !profileIdInput || !submitProfileBtn || !cancelProfileEditBtn) {
            return;
        }

        const displayName = entry.dataset.displayName ?? '';
        const gradeLevel = entry.dataset.gradeLevel ?? '';
        const dateOfBirth = entry.dataset.dateOfBirth ?? '';
        const notes = entry.dataset.notes ?? '';

        const displayNameInput = createStudentProfileForm.querySelector('#newProfileDisplayName');
        if (displayNameInput) {
            displayNameInput.value = displayName;
        }
        const gradeLevelInput = createStudentProfileForm.querySelector('#newProfileGradeLevel');
        if (gradeLevelInput) {
            gradeLevelInput.value = gradeLevel;
        }
        const dateField = createStudentProfileForm.querySelector('#newProfileDateOfBirth');
        if (dateField) {
            dateField.value = dateOfBirth;
        }
        const notesField = createStudentProfileForm.querySelector('#newProfileNotes');
        if (notesField) {
            notesField.value = notes;
        }

        profileIdInput.value = entry.dataset.profileId ?? '';
        submitProfileBtn.innerHTML = '<i class="fas fa-save me-2"></i>ذخیره تغییرات';
        cancelProfileEditBtn.classList.remove('d-none');
    };

    if (profilesList) {
        profilesList.addEventListener('click', async (event) => {
            const target = event.target;
            if (!(target instanceof HTMLElement)) {
                return;
            }

            const action = target.dataset.action;
            if (!action) {
                return;
            }

            const entry = target.closest('.student-profile-entry');
            if (!entry) {
                return;
            }

            const profileId = entry.dataset.profileId;
            if (!profileId) {
                return;
            }

            const profilesContainer = profilesList;
            const archiveUrl = profilesContainer.getAttribute('data-archive-url') ?? '';
            const restoreUrl = profilesContainer.getAttribute('data-restore-url') ?? '';
            const setActiveUrl = profilesContainer.getAttribute('data-set-active-url') ?? '';
            const updateUrl = profilesContainer.getAttribute('data-update-url') ?? '';

            try {
                switch (action) {
                    case 'set-active': {
                        const result = await fetchJson(setActiveUrl, { profileId });
                        handleResponse(result, 'پروفایل فعال به‌روزرسانی شد');
                        break;
                    }
                    case 'edit': {
                        enterEditMode(entry);
                        break;
                    }
                    case 'archive': {
                        const confirmed = window.confirm('آیا از آرشیو کردن این پروفایل مطمئن هستید؟');
                        if (!confirmed) {
                            return;
                        }
                        const result = await fetchJson(archiveUrl, { profileId });
                        handleResponse(result, 'پروفایل با موفقیت آرشیو شد');
                        break;
                    }
                    case 'restore': {
                        const result = await fetchJson(restoreUrl, { profileId });
                        handleResponse(result, 'پروفایل با موفقیت فعال شد');
                        break;
                    }
                }
            } catch (error) {
                console.error('Student profile action failed', error);
                if (window.toastr) {
                    window.toastr.error('خطا در انجام عملیات روی پروفایل یادگیرنده');
                }
            }
        });
    }

    if (cancelProfileEditBtn) {
        cancelProfileEditBtn.addEventListener('click', () => {
            resetStudentProfileForm();
        });
    }

    const changePasswordBtn = document.getElementById('changePasswordBtn');
    if (changePasswordBtn) {
        changePasswordBtn.addEventListener('click', () => {
            if (window.toastr) {
                window.toastr.info('برای تغییر رمز عبور با پشتیبانی تماس بگیرید');
            }
        });
    }
});

