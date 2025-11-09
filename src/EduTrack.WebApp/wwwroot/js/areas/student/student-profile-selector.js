document.addEventListener('DOMContentLoaded', () => {
    const desktopProfile = document.querySelector('[data-component="header-profile"]');
    const desktopToggle = desktopProfile?.querySelector('[data-action="toggle-profile-dropdown"]');
    const desktopDropdown = desktopProfile?.querySelector('[data-element="profile-dropdown"]');
    const overlay = document.querySelector('[data-overlay="header-profile"]');
    const overlayCloseButtons = overlay ? overlay.querySelectorAll('[data-action="header-close-overlay"]') : [];
    const overlayList = overlay?.querySelector('[data-element="header-profile-list"]');
    const overlayCreateForm = document.getElementById('headerProfileCreateForm');
    const overlayCreateInput = document.getElementById('headerProfileCreateName');
    const setForm = document.getElementById('headerProfileSetForm');
    const openCreateButtons = document.querySelectorAll('[data-action="header-open-create"]');
    const openMenuButtons = document.querySelectorAll('[data-action="header-open-menu"]');
    const createCancelButtons = overlay ? overlay.querySelectorAll('[data-action="header-cancel-create"]') : [];
    const mobileProfile = document.querySelector('[data-component="mobile-header-profile"]');
    const mobileDropdown = mobileProfile?.querySelector('[data-element="mobile-profile-dropdown"]');
    const mobileToggle = mobileProfile?.querySelector('[data-action="header-open-menu"]');
    const desktopInlineCreateToggle = desktopProfile?.querySelector('[data-action="header-inline-create-toggle"]');
    const desktopInlineCreatePanel = document.getElementById('desktopProfileInlineCreateForm');
    const desktopInlineCreateInput = document.getElementById('desktopProfileInlineCreateName');
    const desktopInlineCreateSubmit = desktopInlineCreatePanel?.querySelector('[data-action="header-inline-create-submit"]');
    const mobileInlineCreateToggle = mobileProfile?.querySelector('[data-action="header-inline-create-toggle"]');
    const mobileInlineCreatePanel = document.getElementById('mobileProfileInlineCreateForm');
    const mobileInlineCreateInput = document.getElementById('mobileProfileInlineCreateName');
    const mobileInlineCreateSubmit = mobileInlineCreatePanel?.querySelector('[data-action="header-inline-create-submit"]');

    const getAntiForgeryToken = () => {
        const sources = [
            () => overlayCreateForm?.querySelector('input[name="__RequestVerificationToken"]')?.value,
            () => desktopInlineCreatePanel?.querySelector('input[name="__RequestVerificationToken"]')?.value,
            () => mobileInlineCreatePanel?.querySelector('input[name="__RequestVerificationToken"]')?.value,
            () => setForm?.querySelector('input[name="__RequestVerificationToken"]')?.value
        ];

        for (const getSource of sources) {
            const token = getSource();
            if (token) {
                return token;
            }
        }

        return null;
    };

    const closeDesktopDropdown = () => {
        if (!desktopDropdown || !desktopToggle) {
            return;
        }
        desktopDropdown.hidden = true;
        desktopToggle.setAttribute('aria-expanded', 'false');
        desktopToggle.classList.remove('is-open');
        document.removeEventListener('click', handleOutsideClick);
        resetInlineCreateForm(desktopInlineCreatePanel, desktopInlineCreateInput, desktopInlineCreateToggle, desktopInlineCreateSubmit);
    };

    const handleOutsideClick = (event) => {
        if (!desktopProfile || desktopProfile.contains(event.target)) {
            return;
        }
        closeDesktopDropdown();
    };

    const toggleDesktopDropdown = () => {
        if (!desktopDropdown || !desktopToggle) {
            return;
        }
        const isHidden = desktopDropdown.hasAttribute('hidden');
        if (isHidden) {
            desktopDropdown.hidden = false;
            desktopToggle.setAttribute('aria-expanded', 'true');
            desktopToggle.classList.add('is-open');
            window.setTimeout(() => {
                document.addEventListener('click', handleOutsideClick);
            }, 0);
        } else {
            closeDesktopDropdown();
        }
    };

    desktopToggle?.addEventListener('click', (event) => {
        event.preventDefault();
        toggleDesktopDropdown();
    });

    const toggleBodyScroll = (disable) => {
        if (disable) {
            document.body.dataset.headerProfileOverlay = 'open';
            document.body.style.overflow = 'hidden';
        } else {
            delete document.body.dataset.headerProfileOverlay;
            document.body.style.overflow = '';
        }
    };

    const handleMobileOutsideClick = (event) => {
        if (!mobileProfile || mobileProfile.contains(event.target)) {
            return;
        }
        closeMobileDropdown();
    };

    const closeMobileDropdown = () => {
        if (!mobileDropdown || !mobileToggle) {
            return;
        }
        mobileDropdown.hidden = true;
        mobileToggle.setAttribute('aria-expanded', 'false');
        document.removeEventListener('click', handleMobileOutsideClick);
        resetInlineCreateForm(mobileInlineCreatePanel, mobileInlineCreateInput, mobileInlineCreateToggle, mobileInlineCreateSubmit);
    };

    const toggleMobileDropdown = () => {
        if (!mobileDropdown || !mobileToggle) {
            return;
        }
        const isHidden = mobileDropdown.hidden;
        if (isHidden) {
            mobileDropdown.hidden = false;
            mobileToggle.setAttribute('aria-expanded', 'true');
            window.setTimeout(() => {
                document.addEventListener('click', handleMobileOutsideClick);
            }, 0);
        } else {
            closeMobileDropdown();
        }
    };

    const showOverlayCreateForm = () => {
        if (!overlayCreateForm) {
            return;
        }
        overlayCreateForm.hidden = false;
        window.setTimeout(() => overlayCreateInput?.focus(), 80);
    };

    const hideOverlayCreateForm = () => {
        if (!overlayCreateForm) {
            return;
        }
        overlayCreateForm.hidden = true;
        if (overlayCreateInput) {
            overlayCreateInput.value = '';
        }
    };

    const openOverlay = (focusCreate = false) => {
        if (!overlay) {
            return;
        }
        closeDesktopDropdown();
        closeMobileDropdown();
        overlay.hidden = false;
        overlay.setAttribute('aria-hidden', 'false');
        toggleBodyScroll(true);
        if (focusCreate) {
            showOverlayCreateForm();
        } else {
            hideOverlayCreateForm();
        }
    };

    const closeOverlay = () => {
        if (!overlay) {
            return;
        }
        overlay.hidden = true;
        overlay.setAttribute('aria-hidden', 'true');
        toggleBodyScroll(false);
        hideOverlayCreateForm();
        closeMobileDropdown();
        openMenuButtons.forEach((button) => button.setAttribute('aria-expanded', 'false'));
    };

    openCreateButtons.forEach((button) => {
        button.addEventListener('click', (event) => {
            event.preventDefault();
            const mode = button.getAttribute('data-open-mode');
            const shouldFocusCreate = mode === 'create';
            closeMobileDropdown();
            openOverlay(shouldFocusCreate);
        });
    });

    openMenuButtons.forEach((button) => {
        button.addEventListener('click', (event) => {
            event.preventDefault();
            if (button === mobileToggle) {
                toggleMobileDropdown();
            } else {
                openOverlay(false);
            }
        });
    });

    overlayCloseButtons.forEach((button) => {
        button.addEventListener('click', (event) => {
            event.preventDefault();
            closeOverlay();
        });
    });

    overlay?.addEventListener('click', (event) => {
        if (event.target === overlay) {
            closeOverlay();
        }
    });

    document.addEventListener('keydown', (event) => {
        if (event.key === 'Escape') {
            closeOverlay();
            closeDesktopDropdown();
        }
    });

    createCancelButtons.forEach((button) => {
        button.addEventListener('click', (event) => {
            event.preventDefault();
            closeOverlay();
        });
    });

    const resetInlineCreateForm = (panel, input, toggleButton, submitButton) => {
        if (!panel) {
            return;
        }

        panel.hidden = true;
        if (input) {
            input.value = '';
            input.removeAttribute('disabled');
        }

        submitButton?.removeAttribute('disabled');

        if (toggleButton) {
            toggleButton.setAttribute('aria-expanded', 'false');
        }
    };

    const toggleInlineCreateForm = (panel, input, toggleButton, submitButton) => {
        if (!panel || !toggleButton) {
            return;
        }

        const isHidden = panel.hasAttribute('hidden');
        if (isHidden) {
            panel.hidden = false;
            toggleButton.setAttribute('aria-expanded', 'true');
            window.setTimeout(() => input?.focus(), 80);
        } else {
            resetInlineCreateForm(panel, input, toggleButton, submitButton);
        }
    };

    desktopInlineCreateToggle?.addEventListener('click', (event) => {
        event.preventDefault();
        toggleInlineCreateForm(desktopInlineCreatePanel, desktopInlineCreateInput, desktopInlineCreateToggle, desktopInlineCreateSubmit);
    });

    mobileInlineCreateToggle?.addEventListener('click', (event) => {
        event.preventDefault();
        toggleInlineCreateForm(mobileInlineCreatePanel, mobileInlineCreateInput, mobileInlineCreateToggle, mobileInlineCreateSubmit);
    });

    const handleSuccess = (message) => {
        if (window.toastr) {
            window.toastr.success(message || 'عملیات با موفقیت انجام شد');
        }
        window.setTimeout(() => window.location.reload(), 250);
    };

    const handleFailure = (error) => {
        if (window.toastr) {
            window.toastr.error(error || 'خطا در انجام عملیات');
        }
    };

    const submitProfileChange = async (profileIdValue) => {
        if (!setForm) {
            return;
        }

        const token = getAntiForgeryToken();
        if (!token) {
            handleFailure('توکن امنیتی یافت نشد');
            return;
        }

        const formData = new FormData();
        formData.append('profileId', profileIdValue ?? '');
        formData.append('__RequestVerificationToken', token);

        try {
            const response = await fetch(setForm.action, {
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
            if (result?.success) {
                handleSuccess(result.message);
            } else {
                handleFailure(result?.error);
            }
        } catch (error) {
            console.error('Failed to set active profile', error);
            handleFailure('خطا در برقراری ارتباط با سرور');
        }
    };

    const attachProfileSetListeners = (root) => {
        root.querySelectorAll('[data-action="header-profile-set"]').forEach((button) => {
            button.addEventListener('click', async (event) => {
                event.preventDefault();
                const profileId = button.getAttribute('data-profile-id') ?? '';
                closeMobileDropdown();
                closeDesktopDropdown();
                await submitProfileChange(profileId);
            });
        });
    };

    if (desktopDropdown) {
        attachProfileSetListeners(desktopDropdown);
    }

    if (overlayList) {
        attachProfileSetListeners(overlayList);
    }

    if (mobileDropdown) {
        attachProfileSetListeners(mobileDropdown);
    }

    const submitProfileCreation = async ({ actionUrl, input, tokenField, submitButton, onSuccess }) => {
        if (!actionUrl || !input) {
            return;
        }

        const nameValue = input.value.trim();
        if (!nameValue) {
            handleFailure('لطفاً یک نام برای پروفایل وارد کنید');
            input.focus();
            return;
        }

        const token = tokenField?.value ?? getAntiForgeryToken();
        if (!token) {
            handleFailure('توکن امنیتی یافت نشد');
            return;
        }

        submitButton?.setAttribute('disabled', 'true');
        input.setAttribute('disabled', 'true');

        const formData = new FormData();
        formData.append('displayName', nameValue);
        formData.append('__RequestVerificationToken', token);

        try {
            const response = await fetch(actionUrl, {
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
            if (result?.success) {
                if (typeof onSuccess === 'function') {
                    onSuccess();
                }
                handleSuccess(result.message);
            } else {
                handleFailure(result?.error);
            }
        } catch (error) {
            console.error('Failed to create profile', error);
            handleFailure('خطا در برقراری ارتباط با سرور');
        } finally {
            submitButton?.removeAttribute('disabled');
            input.removeAttribute('disabled');
        }
    };

    overlayCreateForm?.addEventListener('submit', async (event) => {
        event.preventDefault();
        await submitProfileCreation({
            actionUrl: overlayCreateForm.action,
            input: overlayCreateInput,
            tokenField: overlayCreateForm.querySelector('input[name="__RequestVerificationToken"]'),
            submitButton: overlayCreateForm.querySelector('[type="submit"]')
        });
    });

    desktopInlineCreateSubmit?.addEventListener('click', async (event) => {
        event.preventDefault();
        await submitProfileCreation({
            actionUrl: desktopInlineCreatePanel?.dataset.submitUrl ?? desktopInlineCreatePanel?.getAttribute('data-submit-url'),
            input: desktopInlineCreateInput,
            tokenField: desktopInlineCreatePanel?.querySelector('input[name="__RequestVerificationToken"]'),
            submitButton: desktopInlineCreateSubmit,
            onSuccess: () => {
                resetInlineCreateForm(desktopInlineCreatePanel, desktopInlineCreateInput, desktopInlineCreateToggle, desktopInlineCreateSubmit);
            }
        });
    });

    mobileInlineCreateSubmit?.addEventListener('click', async (event) => {
        event.preventDefault();
        await submitProfileCreation({
            actionUrl: mobileInlineCreatePanel?.dataset.submitUrl ?? mobileInlineCreatePanel?.getAttribute('data-submit-url'),
            input: mobileInlineCreateInput,
            tokenField: mobileInlineCreatePanel?.querySelector('input[name="__RequestVerificationToken"]'),
            submitButton: mobileInlineCreateSubmit,
            onSuccess: () => {
                resetInlineCreateForm(mobileInlineCreatePanel, mobileInlineCreateInput, mobileInlineCreateToggle, mobileInlineCreateSubmit);
            }
        });
    });
});

